#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class GroundColliderBuilder
{
    const float WalkableNormalThreshold = 0.5f;

    [MenuItem("EotW/Generate Road Colliders (Selected Ground)")]
    static void GenerateForSelected()
    {
        var selected = Selection.gameObjects;
        if (selected.Length == 0)
        {
            Debug.LogWarning("[GroundCollider] Select one or more ground FBX objects in the scene.");
            return;
        }

        int count = 0;
        foreach (var go in selected)
        {
            if (BuildRoadCollider(go))
                count++;
        }

        Debug.Log($"[GroundCollider] Generated road colliders for {count} object(s).");
    }

    [MenuItem("EotW/Generate Road Colliders (All Ground Sets)")]
    static void GenerateForAll()
    {
        var meshFilters = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);
        int count = 0;
        foreach (var mf in meshFilters)
        {
            if (BuildRoadCollider(mf.gameObject))
                count++;
        }

        Debug.Log($"[GroundCollider] Generated road colliders for {count} object(s).");
    }

    static bool BuildRoadCollider(GameObject go)
    {
        var filters = go.GetComponentsInChildren<MeshFilter>();
        if (filters.Length == 0)
        {
            Debug.LogWarning($"[GroundCollider] '{go.name}' has no MeshFilter — skipped.");
            return false;
        }

        var walkablePoints = new List<Vector2>();

        foreach (var mf in filters)
        {
            if (mf.sharedMesh == null) continue;
            var pts = ExtractWalkableSurface(mf.sharedMesh, mf.transform, go.transform);
            walkablePoints.AddRange(pts);
        }

        if (walkablePoints.Count < 2)
        {
            Debug.LogWarning($"[GroundCollider] '{go.name}' — no walkable faces found.");
            return false;
        }

        walkablePoints.Sort((a, b) => a.x.CompareTo(b.x));
        var profile = BuildTopProfile(walkablePoints);
        profile = SimplifyProfile(profile, 0.05f);

        if (profile.Count < 2)
        {
            Debug.LogWarning($"[GroundCollider] '{go.name}' — profile too small.");
            return false;
        }

        ApplyEdgeCollider(go, profile);
        return true;
    }

    static List<Vector2> ExtractWalkableSurface(Mesh mesh, Transform meshTransform, Transform rootTransform)
    {
        var verts = mesh.vertices;
        var tris = mesh.triangles;
        var result = new List<Vector2>();

        for (int i = 0; i < tris.Length; i += 3)
        {
            var v0 = verts[tris[i]];
            var v1 = verts[tris[i + 1]];
            var v2 = verts[tris[i + 2]];

            var worldV0 = meshTransform.TransformPoint(v0);
            var worldV1 = meshTransform.TransformPoint(v1);
            var worldV2 = meshTransform.TransformPoint(v2);

            var edge1 = worldV1 - worldV0;
            var edge2 = worldV2 - worldV0;
            var normal = Vector3.Cross(edge1, edge2).normalized;

            if (normal.y < WalkableNormalThreshold)
                continue;

            var local0 = rootTransform.InverseTransformPoint(worldV0);
            var local1 = rootTransform.InverseTransformPoint(worldV1);
            var local2 = rootTransform.InverseTransformPoint(worldV2);

            result.Add(new Vector2(local0.x, local0.y));
            result.Add(new Vector2(local1.x, local1.y));
            result.Add(new Vector2(local2.x, local2.y));
        }

        return result;
    }

    static List<Vector2> BuildTopProfile(List<Vector2> points)
    {
        if (points.Count < 2) return points;

        float xMin = points[0].x;
        float xMax = points[points.Count - 1].x;
        float xRange = xMax - xMin;

        if (xRange < 0.001f) return points;

        int sliceCount = Mathf.Max(32, Mathf.CeilToInt(xRange / 0.15f));
        float xStep = xRange / sliceCount;

        var profile = new List<Vector2>();

        for (int s = 0; s <= sliceCount; s++)
        {
            float sliceX = xMin + s * xStep;
            float halfStep = xStep * 0.6f;
            float highestY = float.MinValue;
            float bestX = sliceX;

            foreach (var p in points)
            {
                if (p.x < sliceX - halfStep) continue;
                if (p.x > sliceX + halfStep) break;

                if (p.y > highestY)
                {
                    highestY = p.y;
                    bestX = p.x;
                }
            }

            if (highestY > float.MinValue)
                profile.Add(new Vector2(bestX, highestY));
        }

        return profile;
    }

    static List<Vector2> SimplifyProfile(List<Vector2> points, float tolerance)
    {
        if (points.Count <= 3) return points;

        var simplified = new List<Vector2> { points[0] };

        for (int i = 1; i < points.Count - 1; i++)
        {
            var prev = simplified[simplified.Count - 1];
            var next = points[i + 1];
            var curr = points[i];

            float t = (curr.x - prev.x) / Mathf.Max(0.001f, next.x - prev.x);
            float expectedY = Mathf.Lerp(prev.y, next.y, t);

            if (Mathf.Abs(curr.y - expectedY) > tolerance)
                simplified.Add(curr);
        }

        simplified.Add(points[points.Count - 1]);
        return simplified;
    }

    static void ApplyEdgeCollider(GameObject go, List<Vector2> points)
    {
        foreach (var c in go.GetComponents<Collider2D>())
            Object.DestroyImmediate(c);
        foreach (var c in go.GetComponents<MeshCollider>())
            Object.DestroyImmediate(c);

        var edge = go.AddComponent<EdgeCollider2D>();
        edge.points = points.ToArray();

        EditorUtility.SetDirty(go);
    }
}
#endif
