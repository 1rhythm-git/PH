PH Item System Specification
________________________________________
1. 시스템 목적
아이템 시스템은 플레이어가 각 층의 셀 내부에 배치된 아이템을 통과하여 획득하는 구조이다.
일반적인 접촉 즉시 획득 방식 외에도, 아이템마다 요구되는 통과 횟수가 존재할 수 있다.
플레이어는 좌우 이동만 가능하므로 같은 층에서 아이템을 여러 차례 왕복 통과해 획득 조건을 달성한다.
________________________________________
2. 핵심 용어
Required Pass Count
아이템을 획득하기 위해 필요한 통과 횟수이다.
Current Pass Count
현재까지 유효하게 인정된 통과 횟수이다.
Valid Pass
플레이어가 아이템 판정 영역에 진입하고, 이후 완전히 빠져나온 한 번의 통과이다.
Acquired
필요 통과 횟수를 충족해 아이템 효과가 실행되고 아이템이 제거되거나 수집 처리된 상태이다.
________________________________________
3. 통과 판정 규칙
플레이어가 아이템 콜라이더 내부에 계속 머물러도 통과 횟수는 한 번만 증가해야 한다.
판정 순서:
Outside
→ Enter
→ Count +1
→ Inside
→ Exit
→ Ready For Next Pass
같은 프레임이나 같은 진입 상태에서 중복 카운트하면 안 된다.
________________________________________
4. 통과 방향
기본 아이템은 양방향 통과를 인정한다.
Left To Right = Valid
Right To Left = Valid
확장 가능한 방향 규칙:
Any
LeftToRightOnly
RightToLeftOnly
Alternating
Alternating 아이템은 좌→우와 우→좌를 번갈아 통과해야 유효한 방식으로 확장할 수 있다.
초기 구현에서는 Any만 실제 사용하되 enum 확장 구조를 준비할 수 있다.
________________________________________
5. 획득 처리
다음 조건이 만족되면 아이템을 획득한다.
Current Pass Count >= Required Pass Count
획득 시 처리:
1.	중복 획득 방지 상태 설정
2.	아이템 효과 실행
3.	점수 또는 데이터 반영
4.	획득 연출 실행
5.	아이템 비활성화 또는 풀 반환
6.	필요한 경우 저장 요청
________________________________________
6. 아이템 타입
Score
현재 런의 점수에 즉시 반영된다.
필수 데이터 예시:
•	Base Score
•	Score Multiplier
•	Combo Value
•	Bonus Duration
Skill
획득 시 플레이어 또는 적에게 효과를 적용한다.
필수 데이터 예시:
•	Effect ID
•	Target Type
•	Duration
•	Stack Policy
•	Stack Limit
•	Effect Value
Target Type 예시:
Player
AllEnemies
NearestEnemy
CurrentFloorEnemies
GameState

현재 구현된 Skill 아이템:
•	Red Sneaker
	- EffectKey: add_move_speed_percent
	- EffectValue: 20
	- EffectDurationSeconds: 10
	- 효과: 플레이어 이동속도 20% 증가
•	Winged Shoe
	- EffectKey: add_move_speed_percent
	- EffectValue: 50
	- EffectDurationSeconds: 12
	- 효과: 플레이어 이동속도 50% 증가

이동속도 증가 아이템은 영구 강화가 아니다.
효과 지속시간 동안만 `PlayerMotor`의 이동속도에 반영하고, 시간이 끝나면 자동으로 제거한다.
효과 지속 중에는 `PlayerBuffVisualFeedback`을 통해 캐릭터 점멸을 표시한다.

Collection
장기 저장되는 수집 데이터에 반영한다.
필수 데이터 예시:
•	Collection ID
•	Amount
•	Achievement Progress
•	Passive Upgrade Category
•	Unlock Target
________________________________________
7. 효과 구조
아이템 데이터와 아이템 효과 실행을 분리한다.
권장 구조:
ItemDefinition
    ↓
ItemInstance
    ↓
IItemEffect
    ├── AddScoreEffect
    ├── HealHeartEffect
    ├── AddTimeEffect
    ├── PlayerSpeedEffect
    ├── FreezeEnemiesEffect
    ├── RemoveEnemiesEffect
    └── AddCollectionEffect
ItemDefinition은 데이터만 보유한다.
실제 효과는 IItemEffect 구현체가 담당한다.

현재 구현체:
•	AddScoreItemEffect
•	AddTimeItemEffect
•	HealHeartItemEffect
•	AddMoveSpeedItemEffect

현재 데이터 소스:
•	`Assets/_Project/Data/Tables/Items.csv`
•	`Assets/_Project/Data/Tables/ItemIcons.csv`

현재 `Items.csv` 주요 컬럼:
•	ItemId
•	ServerItemId
•	TableVersion
•	Enabled
•	DisplayName
•	ItemType
•	IconKey
•	MinFloor
•	MaxFloor
•	SpawnWeight
•	RequiredPassCount
•	LifetimeSeconds
•	PassDirection
•	EffectKey
•	EffectValue
•	EffectDurationSeconds
•	AffectsScore
•	AffectsProgression
•	ServerValidated
•	MaxAcquirePerRun
•	Rarity

`LifetimeSeconds`는 아이템이 필드에 남아있는 시간이다.
`EffectDurationSeconds`는 획득 후 적용되는 버프 지속시간이다.
두 값은 서로 다른 목적이므로 같은 값으로 묶지 않는다.
________________________________________
8. 중첩 정책
스킬형 아이템은 다음 중첩 정책을 가질 수 있다.
IgnoreNew
RefreshDuration
AddDuration
StackValue
ReplaceWithStronger
예시:
•	무적 아이템 재획득: 지속 시간 갱신
•	이동 속도 증가: 최대 3중첩
•	적 정지: 남은 시간에 추가
•	약한 버프 획득 후 강한 버프 획득: 강한 효과로 교체

현재 이동속도 증가 구현은 동일 런 안에서 복수 버프가 동시에 존재할 수 있다.
각 버프는 독립적인 만료 시간을 가지며, 활성 버프의 퍼센트 값을 합산해 최종 이동속도 배율을 계산한다.
정식 밸런스 단계에서 Stack Policy와 Stack Limit을 데이터화할 수 있다.
________________________________________
9. 아이템 진행 UI
필요 통과 횟수가 2 이상이면 진행 상태를 표시한다.
초기 구현 권장:
1 / 3
2 / 3
추후 확장:
•	아이콘 테두리 게이지
•	색상 변화
•	흔들림
•	발광 강도
•	단계별 스프라이트
진행 UI는 ItemInstance 내부 로직과 분리한다.

현재 구현:
•	아이템 중앙에 남은 통과 횟수를 숫자로 표시한다.
•	숫자는 흰색 Bold 텍스트를 사용한다.
•	검은 Outline과 Shadow를 사용해 시인성을 보강한다.
•	획득 시 `gain.ogg`, 통과 카운트 시 `pass.ogg` 재생 구조를 사용한다.
________________________________________
10. 스폰 규칙
아이템은 셀 내부에 생성한다.
스폰 데이터는 다음 조건을 가질 수 있다.
•	최소 등장 층
•	최대 등장 층
•	등장 가중치
•	아이템 타입
•	희귀도
•	한 페이지 최대 생성 개수
•	동일 아이템 중복 제한
•	적과 같은 셀 배치 가능 여부
•	엘리베이터 셀 배치 가능 여부
초기 안전 규칙:
•	시작 셀에는 생성하지 않음
•	엘리베이터 셀에는 생성하지 않음
•	적과 동일 위치에 생성하지 않음
•	한 셀에 하나의 아이템만 생성
•	플레이어가 도달할 수 없는 위치에는 생성하지 않음

현재 구현:
•	한 페이지 최대 아이템 수는 `maxItemsPerPage`로 제한한다.
•	좌측 끝 셀과 우측 끝 셀에는 생성하지 않는다.
•	실행마다 `runtimeSeed`를 생성하고, 페이지 인덱스를 섞어 배치 난수를 만든다.
•	`randomizeSeedOnStart`가 켜져 있으면 새 실행마다 배치가 달라진다.
•	`randomizeSeedOnStart`가 꺼져 있으면 `randomSeed` 기반으로 재현 가능한 배치를 만든다.
________________________________________
11. 저장 규칙
Score 아이템:
•	런 종료 결과에 포함
•	개별 아이템 획득 이력은 필수 저장하지 않음
Skill 아이템:
•	기본적으로 런 종료 시 효과 소멸
•	영구 스킬인 경우 Collection 또는 Progress 데이터로 처리
•	이동속도 아이템은 현재 런의 지정 지속시간 동안만 적용하고, 저장 대상이 아니다.
Collection 아이템:
•	획득 즉시 로컬 저장
•	서버 연결 시 동기화 대상
•	고유 아이템은 중복 여부 확인
•	재료형 아이템은 수량 누적
________________________________________
12. 예외 상황
다음 상황을 방어한다.
•	아이템 획득 직후 다시 충돌
•	플레이어가 리스폰하며 아이템 내부에서 생성
•	아이템 효과 실행 중 아이템 오브젝트 파괴
•	페이지 전환 중 통과 판정
•	게임 오버 직전 획득
•	같은 프레임에 여러 아이템 획득
•	수집 데이터 저장 실패
•	Required Pass Count가 0 이하로 설정됨
Required Pass Count가 0 이하이면 런타임에서 최소 1로 보정한다.
________________________________________
13. Inspector 기준
ItemDefinition에서 설정할 항목:
Item ID
Display Name
Description
Icon
Prefab
Item Category
Required Pass Count
Pass Direction
Rarity
Minimum Floor
Spawn Weight
Effect Definitions
Is Persistent
ItemInstance에서 설정할 항목:
Definition
Pass Trigger
Progress View
Acquire Effect Root
________________________________________
14. 테스트 항목
1.	한 번 통과 아이템 획득
2.	세 번 통과 아이템이 정확히 세 번째에 획득
3.	콜라이더 내부 체류 중 중복 증가하지 않음
4.	좌→우 통과 인정
5.	우→좌 통과 인정
6.	획득 후 중복 효과 실행 방지
7.	리스폰 후 정상 판정
8.	페이지 전환 시 남은 아이템 정리
9.	수집형 저장
10.	스킬 중첩 정책 확인
