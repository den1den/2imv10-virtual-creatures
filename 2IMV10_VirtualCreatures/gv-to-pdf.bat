@echo OFF
set DOT_EXE="C:\Program Files (x86)\Graphviz2.38\bin\dot.exe"
%DOT_EXE% -Tpdf -o %~dp1%~n1.pdf %1
start %~dp1%~n1.pdf
