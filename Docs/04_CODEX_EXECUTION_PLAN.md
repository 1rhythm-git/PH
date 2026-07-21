PH Codex Execution Plan
________________________________________
공통 작업 규칙
모든 파트에서 다음 규칙을 지킨다.
1.	Unity 6.3 LTS 기준으로 작성한다.
2.	Input System New만 사용한다.
3.	기존 기능을 임의로 삭제하지 않는다.
4.	아직 구현하지 않은 기능을 임시로 과도하게 연결하지 않는다.
5.	각 파트 완료 후 컴파일 에러를 확인한다.
6.	Inspector 연결 항목을 보고한다.
7.	새로 생성하거나 수정한 파일 목록을 보고한다.
8.	대규모 리팩터링을 하지 않는다.
9.	클래스 책임을 분리한다.
10.	코드 주석은 한국어로 작성한다.
11.	작업 완료 후 `05_WORK_LOG.md`에 완료 내용, 고려 요소, 리팩터링 타이밍을 기록한다.
12.	전체 공정률 100%는 Google Play 실제 출시 완료를 기준으로 하며 기능 구현률과 구분한다.

출시 공정률 기준
•	2026-07-21 현재 Google Play 출시 기준 공정률은 약 45%이다.
•	핵심 게임 루프와 주요 UI 흐름 이후에도 영구 저장, 온라인 기능, 수익화, Android 실기기 QA, 배포 및 스토어 심사가 완료되어야 100%로 판단한다.
•	다음 최우선 작업은 캐릭터별 XP/레벨과 선택/보유/장착 상태를 로컬 저장 모델에 통합하는 것이다.
•	캐릭터 강화는 저장 데이터 계약이 확정된 이후 그 모델을 기반으로 구현한다.
________________________________________
PART 1
프로젝트 베이스 구축
목표
빈 프로젝트에 기본 폴더, 씬, 공통 구조를 만든다.
작업
•	Assets/_Project 폴더 구조 생성
•	Loading 씬 생성
•	Title 씬 생성
•	Lobby 씬 생성
•	InGame 씬 생성
•	기본 Canvas 구조
•	GameRoot
•	Managers
•	EventSystem
•	Main Camera
•	SceneFlowManager
•	RuntimeBootstrapper
•	TitleSceneController
아직 구현하지 않을 것
•	플레이어 이동
•	적
•	아이템
•	서버 SDK
•	랭킹
•	상세 UI 연출
완료 조건
•	Loading, Title, Lobby, InGame 네 씬이 정상적으로 열림
•	씬 전환 가능
•	컴파일 에러 없음
구현 상태
•	완료
현행 메모
•	시작 흐름은 `Loading → Title → Lobby → InGame` 순서이다.
•	Loading은 로딩바 없이 기존 대기와 LAF 로고 강조 기능을 유지한다.
•	Loading 대기 시간은 1.5초이며 LAF 로고 하이라이트 유지 시간은 기존 0.8초를 유지한다.
•	Title은 `Title.png`를 표시하고 Lobby 비동기 로딩 진행률이 100%가 되면 점멸하는 `TOUCH` 입력을 활성화한다.
•	Lobby 전체 화면 배경은 `Lobby.png`를 사용한다.
________________________________________
PART 2
그리드와 무한 층 기반
목표
8×10 그리드와 절대 층수 구조를 구현한다.
작업
•	BuildingGridUI
•	GridCell
•	FloorPageGenerator
•	InfiniteFloorManager
•	수평선 11개
•	좌하단 원점
•	절대 층수
•	내부 페이지 인덱스
•	10층 단위 페이지 전환 기반
완료 조건
•	8×10 셀이 정확히 생성됨
•	Cell_0_0이 왼쪽 아래
•	1~10층이 정상 표시됨
•	다음 페이지 데이터 계산 가능
•	Stage Clear 개념 없음
________________________________________
PART 3
가시성 시스템
목표
현재 층, 지난 층, 미래 층의 밝기를 구분한다.
작업
•	FloorVisibilityController
•	Current Floor 상태
•	Past Floor 상태
•	Future Floor 상태
•	GridPanel과 정확히 정렬되는 가시성 레이어
완료 조건
•	1층 시작 시 1층이 밝음
•	위층은 검게 표시
•	층 상승 시 이전 층은 약간 어두워짐
________________________________________
PART 4
플레이어 생성과 좌우 이동
목표
Lobby 선택값에 따라 플레이어를 런타임 생성하고 좌우 이동시킨다.
작업
•	PlayerSpawner
•	PlayerController
•	PlayerMotor
•	Input System
•	터치 입력 확장 가능 구조
•	시작 셀 배치
•	좌우 이동
•	층 경계 제한
완료 조건
•	씬에 Player가 미리 배치되지 않음
•	런타임 생성
•	좌우 이동 가능
•	화면 밖으로 나가지 않음
________________________________________
PART 5
엘리베이터와 층 상승
목표
플레이어가 다음 층으로 이동할 수 있도록 한다.
작업
•	ElevatorController
•	층 이동 상태
•	현재 층 증가
•	페이지 경계 전환
•	현재 층 UI 갱신
•	최고 도달 층 갱신
완료 조건
•	1층에서 2층 이동
•	10층에서 11층 이동
•	페이지 인덱스 갱신
•	게임이 종료되지 않고 계속 진행
________________________________________
PART 6
하트, 리스폰, 게임 오버
목표
하트 3개와 사망 구조를 구현한다.
작업
•	CharacterDefinition
•	캐릭터별 MaxLife 적용
•	PlayerHealth
•	PlayerRespawnController
•	Normal 모드
•	Hard 모드
•	GameStateController
•	GameOverUI
•	결과창 광고보기 버튼 확장 지점
•	Google AdMob 보상형 광고 부활 정책 준비
•	시간 제한
완료 조건
•	피격 시 하트 감소
•	하트가 남으면 리스폰
•	하트 0이면 게임 종료
•	시간 0이면 게임 종료
•	게임 오버 시 최고 층 확정
•	광고 부활은 아직 구현하지 않더라도 결과창/상태 전환 구조에서 확장 가능
________________________________________
PART 7
점수 시스템
목표
층 기록과 점수를 분리하고, Game Over 기준 런 종료 점수 계산 규칙을 준비한다.
작업
•	ScoreManager
•	Current Run Score
•	Best Score
•	Run Highest Floor
•	Best Highest Floor
•	점수 이벤트 API
•	GameOver 결과 데이터
•	광고 부활 사용 여부 필드 확장 준비
•	Game Over 시 runHighestFloor 확정
•	RunScoreResult에 runHighestFloor 포함
•	Gameplay Score 반영
•	Floor Score 계산: floorMoveCount × floorScoreValue
•	Life Score 계산: remainingHearts × lifeScorePerHeart
•	Game Over 총점 계산: Gameplay Score + Floor Score + Life Score
•	캐릭터 레벨 기본 XP, 층 XP, Total Score 보너스 XP 계산
•	런 획득 게임머니와 Total Score 보너스 게임머니 계산
•	ScoreBalanceData 또는 Inspector 설정으로 점수 계수 관리
•	RunScoreResult 데이터 구조
완료 조건
•	층수와 점수가 별도로 표시됨
•	점수 추가 API가 준비됨
•	런 종료 결과 생성 가능
•	Game Over 결과 데이터에 runHighestFloor가 반드시 포함됨
•	Game Over 기준 총점이 생성됨
•	추후 AdMob 보상형 광고 부활 사용 여부를 저장/랭킹 데이터에 포함 가능
•	보너스별 점수 breakdown을 UI와 저장 데이터에 전달 가능
•	게임머니와 캐릭터 XP를 결과 확정 시 한 번만 지급
•	결과창에 점수, XP, 게임머니 breakdown을 순서대로 표시
•	결과창 글꼴은 `GAME OVER` 111, 결과 상세 48, `CONFIRM` 51 적용
검토 메모
•	Time Bonus는 기본 점수 공식에서 제외하고 Life Score는 포함한다.
•	캐릭터별 이동속도, 방향전환 쿨타임, 아이템 즉시 획득 확률은 점수/랭킹 검증에 영향을 줄 수 있으므로 결과 데이터에 캐릭터 ID를 포함할 수 있어야 한다.
•	무한 상승 구조라도 Game Over 시 해당 런의 최고 도달 층인 runHighestFloor를 확정한다.
•	Line Bonus와 세로 경계 통과 기반 점수는 사용하지 않는다.
구현 상태
•	완료
현행 메모
•	`RunRewardSettings`에서 층 점수, 생명력 점수, 층 XP, 점수 XP 배율, 보너스 게임머니 배율을 조정한다.
•	TopUI는 보유 게임머니, 보유 Ruby, 현재 런 획득 게임머니를 분리해 표시한다.
•	Lobby XP 게이지는 현재 XP 비율을 RectTransform 폭으로 반영하며 XP 0에서는 비어 있다.
________________________________________
PART 8
적 기본 시스템
목표
페이지당 적 7개를 생성하고 수직 이동시킨다.
작업
•	EnemyStageSpawner
•	EnemyPatrol
•	EnemyCollisionHandler
•	EnemyRoot
•	상단 생성
•	위아래 이동
•	페이지 전환 시 정리
완료 조건
•	적 7개 생성
•	수직 이동
•	플레이어 충돌 시 피격
•	UI 요소에 가려지지 않음
•	Game Over 결과창 표시 시 Enemy 이동/충돌/생성 정지
현행 메모
•	현재 구현은 정식 Enemy 클래스가 아니라 `TestEnemySpawner`, `TestEnemyHazard` 기반이다.
•	Enemy 이미지는 경찰 단일 PNG를 사용한다.
•	히트박스 디버그 표시는 테스트용이며 일반 플레이에서는 숨긴다.
________________________________________
PART 9
EnemyTrail
목표
적의 이동에 따라 수직 라인을 생성하고 삭제한다.
작업
•	EnemyTrailLineController
•	내려갈 때 라인 증가
•	올라갈 때 라인 감소
•	상하단 Clamp
•	수평선 접합
완료 조건
•	적 이동과 라인이 동기화됨
•	상단과 하단을 넘지 않음
•	라인 단절 없음
구현 상태
•	완료
•	현재 `TestEnemyHazard`가 Enemy 머리부터 상단까지 가이드라인을 생성하고 이동에 맞춰 길이를 갱신한다.
•	전용 `EnemyTrailLineController` 분리는 정식 Enemy 구조 전환 시 진행할 리팩터링 항목이다.
________________________________________
PART 10
아이템 데이터 기반
목표
아이템 데이터를 테이블 또는 ScriptableObject 기반으로 정의한다.
작업
•	ItemDefinition
•	ItemCategory
•	ItemPassDirection
•	ItemRarity
•	ItemInstance
•	Item ID 검증
•	Required Pass Count
•	EffectKey
•	EffectValue
•	EffectDurationSeconds
완료 조건
•	CSV 또는 Inspector에서 아이템 데이터 관리 가능
•	Score, Skill, Collection 타입 선택 가능
•	통과 횟수 설정 가능
•	아이템이 필드에 남아있는 시간과 획득 후 버프 지속시간을 분리 가능
현행 메모
•	현재 구현은 `Items.csv`, `ItemIcons.csv` 기반이다.
•	`LifetimeSeconds`는 필드 수명, `EffectDurationSeconds`는 효과 지속시간으로 사용한다.
________________________________________
PART 11
아이템 통과 판정
목표
플레이어가 아이템을 여러 번 통과해 획득하도록 한다.
작업
•	ItemPassDetector
•	Enter/Exit 기반 판정
•	내부 체류 중 중복 방지
•	좌우 양방향 인정
•	Current Pass Count
•	Required Pass Count
•	ItemProgressView
완료 조건
•	1회 아이템 정상 획득
•	3회 아이템은 정확히 3번째 통과에 획득
•	콜라이더 내부 체류 중 횟수 증가하지 않음
•	통과 카운트 시 SFX와 남은 횟수 UI가 갱신됨
________________________________________
PART 12
스코어형 아이템
목표
스코어형 아이템 효과를 구현한다.
작업
•	IItemEffect
•	AddScoreEffect
•	ScoreMultiplierEffect
•	Combo 연동 가능 구조
•	획득 연출 이벤트
완료 조건
•	획득 즉시 점수 반영
•	중복 획득 방지
•	효과가 ItemInstance와 분리됨
현행 메모
•	Score Coin과 Pass Orb가 점수형 아이템으로 동작한다.
•	Pass Orb는 보석 아이콘을 사용하고 3회 통과 후 획득한다.
•	Score 계열 아이템은 현재 스폰 시 1~5회 랜덤 통과 카운트를 받고, 추가 통과 1회당 +25% 점수 보정을 받는다.
•	Time 계열 아이템은 현재 스폰 시 1~5회 랜덤 통과 카운트를 받고, `EffectValue x 통과 카운트`만큼 시간을 증가시킨다.
________________________________________
PART 13
스킬형 아이템
목표
플레이어에게 영향을 주는 스킬형 아이템 효과 기반을 구현한다.
현재 범위
•	하트 회복
•	Max Life 증가
•	시간 증가
•	플레이어 이동 속도 증가
작업
•	IItemEffect / ItemEffectResolver
•	지속 시간
•	중첩 정책
•	효과 종료
•	버프 지속 중 플레이어 시각 효과
완료 조건
•	효과 시작과 종료가 정확함
•	중복 효과 정책이 작동함
•	게임 오버 시 효과 정리
구현 상태
•	완료
현행 메모
•	현재 구현된 스킬형 아이템은 `Red Sneaker`, `Winged Shoe`, `Winged Heart`이다.
•	이동속도 증가는 `AddMoveSpeedItemEffect`로 처리한다.
•	이동속도 증가는 영구 적용하지 않으며, 기본 5초에 스폰 시 부여된 1~3 카운트를 곱해 5초, 10초, 15초 동안 적용한다.
•	생명력이 실제로 차감되면 활성 이동속도 효과를 즉시 제거하고 캐릭터 기본 이동속도로 복원한다.
•	효과 지속 중 `PlayerBuffVisualFeedback`으로 캐릭터 점멸을 표시한다.
•	활성 이동속도 버프는 현재 퍼센트 합산 방식으로 계산한다.
•	Max Life 증가는 `AddMaxLifeItemEffect`로 처리하며, 현재 런에서 최대 +1까지만 허용한다.
•	Heart Pack은 3회, Winged Heart는 5회 통과 후 획득한다.
•	Enemy 일시 정지 아이템은 필요성이 낮아 기획 범위에서 제외한다.
•	무적, Enemy 감속 등 추가 효과와 세부 중첩 제한은 추후 레벨 디자인 및 밸런스 단계에서 검토한다.
________________________________________
PART 14
수집형 아이템
목표
장기 저장되는 수집 데이터를 구현한다.
작업
•	ItemCollectionManager
•	CollectionData
•	고유 수집 아이템
•	수량형 수집 아이템
•	업적 진행도 API
•	패시브 강화 연동 포인트
완료 조건
•	획득 데이터가 런 종료 후에도 유지됨
•	중복 아이템 정책 작동
•	저장 서비스와 분리됨
________________________________________
PART 15
아이템 스폰
목표
셀 내부에 아이템을 안전하게 배치한다.
작업
•	ItemSpawnManager
•	셀 기반 스폰
•	시작 셀 제외
•	엘리베이터 셀 제외
•	적 중복 위치 방지
•	층별 등장 조건
•	등장 가중치
•	실행별 랜덤 시드
완료 조건
•	도달 가능한 셀에만 아이템 생성
•	한 셀에 기본 1개
•	페이지 전환 시 아이템 정리
•	실행마다 동일한 배치가 반복되지 않음
현행 메모
•	현재 `ItemSpawner`는 `randomizeSeedOnStart`가 켜져 있으면 실행마다 `runtimeSeed`를 새로 만든다.
•	페이지별 난수는 `runtimeSeed`와 page index를 섞어 생성한다.
•	재현 테스트가 필요하면 `randomizeSeedOnStart`를 끄고 고정 `randomSeed`를 사용한다.
________________________________________
PART 16
로컬 저장 서비스
목표
뒤끝 연결 전 로컬 저장 구조를 만든다.
작업
•	IDataSaveService
•	ILeaderboardService
•	IAchievementService
•	IInventoryService
•	LocalDataSaveService
•	LocalLeaderboardService
•	LocalAchievementService
•	LocalInventoryService
•	데이터 버전
완료 조건
•	최고 층 저장
•	최고 점수 저장
•	수집형 아이템 저장
•	선택 캐릭터와 게임 모드 저장
•	캐릭터별 레벨, 경험치, 보유 및 장착 상태 저장
•	게임 코드가 구체 저장 클래스를 직접 호출하지 않음
구현 상태
•	프로필 재화와 수집/강화 데이터의 로컬 저장 기반은 구현됨
•	캐릭터별 레벨/경험치와 선택/보유/장착 상태의 영구 저장 통합은 미완료
•	다음 작업에서 저장 데이터 버전과 기존 로컬 데이터 마이그레이션을 포함해 우선 처리
________________________________________
PART 17
뒤끝 서버 연결 준비
목표
SDK 연결 전 어댑터 위치와 데이터 흐름을 준비한다.
작업
•	Backend 서비스 클래스 뼈대
•	서버 호출 인터페이스
•	실패 처리
•	보류 데이터
•	오프라인 동기화 큐
•	로그인 전 Guest 모드
아직 하지 않을 것
•	실제 뒤끝 콘솔 설정
•	실제 서버 테이블 생성
•	실제 SDK 로그인 호출
완료 조건
•	로컬 구현체를 뒤끝 구현체로 교체할 수 있는 구조
•	Gameplay 코드 수정 없이 서비스 교체 가능
________________________________________
PART 18
최종 통합 테스트
테스트 흐름
1.	Loading 진입 및 로딩바 미표시 확인
2.	Title 진입 및 로딩 진행률 확인
3.	100% 후 점멸 `TOUCH` 입력으로 Lobby 진입
4.	Lobby 배경 및 캐릭터 UI 확인
5.	캐릭터 선택
6.	모드 선택
7.	InGame 시작
8.	좌우 이동
9.	아이템 여러 번 통과
10.	아이템 획득
11.	적 충돌
12.	리스폰
13.	10층 이상 진행
14.	페이지 전환
15.	스킬 효과
16.	수집 아이템 저장
17.	Game Over
18.	결과창 표시
19.	광고보기 선택 시 보상형 광고 시청 후 부활
20.	확인 선택 시 최고 층과 점수 저장
21.	Lobby 복귀
22.	저장 데이터 확인
완료 조건
•	컴파일 에러 없음
•	Missing Reference 없음
•	NullReferenceException 없음
•	Android 입력 정상
•	페이지 전환 후 아이템과 적 상태 정상
•	저장 데이터 정상
