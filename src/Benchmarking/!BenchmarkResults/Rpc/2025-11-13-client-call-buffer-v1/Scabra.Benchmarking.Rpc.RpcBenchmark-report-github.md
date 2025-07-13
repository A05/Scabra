```

BenchmarkDotNet v0.15.4, Windows 11 (10.0.26100.6899/24H2/2024Update/HudsonValley)
Intel Core Ultra 7 155U 1.70GHz, 1 CPU, 14 logical and 12 physical cores
.NET SDK 9.0.307
  [Host]     : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3


```
| Method                                     | Mean     | Error    | StdDev   |
|------------------------------------------- |---------:|---------:|---------:|
| No_Parameters_No_Return                    | 330.8 μs |  6.57 μs | 15.22 μs |
| Primitive_Parameters_Primitive_Return      | 351.1 μs |  6.91 μs | 12.82 μs |
| Complex_Parameters_Complex_Return          | 556.2 μs | 10.88 μs | 17.26 μs |
| No_Parameters_No_Return_gRpc               | 403.9 μs |  8.77 μs | 25.87 μs |
| Primitive_Parameters_Primitive_Return_gRpc | 433.1 μs |  8.66 μs | 18.08 μs |
| Complex_Parameters_Complex_Return_gRpc     | 596.6 μs | 11.11 μs | 20.31 μs |
