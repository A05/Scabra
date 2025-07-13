```

BenchmarkDotNet v0.15.4, Windows 11 (10.0.26100.6899/24H2/2024Update/HudsonValley)
Intel Core Ultra 7 155U 1.70GHz, 1 CPU, 14 logical and 12 physical cores
.NET SDK 9.0.306
  [Host]     : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 6.0.36 (6.0.36, 6.0.3624.51421), X64 RyuJIT x86-64-v3


```
| Method                                     | Mean        | Error       | StdDev      | Median      |
|------------------------------------------- |------------:|------------:|------------:|------------:|
| No_Parameters_No_Return                    | 65,791.2 μs | 1,313.13 μs | 2,334.08 μs | 66,941.9 μs |
| Primitive_Parameters_Primitive_Return      | 67,283.5 μs | 1,331.05 μs | 2,658.25 μs | 67,388.3 μs |
| Complex_Parameters_Complex_Return          | 70,837.1 μs | 1,407.40 μs | 3,504.91 μs | 71,320.5 μs |
| No_Parameters_No_Return_gRpc               |    188.8 μs |     1.07 μs |     0.95 μs |    188.9 μs |
| Primitive_Parameters_Primitive_Return_gRpc |    201.3 μs |     2.27 μs |     2.13 μs |    201.0 μs |
| Complex_Parameters_Complex_Return_gRpc     |    319.7 μs |     6.10 μs |     5.41 μs |    318.4 μs |
