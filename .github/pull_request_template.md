## Summary

- What changed?
- Why is the change needed?

## Validation

- [ ] `dotnet restore SharpCompress.slnx --locked-mode`
- [ ] `dotnet build SharpCompress.slnx --configuration Release --no-restore`
- [ ] `dotnet csharpier check .`
- [ ] `dotnet test tests/SharpCompress.Test/SharpCompress.Test.csproj --configuration Release --no-build`
- [ ] `dotnet pack src/SharpCompress/SharpCompress.csproj --configuration Release --no-build --output artifacts`
- [ ] Documentation and regression tests are updated where needed.
- [ ] No credentials, tokens, or private archive contents are included.

## Compatibility

Describe any public API, package, format, performance, or stream-lifecycle impact.
