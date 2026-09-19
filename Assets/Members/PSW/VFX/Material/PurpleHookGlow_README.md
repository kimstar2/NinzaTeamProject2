# PurpleHookGlow

`PurpleHookGlow.mat`을 갈고리 SpriteRenderer의 Material에 지정하세요. 텍스처는 `PurpleHook.png`가 이미 연결되어 있습니다. SpriteRenderer Color는 흰색/알파 1에서 시작하세요.

현재 프로젝트의 URP 2D Renderer에 맞는 **Sprite Unlit Shader Graph**입니다. 조명 없이 보이며, 투명도는 Alpha Blend를 사용합니다. SpriteRenderer의 색상과 알파도 반영합니다.

| 머티리얼 항목 | 기본값 | 효과 |
|---|---:|---|
| Core Color (HDR) | 보라색 | 갈고리 본체 색상 |
| Core Intensity | 1.5 | 본체의 밝기. 1이면 기본 색상 |
| Glow Color (HDR) | 보라색 | 주변 번짐 색상 |
| Glow Strength | 2 | 번짐 강도. 0이면 번짐 없음 |
| Glow Radius (UV) | 0.015 | 번짐 거리. 텍스처 크기 대비 비율이며 0이면 번짐 없음 |
| Glow Softness | 0.8 | 낮추면 넓고 옅은 영역이 살아나고, 높이면 가장자리 가까이 모임 |
| Opacity | 1 | 본체와 번짐 전체 투명도. 0이면 완전히 사라짐 |
| Glow Padding | 0.06 | 메시 안쪽 여백. 높이면 갈고리가 조금 작아지고 번짐 공간이 늘어남 |

추천 시작값: Radius 0.01~0.025, Strength 1~3, Softness 0.7~1.2. 단색 도형으로 보려면 Strength 0, Core Intensity 1로 설정합니다.

## 번짐과 Bloom

번짐은 텍스처 알파를 주변에서 샘플링해 머티리얼 자체에서 만듭니다. 카메라 Post Processing이나 Volume Bloom이 없어도 보입니다. HDR + Bloom을 켠 카메라에서는 추가 후처리 발광도 생길 수 있으므로 강도를 낮춰 조절하세요. 기존 카메라나 Volume은 변경하지 않았습니다.

## 스프라이트 설정

원본 PNG는 75% 두께를 목표로 내장 이미지 도구에서 수정한 투명 배경 단색 이미지입니다. 생성 도구의 특성상 픽셀별 두께 비율을 정확히 보장하는 수학적 변환은 아닙니다.

원래 스프라이트 GUID와 슬라이스 ID/영역은 유지하고 Mesh Type을 Full Rect로 변경했습니다. Tight 메시에서는 투명 영역의 번짐이 잘릴 수 있습니다. 번짐이 잘리면 Padding을 늘리거나 Sprite Editor에서 여백을 포함하도록 영역을 넓히세요.

이 머티리얼은 독립된 `PurpleHook.png`의 UV를 사용합니다. Sprite Atlas에 패킹하면 UV가 달라지므로 이 이미지를 Atlas에 넣지 마세요. 2D Sprite용이며 Unity UI Image용 머티리얼은 아닙니다.

## 그래프 구성

`PurpleHookGlow.shadergraph`를 열면 노출된 프로퍼티 → `PurpleHookGlow` Custom Function → Base Color / Alpha 연결을 볼 수 있습니다. Custom Function은 String 모드이며 코드가 그래프 안에 저장되어 추가 HLSL 파일이 필요하지 않습니다.

UV 여백 → 원본 알파 → 7×7 Gaussian 주변 샘플 → 번짐의 범위/강도/감쇠 → 본체와 합성 순서입니다. 번짐을 켰을 때 픽셀당 최대 50회 텍스처 샘플링하므로 화면을 덮는 대량 파티클보다 소수의 스킬 이펙트에 적합합니다.

이미지 편집 프롬프트: “기존 갈고리의 중심 곡선과 방향을 유지하고 두께를 현재의 75%로 줄이기. 보라색 단색, 투명 배경, 빛 번짐과 가로 선 없음.” 내장 image_gen 사용.

## 검증

Unity 6000.3.11f1의 별도 임시 프로젝트에서 그래프/머티리얼 임포트, 텍스처 참조, 셰이더 컴파일 및 SpriteRenderer + URP 2D Renderer 출력 검증을 통과했습니다. Bloom은 끈 상태로 확인했습니다. 동일한 768×768 렌더에서 밝은 픽셀 수는 번짐 활성화 49,287개, 번짐 비활성화 24,308개, Opacity 0일 때 0개였습니다. 기존 게임 씬에는 자동 적용하지 않았습니다.
