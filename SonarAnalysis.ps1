dotnet tool install --global dotnet-sonarscanner
dotnet tool install --global coverlet.console
dotnet sonarscanner begin /d:sonar.login=admin /d:sonar.password=admin /k:"CbUpdate" /d:sonar.host.url="http://localhost:9001" /s:"`pwd`/SonarQube.Analysis.xml"
dotnet build
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
dotnet sonarscanner end /d:sonar.login=admin /d:sonar.password=admin