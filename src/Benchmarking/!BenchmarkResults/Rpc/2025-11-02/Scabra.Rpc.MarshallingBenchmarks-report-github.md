```

BenchmarkDotNet v0.15.4, Windows 11 (10.0.26100.6899/24H2/2024Update/HudsonValley)
Intel Core Ultra 7 155U 1.70GHz, 1 CPU, 14 logical and 12 physical cores
.NET SDK 9.0.306
  [Host]     : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3


```
| Method            | Mean       | Error    | StdDev   |
|------------------ |-----------:|---------:|---------:|
| FullTrip          | 4,894.7 ns | 92.23 ns | 81.76 ns |
| MarshalRequest    | 1,648.2 ns | 31.15 ns | 31.99 ns |
| UnmarshalRequest  | 2,149.7 ns | 42.23 ns | 54.91 ns |
| MarshalResponse   |   821.1 ns | 15.77 ns | 17.53 ns |
| UnmarshalResponse |   393.5 ns |  7.82 ns |  9.01 ns |
