```

BenchmarkDotNet v0.15.4, Windows 11 (10.0.26100.6899/24H2/2024Update/HudsonValley)
Intel Core Ultra 7 155U 1.70GHz, 1 CPU, 14 logical and 12 physical cores
.NET SDK 9.0.307
  [Host]     : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3


```
| Method                                     | Mean     | Error    | StdDev   |
|------------------------------------------- |---------:|---------:|---------:|
| No_Parameters_No_Return                    | 174.6 μs |  4.56 μs | 11.92 μs |
| Primitive_Parameters_Primitive_Return      | 294.1 μs |  4.17 μs |  3.48 μs |
| Complex_Parameters_Complex_Return          | 507.8 μs | 10.09 μs | 12.76 μs |
| No_Parameters_No_Return_gRpc               | 331.1 μs |  6.34 μs |  8.02 μs |
| Primitive_Parameters_Primitive_Return_gRpc | 342.1 μs |  4.80 μs |  4.49 μs |
| Complex_Parameters_Complex_Return_gRpc     | 574.2 μs | 11.04 μs | 19.92 μs |
