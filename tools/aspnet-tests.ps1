& tools\kvm use 1.0.0-beta3 -runtime CLR -x86
push-location test\test.xunit.runner.aspnet
& k test
pop-location

& tools\kvm use 1.0.0-beta3 -runtime CoreCLR -x86
push-location test\test.xunit.runner.aspnet
& k test
pop-location
