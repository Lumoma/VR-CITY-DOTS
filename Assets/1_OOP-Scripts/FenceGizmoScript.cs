using UnityEngine;

public class FenceGizmoScript : MonoBehaviour
{
    [Header("Fence Settings")]
    [SerializeField] private float fenceHeight = 1.0f;
    public float fenceLength = 5.0f; // X-Richtung (transform.right)
    public float fenceWidth = 5.0f;  // Z-Richtung (transform.forward)
    [SerializeField] private float postSpacing = 1.0f;
    [SerializeField] private float postWidth = 0.1f;
    [SerializeField] private int railCount = 2;
    [SerializeField] private float groundOffset = 0.1f;
    
    [Header("Gizmo Colors")]
    [SerializeField] private Color postColor = Color.yellow;
    [SerializeField] private Color railColor = Color.yellow;
    
    [Header("Debug")]
    [SerializeField] private bool alwaysDrawGizmos = true;
    [SerializeField] private bool drawCenterMarker = true;

    private void OnValidate()
    {
        // Force Gizmo refresh when values change in inspector
        #if UNITY_EDITOR
        UnityEditor.SceneView.RepaintAll();
        #endif
    }

    private void OnDrawGizmos()
    {
        if (alwaysDrawGizmos)
        {
            DrawFence();
        }
    }

    private void OnDrawGizmosSelected()
    {
        DrawFence();
    }

    private void DrawFence()
    {
        // Bezugspunkt: 0.1 Einheiten über Boden, Mittelpunkt des Geheges
        Vector3 center = transform.position + Vector3.up * groundOffset;

        Vector3 right = transform.right;
        Vector3 fwd = transform.forward;

        float halfLen = fenceLength * 0.5f;
        float halfWid = fenceWidth * 0.5f;

        // Rechteck-Ecken im Uhrzeigersinn (von oben betrachtet)
        Vector3 bl = center - right * halfLen - fwd * halfWid; // bottom-left
        Vector3 br = center + right * halfLen - fwd * halfWid; // bottom-right
        Vector3 tr = center + right * halfLen + fwd * halfWid; // top-right
        Vector3 tl = center - right * halfLen + fwd * halfWid; // top-left

        // Kanten zeichnen: Pfosten und Latten pro Seite
        DrawEdge(bl, (br - bl).normalized, fenceLength);
        DrawEdge(br, (tr - br).normalized, fenceWidth);
        DrawEdge(tr, (tl - tr).normalized, fenceLength);
        DrawEdge(tl, (bl - tl).normalized, fenceWidth);
    }

    private void DrawEdge(Vector3 start, Vector3 direction, float length)
    {
        // Pfosten entlang der Kante
        Gizmos.color = postColor;
        int postCount = Mathf.Max(2, Mathf.CeilToInt(length / Mathf.Max(0.01f, postSpacing)) + 1);
        float step = length / (postCount - 1);
        for (int i = 0; i < postCount; i++)
        {
            Vector3 pos = start + direction * (i * step);
            DrawPost(pos);
        }

        // Latten/Schienen entlang der Kante
        Gizmos.color = railColor;
        for (int rail = 0; rail < Mathf.Max(0, railCount); rail++)
        {
            float railHeight = (fenceHeight / (railCount + 1)) * (rail + 1);
            Vector3 railStart = start + Vector3.up * railHeight;
            Vector3 railEnd = railStart + direction * length;
            Gizmos.DrawLine(railStart, railEnd);
        }

        // Oberste Abschlusslatte
        Vector3 topStart = start + Vector3.up * fenceHeight;
        Vector3 topEnd = topStart + direction * length;
        Gizmos.DrawLine(topStart, topEnd);
    }

    private void DrawPost(Vector3 position)
    {
        // Zeichne einen quadratischen Pfosten
        float halfWidth = postWidth / 2f;
        Vector3 up = Vector3.up * fenceHeight;
        Vector3 right = transform.right * halfWidth;
        Vector3 forward = transform.forward * halfWidth;
        
        // Vordere Ecken
        Vector3 frontBottomLeft = position - right - forward;
        Vector3 frontBottomRight = position + right - forward;
        Vector3 frontTopLeft = frontBottomLeft + up;
        Vector3 frontTopRight = frontBottomRight + up;
        
        // Hintere Ecken
        Vector3 backBottomLeft = position - right + forward;
        Vector3 backBottomRight = position + right + forward;
        Vector3 backTopLeft = backBottomLeft + up;
        Vector3 backTopRight = backBottomRight + up;
        
        // Zeichne die Kanten des Pfostens
        // Vordere Seite
        Gizmos.DrawLine(frontBottomLeft, frontTopLeft);
        Gizmos.DrawLine(frontBottomRight, frontTopRight);
        Gizmos.DrawLine(frontBottomLeft, frontBottomRight);
        Gizmos.DrawLine(frontTopLeft, frontTopRight);
        
        // Hintere Seite
        Gizmos.DrawLine(backBottomLeft, backTopLeft);
        Gizmos.DrawLine(backBottomRight, backTopRight);
        Gizmos.DrawLine(backBottomLeft, backBottomRight);
        Gizmos.DrawLine(backTopLeft, backTopRight);
        
        // Verbindungslinien zwischen vorne und hinten
        Gizmos.DrawLine(frontBottomLeft, backBottomLeft);
        Gizmos.DrawLine(frontBottomRight, backBottomRight);
        Gizmos.DrawLine(frontTopLeft, backTopLeft);
        Gizmos.DrawLine(frontTopRight, backTopRight);
    }
}

