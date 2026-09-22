# 추가 스킬 설정

## 어디서 바꾸면 됨?

1. DiceInventory의 **Default Dice Data List**에 연결된 기본 목록에서 여섯 면을 확인함.
2. 각 면의 **Skill Data**가 현재 실행할 스킬 데이터임.
3. 추가 스킬 데이터와 프리팹은 `00.GameModule/SkillData/Skills`의 각 스킬 폴더에 있음.
4. 스킬 프리팹 루트의 **Skill Anim Hash**로 첫 번째 / 두 번째 모션 선택. `Skill01 hash`, `Skill02 hash` 쓰면 됨.

현재 모든 캐릭터는 기본 주사위 목록을 런타임에 복제해서 사용함.
역할에 따른 스킬 선택은 아직 적용하지 않은 상태임.

## 스킬 쪽에서 만질 값

- `MeleeStrikeSkill`: Damage. 근접 타격 18, 강한 타격 28. 같은 동작 방식이라 프리팹 설정을 나눠둠.
- 근접 스킬 공통: **Approach Target** 켜짐. **Attack Distance**가 대상 앞에서 멈추는 거리, ActionSeq / ReturnSeq가 이동과 복귀 연출.
- `ArrowShotSkill`: Damage, Arrow Renderer, ProjectileSeq. 화살 사격 16, 강한 사격 26. 화살은 스킬 프리팹 안에 저장돼 있고 타격 프레임에 켜짐.
- `HolyHealSkill`: Heal 20, Heal Particle. 적한테 피해 주는 대신 자기 체력을 최대치까지만 채움. 죽은 캐릭터 부활은 안 함.
- 마법들은 기존 각 스킬 클래스에서 수정. 피해는 기존처럼 BaseDamage를 더함. 회복은 Heal 값만 사용함.

## 배틀 흐름

DiceInventory가 초기화될 때 defaultDiceDataList를 복제함.
주사위 굴리기가 끝나면 나온 면의 DiceData를 런타임 목록에서 찾아 저장함.
기존 ActionCommand → SkillExecutor → SkillLogicExecutor → 각 스킬 순서로 실행됨.
런타임 목록은 캐릭터마다 복제함. 각 면의 DiceData와 SkillData는 기존 에셋 참조를 사용함.
기존 SetDiceData에서 Front 외의 면이 자기 자신을 다시 넣던 부분도 수정해둠.

타격은 클립의 AnimOnAttack, 복귀는 AnimFinishedEnd 이벤트 기준임.
BaseAgentCon의 두 공격 → Idle 전환은 Exit Time 1로 맞춰서 모션 끝까지 재생함.
Enemy03 도끼병의 두 공격 클립에는 빠져 있던 AnimOnAttack을 0.4초에 넣어둠.
스킬 실행 중 중복 요청은 막고, 복귀와 이펙트가 끝난 뒤 다음 커맨드를 한 번만 진행함.
대기와 발사체 이동 완료 확인은 UniTask를 사용함.
스킬이 비어 있거나 바로 취소돼도 ActionCommand가 기다릴 Task를 먼저 잡아서 다음 행동으로 넘어감.
