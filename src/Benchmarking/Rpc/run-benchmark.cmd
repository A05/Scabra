rem Start up the server.
start cmd /k "cd Server && dotnet run -c Release"

rem Give the server a chance to be built successfully.
timeout /t 1

rem Start up the benchmark.
start cmd /k "cd Client && dotnet run -c Release"