(
echo {
echo   "OutputDir": "%OutputDir%",
echo   "Platform": "%Platform%",
echo   "ForceRebuild": %ForceRebuild%,
echo   "Version": "%Version%",
echo   "UpdatePrefixUrl": "%UpdatePrefixUrl%",
echo   "ForceUpdate": %ForceUpdate%,
echo   "AppUpdateUrl": "%AppUpdateUrl%",
echo   "AppUpdateDescription": "%AppUpdateDescription%",
echo }
) > "%ProjectRoot%\Tools\Jenkins\BuildResourceConfig.json"