
echo off

echo %~dp0

echo %1

IF DEFINED Release (
	echo Release
	echo %~dp0
	echo %~dp0ReplaceInternalResourcesDefaultConstructorToPublic.exe %1\boilersGraphics\Properties\Resources.Designer.cs
	%~dp0ReplaceInternalResourcesDefaultConstructorToPublic.exe %1\boilersGraphics\Properties\Resources.Designer.cs
) ELSE (
	echo Debug
	echo %~dp0ReplaceInternalResourcesDefaultConstructorToPublic.exe %1\boilersGraphics\Properties\Resources.Designer.cs
	%~dp0ReplaceInternalResourcesDefaultConstructorToPublic.exe %1\boilersGraphics\Properties\Resources.Designer.cs
)