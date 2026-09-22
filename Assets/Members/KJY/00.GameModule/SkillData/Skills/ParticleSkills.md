# 파티클 스킬 수정할 때

## 어디 보면 됨?

`01.Scripts/Agent/SkillSystem/DiceSkills`에 스킬별 클래스가 있음.
이동, 복귀, 풀 반환, UniTask 취소는 `Skill/AbstractParticleSkill.cs`에 모아둠.
각 스킬에서 볼 건 `AttackAsync`랑 `ApplyStat`임. 기다리는 건 전부 UniTask고 코루틴은 안 씀.
지금은 순회가 단순해서 foreach / for만 사용함. 나중에 쿼리 필요하면 ZLinq의 `AsValueEnumerable()` 쓰면 됨.

| 스킬 | 연결된 기본 주사위 면 | 타격 방식 | 기본 피해 |
| --- | --- | --- | --- |
| Fireball | Left — Dice data 2 | 불덩이 도착하면 1번 | 24 + BaseDamage |
| NecroRain | Right — Dice data 3 | 3번 떨어뜨림 | 매번 8 + BaseDamage |
| HolyStrike | Top — Dice data 4 | 빛 기둥 내려온 다음 1번 | 32 + BaseDamage |
| ArcaneBurst | Bottom — Dice data 5 | 공격 이벤트에서 바로 1번 | 20 + BaseDamage |

위 표는 DiceInventory의 `defaultDiceDataList`에 연결된 기본 주사위임.
캐릭터마다 이 기본 목록을 런타임에 복제해서 사용함.
추가로 만든 근접, 화살, 회복 스킬의 설정은 `RoleSkills.md` 보면 됨.
강령비는 광역이 아니라 전달받은 대상 하나를 때림. 중간에 죽으면 남은 타격은 안 들어감.

## Inspector 연결

각 폴더의 `... Skill.prefab` 열고 스킬 이름 붙은 자식 선택하면 됨.

1. **ActionSeq → On Seq Complete → 스킬.PlaySkillAnim**
2. **스킬 → On Cast → 스킬.PlayCastParticle → Executor.PlayAnim**
   - Fireball은 시전 파티클 없이 PlayAnim만 연결함. 불덩이는 공격 이벤트에서 나감.
3. 애니메이션의 **AnimOnAttack → Executor → 스킬.Attack → AttackAsync**
4. 애니메이션의 **AnimFinishedEnd → Executor → 스킬.AnimEnd**
5. 공격 처리가 남았으면 기다림. 다 끝나면 **On Anim End → ReturnSeq.Sequence**
6. **ReturnSeq → On Seq Complete → 스킬.HandleReturnEnd**
7. 남은 파티클까지 풀에 돌아가면 부모에서 SkillFinished랑 Remove 호출함.

`Executor.onAnimFinished`에는 ApplyStat을 따로 연결할 필요 없음. 각 스킬에서 타이밍 맞춰 적용함.
ReturnSeq에서 Executor.SkillFinished로 바로 넘기면 남은 타격/이펙트보다 먼저 다음 턴이 갈 수 있으니 HandleReturnEnd에 연결해두면 됨.

## 자주 바꿀 값

- 공통: `Advance Distance`, `Effect Offset`, ActionSeq / ReturnSeq의 시간과 Ease.
- ArcaneBurst: `Damage`, `Cast Particle`, `Impact Particle`.
- Fireball: `Damage`, ProjectileSeq의 이동 시간/Ease, ProjectileParticle 크기. TargetPosition은 대상 위치를 넣는 용도임.
- NecroRain: `Damage`, `Hit Count`, `Hit Delay`, `Hit Interval`. Hit Delay는 낙하 시작부터 피해 적용까지, Hit Interval은 그 다음 낙하까지 기다리는 시간임.
- HolyStrike: `Damage`, `Hit Delay`. 빛 기둥 시트가 바닥에 닿는 프레임에 맞추면 됨.

`Animation Timeout`은 애니메이션 끝 이벤트가 안 왔을 때 복귀하는 시간임. 이 시간이 지났다고 없는 타격을 만들어내지는 않음.
스킬 오브젝트가 없어지면 기다리던 UniTask도 취소되고 남은 파티클은 풀로 돌려보냄.

## 이펙트 위치

- 머티리얼: `04.Mat/Skill Mat/<스킬 이름>`
- 풀 파티클: `00.GameModule/Pool/Pooling Item/PoolingParticle/<스킬 이름>`
- 원본 시트: `06.Sprite/Tiny RPG Character Asset Pack 01 v2.0 -Full 22 Characters/Magic(Projectile)`
- Fireball의 날아가는 불덩이는 스킬 프리팹 안의 `ProjectileParticle`임. 앞 4프레임만 반복하다가 도착하면 꺼지고, 뒤 3프레임은 별도 타격 파티클에서 재생함.

풀 파티클 바꾸면 `Pooling List` 등록, PoolItem의 Prefab, 파티클의 Item이 서로 맞는지 보면 됨.
