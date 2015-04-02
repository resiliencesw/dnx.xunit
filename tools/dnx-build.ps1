# Have to bust the cache, because of broken packages from the CLR team
remove-item -recurse -force $(join-path $env:USERPROFILE ".dnx\packages") -ErrorAction SilentlyContinue

# Make sure latest is installed and in use
& tools\dnvm install latest -runtime CoreCLR -arch x86
& tools\dnvm install latest -runtime CLR -arch x86

# Update build number during CI
if ($env:BuildSemanticVersion -ne $null) {
  $content = get-content src\xunit.runner.dnx\project.json
  $content = $content.Replace("99.99.99-dev", $env:BuildSemanticVersion)
  set-content src\xunit.runner.dnx\project.json $content -encoding UTF8
}

# Restore packages and build
& dnu restore
& dnu pack src\xunit.runner.dnx --configuration Release
