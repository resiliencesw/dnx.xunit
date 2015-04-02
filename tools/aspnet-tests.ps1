& tools\dnvm install latest -runtime CLR -arch x86
& dnx test\test.xunit.runner.aspnet test
if ($LastExitCode -ne 0) { exit 1 }

& tools\dnvm install latest -runtime CoreCLR -arch x86
& dnx test\test.xunit.runner.aspnet test
if ($LastExitCode -ne 0) { exit 1 }
