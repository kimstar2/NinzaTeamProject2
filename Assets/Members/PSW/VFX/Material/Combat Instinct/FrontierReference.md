# Frontier reference

Source: https://github.com/leehosong05035/Frontier
Commit: `508ada67c2db8b7c40be8a3a340fd18823105fa8`

- `Assets/02.Characters/SwordMaster/Script/SMPlayerSkill.cs`: preparation, short dash, delayed final slash.
- `Assets/05. Materials/ParticleMat/Shader&Particle&Material/LSSpaceSlice/ScreenSplitSlashParticle.shader`: distance-based slash mask, separate white core and cyan glow.
- `ScreenSplitSlashParticle.mat`: cyan/white palette and strong luminous edge.
- `SwordAttack.prefab` and `SwordAttack_Last.prefab`: short-lived layered particle strikes.

InstinctEnergy.shader adapts the mask/glow approach for this project's URP 2D renderer. It uses procedural curved blades, rings and sparks; it does not require a camera sorting-layer texture or imported texture assets. The executor prefab owns all particle modules/material references; runtime only positions and plays the saved effects.

The reference project's MIT license is retained in Frontier-LICENSE.txt.
