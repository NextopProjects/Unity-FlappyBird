@echo off
setlocal EnableDelayedExpansion

echo ==========================================
echo   최신 업데이트 적용
echo ==========================================

REM ==============================
REM 1. 현재 origin 주소 저장 (사용자 repo)
REM ==============================
for /f "tokens=2" %%i in ('git remote get-url origin') do (
    set STUDENT_REPO=%%i
)

echo 현재 사용자 저장소:
echo %STUDENT_REPO%
echo.

REM ==============================
REM 2. 프로젝트 원격 저장소 주소 설정
REM ==============================
set INSTRUCTOR_REPO=https://github.com/NextopProjects/Unity-FlappyBird

echo 프로젝트 원격 저장소로 변경 중...
git remote set-url origin %INSTRUCTOR_REPO%

echo.
echo 3. 최신 내용 pull 시도...
git pull origin main

IF %ERRORLEVEL% NEQ 0 (
    echo.
    echo ⚠️ Conflict 또는 오류 발생!
    echo.
    set /p confirm=프로젝트 원격 최신 커밋으로 강제 덮어쓰시겠습니까? (Y/N):

    IF /I "!confirm!"=="Y" (
        echo.
        echo 🔥 강제 동기화 진행...

        git fetch origin
        git reset --hard origin/main

        echo.
        echo ✅ 프로젝트 원격 최신 커밋으로 강제 적용 완료
    ) ELSE (
        echo.
        echo ❌ 강제 적용 취소
    )
) ELSE (
    echo.
    echo ✅ 정상적으로 업데이트 완료
)

REM ==============================
REM 4. 다시 사용자 저장소로 복구
REM ==============================
echo.
echo 사용자 저장소로 원복 중...
git remote set-url origin %STUDENT_REPO%

echo.
echo ==========================================
echo   작업 완료 (origin 사용자 repo 복구됨)
echo ==========================================

pause
