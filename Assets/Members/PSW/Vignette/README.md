# Blue Fog Vignette

짙은 남색 화면 테두리와 천천히 흐르는 불규칙한 안개 경계입니다. 입자, 발광, 외부 텍스처, 별도 C# 스크립트는 없습니다.

## 사용

1. `BlueFogVignette.prefab`을 씬의 Hierarchy 최상위에 드래그합니다. 자체 Screen Space - Overlay Canvas가 들어 있습니다.
2. Game 뷰에서 확인합니다. Play 모드에서는 안개가 자동으로 움직입니다.
3. 루트 오브젝트를 켜고 끄면 효과가 켜지고 꺼집니다. CanvasGroup의 Alpha를 0~1로 조절하거나 애니메이션하면 페이드할 수 있습니다.

카메라, Volume, Renderer Feature를 설정할 필요가 없습니다. UI 클릭을 막지 않습니다. Canvas의 Sort Order 기본값은 50입니다. HUD를 효과 위에 표시하려면 HUD Canvas의 Sort Order를 더 높게 설정하세요. 이 에셋은 기존 씬이나 HUD 설정을 자동으로 바꾸지 않습니다.

## 머티리얼 조절

`BlueFogVignette.mat`을 선택합니다. 이 머티리얼을 수정하면 공유하는 모든 프리팹 인스턴스에 적용됩니다. 서로 다른 설정이 필요하면 머티리얼을 복제하여 자식 FogOverlay의 RawImage에 할당하세요.

| 항목 | 설명 |
| --- | --- |
| Deep Blue | 바깥쪽 테두리의 짙은 색 |
| Fog Blue | 안쪽 안개의 색 |
| Opacity | 전체 불투명도 |
| Border Width | 화면 안쪽으로 들어오는 테두리 두께 |
| Inner Edge Softness | 안개 경계가 퍼지는 정도 |
| Fog Irregularity | 경계가 울퉁불퉁해지는 정도. 0이면 매끈한 경계 |
| Fog Scale | 노이즈의 빈도. 높을수록 안개 무늬가 작아짐 |
| Fog Speed | 안개 이동 속도. 0이면 정지 |
| Extra Left Border | 참고 이미지처럼 왼쪽을 더 두껍게 표현. 0이면 좌우 대칭 두께 |

기본 설정은 중앙이 완전히 투명합니다. Border Width와 Softness를 크게 올리면 중앙까지 안개가 들어올 수 있습니다. 안개 이동은 Unity의 셰이더 시간에 따라 동작하므로 timeScale이 0이면 멈춥니다.

셰이더는 화면 가장자리 마스크와 2차원 노이즈를 알파 합성합니다. 실제 볼류메트릭 안개나 Bloom을 사용하지 않습니다. 전체 화면 UI 프리팹용이며, 월드 공간 UI나 Soft RectMask2D 전용 셰이더는 아닙니다.
