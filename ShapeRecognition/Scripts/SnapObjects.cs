using UnityEngine;

namespace ShapeRecognition
{
    public class SnapObjects : MonoBehaviour
    {
        [Range(0, 1)] public float ghostAlpha = 0.4f;
        private GameObject ghostPreview;
        private GridManager gridManager;

        private void Awake()
        {
            gridManager = FindObjectOfType<GridManager>();
        }

        public Vector3 CalculateSnapPos(Vector3 pos)
        {
            if (gridManager == null) return pos;

            float gridSize = gridManager.size;
            Vector3 gridOrigin = gridManager.transform.position;

            float x = Mathf.Floor((pos.x - gridOrigin.x) / gridSize) * gridSize + gridOrigin.x + gridSize / 2f;
            float y = Mathf.Floor((pos.y - gridOrigin.y) / gridSize) * gridSize + gridOrigin.y + gridSize / 2f;

            return new Vector3(x, y, gridOrigin.z);
        }

        public void CreateGhost(Transform root)
        {
            if (ghostPreview != null) Destroy(ghostPreview);

            ghostPreview = Instantiate(root.gameObject);
            ghostPreview.name = "Ghost_Preview";

            MonoBehaviour[] scripts = ghostPreview.GetComponentsInChildren<MonoBehaviour>();
            foreach (var s in scripts)
            {
                if (!(s is SpriteRenderer)) Destroy(s);
            }

            Collider2D[] colliders = ghostPreview.GetComponentsInChildren<Collider2D>();
            foreach (var c in colliders) Destroy(c);

            ghostPreview.transform.position = CalculateSnapPos(root.position);
            ghostPreview.transform.localScale = root.localScale;

            UpdateGhostColor(ghostPreview.transform.position, root);
        }

        public void UpdateGhost(Vector3 newPos, Quaternion rotation, Transform root)
        {
            if (ghostPreview == null) CreateGhost(root);

            Vector3 snapPos = CalculateSnapPos(newPos);
            ghostPreview.transform.position = snapPos;
            ghostPreview.transform.rotation = rotation;

            UpdateGhostColor(snapPos, root);
        }

        private void UpdateGhostColor(Vector3 snapPos, Transform root)
        {
            if (ghostPreview == null) return;

            bool canPlace = CanPlaceAt(snapPos, root);
            Color targetColor = canPlace ? new Color(1, 1, 1, ghostAlpha) : new Color(1, 0, 0, ghostAlpha);

            SpriteRenderer[] srs = ghostPreview.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in srs)
            {
                sr.color = targetColor;
                sr.sortingOrder = 10;
            }
        }

        public bool CanPlaceAt(Vector3 pos, Transform root)
        {
            return gridManager != null ? !gridManager.IsCellFull(pos, root) : true;
        }

        public void DestroyGhost()
        {
            if (ghostPreview != null) Destroy(ghostPreview);
        }
    }
}