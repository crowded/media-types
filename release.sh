#! /bin/bash

projVersion="$(xmllint --xpath '//Project/PropertyGroup/Version/text()' Directory.Build.props)"
parsedVersion=${projVersion:?"Cannot find version in Directory.Build.props"}

cd src
dotnet pack -c Release -p:ContinuousIntegrationBuild=true

echo -e "Version number: ${parsedVersion}"
read -p "Would you like to push this version to NuGet as well? [y/N]: "
if [[ echo && $REPLY =~ ^([Yy]|yes|Yes|YES)$ ]]; then
  dotnet nuget push src/bin/Release/Crowded.MediaTypes.$parsedVersion.nupkg -k $1 -s nuget.org
fi
