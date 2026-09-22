# 선택 표시 수정할 때

`Player_container` 프리팹 안의 기존 `OutlineBlinkEffector`에 `UISelectionEffect` 붙여둠.
원래 있던 카드 Outline이랑 `Light_bg`를 씀. 예전 머티리얼 깜빡임은 꺼둠.
Light_bg는 배경 바로 위, 아이콘보다 뒤에 둠. `SelectionLight` 머티리얼이 빛을 부드럽게 퍼뜨림.

- 플레이어 클릭: 테두리랑 빛이 숨 쉬듯이 켜짐. 아직 체인은 안 생김.
- 적 클릭: 체인 연결하고 빛은 약하게 켜둠.
- 같은 플레이어 다시 클릭: 선택이나 연결 취소.
- 다른 플레이어 클릭: 선택 대기 표시만 옮김. 이미 연결한 애는 유지.
- 배틀 시작: 선택 표시랑 체인 정리. 배틀 끝났다고 다시 켜지지는 않음.

## 어디서 바꾸냐면

`UISelectionEffect` 인스펙터:

- `Light Alpha`: 빛 세기. 지금 0.28.
- `Pulse Time`: 맥동 한 방향 시간. 지금 0.65초.
- `Connected Strength`: 연결한 뒤 남겨둘 강조. 지금 0.55.
- `Outline Width`: X는 평소 두께, Y는 선택 두께. 지금 1 / 3.
- `Fade Time`: 켜고 끄는 시간. 지금 0.14초.
- `Idle Color`: 평소 테두리 색.

색은 각 `PlayerSelector`의 `Player Color`, 선은 `Line Color`에서 바꾸면 됨.
RealScene의 네 플레이어에 `PlayerColors` 안의 역할별 Select / LineGradient 에셋 연결해둠.
탱커는 금색, 딜러는 코랄, 힐러는 민트, 마법사는 보라색.

체인 두께는 `LR_For_Copy`의 LineRenderer, 휘는 정도는 같은 오브젝트의 `MonoLineRenderer > Arc Height`.
`ConnectLine` 머티리얼의 `Flow Speed`는 빛 이동 속도, `Flow Strength`는 이동하는 빛 세기.
이동하는 빛이 싫으면 Strength를 0으로. 그라데이션은 그대로 남음.

## 이벤트 연결

PlayerSelector에서 기존 UnityEvent 그대로 씀. 전부 프리팹에 저장돼 있음.

- `On Select` → `UISelectionEffect.ShowSelected`
- `On Un Select` → `UISelectionEffect.Hide`
- `On Set Target` → `UISelectionEffect.ShowConnected`

선택 판정은 PlayerSelector, 체인 정보는 DiceBattleManager, 연출은 UISelectionEffect / MonoLineRenderer가 맡음.
연출 바꿀 때 배틀 쪽은 안 건드려도 됨.
