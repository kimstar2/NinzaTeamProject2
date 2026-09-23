# Arcane Burst — 비전 폭발

KJY의 `MagicSkill`과 같은 `AbstractSkillLogic` / `SkillLogicExecutor` 실행 계약을 사용하는 단일 대상 공격 스킬입니다.

## 실행 흐름

1. 배틀이 전달한 시전자와 대상의 실제 위치로 시전 위치를 정합니다. 플레이어와 적 모두 대상 방향으로 최대 0.6만큼 전진합니다.
2. `ActionSeq` 완료 → 시전 파티클 → 기존 `SkillAnimHash` 애니메이션을 실행합니다.
3. 애니메이션의 `AnimOnAttack` → 대상 위치에 타격 파티클을 재생하고 **20 + 현재 주사위 BaseDamage**를 한 번 적용합니다.
4. `AnimFinishedEnd` → 대기 애니메이션과 `ReturnSeq`로 시전 전 위치에 복귀합니다.
5. 복귀와 파티클 풀 반환이 모두 끝나면 `SkillFinished()`를 한 번 호출하고 스킬 인스턴스를 제거합니다.

스킬 클래스는 `01.Scripts/Agent/SkillSystem/DiceSkills/ArcaneBurstSkill.cs`임. 타격 로직은 여기 보면 되고, 이동·복귀·풀 반환은 `Skill/AbstractParticleSkill.cs`에서 같이 씀. 기다리는 건 UniTask로 바꿨고 코루틴은 안 씀. 자기 자신을 대상으로 받으면 피해 없이 끝남. 애니메이션 끝 이벤트가 3초 안에 안 오면 추가 타격 없이 복귀함.

`ActionSeq.onSeqComplete → PlaySkillAnim`, `onCast → PlayCastParticle / Executor.PlayAnim`, `onAnimEnd → ReturnSeq.Sequence`, `ReturnSeq.onSeqComplete → HandleReturnEnd`를 Inspector에 연결해둠. 자세한 수정 위치는 상위 폴더의 `ParticleSkills.md` 보면 됨.

## 에셋과 연결

- `Arcane Burst Skill.prefab`: Executor, 전용 스킬 클래스, 이동/복귀 시퀀서, 위치 기준점.
- `Arcane Burst Skill Data.asset`: 기존 `DiceDataSO.SkillData`에 할당할 데이터.
- `04.Mat/Skill Mat/ArcaneBurst`: 시전/타격 머티리얼 2개.
- `00.GameModule/Pool/Pooling Item/PoolingParticle/ArcaneBurst`: 파티클 프리팹과 PoolItemSO 각 2개. 기존 `Pooling List`에 등록됨.
- 시트: `06.Sprite/Tiny RPG Character Asset Pack 01 v2.0 -Full 22 Characters/Magic(Projectile)/Wizard_Attack01_Effect.png` (10 × 1).
- 각 파티클은 1개씩 방출하며 시전 0.45초 / 타격 0.65초, 반복 없음, 종료 Callback으로 풀에 반환됩니다. URP 2D Unlit 머티리얼을 사용합니다.

공통 기본 주사위 목록의 **Bottom 면**에 연결되어 있음. DiceInventory는 이 기본 목록을 런타임에 복제해서 사용함. 프리팹의 `damage`, `advanceDistance`, `effectOffset`과 각 파티클 설정을 Inspector에서 조절하면 됨.

공통 Executor는 파괴 시 애니메이션 이벤트 구독을 해제하고, 실행 중에는 중복 요청을 막음. 기존 MagicSkill도 같은 실행 계약을 사용함.
