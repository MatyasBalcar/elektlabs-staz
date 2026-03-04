@echo off
set DB_NAME=KnihovnaDB.fdb
set DB_USER=SYSDBA
set DB_PASS=masterkey
set ISQL_PATH="C:\fb\Firebird_5_0\isql.exe"

pushd "%~dp0.."
set "PFolder=%CD%"
popd

set "DB_PATH=%PFolder%\%DB_NAME%"

if exist "%DB_PATH%" (
    echo DB ALREADY EXISTS
    exit /b
)
echo ===START DB===
echo SET SQL DIALECT 3; > "%~dp0temp_create.sql"
echo CREATE DATABASE '%DB_PATH%' USER '%DB_USER%' PASSWORD '%DB_PASS%' PAGE_SIZE 8192 DEFAULT CHARACTER SET UTF8; >> "%~dp0temp_create.sql"
echo EXIT; >> "%~dp0temp_create.sql"

%ISQL_PATH% -user %DB_USER% -password %DB_PASS% -i "%~dp0temp_create.sql"
del "%~dp0temp_create.sql"
echo DB CREATED

echo ADDING TABLES AND DATA
%ISQL_PATH% "%DB_PATH%" -user %DB_USER% -password %DB_PASS% -i "%~dp0create_tables.sql"
%ISQL_PATH% "%DB_PATH%" -user %DB_USER% -password %DB_PASS% -i "%~dp0insert_test_data.sql"

pause