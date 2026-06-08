(
echo {
echo   "OutputDir": "%OutputDir%",
echo   "Platform": "%Platform%",
echo   "FullBuild": %FullBuild%,
echo   "IncludeInitialResources": %IncludeInitialResources%,
echo   "BuildType": "%BuildType%",
echo   "BuildAppBundle": %BuildAppBundle%,
echo   "Version": "%Version%",
echo }
) > "%ProjectRoot%\Tools\Jenkins\BuildAppConfig.json"