@echo off
chcp 65001 > nul
echo ========================================
echo TensorBoard 실행
echo ========================================
echo.
echo 브라우저에서 http://localhost:6006 접속
echo 종료하려면 Ctrl+C
echo.

cd python
call .venv\Scripts\activate
cd ..

tensorboard --logdir results --port 6006

pause
