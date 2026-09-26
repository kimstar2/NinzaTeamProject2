# 역할별 스킬 설정

## 같은 면을 역할마다 다르게 사용하기

`AgentAttackType`은 Archer=0, Melee=1, Magic=2, Healer=3이다. 기존 저장값 0~2는 그대로 두고 힐러를 끝에 추가했다. `PlayerType`은 플레이어 슬롯 구분이고, 실제 스킬 선택은 `AgentDataSO.AttackType`으로 한다.

각 DiceDataSO의 **Skill Data Structs**에 네 역할을 한 번씩 넣는다. 같은 SkillData를 모든 역할에 복사하지 않는다. 면의 등급은 공유하지만 사용 스킬은 다르다.

| 면 예시 | Archer | Melee | Magic | Healer |
| --- | --- | --- | --- | --- |
| ArrowShot / MeleeStrike / ArcaneBurst / HolyHeal | 화살 사격 | 근접 타격 | 마력 폭발 | 자기 회복 |
| Fireball / TwinSlash / RadiantStrike | 강한 사격 | 연속 베기 | 화염탄 | 빛의 응징 |
| NecroRain / Renew | 연속 사격 | 연속 베기 | 강령비 | 회복 맥동 |
| DoubleShot / FrostBurst | 연속 사격 | 연속 베기 | 얼음 폭발 | 회복 맥동 |
| PowerShot | 강한 사격 | 연속 베기 | 얼음 폭발 | 회복 맥동 |
| HeavyStrike / HolyStrike / ArcaneVolley | 화염 화살 | 강한 타격 | 마력 연타 | 성스러운 일격 |
| FlameArrow / GreaterHeal | 화염 화살 | 강한 타격 | 마력 연타 | 강한 회복 |

적에게서 얻은 면도 획득한 캐릭터의 AttackType으로 다시 선택한다. 화면의 제목·설명·아이콘과 굴리는 여섯 면도 같은 역할을 기준으로 표시한다. **SkillDataSO.Icon**에 실제 스킬 이미지를 넣는다. DiceDataSO.Icon은 역할이 정해지지 않은 면 자체의 기본 그림이다.

## 이번에 추가한 스킬 8개

레벨 1 기준이며, 실제 값은 SkillDataSO의 ApplyStats.Value × 캐릭터 레벨이다.

| 역할 | 스킬 / 폴더 | 동작 | 기존 스크립트에서 조절할 값 |
| --- | --- | --- | --- |
| Archer | 연속 사격 / DoubleShot | 화살 두 발, 각각 12 피해 | ArrowShotSkill의 Shot Count=2, Shot Interval |
| Archer | 화염 화살 / FlameArrow | 불꽃 궤적과 명중 파편, 30 피해 | ArrowShotSkill의 releaseParticle, impactParticle, arrowTrail |
| Melee | 연속 베기 / TwinSlash | 같은 적을 두 번, 각각 13 피해 | MeleeStrikeSkill의 Hit Count=2, Hit Interval |
| Magic | 얼음 폭발 / FrostBurst | 푸른 파편을 터뜨려 26 피해 | ArcaneBurstSkill의 Cast Particle, Impact Particle |
| Magic | 마력 연타 / ArcaneVolley | 보라색 폭발 세 번, 각각 11 피해 | NecroRainSkill의 Hit Count=3, Hit Delay, Hit Interval |
| Healer | 회복 맥동 / Renew | 초록빛 맥동 세 번, 매번 자신을 9 회복 | HolyHealSkill의 Pulse Count=3, Pulse Interval |
| Healer | 강한 회복 / GreaterHeal | 금빛 입자로 자신을 34 회복 | HolyHealSkill의 Heal Particle |
| Healer | 빛의 응징 / RadiantStrike | 적에게 20 피해, 자신을 8 회복 | HolyStrikeSkill의 Hit Delay, healParticle + ApplyStats의 Damage/Heal |

화염 화살에 지속 화상, 얼음 폭발에 빙결은 없다. 설명에 적힌 직접 피해만 적용한다. 회복 맥동도 한 번의 행동 안에서 세 번 회복하며 턴 지속 효과는 아니다. 현재 게임은 적을 대상으로 선택하므로 회복은 자기 자신에게 적용한다. 최대 체력 초과나 부활은 하지 않는다.

새 런타임 스킬 클래스를 늘리지 않고 기존 스크립트와 프리팹 구성을 재사용했다. 반복 횟수 기본값은 1이므로 기존 사격·근접·회복의 동작은 유지된다. 중간에 대상이 죽으면 남은 공격을 중단한다.

## 어디서 수정하면 됨?

1. `00.GameModule/DiceData/SkillFaces`의 면에서 역할별 Skill Data와 공통 Dice Grade를 확인한다.
2. `00.GameModule/SkillData/Skills/<스킬>`의 Skill Data에서 이름·설명·Icon·ApplyStats를 조절한다.
3. 같은 폴더의 Skill.prefab 안에서 반복 횟수·시간·파티클을 조절한다.
4. 프리팹 루트의 Skill Anim Hash로 Skill01/Skill02 모션을 선택한다.
5. `EnemyData/Characters`의 Dice Data List가 해당 적이 굴리는 여섯 면이다.

피해·회복은 ApplyStats가 기준이다. 기존 BaseDamage/SkillLevel은 현재 실행 계산에 쓰이지 않는다. 설명의 `{0}`, `{1}`은 ApplyStats 순서대로 표시된다. 빛의 응징은 Damage 다음 Heal이다.

## 실행 흐름

`ActionCommand → SkillExecutor → DiceDataSO.GetSkillDataStruct(AttackType) → SkillLogicExecutor → 스킬` 순서다. 애니메이션의 AnimOnAttack이 첫 타격을 시작하고, 추가 타격은 설정한 간격을 따른다. AnimFinishedEnd가 와도 남은 타격을 기다린 후 복귀하며, 파티클 풀 반환까지 끝나야 한 번만 종료한다.
