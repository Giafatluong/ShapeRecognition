using UnityEngine;

namespace ShapeRecognition
{
    public class Zone : MonoBehaviour
    {
        [Header("Zone Bounds")]
        public float minX;
        public float minY;
        public float maxX;
        public float maxY;

        void Awake() 
        {
            UpdateBounds();
        }

        public void UpdateBounds()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                minX = sr.bounds.min.x;
                minY = sr.bounds.min.y;
                maxX = sr.bounds.max.x;
                maxY = sr.bounds.max.y;
            }
        }

        public bool isInsideArea(Vector3 pos)
        {
            return (pos.x >= minX && pos.x <= maxX && pos.y >= minY && pos.y <= maxY);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            UpdateBounds();
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, transform.position.z);
            Vector3 size = new Vector3(maxX - minX, maxY - minY, 0.1f);
            Gizmos.DrawWireCube(center, size);
        }
#endif
    }
}