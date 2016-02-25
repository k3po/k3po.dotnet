@echo off
echo Cleaning WebSocket Library ...

If Exist dist (
del /s /f /q dist\*
rmdir dist
)
echo Clean Successfully
