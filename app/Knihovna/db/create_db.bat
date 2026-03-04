@echo off
cd /d "%~dp0"

echo ===STARTDB===

echo CREATING TEMP SCRIPT

echo CREATE DATABASE 'KnihovnaDB.fdb' USER 'SYSDBA' PASSWORD 'masterkey' DEFAULT CHARACTER SET UTF8; > temp_script.sql
echo IN create_tables.sql; >> temp_script.sql
echo IN insert_test_data.sql; >> temp_script.sql
echo quit; >> temp_script.sql

echo USING ISQL TO CREATE DB
"C:\Program Files\Firebird\Firebird_5_0\isql.exe" -q -i temp_script.sql

echo CLEANUP
del temp_script.sql

echo.
echo Process complete!
pause