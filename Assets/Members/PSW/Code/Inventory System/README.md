# 전투 보상 인벤토리

`PSW/Scene/KJY/KJY Scene.unity`에 연결되어 있습니다. KJY 코드 변경은 없습니다.

## 흐름

`OnEnemyDiceDataBind(DeadRoll)` → `BattleRewardInventory` → LYW `Inventory.AddFragment()` → `BackpackInventoryView` → LYW `FragmentSetter`.

- 일반 굴림은 저장하지 않습니다. 사망 굴림의 중복 방지는 기존 `EnemyDiceInventory`가 담당합니다.
- 같은 면을 여러 번 획득해도 별도 조각으로 보관합니다.
- `RewardDiceFragmentSO`는 기존 `DiceFragmentSO`를 상속하며 원본 `DiceDataSO`, 보상 시점의 `Level`, 표시 이미지를 보관합니다. 원본 SO 에셋은 수정하지 않습니다.
- 배낭이 닫힌 상태에도 보상을 받고, 열린 상태에는 목록을 즉시 갱신합니다. 7열 목록을 세로로 스크롤할 수 있습니다.
- 전투 배낭에서는 재련용 조각 선택을 비활성화했습니다. 기존 LYW 화면은 기본값인 선택 가능 상태를 유지합니다.
- 획득한 아이콘을 왼쪽 클릭하면 전투 정보창과 같은 레이아웃으로 스킬 이름, 아이콘, 획득 당시 레벨의 설명을 표시합니다. 다시 클릭하거나 스크롤하거나 배낭을 닫으면 정보창이 닫힙니다. 빈 슬롯은 클릭 대상으로 만들지 않습니다.
- `OnEnemyDataChanged`에서 적의 공격 유형을 기억하고 보상에 해당 스킬을 저장하므로, 적이 사라지거나 씬이 바뀌어도 같은 설명을 볼 수 있습니다. 유형 정보가 없는 이전 보상은 원본 면의 사용 가능한 스킬 설명을 표시합니다.
- 기본 50칸을 표시합니다. 씬 루트 `Battle Reward Inventory`의 `Max Slots`에서 보관 한도를 조절합니다. 화면도 같은 값을 사용하며 7열의 마지막 행을 채우기 위해 추가 칸을 만들지 않습니다.
- `AddFragment()`는 성공 여부를 반환합니다. 가득 찬 상태의 추가 보상은 저장하지 않고 경고를 남깁니다. 한도를 기존 보유량보다 줄여도 이미 보유한 면은 삭제하거나 숨기지 않습니다.

## 씬 이동 및 다른 기능에서 사용

씬 루트의 `Battle Reward Inventory`가 `DontDestroyOnLoad`로 유지되며 기존 `ServiceLocator`에 `Inventory`로 등록됩니다. 전투 씬 재진입 시 새 저장소는 제거되고 기존 목록을 사용합니다. 등록된 저장소가 없는 시작 씬에서는 이 컴포넌트를 명시적으로 배치해야 합니다.

사건/재련 기능은 `ServiceLocator.TryGet<Inventory>(out var inventory)`로 같은 목록을 가져와 `AddFragment()` / `RemoveFragment()`를 사용하면 됩니다. 목록을 직접 변경하면 `Changed` 알림이 발생하지 않습니다.

다른 씬의 기존 `FragmentSetter`에 공유 목록을 연결하려면 `SetInventory(inventory)`를 호출합니다. `BackpackInventoryView`를 화면에 배치하고 해당 `FragmentSetter`를 연결해도 됩니다. 다른 씬의 사건/재련 규칙 자체는 이번 변경에 포함하지 않았습니다.

획득 면의 원본 정보는 `fragment is RewardDiceFragmentSO reward` 확인 후 `reward.DiceData` / `reward.Level`로 접근합니다. 기존 이미지 전용 조각은 이 타입이 아닐 수 있습니다.

유지 범위는 실행 중 씬 이동입니다. 게임 종료 후 파일 저장/복원은 포함하지 않습니다. 새 게임 시작 시에는 기존 저장소 오브젝트를 파괴하고 새 세션의 저장소를 준비해야 합니다.

## Play Mode 확인

1. 적의 일반 굴림에는 목록이 늘지 않는지 확인합니다.
2. 배낭을 닫고 적의 최후의 주사위를 굴린 뒤 열어 결과 면이 하나 추가되는지 확인합니다.
3. 배낭을 열어 둔 상태에서도 추가 보상이 바로 보이는지 확인합니다.
4. 같은 면을 두 번 받으면 두 칸이 표시되는지, 많은 면은 스크롤로 확인 가능한지 확인합니다.
5. 다른 씬으로 이동한 뒤 전투 씬에 돌아와 목록 유지와 단일 보상 수신을 확인합니다.
