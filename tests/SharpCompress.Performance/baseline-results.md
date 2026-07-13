| Method                           | Mean       | Error    | StdDev   | Gen0    | Gen1    | Gen2    | Allocated |
|--------------------------------- |-----------:|---------:|---------:|--------:|--------:|--------:|----------:|
| &#39;GZip: Compress 100KB&#39;           | 2,075.6 μs | 22.66 μs | 33.22 μs | 33.3333 | 33.3333 | 33.3333 | 247.24 KB |
| &#39;GZip: Compress 100KB (Async)&#39;   | 2,068.2 μs | 20.96 μs | 31.37 μs | 33.3333 | 33.3333 | 33.3333 | 247.42 KB |
| &#39;GZip: Decompress 100KB&#39;         |   337.0 μs |  2.15 μs |  3.15 μs |       - |       - |       - |   1.05 KB |
| &#39;GZip: Decompress 100KB (Async)&#39; |   339.2 μs |  2.07 μs |  3.10 μs |       - |       - |       - |   1.34 KB |
| Method                                          | Mean     | Error    | StdDev   | Gen0     | Gen1     | Gen2     | Allocated  |
|------------------------------------------------ |---------:|---------:|---------:|---------:|---------:|---------:|-----------:|
| &#39;Rar: Extract all entries (Archive API)&#39;        | 705.1 μs | 18.71 μs | 26.83 μs |        - |        - |        - |   154.5 KB |
| &#39;Rar: Extract all entries (Archive API, Async)&#39; | 440.4 μs | 47.82 μs | 70.10 μs |        - |        - |        - |  159.28 KB |
| &#39;Rar: Extract all entries (Reader API)&#39;         | 717.1 μs | 11.02 μs | 15.81 μs |        - |        - |        - |  121.93 KB |
| &#39;Rar: Extract all entries (Reader API, Async)&#39;  | 802.9 μs | 15.26 μs | 20.89 μs | 266.6667 | 266.6667 | 266.6667 | 4775.76 KB |
| Method                                         | Mean     | Error    | StdDev   | Gen0     | Gen1     | Gen2    | Allocated |
|----------------------------------------------- |---------:|---------:|---------:|---------:|---------:|--------:|----------:|
| &#39;Rar5 encrypted: full entry read&#39;              | 26.47 ms | 0.119 ms | 0.174 ms | 700.0000 |  66.6667 |       - |   5.82 MB |
| &#39;Rar5 encrypted: non-seekable full entry read&#39; | 26.55 ms | 0.177 ms | 0.260 ms | 700.0000 | 166.6667 | 33.3333 |    5.9 MB |
| Method                                    | Mean     | Error    | StdDev   | Allocated |
|------------------------------------------ |---------:|---------:|---------:|----------:|
| &#39;Rar: open→read 1MB at N offsets→dispose&#39; | 64.30 μs | 1.413 μs | 2.115 μs | 131.81 KB |
| Method                                          | Mean     | Error   | StdDev   | Gen0    | Gen1    | Gen2    | Allocated |
|------------------------------------------------ |---------:|--------:|---------:|--------:|--------:|--------:|----------:|
| &#39;Rar stored (m0): full entry read&#39;              | 235.4 μs | 1.80 μs |  2.64 μs |       - |       - |       - |  149.1 KB |
| &#39;Rar5 stored (m0): full entry read&#39;             | 231.1 μs | 2.06 μs |  3.08 μs |       - |       - |       - |  89.67 KB |
| &#39;Rar stored multi-volume: full entry read&#39;      | 688.3 μs | 9.11 μs | 13.36 μs |       - |       - |       - | 169.69 KB |
| &#39;Rar stored (m0): non-seekable full entry read&#39; | 254.8 μs | 2.47 μs |  3.69 μs | 33.3333 | 33.3333 | 33.3333 | 186.02 KB |
| Method                                           | Mean      | Error     | StdDev    | Median    | Gen0     | Gen1     | Gen2     | Allocated |
|------------------------------------------------- |----------:|----------:|----------:|----------:|---------:|---------:|---------:|----------:|
| &#39;7Zip LZMA: Extract all entries&#39;                 |  5.720 ms | 0.0328 ms | 0.0490 ms |  5.713 ms |  33.3333 |  33.3333 |  33.3333 | 272.58 KB |
| &#39;7Zip LZMA: Extract all entries (Async)&#39;         | 16.793 ms | 0.1222 ms | 0.1791 ms | 16.755 ms |  33.3333 |  33.3333 |  33.3333 | 317.65 KB |
| &#39;7Zip LZMA2: Extract all entries&#39;                |  5.665 ms | 0.0607 ms | 0.0908 ms |  5.635 ms |  33.3333 |  33.3333 |  33.3333 | 272.35 KB |
| &#39;7Zip LZMA2: Extract all entries (Async)&#39;        | 16.869 ms | 0.1610 ms | 0.2410 ms | 16.765 ms |  33.3333 |  33.3333 |  33.3333 | 317.62 KB |
| &#39;7Zip LZMA2 Reader: Extract all entries&#39;         |  5.783 ms | 0.0634 ms | 0.0949 ms |  5.795 ms |  33.3333 |  33.3333 |  33.3333 | 272.97 KB |
| &#39;7Zip LZMA2 Reader: Extract all entries (Async)&#39; | 10.398 ms | 0.0666 ms | 0.0997 ms | 10.335 ms | 100.0000 | 100.0000 | 100.0000 | 544.45 KB |
| Method                                      | Mean     | Error     | StdDev    | Gen0    | Gen1    | Gen2    | Allocated |
|-------------------------------------------- |---------:|----------:|----------:|--------:|--------:|--------:|----------:|
| &#39;7z solid: open each entry via Archive API&#39; | 5.571 ms | 0.0450 ms | 0.0660 ms | 33.3333 | 33.3333 | 33.3333 | 272.62 KB |
| Method                                                | Mean      | Error    | StdDev   | Gen0     | Gen1     | Gen2     | Allocated  |
|------------------------------------------------------ |----------:|---------:|---------:|---------:|---------:|---------:|-----------:|
| &#39;Tar: Extract all entries (Archive API)&#39;              |  18.75 μs | 0.553 μs | 0.827 μs |        - |        - |        - |   19.45 KB |
| &#39;Tar: Extract all entries (Archive API, Async)&#39;       |  20.53 μs | 0.620 μs | 0.869 μs |        - |        - |        - |   15.38 KB |
| &#39;Tar: Extract all entries (Reader API)&#39;               | 101.77 μs | 2.954 μs | 4.422 μs |  66.6667 |  66.6667 |  66.6667 | 1108.14 KB |
| &#39;Tar: Extract all entries (Archive API) - SystemGzip&#39; |        NA |       NA |       NA |       NA |       NA |       NA |         NA |
| &#39;Tar: Extract all entries (Reader API) - SystemGzip&#39;  | 225.79 μs | 4.619 μs | 6.624 μs | 133.3333 | 133.3333 | 133.3333 | 1124.13 KB |
| &#39;Tar.GZip: Extract all entries&#39;                       |        NA |       NA |       NA |       NA |       NA |       NA |         NA |
| &#39;Tar.GZip: Extract all entries (Async)&#39;               |        NA |       NA |       NA |       NA |       NA |       NA |         NA |
| &#39;Tar: Create archive with small files&#39;                |  23.21 μs | 0.656 μs | 0.920 μs |        - |        - |        - |   68.23 KB |
| &#39;Tar: Create archive with small files (Async)&#39;        |  25.25 μs | 0.465 μs | 0.652 μs |        - |        - |        - |   69.25 KB |
| Method                                                   | Mean      | Error     | StdDev    | Median    | Allocated |
|--------------------------------------------------------- |----------:|----------:|----------:|----------:|----------:|
| &#39;Zip: Extract all entries (Archive API) - SystemDeflate&#39; | 107.09 μs |  1.335 μs |  1.872 μs | 107.45 μs |  71.63 KB |
| &#39;Zip: Extract all entries (Archive API)&#39;                 | 376.30 μs | 58.807 μs | 86.198 μs | 438.63 μs |  81.32 KB |
| &#39;Zip: Extract all entries (Archive API, Async)&#39;          | 277.63 μs |  8.371 μs | 12.530 μs | 277.11 μs |  24.44 KB |
| &#39;Zip: Extract all entries (Reader API) - SystemDeflate&#39;  |  99.37 μs |  1.030 μs |  1.444 μs |  99.31 μs |  12.06 KB |
| &#39;Zip: Extract all entries (Reader API)&#39;                  | 262.06 μs |  4.807 μs |  7.194 μs | 262.16 μs |  22.36 KB |
| &#39;Zip: Extract all entries (Reader API, Async)&#39;           | 272.44 μs |  5.004 μs |  7.490 μs | 271.59 μs |  23.96 KB |
| &#39;Zip: Create archive with small files&#39;                   | 109.66 μs |  2.469 μs |  3.541 μs | 108.97 μs |  85.37 KB |
| &#39;Zip: Create archive with small files (Async)&#39;           | 119.84 μs |  2.838 μs |  4.160 μs | 119.26 μs |  89.03 KB |