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
• Time Over 테스트를 위해 InGame 씬의 `runDurationSeconds`를 30초로 설정
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
• HP 감소는 현재 `TopHUDController.DamageHeart()`를 통해 Game Over 이벤트를 발행하므로, 적 충돌/피격 시스템은 이 API 또는 별도 Health 모델을 통해 연결해야 한다.
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

3. 다음 작업 후보

우선순위 후보:
1. Enemy / 피격 / HP 테스트 가능 상태 만들기
2. 하트/리스폰/피격 구조 정리
3. 점수 시스템 정식화
4. Lobby 기록 저장값 연결
5. TopUI 디자인 교체 전 구조 정리
6. 아이템 아이콘 Addressables 전환 준비
7. BackND 연동 전 로컬 저장 인터페이스 준비
8. Google AdMob 보상형 광고 부활 흐름 설계

현재 권장 다음 작업:
• Lobby 1차 구성까지 완료했으므로 다음은 Enemy/피격/HP 테스트 가능 상태를 만든다.
• 피격/리스폰을 안정적으로 붙이려면 `PlayerHealth` 분리를 선행한다.
• 광고 부활은 Enemy/피격/리스폰이 붙은 뒤 결과창 확장 작업으로 연결한다.

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
