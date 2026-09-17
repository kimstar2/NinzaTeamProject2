# SoftGlowLine 사용법

1. LineRenderer > Materials > Element 0에 `SoftGlowLine.mat`을 넣습니다.
2. LineRenderer Color를 흰색/알파 1로 시작하고, Width로 실제 월드 두께를 조절합니다. 머티리얼 색상과 LineRenderer 색상이 곱해집니다.
3. 카메라의 Rendering > Post Processing을 켜고 HDR를 허용합니다. 프로젝트 URP Asset은 현재 HDR가 켜져 있습니다.
4. 해당 카메라가 사용하는 Global Volume에 Bloom을 활성화합니다. 새 Volume이라면 `SoftGlowLineBloom.asset`을 Profile에 지정할 수 있습니다. 기존 Profile이 있다면 교체하지 말고 Bloom 값을 참고하세요: Threshold 1, Intensity 0.25, Scatter 0.55. 카메라의 Volume Mask에 Volume 오브젝트 레이어가 포함되어야 합니다.

## 머티리얼 조절값

| 항목 | 기본값 | 용도 |
|---|---:|---|
| Line Color (HDR) | 붉은 주황 | 라인 색상. 색상의 알파도 투명도에 반영 |
| Glow Intensity | 2.5 | HDR 출력 배율. Bloom이 너무 강하면 낮추기 |
| Opacity | 1 | 라인 전체 페이드. 0이면 발광도 사라짐 |
| Half Width (UV) | 0.48 | 라인 메시 내부의 반폭. 실제 메시 폭은 LineRenderer Width에서 조절 |
| Edge Softness | 0.7 | 가장자리 페이드 비율. 높일수록 부드러움 |

표준 URP Unlit / Transparent / Additive / 양면 / ZWrite Off입니다. 텍스처 없이 UV.y로 폭 방향 마스크를 만들기 때문에 길이가 늘어도 패턴이 늘어나지 않습니다. UV.x를 사용하지 않으므로 양 끝은 별도로 둥글게 페이드되지 않습니다. 끝 모양은 LineRenderer의 Cap Vertices 또는 Width Curve로 조절하세요.

그래프 흐름: UV.y → 중심(0.5)까지 거리 → Smoothstep → One Minus → Opacity × 색상 알파 → Alpha. HDR 색상 × LineRenderer Vertex Color × Glow Intensity → Base Color. Unlit의 HDR 출력이 Bloom에 반응합니다.

Bloom은 화면 전체 후처리이므로 다른 밝은 오브젝트에도 영향을 줍니다. 개별 라인의 발광은 Glow Intensity로 먼저 조절하세요. 이 프리셋은 씬에 자동 연결하지 않았습니다.

검증: 직렬화 참조, 노드 슬롯 연결 및 마스크 수식 검사 완료. Unity 에디터 임포트/셰이더 컴파일과 Game View 외관 검증은 별도 확인이 필요합니다.
