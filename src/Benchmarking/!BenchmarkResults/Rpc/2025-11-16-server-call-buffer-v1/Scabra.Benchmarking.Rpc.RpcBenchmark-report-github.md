```

BenchmarkDotNet v0.15.4, Windows 11 (10.0.26100.6899/24H2/2024Update/HudsonValley)
Intel Core Ultra 7 155U 1.70GHz, 1 CPU, 14 logical and 12 physical cores
.NET SDK 9.0.307
  [Host]     : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3


```
| Method                                     | Mean     | Error    | StdDev   |
|------------------------------------------- |---------:|---------:|---------:|
| No_Parameters_No_Return                    | 471.8 μs |  9.23 μs | 10.99 μs |
| Primitive_Parameters_Primitive_Return      | 464.8 μs |  8.37 μs |  9.31 μs |
| Complex_Parameters_Complex_Return          | 735.0 μs | 11.51 μs | 10.77 μs |
| No_Parameters_No_Return_gRpc               | 406.5 μs |  7.23 μs |  6.76 μs |
| Primitive_Parameters_Primitive_Return_gRpc | 439.2 μs |  8.41 μs |  8.26 μs |
| Complex_Parameters_Complex_Return_gRpc     | 694.6 μs |  6.49 μs |  6.07 μs |
