@echo OFF

if "%~1"=="" goto blank

SET FOLDER=RGet.%1.bin

rmdir /S /Q RSoft.RGet\bin\publish

dotnet publish -c Release -p:PublishProfile=FolderProfile

mkdir %FOLDER%
copy /Y RSoft.RGet\bin\publish\* %FOLDER%
del %FOLDER%\*.pdb

cd %FOLDER%
7z a ..\..\..\Releases\RSoft.RGet.%1.7z *
cd ..

rmdir /S /Q %FOLDER%

echo ----
echo Release RGet.%1 done
echo ----
goto end

:blank
echo ----
echo Set release name!
echo ----

:end