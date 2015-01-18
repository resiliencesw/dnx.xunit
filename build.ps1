& tools\kvm install 1.0.0-beta2 -runtime CLR -x86
& tools\kvm install 1.0.0-beta2 -runtime CoreCLR -x86
& tools\kvm use 1.0.0-beta2 -runtime CLR -x86
& kpm restore

if ($env:BuildSemanticVersion -ne $null) {
  $content = get-content src\xunit.runner.aspnet\project.json
  $content = $content.Replace("99.99.99", $env:BuildSemanticVersion)
  set-content src\xunit.runner.aspnet\project.json $content -encoding UTF8
}

& kpm build src\xunit.runner.aspnet --configuration Release
