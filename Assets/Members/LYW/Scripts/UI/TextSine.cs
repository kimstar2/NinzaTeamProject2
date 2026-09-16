using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TextSine : MonoBehaviour
{
    [Header("Wave")]
    [SerializeField] private float amplitude = 8f;      // 위아래 움직이는 거리
    [SerializeField] private float speed = 3f;          // 전체 애니메이션 속도
    [SerializeField] private float waveSpacing = 0.5f;  // 글자 사이 시간차

    [Header("Time")]
    [SerializeField] private bool useUnscaledTime = true;

    private TMP_Text text;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        // 원래 글자 Mesh 상태로 갱신
        text.ForceMeshUpdate();

        TMP_TextInfo textInfo = text.textInfo;

        float time = useUnscaledTime
            ? Time.unscaledTime
            : Time.time;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo character = textInfo.characterInfo[i];

            // 공백처럼 실제 Mesh가 없는 문자는 무시
            if (!character.isVisible)
                continue;

            int materialIndex = character.materialReferenceIndex;
            int vertexIndex = character.vertexIndex;

            Vector3[] vertices =
                textInfo.meshInfo[materialIndex].vertices;

            // 글자마다 Phase 차이를 줌
            float y =
                Mathf.Sin(
                    time * speed +
                    i * waveSpacing
                ) * amplitude;

            Vector3 offset = new Vector3(0f, y, 0f);

            // TMP 글자 하나는 꼭짓점 4개
            vertices[vertexIndex + 0] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;
        }

        // 변경된 Vertex를 실제 TMP Mesh에 적용
        text.UpdateVertexData(
            TMP_VertexDataUpdateFlags.Vertices
        );
    }
}