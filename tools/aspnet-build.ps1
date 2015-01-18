# Have to bust the cache, because of broken packages from the CLR team
remove-item -recurse -force $(join-path $env:USERPROFILE ".kpm\packages") -ErrorAction SilentlyContinue

# Make sure beta 2 is installed and in use
& tools\kvm install 1.0.0-beta2 -runtime CLR -x86
& tools\kvm install 1.0.0-beta2 -runtime CoreCLR -x86
& tools\kvm use 1.0.0-beta2 -runtime CLR -x86

# Update build number during CI
if ($env:BuildSemanticVersion -ne $null) {
  $content = get-content src\xunit.runner.aspnet\project.json
  $content = $content.Replace("99.99.99", $env:BuildSemanticVersion)
  set-content src\xunit.runner.aspnet\project.json $content -encoding UTF8
}

# Restore packages and build
& kpm restore
& kpm build src\xunit.runner.aspnet --configuration Release
