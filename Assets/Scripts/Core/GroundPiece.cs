using UnityEngine;

[RequireComponent(typeof(EdgeCollider2D))]
public class GroundPiece : MonoBehaviour
{
    void OnDrawGizmos()
    {
        var edge = GetComponent<EdgeCollider2D>();
        if (edge == null || edge.points.Length < 2) return;

        Gizmos.color = Color.green;
        var pts = edge.points;
        for (int i = 0; i < pts.Length - 1; i++)
        {
            var a = transform.TransformPoint(pts[i]);
            var b = transform.TransformPoint(pts[i + 1]);
            Gizmos.DrawLine(a, b);
            Gizmos.DrawWireSphere(a, 0.05f);
        }
        Gizmos.DrawWireSphere(transform.TransformPoint(pts[pts.Length - 1]), 0.05f);
    }
}
