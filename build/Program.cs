using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GlobExpressions;
using static Bullseye.Targets;
using static SimpleExec.Command;

const string Clean = "clean";
const string Restore = "restore";
const string UpdateLocks = "update-locks";
const string Build = "build";
const string Test = "test";
const string Format = "format";
const string CheckFormat = "check-format";
const string Publish = "publish";
const string DisplayBenchmarkResults = "display-benchmark-results";
const string CompareBenchmarkResults = "compare-benchmark-results";
const string GenerateBaseline = "generate-baseline";
const string WriteBaselineFromResults = "write-baseline-from-results";

Target(
    Clean,
    ["**/bin", "**/obj"],
    dir =>
    {
        IEnumerable<string> GetDirectories(string d)
        {
            return Glob.Directories(".", d);
        }

        void RemoveDirectory(string d)
        {
            if (Directory.Exists(d))
            {
                Console.WriteLine(d);
                Directory.Delete(d, true);
            }
        }

        foreach (var d in GetDirectories(dir))
        {
            RemoveDirectory(d);
        }
    }
);

Target(
    Format,
    () =>
    {
        Run("dotnet", "tool restore");
        Run("dotnet", "csharpier format .");
    }
);
Target(
    CheckFormat,
    () =>
    {
        Run("dotnet", "tool restore");
        Run("dotnet", "csharpier check .");
    }
);
Target(Restore, [CheckFormat], () => Run("dotnet", "restore --locked-mode"));
Target(UpdateLocks, [CheckFormat], () => Run("dotnet", "restore --force-evaluate"));

Target(
    Build,
    [Restore],
    () =>
    {
        Run("dotnet", "build src/SharpCompress/SharpCompress.csproj -c Release --no-restore");
    }
);

Target(
    Test,
    [Build],
    () =>
    {
        foreach (var file in Glob.Files(".", "**/*.Test.csproj"))
        {
            Run("dotnet", $"test {file} -c Release -f net10.0 --no-restore --verbosity=normal");
        }
    }
);

Target(
    Publish,
    [Test],
    () =>
    {
        Run("dotnet", "pack src/SharpCompress/SharpCompress.csproj -c Release -o artifacts/");
    }
);

Target(
    DisplayBenchmarkResults,
    () =>
    {
        var githubStepSummary = Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");
        var resultsDir = "benchmark-results/results";

        if (!Directory.Exists(resultsDir))
        {
            Console.WriteLine("No benchmark results found.");
            return;
        }

        var markdownFiles = Directory
            .GetFiles(resultsDir, "*-report-github.md")
            .OrderBy(f => f)
            .ToList();

        if (markdownFiles.Count == 0)
        {
            Console.WriteLine("No benchmark markdown reports found.");
            return;
        }

        var output = new List<string> { "## Benchmark Results", "" };

        foreach (var file in markdownFiles)
        {
            Console.WriteLine($"Processing {Path.GetFileName(file)}");
            var content = File.ReadAllText(file);
            output.Add(content);
            output.Add("");
        }

        // Write to GitHub Step Summary if available
        if (!string.IsNullOrEmpty(githubStepSummary))
        {
            File.AppendAllLines(githubStepSummary, output);
            Console.WriteLine($"Benchmark results written to GitHub Step Summary");
        }
        else
        {
            // Write to console if not in GitHub Actions
            foreach (var line in output)
            {
                Console.WriteLine(line);
            }
        }
    }
);

Target(
    CompareBenchmarkResults,
    () =>
    {
        var githubStepSummary = Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");
        var baselinePath = "tests/SharpCompress.Performance/baseline-results.md";
        var resultsDir = "benchmark-results/results";

        var output = new List<string> { "## Comparison with Baseline", "" };

        if (!File.Exists(baselinePath))
        {
            Console.WriteLine("Baseline file not found");
            output.Add("⚠️ Baseline file not found. Run `generate-baseline` to create it.");
            WriteOutput(output, githubStepSummary);
            return;
        }

        if (!Directory.Exists(resultsDir))
        {
            Console.WriteLine("No current benchmark results found.");
            output.Add("⚠️ No current benchmark results found. Showing baseline only.");
            output.Add("");
            output.Add("### Baseline Results");
            output.AddRange(File.ReadAllLines(baselinePath));
            WriteOutput(output, githubStepSummary);
            return;
        }

        var markdownFiles = Directory
            .GetFiles(resultsDir, "*-report-github.md")
            .OrderBy(f => f)
            .ToList();

        if (markdownFiles.Count == 0)
        {
            Console.WriteLine("No current benchmark markdown reports found.");
            output.Add("⚠️ No current benchmark results found. Showing baseline only.");
            output.Add("");
            output.Add("### Baseline Results");
            output.AddRange(File.ReadAllLines(baselinePath));
            WriteOutput(output, githubStepSummary);
            return;
        }

        Console.WriteLine("Parsing baseline results...");
        var baselineMetrics = ParseBenchmarkResults(File.ReadAllText(baselinePath));

        Console.WriteLine("Parsing current results...");
        var currentText = string.Join("\n", markdownFiles.Select(f => File.ReadAllText(f)));
        var currentMetrics = ParseBenchmarkResults(currentText);

        Console.WriteLine("Comparing results...");
        output.Add("### Performance Comparison");
        output.Add("");
        output.Add(
            "| Benchmark | Baseline Mean | Current Mean | Change | Baseline Memory | Current Memory | Change |"
        );
        output.Add(
            "|-----------|---------------|--------------|--------|-----------------|----------------|--------|"
        );

        var hasRegressions = false;
        var hasImprovements = false;

        foreach (var method in currentMetrics.Keys.Union(baselineMetrics.Keys).OrderBy(k => k))
        {
            var hasCurrent = currentMetrics.TryGetValue(method, out var current);
            var hasBaseline = baselineMetrics.TryGetValue(method, out var baseline);

            if (!hasCurrent)
            {
                output.Add(
                    $"| {method} | {baseline!.Mean} | ❌ Missing | N/A | {baseline.Memory} | N/A | N/A |"
                );
                continue;
            }

            if (!hasBaseline)
            {
                output.Add(
                    $"| {method} | ❌ New | {current!.Mean} | N/A | N/A | {current.Memory} | N/A |"
                );
                continue;
            }

            var timeChange = CalculateChange(baseline!.MeanValue, current!.MeanValue);
            var memChange = CalculateChange(baseline.MemoryValue, current.MemoryValue);

            var timeIcon =
                timeChange > 25 ? "🔴"
                : timeChange < -25 ? "🟢"
                : "⚪";
            var memIcon =
                memChange > 25 ? "🔴"
                : memChange < -25 ? "🟢"
                : "⚪";

            if (timeChange > 25 || memChange > 25)
            {
                hasRegressions = true;
            }
            if (timeChange < -25 || memChange < -25)
            {
                hasImprovements = true;
            }

            output.Add(
                $"| {method} | {baseline.Mean} | {current.Mean} | {timeIcon} {timeChange:+0.0;-0.0;0}% | {baseline.Memory} | {current.Memory} | {memIcon} {memChange:+0.0;-0.0;0}% |"
            );
        }

        output.Add("");
        output.Add("**Legend:**");
        output.Add("- 🔴 Regression (>25% slower/more memory)");
        output.Add("- 🟢 Improvement (>25% faster/less memory)");
        output.Add("- ⚪ No significant change");

        if (hasRegressions)
        {
            output.Add("");
            output.Add(
                "⚠️ **Warning**: Performance regressions detected. Review the changes carefully."
            );
        }
        else if (hasImprovements)
        {
            output.Add("");
            output.Add("✅ Performance improvements detected!");
        }
        else
        {
            output.Add("");
            output.Add("✅ Performance is stable compared to baseline.");
        }

        WriteOutput(output, githubStepSummary);
    }
);

Target(
    GenerateBaseline,
    () =>
    {
        var perfProject = "tests/SharpCompress.Performance/SharpCompress.Performance.csproj";
        var baselinePath = "tests/SharpCompress.Performance/baseline-results.md";
        var artifactsDir = "baseline-artifacts";

        Console.WriteLine("Building performance project...");
        Run("dotnet", $"build {perfProject} --configuration Release");

        Console.WriteLine("Running benchmarks to generate baseline...");
        Run(
            "dotnet",
            $"run --project {perfProject} --configuration Release --no-build -- --filter \"*\" --exporters markdown --artifacts {artifactsDir}"
        );

        WriteBaselineFromMarkdownReports(Path.Combine(artifactsDir, "results"), baselinePath);

        if (Directory.Exists(artifactsDir))
        {
            Directory.Delete(artifactsDir, true);
            Console.WriteLine("Cleaned up artifacts directory.");
        }
    }
);

Target(
    WriteBaselineFromResults,
    () =>
    {
        WriteBaselineFromMarkdownReports(
            "benchmark-results/results",
            "tests/SharpCompress.Performance/baseline-results.md"
        );
    }
);

Target("default", [Publish], () => Console.WriteLine("Done!"));

await RunTargetsAndExitAsync(args);

static void WriteBaselineFromMarkdownReports(string resultsDir, string baselinePath)
{
    if (!Directory.Exists(resultsDir))
    {
        throw new InvalidOperationException($"No benchmark results found at '{resultsDir}'.");
    }

    var markdownFiles = Directory
        .GetFiles(resultsDir, "*-report-github.md")
        .OrderBy(f => f)
        .ToList();

    if (markdownFiles.Count == 0)
    {
        throw new InvalidOperationException(
            $"No markdown reports (*-report-github.md) found in '{resultsDir}'."
        );
    }

    Console.WriteLine($"Combining {markdownFiles.Count} benchmark reports...");
    var baselineContent = new List<string>();

    foreach (var file in markdownFiles)
    {
        var lines = File.ReadAllLines(file);
        baselineContent.AddRange(lines.Select(l => l.Trim()).Where(l => l.StartsWith('|')));
    }

    File.WriteAllText(baselinePath, string.Join(Environment.NewLine, baselineContent));
    Console.WriteLine($"Baseline written to {baselinePath}");
}

static void WriteOutput(List<string> output, string? githubStepSummary)
{
    if (!string.IsNullOrEmpty(githubStepSummary))
    {
        File.AppendAllLines(githubStepSummary, output);
        Console.WriteLine("Comparison written to GitHub Step Summary");
    }
    else
    {
        foreach (var line in output)
        {
            Console.WriteLine(line);
        }
    }
}

static Dictionary<string, BenchmarkMetric> ParseBenchmarkResults(string markdown)
{
    var metrics = new Dictionary<string, BenchmarkMetric>();
    var lines = markdown.Split('\n');

    for (int i = 0; i < lines.Length; i++)
    {
        var line = lines[i].Trim();

        // Look for table rows with benchmark data
        if (line.StartsWith('|') && line.Contains("&#39;", StringComparison.Ordinal) && i > 0)
        {
            var parts = line.Split('|', StringSplitOptions.TrimEntries);
            if (parts.Length >= 5)
            {
                var method = parts[1].Replace("&#39;", "'", StringComparison.Ordinal);
                var meanStr = parts[2];

                // Find Allocated column - it's usually the last column or labeled "Allocated"
                string memoryStr = "N/A";
                for (int j = parts.Length - 2; j >= 2; j--)
                {
                    if (
                        parts[j].Contains("KB", StringComparison.Ordinal)
                        || parts[j].Contains("MB", StringComparison.Ordinal)
                        || parts[j].Contains("GB", StringComparison.Ordinal)
                        || parts[j].Contains('B', StringComparison.Ordinal)
                    )
                    {
                        memoryStr = parts[j];
                        break;
                    }
                }

                if (
                    !method.Equals("Method", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(method)
                )
                {
                    var metric = new BenchmarkMetric
                    {
                        Method = method,
                        Mean = meanStr,
                        MeanValue = ParseTimeValue(meanStr),
                        Memory = memoryStr,
                        MemoryValue = ParseMemoryValue(memoryStr),
                    };
                    metrics[method] = metric;
                }
            }
        }
    }

    return metrics;
}

static double ParseTimeValue(string timeStr)
{
    if (string.IsNullOrWhiteSpace(timeStr) || timeStr == "N/A" || timeStr == "NA")
    {
        return 0;
    }

    // Remove thousands separators and parse
    timeStr = timeStr.Replace(",", "", StringComparison.Ordinal).Trim();

    var match = Regex.Match(timeStr, @"([\d.]+)\s*(\w+)");
    if (!match.Success)
    {
        return 0;
    }

    var value = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
    var unit = match.Groups[2].Value.ToLower(CultureInfo.InvariantCulture);

    // Convert to microseconds for comparison
    return unit switch
    {
        "s" => value * 1_000_000,
        "ms" => value * 1_000,
        "μs" or "us" => value,
        "ns" => value / 1_000,
        _ => value,
    };
}

static double ParseMemoryValue(string memStr)
{
    if (string.IsNullOrWhiteSpace(memStr) || memStr == "N/A" || memStr == "NA")
    {
        return 0;
    }

    memStr = memStr.Replace(",", "", StringComparison.Ordinal).Trim();

    var match = Regex.Match(memStr, @"([\d.]+)\s*(\w+)");
    if (!match.Success)
    {
        return 0;
    }

    var value = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
    var unit = match.Groups[2].Value.ToUpper(CultureInfo.InvariantCulture);

    // Convert to KB for comparison
    return unit switch
    {
        "GB" => value * 1_024 * 1_024,
        "MB" => value * 1_024,
        "KB" => value,
        "B" => value / 1_024,
        _ => value,
    };
}

static double CalculateChange(double baseline, double current)
{
    if (baseline == 0)
    {
        return 0;
    }
    return ((current - baseline) / baseline) * 100;
}

record BenchmarkMetric
{
    public string Method { get; init; } = "";
    public string Mean { get; init; } = "";
    public double MeanValue { get; init; }
    public string Memory { get; init; } = "";
    public double MemoryValue { get; init; }
}
