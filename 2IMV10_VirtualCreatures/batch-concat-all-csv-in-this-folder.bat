@echo off
del combined.csv 2> NUL
for %%f in (*.csv) do (
   echo %%f
   if not exist combined.csv (
      copy "%%f" combined.csv
   ) else (
      for /F  "usebackq skip=1 delims=" %%a in ("%%f") do (
         echo %%a>> combined.csv
      )
   )
)
pause
set /P fileDate=< combined.csv
%ren combined.csv filename_%fileDate:~0,-1%.mf