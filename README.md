# NuGet Package Diffing Tool

A command line tool that compares two versions of a NuGet package and provides public API differences.

This tool has originally been built to fail a build when a breaking change is detected, using a
published nuget package (in nuget.org) and a local NuGet package.

## Installing

Run the following command from commandline (requires .NET Core 2.1 installed):

```
dotnet tool install --global Uno.PackageDiff
```

## Diffing packages

```
generatepkgdiff --base=Uno.UI --other=C:\temp\Uno.UI.1.43.0-PullRequest0621.917.nupkg --outfile=diff.md
```
