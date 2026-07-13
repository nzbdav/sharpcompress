| Method                           | Mean       | Error    | StdDev   | Median     | Gen0    | Gen1    | Gen2    | Allocated |
|--------------------------------- |-----------:|---------:|---------:|-----------:|--------:|--------:|--------:|----------:|
| &#39;GZip: Compress 100KB&#39;           | 3,392.7 μs | 36.43 μs | 52.24 μs | 3,372.9 μs | 33.3333 | 33.3333 | 33.3333 | 247.25 KB |
| &#39;GZip: Compress 100KB (Async)&#39;   | 3,342.7 μs |  8.74 μs | 11.67 μs | 3,344.8 μs | 33.3333 | 33.3333 | 33.3333 | 247.41 KB |
| &#39;GZip: Decompress 100KB&#39;         |   455.5 μs | 10.77 μs | 16.12 μs |   466.6 μs |       - |       - |       - |   1.06 KB |
| &#39;GZip: Decompress 100KB (Async)&#39; |   450.5 μs |  9.85 μs | 14.74 μs |   442.7 μs |       - |       - |       - |   1.38 KB |
| Method                                          | Mean       | Error    | StdDev   | Gen0     | Gen1     | Gen2     | Allocated  |
|------------------------------------------------ |-----------:|---------:|---------:|---------:|---------:|---------:|-----------:|
| &#39;Rar: Extract all entries (Archive API)&#39;        | 1,219.8 μs | 18.04 μs | 25.88 μs |        - |        - |        - |  154.47 KB |
| &#39;Rar: Extract all entries (Archive API, Async)&#39; |   903.6 μs |  3.70 μs |  4.69 μs |        - |        - |        - |  159.35 KB |
| &#39;Rar: Extract all entries (Reader API)&#39;         | 1,235.7 μs |  3.43 μs |  4.91 μs |        - |        - |        - |  121.95 KB |
| &#39;Rar: Extract all entries (Reader API, Async)&#39;  | 1,455.0 μs |  7.65 μs | 10.97 μs | 500.0000 | 500.0000 | 500.0000 | 4778.95 KB |
| Method                                         | Mean     | Error   | StdDev  | Gen0     | Gen1    | Gen2    | Allocated |
|----------------------------------------------- |---------:|--------:|--------:|---------:|--------:|--------:|----------:|
| &#39;Rar5 encrypted: full entry read&#39;              | 153.4 ms | 0.06 ms | 0.08 ms | 333.3333 |       - |       - |   5.82 MB |
| &#39;Rar5 encrypted: non-seekable full entry read&#39; | 153.8 ms | 0.22 ms | 0.32 ms | 366.6667 | 66.6667 | 33.3333 |    5.9 MB |
| Method                                    | Mean     | Error    | StdDev   | Allocated |
|------------------------------------------ |---------:|---------:|---------:|----------:|
| &#39;Rar: open→read 1MB at N offsets→dispose&#39; | 95.93 μs | 1.083 μs | 1.588 μs | 131.77 KB |
| Method                                          | Mean       | Error   | StdDev  | Gen0    | Gen1    | Gen2    | Allocated |
|------------------------------------------------ |-----------:|--------:|--------:|--------:|--------:|--------:|----------:|
| &#39;Rar stored (m0): full entry read&#39;              |   304.6 μs | 1.64 μs | 2.40 μs |       - |       - |       - | 149.06 KB |
| &#39;Rar5 stored (m0): full entry read&#39;             |   287.5 μs | 0.54 μs | 0.75 μs |       - |       - |       - |  89.64 KB |
| &#39;Rar stored multi-volume: full entry read&#39;      | 1,206.9 μs | 1.80 μs | 2.52 μs |       - |       - |       - | 169.88 KB |
| &#39;Rar stored (m0): non-seekable full entry read&#39; |   324.3 μs | 2.09 μs | 3.00 μs | 33.3333 | 33.3333 | 33.3333 | 186.27 KB |
| Method                                           | Mean      | Error     | StdDev    | Gen0     | Gen1     | Gen2     | Allocated |
|------------------------------------------------- |----------:|----------:|----------:|---------:|---------:|---------:|----------:|
| &#39;7Zip LZMA: Extract all entries&#39;                 |  8.032 ms | 0.0070 ms | 0.0100 ms |  33.3333 |  33.3333 |  33.3333 | 272.58 KB |
| &#39;7Zip LZMA: Extract all entries (Async)&#39;         | 38.383 ms | 0.0443 ms | 0.0607 ms |  33.3333 |  33.3333 |  33.3333 | 317.63 KB |
| &#39;7Zip LZMA2: Extract all entries&#39;                |  8.041 ms | 0.0072 ms | 0.0103 ms |  33.3333 |  33.3333 |  33.3333 | 272.35 KB |
| &#39;7Zip LZMA2: Extract all entries (Async)&#39;        | 38.262 ms | 0.0420 ms | 0.0575 ms |  33.3333 |  33.3333 |  33.3333 | 317.64 KB |
| &#39;7Zip LZMA2 Reader: Extract all entries&#39;         |  8.030 ms | 0.0071 ms | 0.0102 ms |  33.3333 |  33.3333 |  33.3333 | 272.98 KB |
| &#39;7Zip LZMA2 Reader: Extract all entries (Async)&#39; | 24.023 ms | 0.0873 ms | 0.1306 ms | 100.0000 | 100.0000 | 100.0000 | 544.58 KB |
| Method                                      | Mean     | Error     | StdDev    | Gen0    | Gen1    | Gen2    | Allocated |
|-------------------------------------------- |---------:|----------:|----------:|--------:|--------:|--------:|----------:|
| &#39;7z solid: open each entry via Archive API&#39; | 7.992 ms | 0.0136 ms | 0.0196 ms | 33.3333 | 33.3333 | 33.3333 | 272.36 KB |
| Method                                                | Mean      | Error     | StdDev    | Median    | Gen0     | Gen1     | Gen2     | Allocated  |
|------------------------------------------------------ |----------:|----------:|----------:|----------:|---------:|---------:|---------:|-----------:|
| &#39;Tar: Extract all entries (Archive API)&#39;              |  46.48 μs |  0.546 μs |  0.783 μs |  46.38 μs |        - |        - |        - |   19.48 KB |
| &#39;Tar: Extract all entries (Archive API, Async)&#39;       |  54.07 μs |  0.436 μs |  0.597 μs |  54.15 μs |        - |        - |        - |   15.41 KB |
| &#39;Tar: Extract all entries (Reader API)&#39;               | 383.74 μs | 12.630 μs | 18.513 μs | 377.52 μs | 300.0000 | 300.0000 | 300.0000 | 1109.53 KB |
| &#39;Tar: Extract all entries (Archive API) - SystemGzip&#39; |        NA |        NA |        NA |        NA |       NA |       NA |       NA |         NA |
| &#39;Tar: Extract all entries (Reader API) - SystemGzip&#39;  | 612.74 μs |  1.714 μs |  2.346 μs | 612.38 μs | 300.0000 | 300.0000 | 300.0000 |  1124.5 KB |
| &#39;Tar.GZip: Extract all entries&#39;                       |        NA |        NA |        NA |        NA |       NA |       NA |       NA |         NA |
| &#39;Tar.GZip: Extract all entries (Async)&#39;               |        NA |        NA |        NA |        NA |       NA |       NA |       NA |         NA |
| &#39;Tar: Create archive with small files&#39;                |  49.37 μs |  0.867 μs |  1.215 μs |  49.23 μs |        - |        - |        - |   68.25 KB |
| &#39;Tar: Create archive with small files (Async)&#39;        |  35.41 μs | 12.976 μs | 19.422 μs |  20.34 μs |        - |        - |        - |   69.28 KB |
| Method                                                   | Mean     | Error    | StdDev   | Median   | Allocated |
|--------------------------------------------------------- |---------:|---------:|---------:|---------:|----------:|
| &#39;Zip: Extract all entries (Archive API) - SystemDeflate&#39; | 290.7 μs | 39.24 μs | 58.74 μs | 304.2 μs |  71.71 KB |
| &#39;Zip: Extract all entries (Archive API)&#39;                 | 561.3 μs |  1.03 μs |  1.44 μs | 561.2 μs |  81.35 KB |
| &#39;Zip: Extract all entries (Archive API, Async)&#39;          | 583.1 μs |  4.59 μs |  6.28 μs | 580.8 μs |  24.48 KB |
| &#39;Zip: Extract all entries (Reader API) - SystemDeflate&#39;  | 206.6 μs |  0.96 μs |  1.37 μs | 206.2 μs |  12.03 KB |
| &#39;Zip: Extract all entries (Reader API)&#39;                  | 549.9 μs |  1.77 μs |  2.42 μs | 549.7 μs |  22.41 KB |
| &#39;Zip: Extract all entries (Reader API, Async)&#39;           | 570.9 μs |  2.99 μs |  4.19 μs | 569.3 μs |     24 KB |
| &#39;Zip: Create archive with small files&#39;                   | 227.3 μs |  7.42 μs | 10.88 μs | 220.9 μs |  85.34 KB |
| &#39;Zip: Create archive with small files (Async)&#39;           | 240.5 μs |  8.91 μs | 11.89 μs | 233.1 μs |  88.77 KB |