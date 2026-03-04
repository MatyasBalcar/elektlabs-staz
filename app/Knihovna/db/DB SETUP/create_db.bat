@echo off
set DB_NAME=KnihovnaDB.fdb
set DB_USER=SYSDBA
set DB_PASS=masterkey
set ISQL_PATH="C:\fb\isql.exe"

pushd "%~dp0.."
set "PFolder=%CD%"
popd

set "DB_PATH=%PFolder%\%DB_NAME%"

if exist "%DB_PATH%" (
    echo DB ALREADY EXISTS
    exit /b
)

echo ===START DB===
echo CREATING DB USING create_db.sql...

%ISQL_PATH% -user %DB_USER% -password %DB_PASS% -i "%~dp0create_db.sql"

echo DB CREATED

echo ADDING TABLES AND DATA
%ISQL_PATH% "%DB_PATH%" -user %DB_USER% -password %DB_PASS% -i "%~dp0create_tables.sql"
%ISQL_PATH% "%DB_PATH%" -user %DB_USER% -password %DB_PASS% -i "%~dp0insert_test_data.sql"

echo QUERIES COMPLETED
pause