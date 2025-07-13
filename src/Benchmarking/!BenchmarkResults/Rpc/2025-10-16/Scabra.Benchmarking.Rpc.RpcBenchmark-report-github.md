```

BenchmarkDotNet v0.15.4, Windows 11 (10.0.26100.6584/24H2/2024Update/HudsonValley)
Intel Core Ultra 7 155U 1.70GHz, 1 CPU, 14 logical and 12 physical cores
.NET SDK 9.0.306
  [Host]     : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3


```
| Method                                | Mean     | Error    | StdDev   |
|-------------------------------------- |---------:|---------:|---------:|
| No_Parameters_No_Return               | 68.28 ms | 1.354 ms | 3.863 ms |
| Primitive_Parameters_Primitive_Return | 67.82 ms | 1.350 ms | 3.388 ms |
| Complex_Parameters_Complex_Return     | 71.10 ms | 1.401 ms | 2.766 ms |
