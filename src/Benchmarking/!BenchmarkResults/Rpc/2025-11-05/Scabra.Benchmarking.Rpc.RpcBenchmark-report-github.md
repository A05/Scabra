```

BenchmarkDotNet v0.15.4, Windows 11 (10.0.26100.6899/24H2/2024Update/HudsonValley)
Intel Core Ultra 7 155U 1.70GHz, 1 CPU, 14 logical and 12 physical cores
.NET SDK 9.0.306
  [Host]     : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3


```
| Method                                     | Mean        | Error       | StdDev      |
|------------------------------------------- |------------:|------------:|------------:|
| No_Parameters_No_Return                    | 35,607.3 μs |   815.45 μs | 2,391.57 μs |
| Primitive_Parameters_Primitive_Return      | 35,596.7 μs |   794.24 μs | 2,329.36 μs |
| Complex_Parameters_Complex_Return          | 38,919.3 μs | 1,048.10 μs | 3,057.35 μs |
| No_Parameters_No_Return_gRpc               |    236.2 μs |     4.56 μs |     5.43 μs |
| Primitive_Parameters_Primitive_Return_gRpc |    205.8 μs |     4.00 μs |     3.34 μs |
| Complex_Parameters_Complex_Return_gRpc     |    393.4 μs |     6.08 μs |    10.80 μs |
