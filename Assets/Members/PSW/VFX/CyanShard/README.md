# Cyan Shard — 하늘색 빛 조각

URP 17 Shader Graph. 흰 청색 중심, 하늘색 후광, 양끝이 뾰족한 가로 빛 조각입니다.
Blackboard 속성 + UV + Vertex Color → Custom Function(그래프 내부 수식) → Base Color / Alpha 구조입니다. 별도 HLSL 파일이나 빛 텍스처가 필요하지 않습니다.

## 스프라이트
SpriteRenderer의 Sprite에 CyanShard_Quad.png, Material에 CyanShard_Sprite.mat을 넣으세요.
PNG는 흰색 Full Rect UV 캔버스이며, 실제 빛 조각은 머티리얼이 생성합니다.
Draw Mode: Simple. 기본 1×1 유닛. Transform 회전/크기로 방향과 크기를 조절합니다.
SpriteRenderer Color의 색/알파가 적용됩니다.
UV 0~1을 사용하므로 이 PNG는 Sprite Atlas에 넣지 마세요. 임의 Tight Mesh나 잘린 스프라이트는 후광/형태가 잘릴 수 있습니다.

## 파티클 (일반 Particle System)
Renderer > Material에 CyanShard_Particle.mat을 지정하세요. Billboard로 사용하며 Texture Sheet Animation은 끄세요.
Vertex Streams: Position, Color, UV (기본 스트림).
Start Color / Color over Lifetime의 색과 알파로 색상과 페이드 아웃을 조절합니다.
예시: Start Size 0.4~1.2, Start Rotation 0~180도, Lifetime 0.2~0.6초. 이동은 Velocity over Lifetime으로 설정합니다.
기본 조각은 로컬 X 방향이며, Stretched Billboard는 크기와 축을 추가 변형합니다.

## 머티리얼 속성
- Core Color / Core Intensity: 중심 HDR 색 / 밝기.
- Glow Color / Glow Strength: 후광 HDR 색 / 밝기.
- Shard Width: 중심 두께. Glow Width: 후광 폭.
- Shard Length: UV 기준 반길이. Opacity: 전체 알파.

스프라이트는 Sprite Unlit, 파티클은 URP Unlit 타깃입니다. 두 그래프 모두 Transparent / Additive, 양면, ZWrite Off입니다.
후광은 셰이더 자체에 있어 Bloom 없이도 보입니다. 기존 HDR/Bloom이 켜져 있으면 더 밝게 번집니다.
씬, 카메라, 볼륨, 기존 머티리얼은 수정하지 않았습니다.
