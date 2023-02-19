
IF DEFINED Release (
	echo Release
	echo %~dp0ReplaceInternalResourcesDefaultConstructorToPublic.exe ..\boilersGraphics\Properties\Resources.Designer.cs
	%~dp0ReplaceInternalResourcesDefaultConstructorToPublic.exe ..\boilersGraphics\Properties\Resources.Designer.cs
) ELSE (
	echo Debug
	echo %~dp0ReplaceInternalResourcesDefaultConstructorToPublic.exe ..\boilersGraphics\Properties\Resources.Designer.cs
	%~dp0ReplaceInternalResourcesDefaultConstructorToPublic.exe ..\boilersGraphics\Properties\Resources.Designer.cs
)