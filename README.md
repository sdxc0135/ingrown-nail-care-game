# 발톱 구조대 · Ingrown Nail Care Game

내성발톱 케어를 소재로 한 모바일 캐주얼 게임 프로토타입입니다. 실제 발가락 매크로 케어 영상의 분위기에서 영감을 받아 청록색 패브릭, 따뜻한 피부톤, 금속 도구 질감의 오리지널 비주얼을 사용합니다.

## 필요한 환경

- **Node.js 20 이상**: 개발 서버 실행과 Sites 배포 도구에 사용합니다.
- **Git**: 커밋과 버전 관리에 사용합니다.
- **GitHub 계정**: 소스 저장소 `sdxc0135/ingrown-nail-care-game`에 코드를 저장합니다.
- **브라우저**: Chrome, Edge 또는 최신 모바일 브라우저에서 실행합니다.

현재 프로토타입은 정적 HTML/CSS/JavaScript라 별도 npm 패키지, 데이터베이스, 백엔드 서버가 필요하지 않습니다.

## 로컬 실행

```powershell
node -e "const http=require('http'),fs=require('fs'),path=require('path'); const root=path.resolve('dist'); http.createServer((req,res)=>{let p=path.join(root,req.url==='/'?'index.html':req.url); if(!fs.existsSync(p)){res.statusCode=404;res.end('not found');return;} res.end(fs.readFileSync(p));}).listen(4173,()=>console.log('http://localhost:4173'));"
```

그 다음 브라우저에서 `http://localhost:4173`을 엽니다.

## 게임 흐름

1. 진료 시작
2. 클린 버블로 소독
3. 말랑 핀셋으로 끝부분 정리
4. 진정 크림 바르기
5. 포근 붕대로 마무리

실제 증상이나 치료 방법을 안내하는 앱이 아니라 게임 속 연출용 프로토타입입니다.

## 배포 구조

- `dist/index.html`: 정적 게임 화면
- `dist/assets/ingrown-toe-macro.png`: 오리지널 실사 매크로 이미지
- `.openai/hosting.json`: Sites 배포 설정

앱 버전은 이후 Capacitor, React Native 또는 Flutter 중 하나를 선택해 포장할 수 있습니다.

