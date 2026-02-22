@echo off
setlocal EnableDelayedExpansion

echo ==========================================
echo   Force Update From Instructor Repo
echo ==========================================

REM ==============================
REM 1. 현재 origin (학생 repo) 저장
REM ==============================
for /f "delims=" %%i in ('git remote get-url origin') do (
    set STUDENT_REPO=%%i
)

echo Current student repo:
echo %STUDENT_REPO%
echo.

REM ==============================
REM 2. 강사 repo 주소
REM ==============================
set INSTRUCTOR_REPO=https://github.com/NextopProjects/Unity-FlappyBird

echo Switching to instructor repository...
git remote set-url origin %INSTRUCTOR_REPO%

IF ERRORLEVEL 1 (
    echo Failed to change remote.
    goto END
)

echo.
echo Fetching instructor latest...
git fetch origin

IF ERRORLEVEL 1 (
    echo Fetch failed.
    goto RESTORE
)

echo.
echo Force overwriting local with instructor main...
git reset --hard origin/main

IF ERRORLEVEL 1 (
    echo Reset failed.
    goto RESTORE
)

echo.
echo Force update complete.

REM ==============================
REM 3. origin을 다시 학생 repo로 복구
REM ==============================
:RESTORE
echo.
echo Restoring student repository...
git remote set-url origin %STUDENT_REPO%

echo.
echo ==========================================
echo   Done (origin restored to student repo)
echo ==========================================

:END
pause