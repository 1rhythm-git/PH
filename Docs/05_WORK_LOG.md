PH Work Log
________________________________________

1. 문서 목적

이 문서는 실제 구현 진행 상황, 다음 작업 후보, 고려해야 할 요소, 리팩터링 타이밍을 누적 기록한다.

기획/전체 구조는 `00_MASTER_PROJECT_BRIEF.md`, 아이템 세부 규칙은 `02_ITEM_SYSTEM_SPEC.md`, PART별 실행 계획은 `04_CODEX_EXECUTION_PLAN.md`를 기준으로 한다.

________________________________________

2. 현재까지 완료된 주요 작업

2.1 기본 화면 구조

완료 내용:
• `Canvas/TopUI`, `Canvas/MiddleUI`, `Canvas/BottomUI` 기반 구조 구성
• `MiddleUI` 내부에 그리드, 라인, 층 라인, 플레이어, 엘리베이터, 아이템 레이어 구성
• 사용자가 테스트용으로 넣은 Canvas 이미지와 `TopUI`, `BottomUI`의 `Image` 컴포넌트는 유지

고려 사항:
• 추후 실제 디자인 적용 시 임시 런타임 생성 UI와 수동 배치 UI가 섞이지 않도록 정리 필요
• `MiddleUI`의 모든 플레이 레이어는 `BottomUI`보다 앞에 보여야 함

리팩터링 타이밍:
• TopUI 최종 와이어프레임 또는 아트 가이드가 확정될 때
• HUD 항목이 현재보다 많아져 런타임 자동 생성 방식이 유지보수에 불리해질 때

________________________________________

2.2 그리드 / 층 / 페이지 / 가시성

완료 내용:
• 8 x 10 UI 그리드 구성
• 절대 층수와 페이지 인덱스 기반 진행 구조 구현
• `Next Page` 실행 시 현재 층과 page index 변경 확인
• 현재 층은 밝게, 지나간 층은 어둡게, 도달하지 않은 층은 검게 표시
• 층 숫자 표기는 제거
• 실행 중 일부 층 경계선이 보이지 않던 문제 보정
• `OnValidate` 중 UI 레이아웃 변경으로 발생하던 `SendMessage cannot be called during Awake, CheckConsistency, or OnValidate` 오류 대응

고려 사항:
• UI 배경 이미지가 들어가도 현재 층 시인성이 유지되어야 함
• 가시성 레이어는 플레이 요소보다 뒤에 있어야 함
• 층 라인과 그리드 셀의 정렬이 해상도 변경에도 유지되어야 함

리팩터링 타이밍:
• 배경/포그/층 밝기 연출이 아트 기준으로 확정될 때
• 층 단위 연출이 늘어나 `BuildingGridUI`, `FloorVisibilityController`, `FloorLineUI` 책임이 과도하게 섞일 때

________________________________________

2.3 플레이어 / 입력

완료 내용:
• 플레이어 런타임 생성
• 좌우 이동 구현
• 화면 밖 이탈 방지
• 모바일 원터치 조작 구현
• `MiddleUI` 영역 터치 시 바라보는 방향으로 지속 이동
• 다시 터치하면 피벗 후 반대 방향 지속 이동
• 키보드 커서 입력은 테스트용으로 유지
• 엘리베이터 상승 중 조작 잠금 처리

고려 사항:
• 모바일 터치가 기본이며 키보드는 테스트 수단으로 유지
• 상승/리스폰/페이지 전환 중 위치 보정이 통과 판정이나 점수 판정으로 계산되면 안 됨

리팩터링 타이밍:
• 조작 방식이 터치 외 스와이프, 버튼 UI, 게임패드까지 확장될 때
• 입력 처리와 이동 상태 처리가 한 클래스에 과도하게 누적될 때

________________________________________

2.4 엘리베이터 / 층 상승

완료 내용:
• 플레이어가 엘리베이터 중심에 도달하면 상승
• 중심 일치 후 조작 불가
• 플레이어가 순간이동하지 않고 엘리베이터와 함께 다음 층으로 이동
• 상승 중 플레이어 중심 위치 유지
• 홀수 층은 우측, 짝수 층은 좌측에 엘리베이터 배치
• 다음 층의 진행 방향은 좌→우, 우→좌를 반복
• 엘리베이터는 `FloorLine` 아래 납작한 형태로 표시
• 엘리베이터는 `FloorLine`보다 앞에 그려짐
• 상승 시 엘리베이터 바닥부터 같은 색상의 가이드라인 표시
• 가이드라인은 상승 후 사라지지 않음
• 상승한 엘리베이터는 도착 층에서 멈추고 재동작하지 않음
• `Next Page` 전환 시 새 페이지 엘리베이터가 이미 동작된 상태로 표시되던 문제 수정
• `Next Page` 전환 시 동작 전 가이드라인이 미리 표시되던 문제 수정
• 상승 중 동작하지 않는 다른 엘리베이터가 깜박이며 사라지던 문제 수정

고려 사항:
• 도착한 층의 벽에 플레이어가 붙지 않고 엘리베이터 중심을 유지해야 함
• 10층에서 11층으로 이동할 때 타고 온 엘리베이터 오브젝트는 보이되, 해당 가이드라인은 표시하지 않아야 함
• 페이지 전환 시 새 페이지의 엘리베이터는 사용 전 상태여야 함

리팩터링 타이밍:
• 엘리베이터 외 다른 층 이동 장치가 추가될 때
• 상승 연출, 사운드, 이펙트, 카메라 연출이 붙어 `ElevatorController`가 과도해질 때
• 엘리베이터 상태 저장/복원이 필요한 시점

________________________________________

2.5 TopUI / HUD

완료 내용:
• 캐릭터 초상화 영역
• 플레이어 레벨
• 닉네임
• HP 하트
• 타이머
• 현재 층
• 점수
• 아이템 획득 상태 표시
• TopUI 폰트 크기 확대 및 상단 정렬 보정
• HP 하트가 가려지거나 보이지 않던 문제 보정
• 하트 획득 시 Max Life를 넘지 않도록 처리
• Max Life 상태에서 하트를 획득하면 점수 보너스로 전환

고려 사항:
• 현재 HUD는 테스트 편의를 위해 런타임 생성 성격이 강함
• 추후 디자인 변경 시 배치된 UI 참조를 갱신하는 구조가 더 적합함
• 아이템 획득 표시 API는 유지하되, 내부 UI 구성은 디자인에 맞게 교체 가능해야 함

리팩터링 타이밍:
• TopUI 최종 디자인이 확정될 때
• 초상화, 레벨, 닉네임, 재화, 버프, 옵션 등 표시 항목이 확장될 때
• 런타임 생성 UI가 실제 아트 프리팹과 충돌하기 시작할 때

권장 리팩터링 방향:
• `TopHUDController`는 데이터 갱신만 담당
• 실제 오브젝트는 `TopUI` 하위에 수동 배치 또는 프리팹으로 구성
• `Portrait`, `Level`, `Nickname`, `Hearts`, `Timer`, `Score`, `Floor`, `ItemStatus` 참조를 Inspector로 연결

________________________________________

2.6 아이템 시스템

완료 내용:
• CSV 기반 아이템 테이블 구성
• 아이템 타입: Score, Time, Heal 우선 적용
• 아이템은 해당 층 바닥선 위, 셀 중앙에 배치
• 아이템 크기 64 x 64 적용
• 통과 필요 횟수 `RequiredPassCount` CSV 설정
• 통과 횟수 UI 표시 및 통과 시 차감
• 0이 되는 순간 획득
• 아이템 수명 `LifetimeSeconds` CSV 설정
• 해당 층에 도달하면 수명 체크 시작
• 시간이 지나면 소멸
• 시간이 남아도 플레이어가 해당 층을 벗어나면 소멸
• 좌측 끝 셀과 우측 끝 셀에는 아이템 생성 금지
• 엘리베이터 위치 셀에는 아이템 생성 금지
• 아이템 획득 시 TopUI에 점수/시간/생명력 반영
• Score는 네모, Time은 동그라미, Heal은 하트 fallback 도형으로 표시
• 아이템 이미지 관리를 위해 `IconKey` 기반 구조 도입
• `ItemIcons.csv`, `ItemIconDefinition`, `ItemIconTable`, `IItemIconProvider`, `FallbackItemIconProvider` 추가

고려 사항:
• 향후 아이템은 ID별 아이콘 파일명을 직접 들고 있기보다 `IconKey`를 들고, 별도 아이콘 테이블이 실제 로컬/원격 리소스를 매핑하는 구조가 적합함
• 서버에는 런 결과 검증에 필요한 `ServerItemId`, `TableVersion`, `EffectKey`, `EffectValue`, 획득 이벤트 정보가 전달 가능해야 함
• BackND 연동 시 게임 플레이 코드는 뒤끝 SDK에 직접 의존하지 않아야 함
• 아이콘은 가능하면 작은 단위 파일 대량 로딩보다 Atlas 또는 Addressables 그룹으로 관리
• 원격 아이콘은 `RemotePath`, `Hash`, `Version`을 기준으로 캐시 무효화 가능해야 함

리팩터링 타이밍:
• 실제 아이콘 이미지 리소스가 준비될 때
• 아이템 타입이 Score, Time, Heal 외 Skill, Collection으로 확장될 때
• `ItemInstance` 내부 효과 실행 switch가 커지기 시작할 때
• 아이템 획득 연출, 사운드, 이펙트, 저장 요청이 추가될 때
• 서버 검증형 아이템과 클라이언트 즉시 적용형 아이템이 분리될 때

권장 리팩터링 방향:
• `ItemDefinition`은 데이터만 담당
• `ItemInstance`는 통과/획득 상태만 담당
• 효과 실행은 `IItemEffect` 또는 `ItemEffectResolver`로 분리
• 아이콘 로딩은 `IItemIconProvider` 구현체로 분리
• 로컬 fallback, Addressables, 서버 다운로드 provider를 교체 가능하게 유지

________________________________________

2.7 아이템 효과 분리 / Game Over 기반

완료 내용:
• `IItemEffect` 인터페이스 추가
• `AddScoreItemEffect`, `AddTimeItemEffect`, `HealHeartItemEffect` 추가
• `ItemEffectResolver`를 통해 `EffectKey`와 `ItemType` 기준으로 효과 실행
• `ItemInstance`는 통과/획득 상태와 이벤트 기록을 담당하고, 실제 효과 실행 판단은 resolver로 분리
• `GameOverReason` 추가
• `GameStateController` 추가
• `TopHUDController`에서 Time Over와 Life Depleted 이벤트 발행
• 시간 0초 도달 시 Game Over
• HP 0 도달 시 Game Over
• Game Over 시 타이머 정지, 플레이어 입력 정지, 플레이어 이동 잠금, 엘리베이터/아이템 스폰 컨트롤러 비활성화
• Time Over 테스트를 위해 InGame 씬의 `runDurationSeconds`를 90초로 설정
• Game Over 시 전체 화면 반투명 검은 오버레이 표시
• 오버레이 중앙에 흰색 `GAME OVER` 텍스트 표시
• Game Over 결과창에 종료 사유, 최고 도달 층, 점수, 남은 시간, 남은 HP, 획득 아이템 수 표시
• Game Over 결과창의 `CONFIRM` 버튼 터치/클릭 시 인게임 종료 흐름 실행
• 현재 인게임 종료 흐름은 `SceneFlowManager.LoadLobby()` 호출로 연결

고려 사항:
• 현재 Game Over는 인게임 종료 상태까지만 담당한다.
• 추후 결과창과 로비 이동은 `GameStateController`에서 직접 UI를 만들기보다 결과 데이터 생성 후 별도 UI/SceneFlow 계층으로 전달하는 방식이 적합하다.
• 현재 흐름은 `GAME OVER -> 결과창 표시 -> 확인 버튼 -> Lobby`로 구성되어 있다.
• 추후 Google AdMob 보상형 광고를 붙이면 결과창에 광고보기 버튼을 추가하고, 광고 시청 완료 후 부활하는 흐름을 연결한다.
• HP 감소는 현재 `PlayerHealth.TakeDamage()`를 통해 처리하고, HP 0 도달 시 Game Over로 연결한다.
• Game Over 이후에도 결과 산정에 필요한 최고 층, 점수, 아이템 이벤트는 보존되어야 한다.

리팩터링 타이밍:
• Game Over 결과창에 상세 보상/저장 상태/재시도 버튼을 추가하기 직전
• Google AdMob 보상형 광고보기/부활 버튼을 결과창에 추가하기 직전
• Game Over 연출이 페이드, 사운드, 결과 요약, 버튼 UI를 포함하게 될 때
• 적 피격/리스폰/무적 시간 시스템이 추가될 때
• 점수 정산과 최고 층 저장이 붙을 때
• BackND 런 결과 저장을 연결하기 직전
• `TopHUDController`가 HP/Timer 데이터 소유와 표시 책임을 동시에 갖는 것이 부담될 때

권장 리팩터링 방향:
• HP와 Timer는 장기적으로 `PlayerHealth`, `RunTimer` 같은 모델 클래스로 분리
• `TopHUDController`는 표시만 담당
• `GameStateController`는 상태 전환과 결과 데이터 확정만 담당
• 결과창은 별도 `GameOverResultUI` 또는 `RunResultPresenter`로 분리
• 광고 부활은 `IRewardedAdService` 같은 인터페이스로 분리해 결과창 UI가 AdMob SDK에 직접 의존하지 않게 처리
• 로비 이동은 `SceneFlowManager`를 통해 처리

________________________________________

2.8 Game Over 결과 데이터

완료 내용:
• `RunResultData` 추가
• Game Over 종료 사유, 최고 도달 층, 점수, 남은 시간, 남은 HP, 획득 아이템 이벤트 목록을 하나의 결과 데이터로 묶음
• `GameStateController`가 Game Over 확정 시 `LastRunResultData`를 생성해 보관
• `TopHUDController`의 점수/시간/HP 값과 `InfiniteFloorManager`의 최고 도달 층, `RunItemEventRecorder`의 획득 아이템 이벤트를 연결
• Game Over 오버레이 클릭으로 바로 Lobby로 이동하지 않고, 결과창의 `CONFIRM` 버튼 클릭 시 Lobby로 이동

고려 사항:
• 현재 `RunResultData`는 결과창/저장 시스템이 붙기 전까지 `GameStateController.LastRunResultData`로 보관한다.
• 획득 아이템 이벤트는 결과 생성 시점에 새 리스트로 복사되어 이후 Recorder 변경과 분리된다.
• 현재 점수/시간/HP의 실제 소유자는 아직 `TopHUDController`이므로, 장기적으로는 표시 UI와 런 데이터 모델을 분리하는 것이 적합하다.
• 광고 부활이 들어가면 `RunResultData`에 reviveUsed, reviveSource, reviveCount 같은 필드를 추가해 저장/랭킹 검증에 활용한다.
• 광고 시청 실패/취소/보상 미지급 상태에서는 부활하지 않고 결과창을 유지해야 한다.

리팩터링 타이밍:
• Game Over 결과창에 상세 보상/저장 상태/재시도 버튼을 추가하기 직전
• Google AdMob 보상형 광고 부활을 연결하기 직전
• BackND 런 결과 저장 또는 랭킹 업로드를 연결하기 직전
• 점수 산식, 층 보너스, 아이템 보너스 등 최종 정산 단계가 추가될 때

권장 리팩터링 방향:
• `RunResultData`는 저장/표시용 불변 스냅샷으로 유지
• 결과창은 `GameStateController.LastRunResultData`를 받아 표시만 담당
• 결과창의 광고보기 버튼은 `IRewardedAdService`를 통해 보상형 광고 결과만 받고, 실제 부활 처리는 GameState/Respawn 계층으로 위임
• BackND 업로드는 별도 `IRunResultRepository` 또는 서비스 인터페이스 뒤에 둬서 게임플레이 코드가 SDK에 직접 의존하지 않게 유지

________________________________________

2.9 Lobby 1차 구성

완료 내용:
• `LobbyController` 추가
• 기존 Lobby 씬의 `Canvas/HeaderUI`, `Canvas/ContentUI`, `Canvas/FooterUI` 구조 유지
• Header에 게임 제목, 플레이어 레벨/닉네임, 로그인 상태 자리 표시
• Content에 최고층/최고점수 표시 영역 추가
• `START` 버튼 추가
• `START` 버튼 클릭 시 `SceneFlowManager.LoadInGame()`으로 InGame 진입
• Ranking, Shop, Options, Ad Slot 자리 버튼 추가
• Footer에 배너 광고/BackND 상태 자리 표시
• `Lobby -> InGame -> Game Over 결과창 -> Confirm -> Lobby` 전체 순환 테스트가 가능하도록 구성

고려 사항:
• 현재 최고층/최고점수는 임시 Inspector 값이다.
• 저장 시스템이 붙으면 `BestHighestFloor`, `BestScore`를 로컬 저장/BackND 동기화 값으로 갱신해야 한다.
• Ranking, Shop, Options, Ad Slot은 아직 비활성 자리 버튼이다.
• 최종 UI 아트가 확정되기 전까지는 런타임 생성 UI로 유지한다.
• 추후 배너 광고는 Footer 영역, 보상형 광고는 Game Over 결과창에서 분리해 연결한다.

리팩터링 타이밍:
• Lobby 최종 디자인이 확정될 때
• 로그인, 랭킹, 상점, 광고 SDK가 실제로 붙을 때
• 기록 표시가 로컬/서버 저장 상태를 함께 보여줘야 할 때

권장 리팩터링 방향:
• `LobbyController`는 버튼 흐름과 데이터 반영만 담당
• 실제 UI 오브젝트는 수동 배치 또는 프리팹으로 분리
• 기록 데이터는 별도 `IPlayerRecordRepository` 또는 런 기록 서비스에서 받아 표시

________________________________________

2.10 씬 전환 후 InGame 초기화 문제 수정

완료 내용:
• `Loading -> Lobby -> InGame` 경로에서 InGame이 정상 실행되지 않던 원인 확인
• InGame 단독 실행은 정상이고 씬 전환 진입에서만 문제 발생
• 원인은 `SceneFlowManager` 중복 처리에서 `Destroy(gameObject)`를 호출하던 구조
• InGame 씬의 `Managers` GameObject에는 `SceneFlowManager` 외에 `InfiniteFloorManager`, `PlayerSpawner`, `ElevatorController`, `ItemSpawner`, `GameStateController`가 함께 붙어 있음
• 씬 전환으로 들어오면 기존 `SceneFlowManager`가 `DontDestroyOnLoad`로 유지되어 InGame 쪽 `SceneFlowManager`가 중복으로 판단됨
• 이때 `Managers` GameObject 전체가 파괴되어 InGame 핵심 컨트롤러들이 함께 사라지는 문제 발생
• 중복 발견 시 GameObject 전체가 아니라 `SceneFlowManager` 컴포넌트만 `Destroy(this)`로 제거하도록 수정

고려 사항:
• 씬별 `Managers` 오브젝트는 이름이 같아도 해당 씬의 전용 런타임 컨트롤러를 포함할 수 있다.
• 싱글톤 중복 처리에서는 GameObject 전체 삭제보다 컴포넌트 단위 삭제가 안전하다.
• 장기적으로는 영속 매니저와 씬 전용 매니저 GameObject를 분리하는 것이 명확하다.

리팩터링 타이밍:
• 전역 매니저가 SceneFlow 외 저장, 광고, 사운드, 인증 등으로 늘어날 때
• 씬 전용 컨트롤러와 전역 컨트롤러의 배치 규칙을 정리할 때

권장 리팩터링 방향:
• 전역 매니저는 `GlobalManagers` 같은 별도 루트에 배치
• 씬 전용 매니저는 `SceneManagers` 또는 각 시스템별 GameObject에 배치
• 중복 싱글톤은 자기 컴포넌트만 제거하고, 같은 GameObject의 다른 컴포넌트는 건드리지 않도록 유지

________________________________________

2.11 캐릭터 스테이터스 기반

완료 내용:
• `CharacterDefinition` ScriptableObject 추가
• 테스트용 `DefaultCharacter` 에셋 추가
• 테스트용 `TriangleLowSpecCharacter` 에셋 추가
• 캐릭터 외형 모양 `Square`, `Triangle` 선택 구조 추가
• `PlayerShapeGraphic` 추가로 UI Player를 네모/세모 형태로 런타임 렌더링
• InGame 씬의 `PlayerSpawner.characterDefinition`에 `DefaultCharacter` 연결
• 캐릭터 기본 스탯 5종 정의: 이동속도, 방향전환 쿨타임, 부스터 게이지, MaxLife, 아이템 즉시 획득 확률
• `PlayerCharacterRuntime` 추가
• `PlayerSpawner`에서 선택 캐릭터 데이터를 Player/HUD에 적용하는 흐름 추가
• 캐릭터 이동속도를 `PlayerMotor` 이동속도에 적용
• 캐릭터 방향전환 쿨타임을 `PlayerController` 피벗 입력에 적용
• 이동거리와 방향전환에 따라 부스터 게이지가 누적되도록 연결
• 부스터 게이지를 TopHUD에 배경/채움 게이지와 `BOOST %` 라벨로 표시
• 캐릭터 MaxLife를 TopHUD 하트 수에 적용
• 캐릭터 아이템 즉시 획득 확률을 `ItemInstance` 획득 판정에 연결

고려 사항:
• 부스터 100% 도달 시 효과는 캐릭터 고유 스킬 버튼이 아니라 피버타임 발동으로 기획을 조정했다.
• 초기 조작 정책은 별도 버튼 없이 100% 도달 후 다음 방향전환 시 자동 발동하는 방향으로 둔다.
• 캐릭터 데이터가 연결되지 않으면 기존 테스트 기본값이 유지된다.
• 현재 InGame에는 네모 기본 캐릭터가 연결되어 있으며, 세모 저성능 캐릭터는 `PlayerSpawner.characterDefinition`에 수동으로 교체해 테스트한다.
• 아이템 즉시 획득 확률은 RequiredPassCount를 무시하는 강한 효과이므로 서버 검증/랭킹 정책과 함께 관리해야 한다.
• 캐릭터는 게임머니, 광고보상, 업적 보상 등 다양한 방식으로 해금될 예정이므로 보유/장착 상태는 별도 저장 모델이 필요하다.

리팩터링 타이밍:
• Lobby에 캐릭터 선택/보유 UI를 추가하기 직전
• BackND 캐릭터 보유/장착 저장을 연결하기 직전
• 피버타임 효과가 캐릭터별로 실제 구현되기 직전
• 점수/랭킹 검증에 캐릭터 ID와 스탯 버전을 포함해야 할 때

권장 리팩터링 방향:
• `CharacterDefinition`은 캐릭터 정적 데이터만 담당
• `PlayerCharacterRuntime`은 한 런에서 변하는 부스터 게이지/확률 판정만 담당
• 캐릭터 보유/구매/장착은 `ICharacterInventoryRepository` 같은 저장 인터페이스 뒤에 둔다.
• 피버타임 효과는 `ICharacterFeverEffect` 또는 effect resolver로 분리해 캐릭터별 확장성을 유지한다.

________________________________________

2.12 Lobby 캐릭터 선택 UI / InGame 선택 캐릭터 적용

완료 내용:
• Lobby에 테스트 캐릭터 선택 영역 추가
• `DefaultCharacter`, `TriangleLowSpecCharacter`를 Lobby 선택 목록에 연결
• `CharacterSelectionState` 추가로 씬 전환 중 선택 캐릭터를 보관
• START 버튼 실행 전 현재 선택 캐릭터를 확정하도록 처리
• `PlayerSpawner`가 Lobby 선택 캐릭터를 Inspector 기본값보다 우선 적용하도록 변경
• InGame 단독 실행 시에는 기존 `PlayerSpawner.characterDefinition`을 fallback으로 유지

고려 사항:
• 현재 선택 상태는 런타임 정적 상태이므로 앱 재실행/에디터 도메인 리로드 이후에는 Lobby 목록의 첫 캐릭터가 기본값이 된다.
• 캐릭터 보유/구매/장착 저장은 아직 구현하지 않는다.
• 현재 UI는 테스트용 버튼 기반이며, 캐릭터가 늘어나면 스크롤 목록 또는 카드 리스트로 전환이 필요하다.
• 세모 캐릭터가 선택되어 InGame에 진입하면 외형과 스탯이 함께 변경되어야 한다.

리팩터링 타이밍:
• 캐릭터가 4개 이상으로 늘어나는 시점
• 캐릭터 보유/해금/장착 상태를 저장해야 하는 시점
• BackND 프로필/인벤토리 저장과 연결하기 직전
• 캐릭터 스탯 버전이 런 결과/랭킹 검증 데이터에 포함되어야 할 때

권장 리팩터링 방향:
• `CharacterSelectionState`는 임시 런타임 선택 상태로 유지
• 저장이 필요해지면 `CharacterSelectionState` 뒤에 로컬 저장/BackND 저장 인터페이스를 추가
• Lobby UI는 표시/입력만 담당하고, 보유/장착 판정은 별도 서비스로 분리

________________________________________

2.13 PlayerHealth / 테스트 Enemy 피격 흐름

완료 내용:
• `PlayerHealth` 추가로 HP 소유 책임을 HUD에서 분리
• 선택 캐릭터의 `MaxLife`를 `PlayerHealth` 초기 체력으로 적용
• `PlayerHealth`가 HUD 하트 표시를 동기화하도록 연결
• HP 0 도달 시 `GameStateController.RequestGameOver(GameOverReason.LifeDepleted)` 호출
• 피격 후 짧은 무적 시간과 시작 컬럼 리스폰 처리 추가
• 회복 아이템이 `PlayerHealth`를 우선 회복하고, 만피일 때 기존 점수 보너스를 유지하도록 변경
• `TestEnemySpawner`, `TestEnemyHazard` 추가
• InGame 씬 `Managers`에 `TestEnemySpawner` 자동 연결
• EnemyLayer가 없으면 런타임에 `MiddleUI` 아래 자동 생성

고려 사항:
• 현재 Enemy는 테스트용 Hazard 기반이며 정식 Enemy AI/순찰 구조는 미구현이지만, EnemyTrail 기능은 가이드라인 방식으로 구현되어 있다.
• 테스트 Enemy는 현재 플레이 층 기준으로 표시되고, 플레이어와 UI Rect가 겹치면 1 데미지를 준다.
• 피격 후 리스폰은 현재 시작 컬럼으로 워프하는 최소 구현이다.
• 기획 문서의 “피격 후 항상 1층부터 다시 시작” 규칙은 아직 전체 층 리셋까지 적용하지 않았다.
• 광고 부활은 `PlayerHealth.Revive()` 진입점을 통해 연결할 수 있도록 준비만 했다.

리팩터링 타이밍:
• 정식 Enemy 이동 패턴과 전용 EnemyTrail 컨트롤러 구조로 전환할 때
• 피격 후 층 리셋, 아이템 리셋, 적 상태 리셋 규칙을 확정할 때
• 보상형 광고 부활에서 HP/타이머/위치 복구 정책을 구현할 때

권장 리팩터링 방향:
• `PlayerHealth`는 HP/무적/부활만 담당
• 리스폰 위치와 층 리셋은 별도 `PlayerRespawnController`로 분리
• 테스트 Enemy는 정식 `EnemyStageSpawner`, `EnemyCollisionHandler`로 대체

________________________________________

2.14 Enemy 라인 배치 / 수직 이동 규칙

완료 내용:
• 테스트 Enemy 배치를 셀 중심이 아니라 셀과 셀 사이 세로 라인 기준으로 변경
• 좌우 양끝 라인을 제외한 내부 라인만 배치 대상으로 사용
• 시작 시 내부 라인 1~7에 Enemy 7개가 모두 생성되도록 변경
• 라인 순번에 따라 각 Enemy 속도 범위가 다르게 적용되도록 변경
• 최초 생성 위치를 화면 최상단에 머리가 붙은 상태로 설정
• Enemy 이동을 좌우 이동 없이 상하 수직 이동으로 제한
• 바닥 또는 천장에 닿으면 방향을 반전하고 수직 속도를 `minVerticalSpeed`~`maxVerticalSpeed` 사이에서 재추첨
• Enemy 머리부터 최상단까지 흰색 가이드라인을 표시
• Enemy가 내려갈 때 가이드라인이 길어지고 올라갈 때 줄어드는 연출 추가
• 빠른 수직 이동 중 프레임 사이를 통과해 충돌이 누락될 수 있어 Enemy 충돌 높이를 이동거리 기준으로 보정
• UI World Rect 겹침과 라인 허용치 혼합 방식 대신 Enemy/Player를 같은 부모 로컬 좌표로 환산해 히트박스 충돌 판정
• 아이템 충돌 경로에는 HP 차감 호출이 없으며, 회복 아이템은 `PlayerHealth.Heal()`만 호출하는 것으로 확인
• Enemy 히트박스 디버그 표시 추가
• Enemy 가이드라인을 전용 뒤쪽 레이어로 분리해 캐릭터보다 뒤에 표시
• MiddleUI 렌더링 순서를 `EnemyGuideLineLayer -> EnemyLayer -> PlayerLayer` 순으로 전경 배치해 층 밝기/가시성 오버레이에 가려지지 않도록 조정
• 시작 엘리베이터 위치에서 정지 중 과판정이 발생하지 않도록 충돌 보정 폭과 라인 허용 범위를 축소
• 피격 후 복귀 위치를 단순 시작 컬럼이 아니라 현재 층에 진입할 때 사용한 엘리베이터 컬럼으로 변경
• 피격 직후 이동 입력이 리스폰 위치를 덮어쓰지 않도록 짧은 이동 잠금 처리 추가
• 리스폰 이동 잠금 시간을 늘리고 무적 시간 동안 플레이어 점멸 연출 추가
• NextPage 진입 시 Enemy 7개를 다시 최상단에서 생성해 출발하도록 변경
• 테스트 시간을 30초에서 90초로 상향
• 기존 피격/HP 감소/Game Over 연결은 유지

고려 사항:
• 현재는 내부 라인 7개에 테스트 Enemy를 1개씩 생성한다.
• 라인 인덱스는 8컬럼 기준 1~7이 유효하며, 0과 8은 좌우 외곽선이라 사용하지 않는다.
• 속도 변화는 벽 접촉 시 즉시 랜덤 재추첨하는 1차 구현이다.
• 현재 속도 차이는 `speedStepPerLine`으로 라인 순번마다 가산하는 방식이다.
• 정식 난이도 설계 시 층별 Enemy 개수, 속도 범위, 라인 선택 규칙, 가이드라인 표시 시간을 별도 데이터로 분리해야 한다.
• Enemy 피격 후 복귀 위치는 1층만 `PlayerHealth.respawnColumn`, 2층 이상은 `ElevatorController.CurrentFloorStartColumn` 기준이다.
• 현재 점멸은 `CanvasRenderer.SetAlpha()` 기반의 임시 연출이다.
• NextPage Enemy 재생성은 `InfiniteFloorManager.CurrentFloorChanged`에서 page index 변경을 감지해 처리한다.
• 히트박스 디버그 표시는 테스트용이며, 최종 연출 단계에서 비활성화하거나 에디터 전용 표시로 전환한다.
• 추후 난이도/레벨 디자인에서 Enemy 또는 EnemyTrail을 가시성 레이어 뒤로 보내는 규칙을 별도 옵션으로 분리할 수 있다.

리팩터링 타이밍:
• 여러 Enemy를 층별로 배치할 때
• EnemyTrail을 `TestEnemyHazard`에서 전용 컨트롤러로 분리할 때
• Enemy 타입별 이동 패턴이 늘어날 때

권장 리팩터링 방향:
• `TestEnemySpawner`를 `EnemyStageSpawner`로 확장
• Enemy 배치 데이터는 라인 인덱스, 속도 범위, 데미지, 크기를 가진 데이터 구조로 분리
• EnemyTrail은 라인 인덱스와 현재 이동 방향을 받아 표시하는 전용 컨트롤러로 분리

________________________________________

2.15 캐릭터 스프라이트 리소스 적용

완료 내용:
• Enemy 기본 이미지를 `Art/Enemy/police.png` 기반 경찰 PNG로 교체
• Enemy는 별도 애니메이션 없이 단일 PNG를 사용하는 구조로 적용
• Enemy 히트박스는 유지하고 이미지만 크기 조정
• Enemy 히트박스 디버그 표시는 일반 플레이 중 숨김 처리
• 기본 스파이 캐릭터 스프라이트 제작 및 적용
• 스파이 캐릭터 리소스 구조를 Idle 2장, Walk 2장, Run 2장, 정면 portrait 1장으로 구성
• 두 번째 캐릭터 슬롯을 닌자 콘셉트로 교체
• 닌자 캐릭터 리소스 구조도 스파이와 동일하게 구성
• 닌자 Walk 프레임에서 본체와 떨어진 잡티 픽셀을 제거
• 로비 캐릭터 순서는 `AgentX -> Ninja`로 유지
• InGame 단독 실행 fallback 캐릭터는 `AgentX`로 유지
• 스파이와 닌자의 스테이터스 값은 밸런스 테스트용으로 서로 교환 적용

고려 사항:
• Run 스프라이트는 현재 기본 이동에는 사용하지 않고, 추후 스킬/아이템 효과용으로 보관한다.
• 캐릭터 선택 화면 정면 이미지는 현재 portrait 용도이며, 최종 로비 카드 디자인에 맞춰 크롭/비율 조정이 필요할 수 있다.
• 스파이/닌자 스탯은 현재 밸런스 테스트 값이므로 정식 릴리즈 전 캐릭터별 장단점 정책을 다시 확정해야 한다.

리팩터링 타이밍:
• 캐릭터가 3개 이상으로 늘어날 때
• 캐릭터 스프라이트 아틀라스 또는 Addressables 관리가 필요할 때
• 캐릭터별 Run 애니메이션을 실제 부스터/아이템 효과에 연결할 때

권장 리팩터링 방향:
• 캐릭터 리소스는 `CharacterDefinition`에서 직접 Sprite 배열을 들되, 장기적으로는 Addressables 키 또는 스킨 데이터로 분리
• 로비 캐릭터 버튼은 이름 텍스트가 아니라 portrait 중심 카드 UI로 전환
• 캐릭터 스탯 버전 필드를 런 결과 데이터에 포함할 수 있도록 준비

________________________________________

2.16 사운드 파일 구조

완료 내용:
• SFX용 리소스 경로를 `Assets/_Project/Resources/Audio/SFX`로 확립
• `GameSfxPlayer` 추가
• 현재 예정 사운드 키:
  - `Enemy.ogg`: Enemy가 바닥 또는 천장에 부딪힐 때
  - `pass.ogg`: Player가 Item을 통과해 카운트될 때
  - `gain.ogg`: Player가 Item을 획득할 때
• SFX 기본 볼륨을 0.5로 설정
• Enemy 바운스 시 Enemy SFX 재생 호출 연결
• 아이템 통과/획득 시 pass/gain SFX 재생 호출 연결

고려 사항:
• 실제 `.ogg` 파일은 아직 사용자가 업로드할 예정인 리소스다.
• 파일명과 Resources 로드 경로가 맞지 않으면 소리가 재생되지 않는다.
• 추후 옵션에서 SFX 볼륨을 제어해야 하므로 `GameSfxPlayer`는 옵션 저장값을 받을 수 있어야 한다.

리팩터링 타이밍:
• BGM, UI 사운드, 캐릭터/스킬 사운드가 추가될 때
• 옵션 UI에서 SFX/BGM 볼륨 슬라이더를 구현할 때
• Resources 로딩을 Addressables 또는 AudioMixer 기반으로 전환할 때

권장 리팩터링 방향:
• `GameSfxPlayer`는 AudioMixer Group과 연결
• 사운드 키와 경로는 CSV 또는 ScriptableObject 테이블로 분리
• 옵션 저장값은 별도 Settings 서비스에서 관리

________________________________________

2.17 아이템 아이콘 / 랜덤 배치 / 신발 아이템

완료 내용:
• 생명력 하트, 시간 시계, 점수 코인, 점수 보석 아이콘 제작 및 `Resources/Items/Icons` 경로에 저장
• `ResourceItemIconProvider` 추가로 `ItemIcons.csv`의 `LocalAddress`를 통해 Sprite 로드
• 아이템 크기를 1.2배로 확대
• 아이템 카운트 숫자를 흰색으로 변경하고 Outline/Shadow 추가
• 아이템 배치가 매번 동일한 문제를 줄이기 위해 런타임 시드 기반 랜덤 배치로 변경
• `randomizeSeedOnStart`가 켜져 있으면 실행마다 배치가 달라짐
• 빨간 운동화 아이콘 제작 및 `Red Sneaker` 아이템 등록
• 날개달린 신발 아이콘 제작 및 `Winged Shoe` 아이템 등록
• `AddMoveSpeedItemEffect` 추가
• `EffectDurationSeconds` 컬럼 추가
• 빨간 운동화는 이동속도 +20%, 초기 구현 기준 10초 지속
• 날개달린 신발은 이동속도 +50%, 초기 구현 기준 10초 지속
• 이동속도 아이템 효과는 영구 적용이 아니라 지속시간 후 자동 제거
• 지속시간 동안 `PlayerBuffVisualFeedback`으로 캐릭터 점멸 효과 적용
• TopUI에 현재 이동속도 버프 보너스와 남은 시간을 표시하는 `SpeedBuffText` 추가
• `PlayerSpawner`가 런타임 생성된 `PlayerMotor`를 `TopHUDController`에 바인딩하도록 연결

고려 사항:
• 이동속도 버프는 현재 활성 버프의 퍼센트를 합산하는 방식이다.
• 정식 밸런스에서는 Stack Policy와 Stack Limit을 데이터화해야 한다.
• `LifetimeSeconds`는 필드에 남아있는 시간이고, `EffectDurationSeconds`는 획득 후 효과 지속시간이다.
• 신발 아이템은 현재 런 한정 효과이며 저장 대상이 아니다.
• 아이콘은 현재 개별 PNG 로드 구조이므로 아이콘이 늘어나면 Atlas 또는 Addressables 전환을 검토한다.
• 현재 버프 UI는 TopUI 우측 텍스트 한 줄 표시이며, 최종 UI에서는 아이콘+타이머 형태로 전환하는 것이 적합하다.

리팩터링 타이밍:
• 스킬형 아이템이 5종 이상으로 늘어날 때
• 버프 중첩 정책이 아이템별로 달라질 때
• 버프 아이콘/남은 시간 UI가 필요할 때
• 아이콘 리소스가 많아져 개별 PNG 관리가 부담될 때

권장 리팩터링 방향:
• `PlayerMotor`의 버프 목록은 장기적으로 `PlayerBuffController`로 분리
• 점멸/발광/속도 잔상 등 버프 연출은 `PlayerBuffVisualFeedback`에서 효과 타입별로 확장
• `ItemDefinition`에 StackPolicy, StackLimit, EffectTarget 같은 컬럼 추가 검토

________________________________________

2.18 이동 제한 / 엘리베이터 도착 처리

완료 내용:
• 플레이어 이동 범위를 좌측 끝 셀과 우측 끝 셀 중심으로 제한
• 좌우 끝에 도달하면 벽에 부딪히는 연출 없이 방향만 피벗하고 Idle로 정지
• 상승하는 엘리베이터에 도착했을 때 피벗 후 Idle 상태로 전환하고 상승 시작
• 엘리베이터 상승 직전 1프레임 대기해 Idle 전환이 시각적으로 적용될 수 있도록 처리

고려 사항:
• 현재 이동 제한은 8컬럼 구조를 전제로 한다.
• 향후 층별 시작/목표 컬럼이 달라지면 목표 지점 판정도 층 데이터 기반으로 분리해야 한다.

리팩터링 타이밍:
• 층마다 엘리베이터 위치나 목표 지점이 달라질 때
• 벽 충돌, 자동 정지, 피벗 연출이 캐릭터별로 달라질 때

권장 리팩터링 방향:
• `PlayerMotor`는 위치 계산과 이동만 담당
• 목표 도착/정지/피벗 상태 전환은 `PlayerController` 또는 별도 MovementState로 분리

________________________________________

2.19 Game Over 결과창 중 런타임 정지 보강

완료 내용:
• Game Over 결과창 표시 시 Enemy 동작도 함께 정지하도록 변경
• `TestEnemyHazard`를 비활성화해 이동/충돌/바운스 사운드 추가 발생을 방지
• `TestEnemySpawner`도 함께 비활성화해 결과창 이후 새 Enemy 생성 여지를 차단

고려 사항:
• 현재 결과창 진입 시 플레이어, 타이머, 엘리베이터, 아이템 스폰, Enemy 스폰/이동이 모두 정지된다.
• 향후 일시정지와 Game Over 정지가 분리되면 정지 대상 목록을 공통 인터페이스로 관리하는 것이 적합하다.

리팩터링 타이밍:
• Pause 기능을 추가할 때
• Game Over 연출 중 일부 오브젝트만 계속 움직이는 정책이 필요할 때
• Enemy 시스템이 테스트 클래스에서 정식 클래스로 교체될 때

권장 리팩터링 방향:
• `IGameplayPausable` 같은 인터페이스로 정지 가능한 런타임 시스템을 등록
• `GameStateController`는 직접 FindObjectsByType을 호출하기보다 등록된 시스템에 상태 변경을 브로드캐스트

________________________________________

2.20 Enemy 배치 난이도 규칙 초안

완료 내용:
• `TestEnemySpawner`의 고정 전체 내부 라인 생성 방식을 난이도 기반 개수 생성으로 변경
• 기본 Enemy 수는 페이지당 4개, 최대 7개로 설정
• 10층 단위로 Enemy 수가 1개씩 증가하도록 설정
• 실행 시 runtime seed를 새로 생성하고, 페이지별 라인 선택에 함께 적용해 재실행 시 동일 배치 반복을 완화
• 난이도 단계마다 Enemy 세로 이동 속도에 추가 보정값을 적용

고려 사항:
• `randomizeSeedOnStart`가 켜져 있으면 실행마다 Enemy 라인 배치가 달라진다.
• `randomizeSeedOnStart`를 끄면 `enemyLineRandomSeed` 기준으로 재현 가능한 테스트 배치를 사용할 수 있다.
• EnemyTrail 구현 시 이 라인 선택 결과를 기반으로 경로 예고 UI를 연결하면 된다.
• 난이도 수치는 플레이 감각 확인 후 `InGame` 씬의 `TestEnemySpawner` Inspector 값으로 조정 가능하다.

리팩터링 타이밍:
• Enemy 시스템이 테스트 클래스에서 정식 클래스 구조로 교체될 때
• Enemy 종류, 출현 패턴, 층별 금지 라인 같은 데이터 규칙이 필요할 때
• EnemyTrail 예고선과 실제 이동 패턴을 동일 데이터로 묶어야 할 때

권장 리팩터링 방향:
• `EnemySpawnProfile` 또는 ScriptableObject 기반 난이도 테이블로 분리
• 라인 선택, 개수, 속도 보정, Enemy 타입 선택을 독립 정책 클래스로 분리

________________________________________

2.21 아이템 획득 피드백 텍스트

완료 내용:
• 아이템 획득 시 플레이어 머리 위에 텍스트가 표시되는 `PlayerItemPickupFeedback` 추가
• 아이템 통과 카운트가 인정될 때 `PASS` 텍스트 피드백 표시
• 텍스트는 위로 살짝 이동하면서 페이드아웃되도록 구현
• 이동속도 증가 아이템은 `SPEED UP` 노란색으로 표시
• 점수 아이템은 `+SCORE` 흰색으로 표시
• 시간 아이템은 `TIME UP` 파란색으로 표시
• 생명력 아이템은 `GET Life` 붉은색으로 표시
• `PlayerSpawner`에서 런타임 생성 플레이어에 피드백 컴포넌트를 자동 부착

고려 사항:
• 현재 피드백 텍스트는 플레이어 RectTransform의 자식으로 생성되므로 플레이어 이동을 따라간다.
• 정식 UI 연출 단계에서는 글꼴, 크기, 색상, 상승 거리, 지속시간을 데이터화할 수 있다.
• 생명력이 이미 최대치여서 점수 보너스로 전환되는 경우에도 생명력 아이템 자체 피드백은 `GET Life`로 표시된다.

리팩터링 타이밍:
• 획득 연출 종류가 아이템별로 달라질 때
• 텍스트 외 아이콘, 파티클, 사운드 변형이 필요할 때
• 아이템 정의 테이블에서 피드백 문구/색상을 직접 관리해야 할 때

________________________________________

2.22 Max Life 증가 아이템

완료 내용:
• `Winged Heart` 아이템 추가
• `add_max_life` 효과를 처리하는 `AddMaxLifeItemEffect` 추가
• `PlayerHealth`에 런 중 Max Life 아이템 보너스 한도 추적 추가
• 현재 생명력이 Max이고 추가 슬롯이 비활성일 때 Max Life +1 적용
• 생명력이 차감된 상태에서는 Max Life 증가 없이 생명력만 회복
• 추가 슬롯 활성 중 재획득하면 SCORE로 환산하고, 슬롯 소모 후에는 Max Life +1 재획득 가능
• 날개 달린 하트 아이콘 `max_life_heart.png` 추가
• 획득 피드백 문구는 `MAX LIFE`, 색상은 붉은색으로 표시

고려 사항:
• 현재 Max Life 아이템 보너스 한도는 코드에서 1로 고정되어 있다.
• SCORE 환산은 기존 만피 하트 보너스와 같은 `FullHeartScoreBonusPerHeart` 기준을 사용한다.
• 아이템 자체는 재획득 가능해야 SCORE 환산이 가능하므로 `MaxAcquirePerRun`은 제한하지 않는다.

리팩터링 타이밍:
• 캐릭터별 Max Life 증가 한도가 달라질 때
• Max Life 증가량, 회복량, SCORE 환산량을 아이템 데이터로 분리해야 할 때
• 장기 성장형 Max Life 증가 아이템이 추가될 때

________________________________________

2.23 아이템 통과 카운트 규칙 수정

완료 내용:
• Heart Pack 통과 카운트를 3회로 변경
• Winged Heart 통과 카운트를 5회로 변경
• Time, Score 계열 아이템은 스폰 시 1~5회 랜덤 카운트를 부여하도록 변경
• Time 계열 아이템은 통과 카운트에 비례해 시간 증가량이 상승하도록 변경
• Score 계열 아이템은 통과 카운트가 높을수록 점수 보정이 붙도록 변경
• 현재 점수 보정은 추가 통과 1회당 +25%로 설정
• `ItemSpawner` Inspector 값으로 랜덤 카운트 범위와 점수 보정률을 조정할 수 있도록 추가

고려 사항:
• Time 아이템은 `EffectValue x 통과 카운트`만큼 시간을 증가시킨다.
• 점수 보정은 Score 타입, `add_score`, `AffectsScore` 아이템에만 적용한다.
• 즉시 획득 캐릭터 효과가 발동하면 랜덤 카운트와 관계없이 바로 획득된다.

리팩터링 타이밍:
• 아이템별 카운트 범위와 보정률이 달라질 때
• 보정 대상이 점수 외 시간/회복/버프 지속시간까지 확장될 때
• CSV에 `MinPassCount`, `MaxPassCount`, `PassBonusPolicy` 같은 컬럼이 필요해질 때

________________________________________

2.24 Enemy 피격 텍스트 피드백

완료 내용:
• Enemy와 충돌해 실제 데미지가 적용될 때 플레이어 머리 위에 `Oops!` 텍스트 표시
• 기존 `PlayerItemPickupFeedback` 연출을 재사용해 위로 이동하며 페이드아웃되도록 연결
• 무적 시간 또는 Game Over 상태로 `TakeDamage()`가 실패한 경우에는 피드백을 표시하지 않음
• 피드백 텍스트 크기는 `PASS` 기본 크기, `Oops!` 2배, 아이템 획득 문구 1.5배로 조정
• Enemy 피격 후 복귀 위치는 1층만 Player 시작 컬럼, 2층 이상은 기존처럼 현재 층 시작 승강기 컬럼을 사용

고려 사항:
• 현재 문구 색상은 주황빛 붉은색이다.
• 정식 Enemy 시스템으로 분리할 때 피격 피드백은 `PlayerDamageFeedback` 같은 별도 컴포넌트로 분리할 수 있다.

________________________________________

2.25 InGame 배경 이미지 초안

완료 내용:
• `image.png`와 `sample_ingame.JPG`를 참고해 InGame용 픽셀아트 배경 초안 제작
• 무한 상승하는 야간 스파이 빌딩 단면 콘셉트 적용
• 10개 층이 한 화면에 들어오는 세로형 빌딩 구조로 구성
• 기존 Loading/Lobby 배경과 같은 841x1870 비율로 저장
• `Assets/_Project/Art/Backgrounds/InGame_Background.png` 추가
• InGame 씬의 기존 `image.png` 배경 Sprite 참조를 `InGame_Background.png`로 교체

고려 사항:
• 실제 플레이 시 GridPanel, VisibilityLayer, Player/Enemy/Item 가시성과 함께 확인해야 한다.
• 필요하면 배경 대비를 낮춘 버전 또는 Grid 전용 크롭 버전을 추가 제작한다.

________________________________________

2.26 부스터 게이지 / 피버타임 기획 조정

변경 배경:
• 기존 계획은 이동 및 피벗으로 획득하는 게이지가 100%가 되었을 때 캐릭터 고유 스킬 또는 버프를 발동하는 구조였다.
• 모바일 조작에서 별도 스킬 버튼을 추가하면 조작 부담이 커질 수 있어, 게이지를 피버타임 발동 자원으로 사용하는 방향으로 조정했다.

기획 결정:
• 현재 `BOOST %`로 표시되는 게이지는 기획상 피버타임 게이지로 전환한다.
• 게이지 축적 조건은 기존과 동일하게 이동거리와 방향전환을 사용한다.
• 게이지 100% 도달 후에는 별도 버튼 없이 다음 방향전환 시 피버타임을 자동 발동하는 방향을 기본안으로 둔다.
• 피버타임 중에는 이동속도, 아이템 획득, Enemy 회피, 점수 보정 등 런 플레이에 직접 영향을 주는 보너스를 적용할 수 있다.
• 캐릭터별 고유성은 수동 스킬이 아니라 피버타임 효과 차별화로 확장한다.
• 수동 스킬 버튼은 초기 출시 범위에서 제외하고, 필요 시 추후 캐릭터/아이템 시스템 확장 단계에서 재검토한다.

구현 상태:
• 현재 코드는 게이지 누적과 TopHUD 표시까지만 구현되어 있다.
• 피버타임 발동 조건, 지속시간, 효과, UI 연출은 아직 구현 전이다.

________________________________________

2.27 런타임 구조 리팩터링 1차

완료 내용:
• 아이템 효과 실행 결과를 `ItemEffectResult`로 반환하도록 통합
• 날개 하트와 일반 하트가 점수로 전환될 때 머리 위 문구도 `+SCORE`로 표시되도록 실행 순서 수정
• 아이템 효과 문자열을 `ItemEffectKeys`로 통합
• 하트 3회와 날개 하트 5회 규칙의 코드 하드코딩을 제거하고 `Items.csv`의 `RequiredPassCount`를 단일 기준으로 사용
• `ItemInstance`의 중복 피드백 컴포넌트 탐색을 정리하고 충돌 검사 배열을 재사용하도록 변경
• `IGameplayPausable`을 추가해 Game Over 시 Enemy 정지를 구체 클래스 검색에서 공통 계약 기반으로 변경
• 게이지 도메인 API와 TopHUD 표시를 `Booster`에서 `Fever` 중심으로 정리
• 기존 캐릭터와 씬의 직렬화 값은 `FormerlySerializedAs`와 호환 속성으로 유지

검증 상태:
• `git diff --check` 통과
• Unity Test Framework와 생성된 C# 프로젝트가 없어 자동 컴파일 및 테스트는 미실행
• Unity Editor에서 아이템 결과, Game Over 정지, Fever 게이지 에셋 값 유지 여부를 수동 확인해야 함

남은 리팩터링 후보:
• `ItemInstance`의 패스 판정과 수명 관리를 별도 컴포넌트로 분리
• 런타임 생성 HUD/Item/Enemy 오브젝트를 프리팹 기반으로 이전
• 정식 Enemy 명칭과 설정 데이터 구조 도입
• EditMode/PlayMode 테스트 Assembly 구성

________________________________________

2.28 이동속도 아이템 카운트 / 지속시간 조정

완료 내용:
• Red Sneaker와 Winged Shoe의 기본 효과 지속시간을 5초로 변경
• 두 신발 모두 스폰 시 1~3 중 랜덤 통과 카운트를 부여
• 최종 지속시간을 `기본 5초 × 통과 카운트`로 계산
• 카운트 1은 5초, 2는 10초, 3은 15초 지속
• 기존 이동속도 증가율 Red Sneaker +20%, Winged Shoe +50%는 유지
• Time/Score 아이템의 기존 1~5 랜덤 카운트 정책은 유지

________________________________________

2.29 PART 9 / PART 13 완료 판정 및 스킬 범위 조정

완료 판정:
• PART 9 EnemyTrail은 `TestEnemyHazard`의 가이드라인 생성, 이동 동기화, 상하단 Clamp, 삭제 처리로 기능 기준 완료
• 전용 `EnemyTrailLineController` 분리는 정식 Enemy 구조 전환 시 리팩터링 항목으로 유지
• PART 13 스킬형 아이템은 이동속도 신발, 회복, 시간, Max Life 효과와 지속시간 처리 기반이 마련되어 완료 처리

기획 조정:
• Enemy 일시 정지 아이템은 플레이 흐름과 현재 게임 방향에 불필요한 기능으로 판단해 기획 범위에서 제외
• Red Sneaker와 Winged Shoe를 대표 스킬형 아이템으로 유지
• 무적, Enemy 감속, 추가 스킬 효과, 중첩 제한과 수치 조정은 추후 레벨 디자인 및 밸런스 단계에서 검토

________________________________________

2.30 TopUI Safe Area 대응

완료 내용:
• `TopSafeAreaController`를 추가해 `Screen.safeArea` 기준 상단 안전 여백 적용
• TopUI 배경은 기존 영역을 유지하고 런타임 HUD 콘텐츠만 안전 영역 아래로 이동
• Safe Area 외에 기본 상단 여백 12를 추가해 일반 화면에서도 최상단 밀착 완화
• 화면 크기 또는 Safe Area 변경 시 런타임에 자동 재계산

변경된 주요 파일:
• `Assets/_Project/Scripts/Runtime/Core/UI/TopSafeAreaController.cs`
• `Assets/_Project/Scenes/InGame.unity`

검증 상태:
• 신규 스크립트와 InGame 씬 컴포넌트 연결 구조 확인
• Unity가 `Assembly-CSharp.dll`을 재생성했고 `TopSafeAreaController` 포함 및 컴파일 오류 없음 확인
• Unity Editor 및 Galaxy S26 실기 화면 검증 필요

남은 이슈:
• 실제 단말기에서 카메라 홀과 상태바 아래로 모든 HUD가 내려오는지 확인
• 여백이 과도하거나 부족하면 `additionalTopPadding` 값을 조정

관련 작업 기준:
• Lobby UI 재구성 전 선행 수정사항

________________________________________

2.31 TopUI 하트 / 층 표시 정렬

완료 내용:
• 피버 게이지 바로 위 한 줄에 하트와 현재 층 표시를 배치
• 하트는 피버 게이지 좌측 기준으로 정렬
• 현재 층은 피버 게이지 우측 기준으로 정렬
• Max Life 증가 시 하트 영역을 확보하기 위해 하트 영역을 층 표시보다 넓게 구성

변경된 주요 파일:
• `Assets/_Project/Scripts/Runtime/Core/UI/TopHUDController.cs`

검증 상태:
• 런타임 HUD 생성 좌표와 정렬값 확인
• Unity 컴파일 및 Play 모드 화면 검증 필요

남은 이슈:
• Max Life 증가 상태와 세 자리 이상 층수에서 텍스트가 겹치지 않는지 확인

관련 작업 기준:
• Lobby UI 재구성 전 선행 수정사항

________________________________________

2.32 이동속도 아이템 재획득 정책 수정

완료 내용:
• 이동속도 버프의 퍼센트 합산 중첩 제거
• 동일 이동속도 아이템 재획득 시 능력치는 유지하고 지속시간만 새로 갱신
• 하위 이동속도 아이템 획득 시 현재 상위 능력치를 유지하고 지속시간만 새로 갱신
• 상위 이동속도 아이템 획득 시 능력치와 지속시간을 모두 상위 아이템 기준으로 갱신
• HUD와 캐릭터 점멸 연출도 실제 활성 능력치 및 갱신된 지속시간과 동기화

변경된 주요 파일:
• `Assets/_Project/Scripts/Runtime/Core/Player/PlayerMotor.cs`
• `Assets/_Project/Scripts/Runtime/Core/Items/AddMoveSpeedItemEffect.cs`
• `Assets/_Project/Scripts/Runtime/Core/Player/PlayerBuffVisualFeedback.cs`
• `Docs/02_ITEM_SYSTEM_SPEC.md`

검증 상태:
• 이동속도 버프 적용 흐름과 HUD 참조 API 정적 확인
• Unity 컴파일 및 Play 모드 아이템 조합별 검증 필요

남은 이슈:
• Red Sneaker와 Winged Shoe를 서로 다른 순서로 획득해 능력치와 타이머를 확인해야 함

관련 작업 기준:
• Lobby UI 재구성 전 선행 수정사항

________________________________________

2.33 이동 / 대시 먼지 및 Run 애니메이션 연동

완료 내용:
• 일반 이동 시 진행 방향 반대쪽 발밑에 작은 픽셀 먼지 발생
• 두 캐릭터에 이미 연결된 Run 스프라이트를 이동속도 버프 중 대시 애니메이션으로 적용
• 대시 중 먼지는 일반 이동보다 발생 주기와 입자 수를 늘리고 긴 잔상 형태와 푸른 밝은 색상 적용
• 먼지 오브젝트를 플레이어 레이어에 남겨 플레이어 이동을 따라붙지 않도록 처리
• 최대 36개 입자를 재사용하는 풀링 구조로 런타임 생성/삭제 부하 제한
• 이동 잠금, 정지, 버프 종료 시 일반 이동 상태와 먼지 정책으로 자동 복귀

변경된 주요 파일:
• `Assets/_Project/Scripts/Runtime/Core/Player/PlayerMovementDustFeedback.cs`
• `Assets/_Project/Scripts/Runtime/Core/Characters/PlayerSpriteAnimator.cs`
• `Assets/_Project/Scripts/Runtime/Core/Player/PlayerMotor.cs`
• `Assets/_Project/Scripts/Runtime/Core/Player/PlayerSpawner.cs`
• `Docs/00_MASTER_PROJECT_BRIEF.md`
• `Docs/02_ITEM_SYSTEM_SPEC.md`

검증 상태:
• Run 스프라이트 데이터 연결과 런타임 플레이어 자동 부착 흐름 정적 확인
• Unity 6000.3.17f1 Batch Mode 컴파일 성공 및 `Assembly-CSharp.dll`에 신규 타입 포함 확인
• Play 모드 일반/대시 이동 연출 검증 필요

남은 이슈:
• 실제 단말기에서 먼지 크기, 색상, 발생 밀도와 성능 확인
• Spy/Ninja 캐릭터별 Run 프레임 전환과 좌우 반전 확인

관련 작업 기준:
• Lobby UI 재구성 전 선행 수정사항

________________________________________

2.34 캐릭터 XP / 레벨 스킬 / Page Chance 기반

완료 내용:
• 캐릭터별 경험치와 레벨 진행 상태 추가
• `CharacterDefinition`에 레벨별 필요 XP 테이블과 고유 스킬 해금 레벨, 이름, 설명, Page Chance 추가
• 경험치 누적 시 여러 레벨을 연속 처리할 수 있는 `CharacterProgressionState.AddExperience()` API 추가
• TopUI 캐릭터 초상화와 레벨 영역 아래 XP 게이지 추가
• Lobby 캐릭터 설명에 해금 전 `LOCKED Lv.N`, 해금 후 `ACTIVE` 상태와 Chance 수치 표시
• 인게임에는 고유 스킬 설명 UI를 추가하지 않고 해금된 버프만 적용
• 해금된 Page Chance 판정 성공 시 Time 또는 Skill 아이템 1개를 해당 페이지에 보장
• AgentX는 Lv.3에 Chance 15%, Ninja는 Lv.4에 Chance 20% 해금

변경된 주요 파일:
• `Assets/_Project/Scripts/Runtime/Core/Characters/CharacterDefinition.cs`
• `Assets/_Project/Scripts/Runtime/Core/Characters/CharacterProgressionState.cs`
• `Assets/_Project/Scripts/Runtime/Core/Characters/PlayerCharacterRuntime.cs`
• `Assets/_Project/Scripts/Runtime/Core/UI/TopHUDController.cs`
• `Assets/_Project/Scripts/Runtime/Core/UI/LobbyController.cs`
• `Assets/_Project/Scripts/Runtime/Core/Items/ItemSpawner.cs`
• `Assets/_Project/Data/Characters/DefaultCharacter.asset`
• `Assets/_Project/Data/Characters/TriangleLowSpecCharacter.asset`
• `Docs/00_MASTER_PROJECT_BRIEF.md`
• `Docs/02_ITEM_SYSTEM_SPEC.md`

검증 상태:
• 캐릭터별 XP, 레벨업, 스킬 해금, HUD/Lobby 갱신 흐름 정적 확인
• Unity 6000.3.17f1 Editor 자동 컴파일 및 도메인 리로드 성공
• Unity 6000.3.17f1 Batch Mode 컴파일 종료 코드 0 확인
• Play 모드 레벨 경계/페이지 보장 스폰 검증 필요

남은 이슈:
• 런 종료 획득 XP 결과식과 지급 시점 확정
• 로컬 및 BackND 캐릭터 진행 상태 저장 연동
• 캐릭터별 필요 XP, 해금 레벨, Chance 밸런스 확정

관련 작업 기준:
• Lobby UI 재구성과 수집형 아이템 작업 전 캐릭터 성장 기반 보완

________________________________________

2.35 TopUI XP / 피버 게이지 배치 보완

완료 내용:
• XP 게이지 가로 크기를 기존 대비 70%로 축소
• XP 게이지 세로 크기를 기존 대비 120%로 확대
• 피버 게이지를 TopUI 바닥에 고정하고 전체 가로 폭으로 확장
• 층수를 피버 게이지 좌측 위에 좌측 정렬
• 하트를 피버 게이지 우측 위에 우측 정렬
• 피버 게이지와 두 상태 표시를 하단 앵커 기준으로 묶어 TopUI 높이 변화 시 함께 이동하도록 보완

변경된 주요 파일:
• `Assets/_Project/Scripts/Runtime/Core/UI/TopHUDController.cs`
• `Docs/05_WORK_LOG.md`

검증 상태:
• RectTransform 앵커와 오프셋 정적 확인
• `git diff --check` 통과
• Unity Batch Mode는 실행 중인 Editor의 프로젝트 점유로 중단되어 컴파일 재검증 필요
• Play 모드 해상도별 배치 검증 필요

남은 이슈:
• Galaxy S26 실기기에서 Safe Area 적용 후 피버 게이지와 MiddleUI 경계 확인
• 작은 화면에서 우측 상태 텍스트와 하트 영역 간 간격 확인

관련 작업 기준:
• Lobby UI 재구성 전 TopUI 최종 배치 보완

________________________________________

2.36 TopUI 하트 / 층수 우측 정렬 순서 변경

완료 내용:
• 하트와 층수를 모두 피버 게이지 우측 위에 배치
• 두 항목 모두 우측 정렬 적용
• 화면 가장 오른쪽에 층수, 그 왼쪽에 하트가 표시되도록 순서 변경
• 피버 게이지 기준 하단 앵커와 세로 위치는 기존 보완값 유지

변경된 주요 파일:
• `Assets/_Project/Scripts/Runtime/Core/UI/TopHUDController.cs`
• `Docs/05_WORK_LOG.md`

검증 상태:
• RectTransform 가로 구간과 정렬값 정적 확인
• Unity 컴파일 및 Play 모드 배치 검증 필요

남은 이슈:
• 높은 층수 표기에서 하트와의 간격 확인

관련 작업 기준:
• 사용자 단말 확인 후 요청된 TopUI 정렬 보완

________________________________________

2.37 Lobby 단일 캐릭터 초상화 선택 레이아웃

완료 내용:
• BEST RECORD 패널 세로 높이를 기존 0.34에서 0.17로 절반 축소
• 기록 정보를 제목, 최고 층, 최고 점수의 가로 한 줄 구성으로 변경
• CHARACTER 패널 세로 높이를 기존 0.27에서 0.44로 확장
• 캐릭터 에셋에 연결된 제작 전면 이미지를 중앙 단일 초상화로 출력
• 캐릭터 이름은 초상화 아래, 캐릭터 레벨은 패널 우상단에 표시
• 기존 레벨 스킬 잠금/활성 설명은 캐릭터 패널 하단에 유지
• 초상화 좌우에 이미지 기반 화살표 버튼을 배치하고 캐릭터 순환 선택 연결
• null 캐릭터 슬롯은 건너뛰고 선택 가능한 캐릭터가 하나면 화살표 버튼 비활성화

변경된 주요 파일:
• `Assets/_Project/Scripts/Runtime/Core/UI/LobbyController.cs`
• `Docs/00_MASTER_PROJECT_BRIEF.md`
• `Docs/05_WORK_LOG.md`

검증 상태:
• 캐릭터 에셋의 전면 초상화 Sprite 연결 확인
• 레이아웃 앵커, 순환 선택, 레벨/스킬/초상화 갱신 흐름 정적 확인
• Unity 6000.3.17f1 Editor 컴파일 및 도메인 리로드 성공
• Play 모드 좌우 선택 검증 필요

남은 이슈:
• 실제 화면에서 캐릭터별 전면 이미지 크기 차이 확인
• 긴 캐릭터 이름과 고레벨 숫자 표시 여유 확인
• 추후 정식 UI 아트 적용 시 런타임 화살표 Sprite를 아트 에셋으로 교체 가능

관련 작업 기준:
• 사용자 요청 순서의 Lobby UI 재구성 단계

________________________________________

2.38 날개하트 추가 MaxLife 소모 정책 수정

완료 내용:
• 날개하트로 증가한 MaxLife를 아이템 보너스 슬롯으로 추적하는 기존 구조 유지
• 피해 발생 시 날개하트 추가 생명력 슬롯을 우선 소모
• 추가 슬롯 소모와 동시에 `maxLifeBonusFromItems`와 `maxLife`를 원래 캐릭터 수치로 복귀
• 추가 슬롯 활성 중에는 MaxLife가 캐릭터 기본 수치 +1을 넘지 않도록 제한
• 추가 슬롯 소모 후에는 날개하트를 다시 획득해 MaxLife +1 적용 가능
• HUD 동기화 전에 MaxLife를 복구해 빈 추가 하트 슬롯이 남지 않도록 수정
• 2 이상의 피해가 들어오면 추가 슬롯 제거 후 남은 피해는 기존 생명력에 그대로 반영

변경된 주요 파일:
• `Assets/_Project/Scripts/Runtime/Core/Player/PlayerHealth.cs`
• `Docs/02_ITEM_SYSTEM_SPEC.md`
• `Docs/05_WORK_LOG.md`

검증 상태:
• 기본 MaxLife 3, 추가 MaxLife 1, 피해 1 기준 `4/4 → 3/3` 흐름 정적 확인
• 추가 MaxLife 상태에서 피해 2 기준 `4/4 → 2/3` 흐름 정적 확인
• Unity 6000.3.17f1 Batch Mode 컴파일 종료 코드 0 확인
• Play 모드 날개하트 재현 검증 필요

남은 이슈:
• MaxLife 보너스 한도를 2 이상으로 확장할 경우 슬롯별 연출 정책 검토

관련 작업 기준:
• Winged Heart 런타임 생명력 슬롯 회귀 수정

________________________________________

2.39 날개하트 추가 슬롯 재획득 정책 적용

완료 내용:
• 런 중 MaxLife 지급 이력 제한 제거
• 현재 활성 추가 슬롯 수만 기준으로 MaxLife 증가 제한
• 추가 슬롯 활성 중 재획득 시 MaxLife 중첩 없이 SCORE 전환 유지
• 추가 슬롯이 피해로 소모되면 캐릭터 기본 MaxLife로 복귀
• 기본 MaxLife 복귀 후 Max 상태에서 날개하트를 재획득하면 다시 MaxLife +1 적용

변경된 주요 파일:
• `Assets/_Project/Scripts/Runtime/Core/Player/PlayerHealth.cs`
• `Docs/02_ITEM_SYSTEM_SPEC.md`
• `Docs/05_WORK_LOG.md`

검증 상태:
• `3/3 → 획득 → 4/4 → 피해 → 3/3 → 재획득 → 4/4` 흐름 정적 확인
• Unity 6000.3.17f1 Batch Mode 컴파일 종료 코드 0 확인
• Play 모드 반복 재획득 검증 필요

남은 이슈:
• 추가 슬롯 활성 중 SCORE 전환량의 밸런스 확인

관련 작업 기준:
• 사용자 확정 Winged Heart 반복 재획득 정책

________________________________________

2.40 전 캐릭터 피버 게이지 증가량 30% 조정

완료 내용:
• 모든 현재 캐릭터의 이동 및 피벗 피버 게이지 증가량을 기존 값의 30%로 조정
• AgentX 이동 증가량 `0.5 → 0.15`, 피벗 증가량 `0.75 → 0.225`
• Ninja 이동 증가량 `1.0 → 0.3`, 피벗 증가량 `1.5 → 0.45`
• 캐릭터 간 상대적인 피버 증가량 차이는 기존 비율 유지
• 공통 코드 배율 없이 `CharacterDefinition` 에셋 값으로 관리하는 기존 데이터 구조 유지

변경된 주요 파일:
• `Assets/_Project/Data/Characters/DefaultCharacter.asset`
• `Assets/_Project/Data/Characters/TriangleLowSpecCharacter.asset`
• `Docs/00_MASTER_PROJECT_BRIEF.md`
• `Docs/05_WORK_LOG.md`

검증 상태:
• 등록된 캐릭터 에셋 2개의 피버 증가량 계산값 확인
• Unity 에셋 Import 및 Play 모드 게이지 누적 속도 검증 필요

남은 이슈:
• 실제 플레이 시간 기준 피버 100% 도달 속도 확인

관련 작업 기준:
• 사용자 요청 전 캐릭터 피버 증가량 0.3배 밸런스 조정

________________________________________

2.41 기본 캐릭터 표시 명칭 AgentX 변경

완료 내용:
• 기본 스파이 캐릭터의 `displayName`을 `Default Spy`에서 `AgentX`로 변경
• Lobby 캐릭터 이름과 표시 명칭을 사용하는 UI에 AgentX 자동 반영
• `characterId: default`, `DefaultCharacter.asset`, GUID는 유지해 기존 선택 및 참조 호환성 보존
• 마스터 프로젝트 문서와 작업 로그의 사용자 노출 명칭 갱신

변경된 주요 파일:
• `Assets/_Project/Data/Characters/DefaultCharacter.asset`
• `Docs/00_MASTER_PROJECT_BRIEF.md`
• `Docs/05_WORK_LOG.md`

검증 상태:
• 표시 명칭과 내부 식별자 분리 상태 확인
• Unity 에셋 Import 및 Lobby 표시 확인 필요

남은 이슈:
• 없음

관련 작업 기준:
• 사용자 요청 기본 캐릭터 명칭 변경

________________________________________

2.42 Alice / Landy 캐릭터 추가

완료 내용:
• Alice 전면 초상화와 Idle/Walk/Run 각 2프레임 픽셀 아트 생성
• Alice를 금발 양갈래 메이드, 빨간 토끼귀 머리띠 콘셉트로 구성
• Alice 기동/피버/Chance 스테이터스를 AgentX의 1.5배로 설정하고 MaxLife 3 적용
• Landy 전면 초상화와 Idle/Walk/Run 각 2프레임 픽셀 아트 생성
• Landy를 힙합 모자, 선글라스, 금목걸이를 착용한 고릴라 콘셉트로 구성
• Landy 기동/피버/Chance 스테이터스를 AgentX의 1.2배로 설정하고 MaxLife 4 적용
• 방향전환 성능 배율은 쿨타임 역수로 적용해 Alice 0.2초, Landy 0.25초 설정
• Lobby 캐릭터 순서를 `AgentX → Alice → Landy → Ninja`로 변경
• 신규 PNG를 Sprite/Point/Mipmap Off/Alpha Transparency 설정으로 통일

현재 주요 스테이터스:
• Alice: 이동 3.0, 피벗 0.2초, 이동 피버 0.225, 피벗 피버 0.3375, Chance 22.5%, MaxLife 3
• Landy: 이동 2.4, 피벗 0.25초, 이동 피버 0.18, 피벗 피버 0.27, Chance 18%, MaxLife 4

변경된 주요 파일:
• `Assets/_Project/Art/Characters/alice_front.png`
• `Assets/_Project/Art/Characters/landy_front.png`
• `Assets/_Project/Art/Characters/Alice_Default/**`
• `Assets/_Project/Art/Characters/Landy_Default/**`
• `Assets/_Project/Data/Characters/AliceCharacter.asset`
• `Assets/_Project/Data/Characters/LandyCharacter.asset`
• `Assets/_Project/Scenes/Lobby.unity`
• `Docs/00_MASTER_PROJECT_BRIEF.md`
• `Docs/05_WORK_LOG.md`

검증 상태:
• 생성 전면 이미지와 대표 Run 프레임 투명 알파 시각 확인
• 캐릭터별 스프라이트 GUID 및 데이터 참조 정적 확인
• Unity 6000.3.17f1 배치 모드 에셋 Import 및 컴파일 정상 종료
• Alice/Landy CharacterDefinition NativeFormatImporter 등록 확인
• Play 모드 4종 순환 선택 및 인게임 애니메이션 검증 필요

남은 이슈:
• Alice의 두 번째 Walk 프레임은 시트의 중립 이동 포즈를 재사용하므로 향후 전용 프레임 교체 가능
• 실제 플레이에서 Alice/Landy 캐릭터 크기와 피벗·대시 애니메이션 확인
• 신규 캐릭터 스킬 이름과 해금 레벨의 최종 기획 확정

관련 작업 기준:
• 사용자 요청 캐릭터 2종 추가 및 Lobby 순서 지정

________________________________________

2.43 Alice / Landy 로비 선택 순서 교환

완료 내용:
• Lobby 캐릭터 순서를 `AgentX → Landy → Alice → Ninja`로 변경
• 캐릭터 데이터, 능력치 및 이미지 에셋은 기존 설정 유지

변경된 주요 파일:
• `Assets/_Project/Scenes/Lobby.unity`
• `Docs/00_MASTER_PROJECT_BRIEF.md`
• `Docs/05_WORK_LOG.md`

검증 상태:
• LobbyController `availableCharacters` GUID 배열 순서 정적 확인

관련 작업 기준:
• 사용자 요청 Alice와 Landy의 로비 선택 순서 교환

________________________________________

2.44 Alice / Landy 2등신 픽셀아트 재구성

완료 내용:
• Alice와 Landy 로비 초상화를 AgentX/Ninja 기준의 작은 2등신 픽셀아트 비율로 교체
• 초상화 캔버스 내 캐릭터 점유율을 약 72% 이하로 조정해 과도한 확대 표시 완화
• 두 캐릭터의 Idle/Walk/Run 각 2프레임을 오른쪽 3/4 측면 포즈로 전면 교체
• 애니메이션 개별 프레임을 기존 캐릭터와 유사한 398×435 규격으로 통일
• 기존 PNG 파일명과 Unity GUID를 유지해 CharacterDefinition 참조 변경 없이 적용

변경된 주요 파일:
• `Assets/_Project/Art/Characters/alice_front*.png`
• `Assets/_Project/Art/Characters/landy_front*.png`
• `Assets/_Project/Art/Characters/alice_spritesheet*.png`
• `Assets/_Project/Art/Characters/landy_spritesheet*.png`
• `Assets/_Project/Art/Characters/Alice_Default/*.png`
• `Assets/_Project/Art/Characters/Landy_Default/*.png`
• `Docs/05_WORK_LOG.md`

검증 상태:
• Alice/Landy 초상화의 2등신 비율과 축소된 캔버스 점유율 시각 확인
• 대표 Run 프레임의 오른쪽 3/4 측면 진행 포즈 시각 확인
• 모든 개별 애니메이션 프레임 RGBA 398×435 규격 확인
• 대표 초상화/Run 프레임 4개 모서리 Alpha 0 확인

남은 확인:
• 실행 중인 Unity Editor에서 자동 임포트 완료 후 Lobby 및 InGame Play 모드 표시 확인

관련 작업 기준:
• 사용자 요청 기존 Spy/Ninja와 같은 2등신 픽셀아트 스타일 및 비정면 이동 애니메이션 적용

________________________________________

2.45 AgentX / Ninja 초상화 기본 규격 통일

완료 내용:
• AgentX와 Ninja 전면 초상화를 Alice/Landy와 동일한 1024×1536 RGBA 캔버스로 재정규화
• 원본 픽셀아트 디자인과 비율을 유지하면서 Nearest Neighbor 방식으로 크기 조정
• 캐릭터 높이를 캔버스 약 66%로 맞추고 수직 중앙보다 약간 위에 배치
• 재생성 원본인 `spy_front_chromakey.png`, `ninja_front_chromakey.png`도 같은 캔버스 규격으로 갱신
• 향후 캐릭터 초상화 기본 규격을 마스터 문서에 명시

초상화 기본 규격:
• 1024×1536 RGBA PNG, 투명 배경
• 2등신 전신 픽셀아트, 원본 종횡비 유지
• 캐릭터 높이 약 60~72%, 수직 중앙보다 약간 위 배치
• Point 필터, Mipmap Off, Alpha Is Transparency

변경된 주요 파일:
• `Assets/_Project/Art/Characters/spy_front.png`
• `Assets/_Project/Art/Characters/spy_front_chromakey.png`
• `Assets/_Project/Art/Characters/ninja_front.png`
• `Assets/_Project/Art/Characters/ninja_front_chromakey.png`
• `Docs/00_MASTER_PROJECT_BRIEF.md`
• `Docs/05_WORK_LOG.md`

검증 상태:
• AgentX/Ninja 초상화 1024×1536 RGBA 확인
• 두 초상화의 4개 모서리 Alpha 0 확인
• 기존 Sprite GUID 및 CharacterDefinition 참조 유지
• AgentX/Ninja 표시 크기와 여백 시각 확인

남은 확인:
• 실행 중인 Unity Editor 자동 임포트 후 Lobby에서 네 캐릭터 표시 크기 비교

관련 작업 기준:
• 사용자 요청 AgentX/Ninja 초상화 크기 통일 및 향후 기본 규격 확정

________________________________________

2.46 concept 참고 Landy 체형 샘플 제작

완료 내용:
• `/concept` 참고 이미지의 각진 실루엣과 단순한 면 분할을 Landy 픽셀아트에 반영
• 기존 힙합 모자, 선글라스, 금목걸이, 검정 스트리트웨어 정체성 유지
• 상체와 어깨를 넓히고 팔·전완·주먹을 크게 확장해 고릴라 체형 강화
• 양팔을 무릎 아래까지 늘리고 주먹이 지면 가까이 내려오는 실루엣 적용
• 로비 초상화 기본 규격인 1024×1536 RGBA 투명 PNG로 샘플 저장
• 현재 Lobby/InGame Landy 에셋은 교체하지 않고 승인용 concept 샘플로 분리

생성 파일:
• `concept/Landy/landy_sample_v1.png`
• `concept/Landy/landy_sample_v1_chromakey.png`

검증 상태:
• 1024×1536 RGBA 및 4개 모서리 Alpha 0 확인
• 얼굴, 선글라스, 주둥이, 금목걸이 보존 확인
• 긴 팔과 지면 가까운 대형 주먹 실루엣 시각 확인

남은 확인:
• 사용자 컨셉 승인 후 로비 초상화와 Idle/Walk/Run 6프레임 실사용 에셋 제작 여부 결정

관련 작업 기준:
• 사용자 요청 concept 참고 Landy 샘플 및 고릴라형 긴 팔 체형 적용

________________________________________

2.47 Landy 긴팔 고릴라 디자인 실사용 적용

완료 내용:
• 승인용 concept Landy 샘플을 실제 로비 전면 초상화에 적용
• 긴 팔, 굵은 전완, 대형 주먹, 짧은 하체를 유지한 Idle/Walk/Run 각 2프레임 제작
• Idle 프레임은 양 주먹이 지면 가까이 내려오는 너클 자세로 구성
• Walk 프레임은 주먹과 짧은 다리가 번갈아 전진하는 측면 너클 보행으로 구성
• Run 프레임은 상체를 진행 방향으로 숙이고 긴 팔을 크게 교차하는 동작으로 구성
• 기존 PNG 파일명과 Sprite GUID를 유지해 CharacterDefinition 참조 및 능력치 변경 없이 적용

캐릭터 애니메이션 지침 추가:
• 디자인 기반 6프레임은 `Idle 2 / Walk 2 / Run 2` 구성을 사용
• 6프레임은 정면이 아닌 화면 오른쪽을 향한 측면 기반으로 제작
• 얼굴, 몸통, 골반, 발이 동일한 측면 방향을 유지
• 화면 왼쪽 이동은 런타임 Sprite 좌우 반전으로 처리
• 해당 규칙을 `AGENTS.md`와 `Docs/00_MASTER_PROJECT_BRIEF.md`에 기록

변경된 주요 파일:
• `Assets/_Project/Art/Characters/landy_front*.png`
• `Assets/_Project/Art/Characters/landy_spritesheet*.png`
• `Assets/_Project/Art/Characters/Landy_Default/*.png`
• `AGENTS.md`
• `Docs/00_MASTER_PROJECT_BRIEF.md`
• `Docs/05_WORK_LOG.md`

검증 상태:
• 로비 초상화 1024×1536 RGBA 및 4개 모서리 Alpha 0 확인
• 6개 인게임 프레임 398×435 RGBA 및 4개 모서리 Alpha 0 확인
• Idle/Walk/Run 전체 오른쪽 측면 실루엣 시각 확인
• 기존 Sprite GUID 및 LandyCharacter 참조 유지 확인

남은 확인:
• Unity Play 모드에서 Lobby 초상화 표시 크기 확인
• InGame에서 좌우 반전, Idle/Walk/Run 전환 및 셀 경계 내 크기 확인

관련 작업 기준:
• 사용자 요청 Landy concept 디자인 후속 적용 및 6프레임 측면 기반 제작 지침 고정

________________________________________

2.48 Landy 이동 전방 점멸 잡티 제거

문제 상태:
• Landy 이동 애니메이션 재생 중 진행 방향 앞쪽에 작은 색상 픽셀이 프레임마다 점멸

확인 원인:
• 크로마키 배경 제거 후 선글라스와 캐릭터 외곽에 저명도 녹색 Hue 픽셀이 잔존
• `landy_run_02` 전방에 2×2 녹색 잔여 픽셀 좌표 확인
• 투명화 과정에서 Alpha 0 픽셀 RGB가 흰색으로 저장돼 가장자리 샘플링 시 흰 점이 생길 가능성 확인

수정 내용:
• Landy 투명 시트 및 Idle/Walk/Run 6프레임의 녹색 Hue 잔여 픽셀 제거
• Alpha 0 픽셀 RGB를 `(0,0,0,0)`으로 정규화해 투명 가장자리 흰색 번짐 방지
• 금목걸이와 선글라스 금색 장식은 Hue 범위에서 제외해 원본 디자인 유지

변경된 주요 파일:
• `Assets/_Project/Art/Characters/landy_spritesheet.png`
• `Assets/_Project/Art/Characters/Landy_Default/*.png`
• `Docs/05_WORK_LOG.md`

검증 상태:
• Idle/Run 대표 프레임 진행 방향 앞쪽 녹색 점 제거 시각 확인
• 투명 경계의 흰 점 제거 시각 확인
• 6프레임 규격과 기존 Sprite GUID 유지 확인

남은 확인:
• Unity Play 모드에서 이동 방향 좌우 반전 및 전체 프레임 점멸 재현 여부 확인

관련 작업 기준:
• 사용자 제보 Landy 인게임 이동 중 진행 방향 앞쪽 잡티 점멸

________________________________________

2.49 Landy 진행 방향 신발 조각 제거

문제 상태:
• `landy_walk_01`에서 본체와 분리된 신발 컴포넌트가 프레임 우측에 남아 진행 방향 앞쪽에 잘린 신발처럼 표시됨

원인:
• 6프레임 시트 분할 시 인접 프레임의 신발 픽셀이 현재 셀에 포함됨
• `PlayerSpriteAnimator`의 좌우 반전과 Sprite Pivot은 신발을 재배치하는 원인이 아니며, PNG 프레임 크롭 결과가 직접 원인으로 확인됨

수정 내용:
• Landy 6개 개별 프레임에서 가장 큰 본체 연결 컴포넌트만 유지
• 본체와 분리된 신발 및 외부 픽셀 컴포넌트 제거
• 기존 프레임 규격, Sprite GUID, 좌우 반전 로직은 유지

변경된 주요 파일:
• `Assets/_Project/Art/Characters/Landy_Default/*.png`
• `Docs/05_WORK_LOG.md`

검증 상태:
• Walk/Idle/Run 대표 프레임에서 진행 방향 앞쪽 분리 신발 제거 확인
• `landy_walk_01` 분리 신발 컴포넌트 제거 확인
• 이동 애니메이션 코드와 Sprite Pivot 변경 없이 해결

남은 확인:
• Unity Play 모드에서 좌우 이동과 프레임 교체 시 신발 위치 최종 확인

관련 작업 기준:
• 사용자 제보 진행 방향 앞쪽 신발 잘림 및 뒤쪽 신발 조각 전방 표시

________________________________________

2.50 Landy 인게임 사이즈 왜곡 후속 작업 예약

상태:
• 미완료
• 재시작 최우선

현재 문제:
• Landy 인게임 애니메이션에서 프레임별 캐릭터 크기와 비율 왜곡이 크게 발생
• 동일한 398×435 캔버스를 사용하지만 프레임별 본체 불투명 영역, 정규화 배율, 중심점이 달라 시각 크기가 흔들릴 가능성이 있음
• 긴 팔과 대형 주먹 체형 때문에 일반 캐릭터 기준의 최대 폭/높이 정규화가 Landy 실루엣을 과도하게 축소하거나 확대할 가능성이 있음

다음 작업 시작 시 우선 확인:
• Landy Idle/Walk/Run 6프레임 불투명 영역의 가로·세로 크기 및 중심 좌표 비교
• 발바닥 기준선과 머리 높이를 공통 기준으로 재정렬
• 프레임별 개별 최대 맞춤이 아닌 Landy 공통 배율 적용
• `SpriteVisual` RectTransform, `spriteVisualScale`, `Image.preserveAspect`, 중앙 Pivot 상호작용 확인
• 좌우 반전 시 위치 이동과 셀 경계 클리핑 재현 확인

완료 조건:
• Idle/Walk/Run 전환 중 Landy의 머리 높이와 몸통 크기가 일정하게 유지
• 긴 팔과 주먹이 셀 경계를 벗어나 잘리지 않음
• 좌우 반전 시 캐릭터 중심과 발바닥 기준선이 이동하지 않음
• AgentX/Alice/Ninja의 기존 표시 크기에는 영향 없음

브리핑 규칙:
• Codex 종료 후 재시작 시 이 항목을 다음 작업 브리핑의 첫 번째 항목으로 보고
• 이 문제가 완료되기 전에는 Lobby UI 재구성 및 수집형 아이템 작업보다 우선 처리

관련 작업 기준:
• 사용자 요청 Landy 인게임 사이즈 왜곡을 다음 작업 최우선으로 기록

________________________________________

2.51 이동속도 아이템 점멸 연출 제거

완료 내용:
• 이동속도 아이템 획득 시 호출되던 캐릭터 점멸 연출을 제거
• 이동속도 버프 적용, 지속시간 갱신, HUD 상태 표시는 유지
• 이동속도 버프 중 Run 애니메이션 및 대시 먼지 연출은 기존 흐름 유지

변경된 주요 파일:
• `Assets/_Project/Scripts/Runtime/Core/Items/AddMoveSpeedItemEffect.cs`
• `Docs/05_WORK_LOG.md`

검증 상태:
• `AddMoveSpeedItemEffect.Execute()`에서 `PlayerBuffVisualFeedback.PlayBlink()` 호출 제거 확인
• 이동속도 버프 적용 및 HUD 표시 코드 유지 확인
• Unity 컴파일 및 Play 모드 아이템 획득 검증 필요

남은 확인:
• Red Sneaker와 Winged Shoe 획득 시 캐릭터 점멸 없이 Run 애니메이션만 적용되는지 확인
• 피격 무적 점멸 등 다른 점멸 연출에는 영향이 없는지 확인

관련 작업 기준:
• 사용자 요청: 이동속도 아이템 획득 시 캐릭터 점멸효과 제거

________________________________________

2.52 Walk / Run 교차 프레임 제작 지침 등록

완료 내용:
• 신규 캐릭터 및 기존 캐릭터 수정 시 Walk/Run 2프레임의 좌우 손발 교차를 필수 제작 기준으로 등록
• 같은 손과 같은 발이 2프레임 모두 유지되어 캐릭터가 끌려가는 것처럼 보이는 동작을 금지 기준으로 명시
• Walk는 앞발 접지와 뒷발 회수, Run은 더 큰 보폭과 팔 스윙 및 공중감을 구분하도록 지침 추가

변경된 주요 파일:
• `AGENTS.md`
• `Docs/00_MASTER_PROJECT_BRIEF.md`
• `Docs/05_WORK_LOG.md`

검증 상태:
• 캐릭터 아트 제작 지침과 마스터 브리프의 캐릭터 애니메이션 기본 규격에 동일 기준 반영 확인
• 실제 스프라이트 제작/수정은 이번 작업 범위에서 제외

남은 확인:
• 향후 AgentX/Ninja/Alice/Landy Walk/Run 수정 시 좌우 손발 교차가 프레임별로 확실히 읽히는지 확인
• `concept/Walk_Guide`의 Walk/Run 기준 이미지를 다음 캐릭터 제작 또는 수정 시 기준 자료로 함께 참조

관련 작업 기준:
• 사용자 요청: Walk/Run이 같은 손발 위치를 유지해 끌고 가는 느낌이 나므로 향후 캐릭터 생성 규칙에 좌우 손발 교차 프레임 지침 반영

________________________________________

2.53 TopUI / BottomUI HUD 배치 및 Android 하트 표시 수정

완료 내용:
• 피버 게이지를 TopUI 런타임 루트가 아니라 `BottomUI` 하위 런타임 오브젝트로 생성
• 피버 게이지를 `BottomUI` 최상단에 붙도록 앵커와 오프셋 조정
• 하트와 층수는 기존 x축 배치를 유지하고 y축만 TopUI 바닥에 밀착
• Android APK에서 `♥` 텍스트 글리프가 표시되지 않을 수 있는 문제를 피하기 위해 하트 표시를 텍스트에서 `Resources/Items/Icons/heart` 이미지 배열로 변경
• 피격 후 빈 하트 상태도 같은 아이콘의 낮은 Alpha 표시로 처리

변경된 주요 파일:
• `Assets/_Project/Scripts/Runtime/Core/UI/TopHUDController.cs`
• `Docs/05_WORK_LOG.md`

검증 상태:
• `BottomUI` 씬 오브젝트 탐색 후 피버 게이지 생성, 미탐색 시 기존 TopHUD 루트 fallback 유지
• 하트/층수 x축 앵커 유지 확인
• `Resources/Items/Icons/heart` Sprite 리소스 경로 확인
• `git diff --check` 통과
• Unity 컴파일 및 Android APK 실기 검증 필요

남은 확인:
• Android APK 실행 직후 가득 찬 하트가 보이는지 확인
• 피격 후 빈 하트가 낮은 Alpha로 보이고 남은 하트가 계속 보이는지 확인
• 피버 게이지가 BottomUI 최상단에서 MiddleUI와 겹치지 않는지 확인

관련 작업 기준:
• 사용자 요청: 피버 게이지 BottomUI 최상단 부착, 하트/층수 TopUI 바닥 밀착, APK에서 초기 하트 미표시 버그 수정

________________________________________

3. 다음 작업 후보

우선순위 후보:
1. Landy 인게임 사이즈 왜곡 수정 (`2.50`, 재시작 최우선)
2. 추가 수정사항 반영
3. Lobby UI 재구성
4. PART 14 수집형 아이템 기반 설계
5. PlayerRespawnController 정식 분리
6. 피버타임 발동/효과 정책 정의
7. 점수 시스템 정식화
8. Lobby 캐릭터 보유/장착 저장값 연결
9. TopUI 디자인 교체 전 구조 정리
10. Google AdMob 보상형 광고 부활 흐름 설계

현재 권장 다음 작업:
• 가장 먼저 Landy 6프레임의 공통 배율, 중심점, 발바닥 기준선을 재정규화해 인게임 사이즈 왜곡을 수정한다.
• 사용자 요청 순서에 따라 추가 수정사항 반영 후 Lobby UI를 재구성한다.
• Lobby UI 재구성 완료 후 PART 14 수집형 아이템을 진행한다.
• 광고 부활 작업 전에 `PlayerRespawnController`를 분리해 일반 피격과 광고 부활의 복귀 정책을 구분한다.
• 피버타임은 발동 조건과 캐릭터별 효과 정책을 확정한 뒤 구현한다.
• 광고 부활은 리스폰 정책이 확정된 뒤 결과창 확장 작업으로 연결한다.

________________________________________

4. 서버 / BackND 고려 사항

현재 기준:
• BackND SDK는 아직 설치하지 않는다.
• 게임 플레이 로직은 서버 SDK를 직접 호출하지 않는다.
• 런 결과, 아이템 획득 이벤트, 최고 층, 점수, 수집형 아이템은 서비스 인터페이스 뒤로 분리한다.

필요한 추상화 후보:
• `IRunResultService`
• `ILeaderboardService`
• `IPlayerProfileService`
• `IInventoryService`
• `IItemCatalogService`

아이템 서버 검증 시 고려:
• `ItemId`는 클라이언트 테이블 ID
• `ServerItemId`는 서버 검증 ID
• `TableVersion`은 클라이언트/서버 테이블 불일치 검증용
• `EffectKey`, `EffectValue`는 서버가 허용한 범위인지 검증 가능해야 함
• 수집형 아이템은 중복/수량/충돌 해결 정책 필요

리팩터링 타이밍:
• 로그인/랭킹/런 결과 저장 중 하나를 실제 구현하기 직전
• 수집형 아이템이 추가되기 직전
• 오프라인 플레이와 온라인 동기화 정책이 필요해질 때

________________________________________

5. 앞으로 작업할 때 기록 규칙

각 작업 완료 후 이 문서에 다음을 갱신한다.

• 완료한 작업
• 변경된 주요 파일
• 수동으로 확인한 항목
• 남은 이슈
• 고려해야 할 요소
• 리팩터링이 필요한 시점

문서 갱신은 코드 변경과 별개로 누락되지 않도록 한다.
