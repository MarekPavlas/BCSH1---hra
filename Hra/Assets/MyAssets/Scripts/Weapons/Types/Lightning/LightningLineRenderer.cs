using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightningLineRenderer : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;

    [Header("Shape")]
    [Range(2, 80)] public int segments = 25;
    public float amplitude = 0.35f;        
    public float flickerSpeed = 25f;       

    [Header("Look")]
    public float startWidth = 0.08f;
    public float endWidth = 0.02f;

    [Header("Debug")]
    public bool onlyWhenEnabled = true;

    LineRenderer lr;
    Vector3[] points;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = segments;
        lr.startWidth = startWidth;
        lr.endWidth = endWidth;

        points = new Vector3[segments];
    }

    void LateUpdate()
    {
        if (onlyWhenEnabled && !lr.enabled) return;
        if (startPoint == null || endPoint == null) return;

        Vector3 a = startPoint.position;
        Vector3 b = endPoint.position;

        Vector3 dir = (b - a);
        float len = dir.magnitude;
        if (len < 0.001f) return;

        Vector3 forward = dir / len;

        Vector3 up = Vector3.up;
        if (Vector3.Dot(up, forward) > 0.9f) up = Vector3.right;
        Vector3 right = Vector3.Cross(forward, up).normalized;
        Vector3 normal = Vector3.Cross(right, forward).normalized;

        float tTime = Time.time * flickerSpeed;

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);

            Vector3 p = Vector3.Lerp(a, b, t);

            if (i != 0 && i != segments - 1)
            {
                float fade = Mathf.Sin(t * Mathf.PI); 
                float n1 = Mathf.PerlinNoise(t * 10f, tTime) * 2f - 1f;
                float n2 = Mathf.PerlinNoise(t * 10f + 100f, tTime) * 2f - 1f;

                Vector3 offset = (right * n1 + normal * n2) * amplitude * fade;
                p += offset;
            }

            points[i] = p;
        }

        lr.positionCount = segments;
        lr.SetPositions(points);
    }
}