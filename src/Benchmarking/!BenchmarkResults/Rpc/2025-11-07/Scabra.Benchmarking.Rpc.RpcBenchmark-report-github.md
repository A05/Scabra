```

BenchmarkDotNet v0.15.4, Windows 11 (10.0.26100.6899/24H2/2024Update/HudsonValley)
Intel Core Ultra 7 155U 1.70GHz, 1 CPU, 14 logical and 12 physical cores
.NET SDK 9.0.306
  [Host]     : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3


```
| Method                                     | Mean     | Error   | StdDev  |
|------------------------------------------- |---------:|--------:|--------:|
| No_Parameters_No_Return                    | 344.7 μs | 5.99 μs | 5.01 μs |
| Primitive_Parameters_Primitive_Return      | 362.6 μs | 4.21 μs | 3.52 μs |
| Complex_Parameters_Complex_Return          | 597.8 μs | 9.61 μs | 9.44 μs |
| No_Parameters_No_Return_gRpc               | 371.3 μs | 7.41 μs | 7.61 μs |
| Primitive_Parameters_Primitive_Return_gRpc | 390.6 μs | 2.51 μs | 2.22 μs |
| Complex_Parameters_Complex_Return_gRpc     | 581.9 μs | 7.15 μs | 6.34 μs |
