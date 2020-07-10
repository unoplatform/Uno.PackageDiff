# NuGet Package Diffing Tool

A command line tool that compares two versions of a NuGet package and provides public API differences.

This tool has originally been built to fail a build when a breaking change is detected, using a
published nuget package (in nuget.org) and a local NuGet package.

## Installing

Run the following command from command line (requires .NET Core 2.1 installed):

```
dotnet tool install --global Uno.PackageDiff
```

## Diffing packages

```
generatepkgdiff --base=Uno.UI --other=C:\temp\Uno.UI.1.43.0-PullRequest0621.917.nupkg --outfile=diff.md
```

The tool returns a non-zero value when differences are found, otherwise zero.

## How to provide an ignore set

The diff tool accepts a "ignore set" file which gives the ability to ignore specific differences. Those differences appear strike-out in the resulting markdown file.

Here's the format:

```xml
<DiffIgnore>
  <IgnoreSets>
    <IgnoreSet baseVersion="1.0.0">
      <Types>
        <Member fullName="MyNamespace.MyMissingClass" />
      </Types>
      <Properties>
        <Member fullName="MyNamespace.MyClass.MyProperty" />
      </Properties>
      <Fields>
        <Member fullName="MyNamespace.MyClass.myField" />
      </Fields>
      <Events>
        <Member fullName="MyNamespace.MyClass.MyEvent" />
      </Events>
      <Methods>
        <Member fullName="MyNamespace.MyClass.MyMethod" />
      </Methods>
      <Methods>
        <Member fullName="MyNamespace.MyClass.MyMethod" />
      </Methods>
      <Assemblies>
        <Member fullName="MyMissingAssemblyName" />
      </Assemblies>
    </IgnoreSet>
  </IgnoreSets>
</DiffIgnore>
```

The `baseVersion` attribute denotes the version for which the Ignore Set has been authored. This enables for the automatic discarding of existing sets when a new package version is published in nuget.org.

The `fullname` of members should be the exact string provided in the markdown file when a difference is identified.
