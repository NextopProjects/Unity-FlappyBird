@echo off
chcp 65001 > nul
echo ========================================
echo ML-Agent 학습
echo ========================================
echo.
echo 1. 새 학습 시작
echo 2. 이전 학습 재개
echo.
set /p choice="선택 (1 또는 2): "

cd python
call .venv\Scripts\activate
cd ..

if "%choice%"=="1" (
    echo.
    echo [새 학습 시작]
    mlagents-learn config/trainer_config.yaml --run-id=flappy_bird_v1 --force
) else if "%choice%"=="2" (
    echo.
    echo [이전 학습 재개]
    mlagents-learn config/trainer_config.yaml --run-id=flappy_bird_v1 --resume
) else (
    echo 잘못된 선택입니다.
    pause
    exit /b 1
)

echo.
echo ========================================
echo 학습 완료
echo ========================================
echo.
echo 모델 파일 위치: results/flappy_bird_v1/
echo.
pause
