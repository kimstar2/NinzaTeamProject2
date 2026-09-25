# RoundMaker 변경 사항

## 사용 방법

전투 노드에 들어갈 때 `roundMaker.MakeRound(players, enemies)`를 호출합니다.
플레이어는 현재 씬의 Tanker / Dealer / Healer / Mage 슬롯에 직업별로 연결되므로, 직업당 한 명씩 최대 네 명을 전달합니다. 적 데이터는 네 개를 넘겨도 됩니다. 코스트 오름차순으로 먼저 네 명까지 배치하고 나머지는 대기시킵니다. 같은 EnemyDataSO를 여러 번 넣을 수 있습니다.

사망한 적의 사망 주사위와 UI·주사위 슬롯의 퇴장 모션이 완료되고 현재 전투도 끝나면, RoundMaker가 빈 슬롯에 다음 데이터를 넣고 `EnemySelector.Init()`을 호출합니다. UI와 주사위는 생존 유닛 뒤로 입장합니다. 실제 적 유닛의 DefaultPosition도 UI 판넬 순서에 해당하는 월드 자리로 옮겨, 살아 있는 적은 빈자리를 메우고 새 적은 마지막 자리에 들어옵니다. `ReMake()`를 반복 호출해도 생존 유닛을 중복 배치하지 않습니다.

퇴장이 끝난 뒤 새 유닛이 들어올 때까지의 시간은 RealScene의 RoundMaker 컴포넌트에서 `Reinforcement Delay`(초)로 조절합니다. 현재 값은 0.75초입니다. 새 적이 입장하면 완료되지 않은 그 적의 주사위만 첫 굴림을 시작합니다. 사망한 슬롯은 사망 주사위가 완료될 때까지, 새 적은 첫 굴림이 완료될 때까지 전투 시작을 기다립니다.

다음 노드도 `MakeRound`로 시작합니다. 같은 데이터의 살아 있는 플레이어는 현재 체력을 유지합니다. 다른 플레이어 데이터 또는 사망한 플레이어를 다시 전달하면 해당 슬롯을 초기 체력으로 초기화합니다. 죽은 플레이어를 계속 제외하려면 입력 배열에서도 제외하면 됩니다.

대기 적과 전투 적이 모두 없어지고 마지막 퇴장까지 끝나면 `IsRoundClear`가 true가 되고 `onRoundClear`가 한 번 호출됩니다. 노드 완료 처리는 이 UnityEvent에 연결할 수 있습니다.

## 문제 원인

- 기존 RoundMaker는 사망 시 데이터 목록만 바꾸고 실제 증원 배치를 실행하지 않았습니다.
- ReMake는 생존자까지 다시 배치하려 했고, 빈 슬롯이 없으면 First()에서 예외가 발생했습니다.
- 동일 EnemyDataSO를 공유하는 여러 적에 에셋 사망 이벤트를 중복 등록했습니다.
- 퇴장 후 데이터와 removing 상태, 체력 모듈의 사망 상태, 사망 주사위 완료 상태가 남았습니다.
- UI만 다시 켜도 별도로 배치된 유닛과 주사위 슬롯은 복원되지 않았습니다.
- 첫 수정에서 실제 유닛의 부모에 TweenLayoutGroup이 있다고 잘못 가정했습니다. RealScene은 유닛과 주사위가 다른 계층에 있어, Test → HideFromBattle에서 NullReferenceException이 발생했습니다. 각 Selector에 DiceLayoutGroup과 DiceLayoutTarget을 직렬화 참조로 연결하고, 유닛은 MyAgent로 직접 활성화하도록 수정했습니다.
- 주사위용 TweenLayoutGroup 두 개가 다른 계층의 UI VerticalLayoutGroup을 참조해 주사위 좌표가 재정렬되지 않았습니다. RealScene에서 UI 레이아웃은 해당 UI 그룹에, 주사위 레이아웃은 저장된 월드 슬롯 좌표에 연결했습니다. 주사위 항목에 잘못 연결된 UI CanvasGroup도 제거했습니다.
- 새 적의 실제 GameObject는 재사용한 Selector의 원래 DefaultPosition으로 돌아가, 맨 아래로 들어온 UI 판넬과 다른 높이에 나타났습니다. RoundMaker가 시작 시 저장한 네 월드 자리와 UI 활성 순서를 연결해, 생존자와 신규 적의 DefaultPosition을 함께 재배치합니다.

## 변경 파일

아래 스크립트 경로는 `Assets/Members/KJY/01.Scripts/` 기준입니다.

| 파일 | 변경 내용 |
| --- | --- |
| Dice/Battle/RoundMaker.cs | 데이터 검증, 코스트 정렬, 최대 네 슬롯과 대기열 관리, 두 레이아웃의 퇴장 완료 감지, 전투 종료 후 자동 증원, 라운드 완료 이벤트. UI 활성 순서에 맞춰 적 유닛의 월드 자리를 재배치합니다. 에셋별 사망 구독과 사용하지 않는 목록을 제거했습니다. |
| UI/TweenLayoutGroup.cs | 퇴장 완료 시 실제 비활성화와 OnRemoved 알림. Hide/Add로 슬롯을 재사용하고 입장 순서, 크기, 투명도, 클릭 가능 상태와 위치를 복원합니다. 현재 활성 순서를 RoundMaker에 제공합니다. 기존 TweenSequencer 설정을 사용합니다. |
| Agent/AbstractSelector.cs | HideFromBattle/EnterBattle에서 UI, 주사위 슬롯, 실제 유닛을 각각 처리합니다. 주사위 레이아웃과 대상은 Inspector에서 연결합니다. 초기 체력 UI도 동기화합니다. |
| Agent/Enemy/EnemySelector.cs | Init에서 체력, 선택, 사망·주사위 상태를 복원하고 재입장합니다. ClearData로 슬롯을 비웁니다. 데이터 이벤트 구독 해제 누락도 수정했습니다. |
| Agent/Player/PlayerSelector.cs | 직업별 새 데이터 적용과 Init, 파티 재입장, 생존자 체력 유지, 잠금 해제. |
| Agent/AgentAnim.cs | 새 데이터의 컨트롤러를 적용할 때 애니메이터를 초기 상태로 돌립니다. |
| Agent/Enemy/Enemy.cs | 기존 EventChannel의 생존 상태 알림을 받아 onInit UnityEvent를 실행합니다. |
| Agent/Enemy/Dice/EnemyDice.cs | 재사용 시 사망 주사위 완료 플래그, 회전 중 상태, 크기, 색, 잠금을 초기화합니다. |
| Agent/Enemy/Dice/EnemyDiceInventory.cs | 재사용 시 사망 주사위 수신 플래그와 저장된 주사위를 초기화합니다. 비활성 유닛은 Apply하지 않습니다. |
| Agent/Enemy/Dice/EnemyDiceRollManager.cs | 증원 슬롯은 다시 완료 검사에 포함하고 빈 슬롯은 제외합니다. 첫 굴림 대기와 굴림 중 상태를 구분하며, 증원 시 새 슬롯의 주사위만 굴립니다. |
| Events/Dice/Agent/Enemy/OnEnemyRollRaise.cs | 기존 전체 적 굴림과 증원 슬롯만 굴리는 요청을 구분합니다. |
| Agent/Enemy/Dice/EnemyDiceDataBinder.cs | 재사용할 때 이전 적의 사망 주사위 스킬 표시 데이터를 비웁니다. |
| Agent/Enemy/Dice/EnemyDiceDataReceiver.cs | 생존 상태 알림을 사망 연출로 처리하지 않도록 수정했습니다. |
| Agent/Player/Dice/PlayerDice.cs | 새 플레이어 데이터 수신 시 사망 상태와 잠금을 해제합니다. |
| Agent/Player/Dice/PlayerDiceRollManager.cs | 다시 참가하는 플레이어를 주사위 완료 검사에 복귀시킵니다. |
| Agent/Player/Dice/PlayerDiceInventory.cs | 비활성 슬롯에는 지연 Apply를 예약하지 않습니다. |
| Agent/Player/Dice/PlayerDiceDataBinder.cs | 새 플레이어 데이터를 스킬 UI 바인더에도 전달합니다. |
| Dice/Battle/DiceBattleManager.cs | 현재 전투 여부 공개와 이전 선택·연결 정리 메서드 추가. |
| Module/Util/WaitOnPlay.cs | 비활성화 시 이전 유닛의 지연 연출을 취소합니다. |
| Util/MaterialEffect.cs | 비활성화 시 효과를 정지하고 효과 값을 즉시 복원하는 메서드를 추가했습니다. |

씬·프리팹 변경:

- `Assets/Members/KJY/02.RealScene/RealScene.unity`: RoundMaker에 기존 적 UI 레이아웃과 주사위 레이아웃을 연결했습니다. 플레이어 4개와 적 4개의 Selector에도 각자의 DiceLayoutGroup과 DiceLayoutTarget을 연결했습니다. UI 레이아웃은 자기 UI 그룹을 사용하고, 주사위 레이아웃은 월드 슬롯 위치를 사용하도록 참조를 바로잡았습니다. 수동으로 다시 연결할 필요는 없습니다.
- `Assets/Members/KJY/00.GameModule/PlayPrefabs/Enemy.prefab`: Enemy.onInit에 기존 사라짐 효과를 0으로 되돌리는 호출을 연결했습니다.

새 관리 클래스나 전역 검색을 추가하지 않았습니다. 기존 ModuleOwner, 직렬화 참조, EventChannel, UnityEvent, TweenSequencer 구성을 사용합니다.

## 검증

Unity 6000.3.11f1의 프로젝트 컴파일 설정으로 컴파일에 성공했습니다. 이후 실제 씬 검증에서 시작 지연 굴림, 증원 대기, 새 적 주사위의 굴림 완료 및 실제 면과 UI 데이터 일치를 포함한 21개 항목을 통과했습니다.

첫 검증에서는 유닛을 주사위 슬롯 밑에 둔 임시 구성을 사용해 실제 씬의 계층 차이를 놓쳤습니다. 이를 보완해 노드 씬과 RealScene, 참조하는 프리팹·데이터를 별도 임시 프로젝트로 복사하고, 수정 코드를 연결해 Play Mode에서 실행했습니다. 저장된 UnityEvent와 실제 TweenSequencer를 사용하여 씬 전환, Test 시작, 네 적 배치, 플레이어 주사위 코스트 순서, 사망 후 남은 적 주사위의 빈자리 이동과 새 주사위의 마지막 칸 배치, 생존 적 GameObject의 월드 자리 이동과 새 적의 마지막 자리 입장, 같은 슬롯의 두 차례 사망·재입장, Test 재실행 등 15개 항목을 통과했습니다. 이 실행 과정에서 게임 코드 오류는 발생하지 않았습니다.

별도의 회귀 검증도 유닛과 주사위 계층을 분리하도록 수정한 뒤 31개 항목을 다시 통과했습니다. 코스트 정렬, 같은 EnemyDataSO의 중복 사용, 두 슬롯의 동시 퇴장, 전투 종료까지 교체 지연, 라운드 완료 1회 호출, 다음 노드의 플레이어 체력 유지 및 빈 슬롯 제외를 확인했습니다.

복사 프로젝트 검증은 화면 없이 실행했으므로 최종 연출 화면을 육안 확인한 것은 아닙니다. 원본 에디터의 작업 중인 씬은 직접 조작하지 않았습니다. 실행 확인은 수정된 RealScene의 Play Mode에서 RoundMaker 컴포넌트의 Test 메뉴로 시작할 수 있습니다. 이후 적 처치와 사망 주사위 처리를 진행하면 자동으로 대기 유닛이 입장합니다.
