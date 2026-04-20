using UnityEngine;

public class WaypointCircuit : MonoBehaviour
{
    [Header("Waypoints")]
    [Tooltip("Drag waypoint Transforms here in track order. " +
             "Alternatively, make them children of this GameObject and hit 'Collect Children'.")]
    public Transform[] waypoints;

    [Header("Settings")]
    [Tooltip("How close the AI must get to a waypoint to advance to the next one")]
    public float waypointReachRadius = 6f;

    [Tooltip("Draw gizmos in the editor")]
    public bool drawGizmos = true;
    public Color gizmoColor = new Color(0f, 1f, 0.4f, 0.8f);

    public Vector3 GetWaypointPosition(int index)
    {
        return waypoints[WrapIndex(index)].position;
    }

    public int NextIndex(int current)
    {
        return WrapIndex(current + 1);
    }


    public Vector3 GetLookAheadPoint(Vector3 fromPos, int startIndex, float distance)
    {
        if (waypoints == null || waypoints.Length == 0) return fromPos;

        float remaining = distance;
        Vector3 current = fromPos;
        int idx = startIndex;

        for (int safety = 0; safety < waypoints.Length; safety++)
        {
            Vector3 next = GetWaypointPosition(idx);
            float seg = Vector3.Distance(current, next);

            if (seg >= remaining)
            {
                // Interpolate along this segment
                return Vector3.Lerp(current, next, remaining / seg);
            }

            remaining -= seg;
            current = next;
            idx = NextIndex(idx);
        }

        // Fallback: just return the waypoint at startIndex
        return GetWaypointPosition(startIndex);
    }

    public int GetNearestWaypointIndex(Vector3 worldPos)
    {
        int best = 0;
        float bestDist = float.MaxValue;

        for (int i = 0; i < waypoints.Length; i++)
        {
            float d = Vector3.SqrMagnitude(waypoints[i].position - worldPos);
            if (d < bestDist)
            {
                bestDist = d;
                best = i;
            }
        }
        return best;
    }

    public float GetProgressDifference(Vector3 posA, Vector3 posB)
    {
        float progressA = GetCircuitProgress(posA);
        float progressB = GetCircuitProgress(posB);
        return progressA - progressB;
    }

    private float GetCircuitProgress(Vector3 worldPos)
    {
        if (waypoints == null || waypoints.Length < 2) return 0f;

        float totalLength = CircuitLength();
        float bestDist = float.MaxValue;
        float progressOnBest = 0f;
        float accumulated = 0f;

        for (int i = 0; i < waypoints.Length; i++)
        {
            int next = NextIndex(i);
            Vector3 segStart = waypoints[i].position;
            Vector3 segEnd = waypoints[next].position;
            float segLen = Vector3.Distance(segStart, segEnd);

            Vector3 closest = ClosestPointOnSegment(worldPos, segStart, segEnd);
            float d = Vector3.Distance(worldPos, closest);

            if (d < bestDist)
            {
                bestDist = d;
                float t = (segLen > 0f)
                    ? Vector3.Distance(segStart, closest) / segLen
                    : 0f;
                progressOnBest = (accumulated + t * segLen) / totalLength;
            }

            accumulated += segLen;
        }

        return progressOnBest;
    }

    private float CircuitLength()
    {
        float len = 0f;
        for (int i = 0; i < waypoints.Length; i++)
            len += Vector3.Distance(waypoints[i].position, waypoints[NextIndex(i)].position);
        return Mathf.Max(len, 0.001f);
    }

    private static Vector3 ClosestPointOnSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(point - a, ab);
        float d = Vector3.Dot(ab, ab);
        if (d < 0.0001f) return a;
        t = Mathf.Clamp01(t / d);
        return a + t * ab;
    }

    private int WrapIndex(int i)
    {
        if (waypoints == null || waypoints.Length == 0) return 0;
        return ((i % waypoints.Length) + waypoints.Length) % waypoints.Length;
    }


#if UNITY_EDITOR
    [ContextMenu("Collect Children as Waypoints")]
    private void CollectChildren()
    {
        waypoints = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            waypoints[i] = transform.GetChild(i);
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[WaypointCircuit] Collected {waypoints.Length} child waypoints.");
    }
#endif

    private void OnDrawGizmos()
    {
        if (!drawGizmos || waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = gizmoColor;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            // Line to next waypoint
            int next = WrapIndex(i + 1);
            if (waypoints[next] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);

            // Sphere at each waypoint
            Gizmos.DrawWireSphere(waypoints[i].position, waypointReachRadius * 0.5f);

            // Number label via handles (editor only)
#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                waypoints[i].position + Vector3.up * 2f,
                i.ToString());
#endif
        }
    }
}
