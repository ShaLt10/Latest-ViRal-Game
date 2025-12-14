using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TMPRotator : MonoBehaviour
{
    public float angle = 45f; // Rotation angle in degrees

    TMP_Text textComponent;

    void Start()
    {
        textComponent = GetComponent<TMP_Text>();
        textComponent.ForceMeshUpdate(); // Ensure text info is up to date
        textComponent.enableAutoSizing = false;
        RotateEntireText(angle);
        textComponent.havePropertiesChanged = true;
        textComponent.ForceMeshUpdate();
    }

    void RotateEntireText(float angle)
    {
        TMP_TextInfo textInfo = textComponent.textInfo;

        // 1. Calculate bounds for all visible characters
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, 0);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, 0);

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            min = Vector3.Min(min, charInfo.bottomLeft);
            max = Vector3.Max(max, charInfo.topRight);
        }

        Vector3 center = textInfo.characterInfo[0].bottomLeft;
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, angle), Vector3.one);

        // 2. Apply rotation to all visible character vertices
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;
            int materialIndex = charInfo.materialReferenceIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            for (int j = 0; j < 4; j++)
            {
                Vector3 orig = vertices[vertexIndex + j];
                Vector3 offset = orig - center;
                vertices[vertexIndex + j] = rotationMatrix.MultiplyPoint3x4(offset) + center;
            }
        }

        // 3. Push updated mesh data
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }

}
