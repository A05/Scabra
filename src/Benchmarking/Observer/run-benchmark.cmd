rem Start up the repeater.
start cmd /k "cd Repeater && dotnet run -c Release"

rem Give the repeater a chance to be built successfully.
timeout /t 1

rem Start up the benchmark.
start cmd /k "cd Terminator && dotnet run -c Release"