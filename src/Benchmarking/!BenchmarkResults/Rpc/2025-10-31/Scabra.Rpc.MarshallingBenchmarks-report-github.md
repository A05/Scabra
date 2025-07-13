```

BenchmarkDotNet v0.15.4, Windows 11 (10.0.26100.6899/24H2/2024Update/HudsonValley)
Intel Core Ultra 7 155U 1.70GHz, 1 CPU, 14 logical and 12 physical cores
.NET SDK 9.0.306
  [Host]     : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3


```
| Method            | Mean       | Error     | StdDev    |
|------------------ |-----------:|----------:|----------:|
| FullTrip          | 5,781.2 ns | 115.82 ns | 341.49 ns |
| MarshalRequest    | 1,859.4 ns |  37.02 ns |  92.89 ns |
| UnmarshalRequest  | 2,802.6 ns |  57.25 ns | 166.09 ns |
| MarshalResponse   |   808.8 ns |  16.23 ns |  39.80 ns |
| UnmarshalResponse |   359.4 ns |   7.20 ns |  17.26 ns |
