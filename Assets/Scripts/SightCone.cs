using UnityEngine;

// What an enemy can see, drawn as a see-through cone. Anything on the obstacle layers blocks it.
// Sits on a child of the enemy, which points it each frame.
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SightCone : MonoBehaviour
{
    [Tooltip("How far the enemy can see, in world units.")]
    [SerializeField] private float viewDistance = 20f;

    [Tooltip("Width of the cone in degrees.")]
    [Range(1f, 360f)]
    [SerializeField] private float viewAngle = 90f;

    [Tooltip("Layers that block sight, like walls. Don't include the characters' layers.")]
    [SerializeField] private LayerMask obstacles;

    [Tooltip("More segments give a smoother edge where walls cut the cone off.")]
    [SerializeField] private int segments = 32;

    [Header("Colors")]
    [SerializeField] private Color calmColor = new Color(1f, 0.9f, 0.4f, 0.2f);
    [SerializeField] private Color alertColor = new Color(1f, 0.15f, 0.1f, 0.3f);

    private Mesh mesh;
    private Vector3[] vertices;
    private Color[] colors;

    // World space direction the cone points.
    public Vector2 Direction { get; set; } = Vector2.down;
    // Switches to the alert color while the enemy is chasing.
    public bool Alert { get; set; }

    private void Awake()
    {
        segments = Mathf.Max(segments, 1);
        vertices = new Vector3[segments + 2];
        colors = new Color[segments + 2];

        // A fan of triangles around the first vertex, which stays at the cone's tip.
        int[] triangles = new int[segments * 3];

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 2;
            triangles[i * 3 + 2] = i + 1;
        }

        mesh = new Mesh { name = "Sight Cone" };
        mesh.MarkDynamic();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    private void OnDestroy()
    {
        Destroy(mesh);
    }

    public bool CanSee(Vector2 point)
    {
        Vector2 origin = transform.position;
        Vector2 toPoint = point - origin;

        return toPoint.sqrMagnitude <= viewDistance * viewDistance
            && Vector2.Angle(Direction, toPoint) <= viewAngle * 0.5f
            && !Physics2D.Linecast(origin, point, obstacles);
    }

    private void LateUpdate()
    {
        Vector2 origin = transform.position;
        float startAngle = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg - viewAngle * 0.5f;

        for (int i = 0; i <= segments; i++)
        {
            float angle = (startAngle + viewAngle * i / segments) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, viewDistance, obstacles);
            float distance = hit ? hit.distance : viewDistance;

            vertices[i + 1] = transform.InverseTransformPoint(origin + direction * distance);
        }

        Color color = Alert ? alertColor : calmColor;

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = color;
        }

        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.RecalculateBounds();
    }
}
