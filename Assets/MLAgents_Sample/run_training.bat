@echo off
REM ===========================================
REM 1. 프로젝트 root로 이동
REM ===========================================
cd ..\..

REM ===========================================
REM 2. 가상환경 활성화
REM ===========================================
call .venv\Scripts\activate.bat

REM ===========================================
REM 3. Unity ML-Agents 학습 실행
REM ===========================================
REM mlagents-learn 명령어를 사용, config 파일 지정, Unity Build는 editor로 연결
mlagents-learn Assets\MLAgents_Sample\rollerball_config.yaml --run-id=RollerBall

REM ===========================================
REM 4. 종료 후 가상환경 비활성화
REM ===========================================
deactivate

pause