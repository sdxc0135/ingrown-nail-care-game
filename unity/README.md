# 발톱 구조대 · Unity Prototype

Unity 6.0.3 기반의 내성발톱 케어 게임 프로토타입입니다.

## 필요한 환경

- Unity Hub
- Unity Editor `6000.3.15f1`
- Unity 라이선스 활성화
- Visual Studio 또는 VS Code
- Android Build Support: Android 앱으로 빌드할 때 필요
- WebGL Build Support: 브라우저 배포가 필요할 때 필요

현재 프로토타입은 별도 npm 패키지가 필요하지 않습니다. 기존 샘플 장면에 붙어 있던 `BootTest`가 실행 시 케어 UI를 자동으로 만들며, `FootCareGameController`가 도구 드래그, 점수, 타이머, 효과음을 처리합니다.

## 파일

- `Assets/_Project/02.Scripts/FootCareGameController.cs`: 게임 화면과 상호작용
- `Assets/_Project/02.Scripts/BootTest.cs`: 게임 컨트롤러 부트스트랩
- `Assets/_Project/04.Materials/ingrown-nail-macro.jpg`: 실사 매크로 이미지 원본
- `Assets/Resources/ingrown-nail-macro.jpg`: 런타임 로딩용 이미지

## 플레이 방식

1. 진료 시작
2. 클린 버블을 고르고 발 화면을 좌우로 드래그
3. 말랑 핀셋을 고르고 아래로 드래그
4. 진정 크림을 고르고 원을 그리듯 드래그
5. 포근 붕대를 고르고 길게 드래그

효과음은 외부 파일 없이 Unity에서 절차적으로 생성합니다. 실제 의료 시술 안내가 아닌 게임 연출용 프로토타입입니다.
