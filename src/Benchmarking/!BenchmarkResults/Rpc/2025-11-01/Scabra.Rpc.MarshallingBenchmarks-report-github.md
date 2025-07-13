```

BenchmarkDotNet v0.15.4, Windows 11 (10.0.26100.6899/24H2/2024Update/HudsonValley)
Intel Core Ultra 7 155U 1.70GHz, 1 CPU, 14 logical and 12 physical cores
.NET SDK 9.0.306
  [Host]     : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3


```
| Method            | Mean       | Error     | StdDev    |
|------------------ |-----------:|----------:|----------:|
| FullTrip          | 5,238.0 ns | 101.28 ns | 116.64 ns |
| MarshalRequest    | 1,765.9 ns |  29.08 ns |  25.78 ns |
| UnmarshalRequest  | 2,282.2 ns |  27.07 ns |  24.00 ns |
| MarshalResponse   |   861.5 ns |  15.28 ns |  14.29 ns |
| UnmarshalResponse |   409.3 ns |   7.20 ns |   6.01 ns |
