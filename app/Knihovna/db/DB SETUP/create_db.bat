@echo off
setlocal enabledelayedexpansion

set "DB_NAME=KnihovnaDB.fdb"
set "DB_USER=SYSDBA"
set "DB_PASS=masterkey"
set "ISQL_PATH=C:\fb\isql.exe"

pushd "%~dp0.."
set "PFolder=%CD%"
popd

set "DB_PATH=%PFolder%\%DB_NAME%"

if exist "%DB_PATH%" (
    echo DB ALREADY EXISTS
    exit /b
)

echo CREATING DB

%ISQL_PATH% -user %DB_USER% -password %DB_PASS% -i "%~dp0create_db.sql"
if %errorlevel% neq 0 (
    echo Error: Failed to create the database. Error code: !errorlevel!
    exit /b 1
)

%ISQL_PATH% "%DB_PATH%" -user %DB_USER% -password %DB_PASS% -i "%~dp0create_tables.sql"
if %errorlevel% neq 0 (
    echo Error: Failed to create tables. Error code: !errorlevel!
    exit /b 1
)

%ISQL_PATH% "%DB_PATH%" -user %DB_USER% -password %DB_PASS% -i "%~dp0insert_test_data.sql"
if %errorlevel% neq 0 (
    echo Error: Failed to insert test data. Error code: !errorlevel!
    exit /b 1
)

echo DB CREATED WITH DATA

pause