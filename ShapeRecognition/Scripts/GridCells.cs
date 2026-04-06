using System.Collections.Generic;
using UnityEngine;

namespace ShapeRecognition
{
    [System.Serializable] 
    public class GridCell
    {
        public List<DragObject> pieces = new List<DragObject>();

        public void Add(DragObject obj)
        {
            if (obj == null) return;
            if (!pieces.Contains(obj)) pieces.Add(obj);
        }

        public void Remove(DragObject obj)
        {
            if (obj == null) return;
            if (pieces.Contains(obj)) pieces.Remove(obj);
        }

        public bool IsFull()
        {
            if (pieces.Count < 2) return false;
            float a1 = GetAngle(pieces[0]);
            float a2 = GetAngle(pieces[1]);

            float diff = Mathf.Abs(a1 - a2);
            return diff == 180f || diff == 180f;
        }

        public bool IsHalf() => pieces.Count == 1;

        public float GetAngle(DragObject obj)
        {
            if (obj == null) return 0;
            float z = obj.transform.eulerAngles.z;
            return Mathf.Round(z / 90f) * 90f % 360f;
        }
    }
}