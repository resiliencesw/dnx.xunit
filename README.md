## <a href="https://github.com/xunit/xunit"><img src="https://raw.github.com/xunit/media/master/full-logo.png" title="xUnit.net ASP.NET v5+ Runner" /></a>

This runner supports [xUnit.net](https://github.com/xunit/xunit) tests for [ASP.NET v5+](https://github.com/aspnet).

### Usage

To install this package, add the following to your project.json

```JSON
{
    "dependencies": {
        "xunit": "2.0.0-*",
        "xunit.runner.aspnet": "1.0.0-*"
    },
    "commands": {
        "test": "xunit.runner.aspnet"
    }
}
```

To run tests from the command line, use the following.

```Shell
# Restore NuGet packages
kpm restore

# Run tests
k test
```
