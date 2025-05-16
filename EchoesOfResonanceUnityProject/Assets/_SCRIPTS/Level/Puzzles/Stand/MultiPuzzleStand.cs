using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
public class MultiPuzzleStand : PuzzleStand
{
    public FocusableLinkedData[] linkedDatas;
    [SerializeField] private int activeIndex;
    public bool singleGemSet;
    public void Awake()
    {
        activeData = linkedDatas[0].data;
        activeData.OnPuzzleCompleted += OnSequenceComplete;
        activeIndex = 0;

        foreach(var l in linkedDatas)
        {
            l.data.solved = 0;
            l.data.Active = false;
        }
    }
    public void CheckRotation(float newPlayerY)
    {
        newPlayerY = (newPlayerY + 360f) % 360f;

        for (int i = 0; i < linkedDatas.Length; i++)
        {
            float min = (linkedDatas[i].minimum + 360f) % 360f;
            float max = (linkedDatas[i].maximum + 360f) % 360f;

            bool inRange;

            if (min < max)
            {
                inRange = newPlayerY >= min && newPlayerY < max;
            }
            else
            {
                inRange = newPlayerY >= min || newPlayerY < max;
            }

            if (inRange && i != activeIndex)
            {
                activeData.OnPuzzleCompleted -= OnSequenceComplete;
                DeactivatePuzzle();

                activeIndex = i;
                activeData = linkedDatas[i].data;

                activeData.OnPuzzleCompleted += OnSequenceComplete;
                SetupPuzzle();
                
                activeData.OnSolvedChanged.Invoke(activeData.solved);

                foreach (var m in linkedDatas[activeIndex].moves)
                {
                    CRManager.Begin(m.destination.ApplyToOverConstant(m.target, m.speed), $"MultiPlateMove{m.target.name}", this);
                }

                progressText.text = $"{linkedDatas[i].actionText} {linkedDatas[i].data.solved} / {linkedDatas[i].data.solutions.Length}";

                if (singleGemSet)
                {
                    for (int j = 0; j < gems.Length; j++)
                    {
                        CRManager.Restart(
                            gems[j].ShiftGem(activeData.solutions[j].note.Pitch, 1.5f, j < activeData.solved),
                            $"ShiftMultiPlateGem{j}", this
                        );
                    }
                }

                break;
            }
        }
    }
    void OnSequenceComplete()
    {
        for(int i = 0; i < linkedDatas.Length; i++)
        {            
            if(i != activeIndex! && linkedDatas[i].data.Solved)
            {
                linkedDatas[i].data.solved = 0;
                linkedDatas[i].data.reset = 0;
                linkedDatas[i].data.Active = false;
            }
        }
    }
    public override void EjectPlayer()
    {
        base.EjectPlayer();

        foreach(var l in linkedDatas)
        {
            if(!l.data.Solved)
            {
                l.data.solved = 0;
                l.data.Active = false;
            }
        }
    }

    void OnDisable()
    {
        activeData.OnPuzzleCompleted -= OnSequenceComplete;
    }
}
[Serializable]
public class FocusableLinkedData
{
    [HideInInspector] public float minimum;
    [HideInInspector] public float maximum;
    public PuzzleData data;
    public string actionText;
    public MoveOnActivate[] moves;

    [Serializable]
    public class MoveOnActivate
    {
        public Transform target;
        public float speed;
        public TrData destination;
    }
}

[CustomEditor(typeof(MultiPuzzleStand))]
public class MultiPuzzleStandEditor : Editor
{
    private const float radius = 80f;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Edit Puzzle Regions"))
        {
            SceneView.RepaintAll();
        }
    }

    private void OnSceneGUI()
    {
        MultiPuzzleStand plate = (MultiPuzzleStand)target;
        if (plate.linkedDatas == null || plate.linkedDatas.Length < 2) return;
        EnsureSegmentContinuity(plate);

        Vector3 center = plate.transform.position + Vector3.up * 2f;
        float radius = 80f;

        for (int i = 0; i < plate.linkedDatas.Length; i++)
        {
            var current = plate.linkedDatas[i];
            var next = plate.linkedDatas[(i + 1) % plate.linkedDatas.Length];

            float minAngle = NormalizeAngle(current.minimum);
            float maxAngle = NormalizeAngle(current.maximum);
            float midAngle = NormalizeAngle(minAngle + Mathf.DeltaAngle(minAngle, maxAngle) / 2f);


            float hue = i / (float)plate.linkedDatas.Length;
            Handles.color = Color.HSVToRGB(hue, 0.6f, 1f);
            Handles.color = new Color(Handles.color.r, Handles.color.g, Handles.color.b, 0.25f);
            DrawSector(center, minAngle, maxAngle, radius);

            // --- Draw radial lines
            Handles.color = Color.white;
            DrawRadialLine(center, minAngle, radius);

            string label = current.data != null ? current.data.name : $"Region {i}";
            Vector3 labelPos = center + AngleToVector(midAngle) * radius;
            Handles.color = Color.yellow;
            Handles.Label(labelPos, label, EditorStyles.boldLabel);

            Vector3 degreeLabelPos = center + AngleToVector(minAngle) * radius * 1.05f;
            Handles.Label(degreeLabelPos, minAngle.ToString(), EditorStyles.boldLabel);

            // --- Draggable maximum angle
            Handles.color = Color.cyan;
            float updatedMax = DrawClampedAngleHandle(center, radius, maxAngle, minAngle, next.maximum);

            if (!Mathf.Approximately(updatedMax, maxAngle))
            {
                Undo.RecordObject(plate, "Adjust Region Boundary");
                current.maximum = NormalizeAngle(updatedMax);
                next.minimum = NormalizeAngle(updatedMax);
                EditorUtility.SetDirty(plate);
            }
        }
    }
    void EnsureSegmentContinuity(MultiPuzzleStand plate)
    {
        var data = plate.linkedDatas;

        if (data == null || data.Length == 0)
            return;

        for (int i = 0; i < data.Length; i++)
        {
            int next = (i + 1) % data.Length;

            // Ensure that minimum of next equals maximum of current
            if (Mathf.Abs(Mathf.DeltaAngle(data[next].minimum, data[i].maximum)) > 0.01f)
            {
                data[next].minimum = data[i].maximum;
            }

            // Ensure the segment spans a minimum angle
            if (data[i].maximum <= data[i].minimum)
            {
                data[i].maximum = (data[i].minimum + 45f) % 360f;
            }
        }
    }

    private Vector3 AngleToVector(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));
    }

    private void DrawRadialLine(Vector3 center, float angleDeg, float radius)
    {
        Vector3 dir = AngleToVector(angleDeg);
        Handles.DrawLine(center, center + dir * radius);
    }
    private float NormalizeAngle(float angle)
    {
        return (angle % 360f + 360f) % 360f;
    }
    private void DrawSector(Vector3 center, float startAngle, float endAngle, float radius)
    {
        int segments = 40;
        float delta = Mathf.DeltaAngle(startAngle, endAngle);
        if (delta <= 0) delta += 360f;

        int steps = Mathf.Max(2, Mathf.RoundToInt(segments * (delta / 360f)));
        List<Vector3> points = new List<Vector3> { center };

        for (int i = 0; i <= steps; i++)
        {
            float a = startAngle + delta * ((float)i / steps);
            points.Add(center + AngleToVector(a) * radius);
        }

        Handles.DrawAAConvexPolygon(points.ToArray());
    }

    private float DrawClampedAngleHandle(Vector3 center, float radius, float currentAngle, float minLimit, float nextMax)
    {
        Vector3 dir = AngleToVector(currentAngle);
        Vector3 worldPos = center + dir * radius;

        EditorGUI.BeginChangeCheck();
        Vector3 newWorldPos = Handles.FreeMoveHandle(worldPos, 1f, Vector3.zero, Handles.CircleHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            Vector3 fromCenter = (newWorldPos - center).normalized;
            float rawAngle = Mathf.Atan2(fromCenter.z, fromCenter.x) * Mathf.Rad2Deg;
            float newAngle = NormalizeAngle(rawAngle);

            float deltaToMin = Mathf.DeltaAngle(minLimit, newAngle);
            float deltaToMax = Mathf.DeltaAngle(newAngle, nextMax);

            if (deltaToMin < 1f) newAngle = NormalizeAngle(minLimit + 5f);
            if (deltaToMax < 1f) newAngle = NormalizeAngle(nextMax - 5f);

            return NormalizeAngle(Mathf.Round(newAngle));
        }

        return Mathf.Round(currentAngle);
    }

}
