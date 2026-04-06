using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ShapeRecognition
{
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        public int width = 10;
        public int height = 10;
        public float size = 1f;
        public GameObject Prefabs;

        private Dictionary<Vector2Int, GridCell> cells = new Dictionary<Vector2Int, GridCell>();
        private List<Vector2Int> allCellPositions = new List<Vector2Int>();

        [Header("Pattern Status")]
        public bool isSquare = false;
        public bool isRectangle = false;
        public bool isTriangle = false;

        public Action OnGridChanged;

        void Start()
        {
            GenGrid();
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.C))
            {
                CheckPatterns();
                Debug.Log($"<color=cyan>Status - Square: {isSquare}, Rect: {isRectangle}, Tri: {isTriangle}</color>");
            }
#endif
        }

        private void GenGrid()
        {
            cells.Clear();
            allCellPositions.Clear();

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Vector2Int gridPos = new Vector2Int(i, j);
                    Vector3 worldPos = new Vector3(
                        transform.position.x + size * i + size / 2f,
                        transform.position.y + size * j + size / 2f,
                        transform.position.z
                    );

                    if (Prefabs != null)
                    {
                        Instantiate(Prefabs, worldPos, Quaternion.identity, transform);
                    }

                    cells[gridPos] = new GridCell();
                    allCellPositions.Add(gridPos);
                }
            }
        }

        public int GetTotalCorrectShapes()
        {
            CheckPatterns();
            int count = 0;
            if (isSquare) count++;
            if (isRectangle) count++;
            if (isTriangle) count++;
            return count;
        }

        public void ResetGrid()
        {
            foreach (var pos in allCellPositions)
            {
                GridCell cell = cells[pos];
                var pieces = new List<DragObject>(cell.pieces);
                foreach (var piece in pieces)
                {
                    if (piece != null)
                    {
                        piece.transform.position = piece.startPos;
                        Vector3 p = piece.transform.position;
                        p.z = transform.position.z;
                        piece.transform.position = p;
                        piece.isDrag = false;
                    }
                }
                cell.pieces.Clear();
            }
            isSquare = isRectangle = isTriangle = false;
        }

        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt((worldPos.x - transform.position.x) / size);
            int y = Mathf.FloorToInt((worldPos.y - transform.position.y) / size);
            return new Vector2Int(x, y);
        }

        public void AddToGrid(Vector3 worldPos, DragObject obj)
        {
            Vector2Int gridPos = WorldToGrid(worldPos);
            if (cells.TryGetValue(gridPos, out GridCell cell))
            {
                if (!cell.pieces.Contains(obj))
                {
                    cell.Add(obj);
                    OnGridChanged?.Invoke();
                }
            }
        }

        public void RemoveFromGrid(DragObject obj)
        {
            bool changed = false;
            foreach (var cell in cells.Values)
            {
                if (cell.pieces.Contains(obj))
                {
                    cell.Remove(obj);
                    changed = true;
                }
            }
            if (changed) OnGridChanged?.Invoke();
        }

        public bool IsCellFull(Vector3 worldPos, Transform movingRoot)
        {
            Vector2Int gridPos = WorldToGrid(worldPos);
            if (cells.TryGetValue(gridPos, out GridCell cell))
            {
                int count = 0;
                foreach (var piece in cell.pieces)
                {
                    if (piece != null && piece.transform != movingRoot) count++;
                }
                return count >= 2;
            }
            return false;
        }

        //Thuat toan Loang
        public void CheckPatterns()
        {
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            isSquare = false;
            isRectangle = false;
            isTriangle = false;

            foreach (var pos in allCellPositions)
            {
                if (visited.Contains(pos) || cells[pos].pieces.Count == 0) continue;

                List<Vector2Int> currentGroup = new List<Vector2Int>();
                Queue<Vector2Int> queue = new Queue<Vector2Int>();

                queue.Enqueue(pos);
                visited.Add(pos);

                while (queue.Count > 0)
                {
                    Vector2Int p = queue.Dequeue();
                    currentGroup.Add(p);

                    Vector2Int[] neighbors = {
                        p + Vector2Int.up, p + Vector2Int.down,
                        p + Vector2Int.left, p + Vector2Int.right
                    };

                    foreach (var n in neighbors)
                    {
                        if (cells.ContainsKey(n) && !visited.Contains(n) && cells[n].pieces.Count > 0)
                        {
                            visited.Add(n);
                            queue.Enqueue(n);
                        }
                    }
                }
                IdentifyShape(currentGroup);
            }
        }

        #region Square and Rectangle Check
        private void IdentifyShape(List<Vector2Int> group)
        {
            if (group.Count == 0) return;

            int fullSquaresCount = 0;
            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;

            foreach (var pos in group)
            {
                minX = Mathf.Min(minX, pos.x);
                maxX = Mathf.Max(maxX, pos.x);
                minY = Mathf.Min(minY, pos.y);
                maxY = Mathf.Max(maxY, pos.y);
                if (cells[pos].IsFull()) fullSquaresCount++;
            }

            int w = maxX - minX + 1;
            int h = maxY - minY + 1;
            bool isSolidBlock = (group.Count == w * h) && (fullSquaresCount == group.Count);

            if (isSolidBlock)
            {
                if (w == h) isSquare = true;
                else isRectangle = true;
            }
            else
            {
                IdentifyTriangle(group);
            }
        }
        #endregion

        #region Triangle Check
        private void IdentifyTriangle(List<Vector2Int> group)
        {
            if (group.Count < 2) return;

            HashSet<Vector2> vertexSet = new HashSet<Vector2>();
            float totalArea = 0;

            foreach (var pos in group)
            {
                GridCell cell = cells[pos];
                foreach (Vector2 v in GetActualVerticesOfCell(pos, cell))
                {
                    vertexSet.Add(new Vector2(Mathf.Round(v.x * 100f) / 100f, Mathf.Round(v.y * 100f) / 100f));
                }
                totalArea += cell.IsFull() ? 1.0f : 0.5f;
            }

            List<Vector2> hull = GetConvexHull(new List<Vector2>(vertexSet));
            List<Vector2> corners = GetCleanCorners(hull);
            float hullArea = GetPolygonArea(corners);

            if (Mathf.Abs(totalArea - hullArea) < 0.05f && corners.Count == 3)
            {
                isTriangle = true;
            }
        }

        private List<Vector2> GetActualVerticesOfCell(Vector2Int pos, GridCell cell)
        {
            List<Vector2> v = new List<Vector2>();
            float x = pos.x;
            float y = pos.y;

            if (cell.IsFull())
            {
                v.Add(new Vector2(x, y)); v.Add(new Vector2(x + 1, y));
                v.Add(new Vector2(x + 1, y + 1)); v.Add(new Vector2(x, y + 1));
            }
            else if (cell.pieces.Count > 0)
            {
                float rot = Mathf.Round(cell.pieces[0].transform.eulerAngles.z / 90f) * 90f % 360f;
                if (rot < 0) rot += 360f;

                if (rot == 0f) { v.Add(new Vector2(x, y)); v.Add(new Vector2(x + 1, y)); v.Add(new Vector2(x, y + 1)); }
                else if (rot == 90f) { v.Add(new Vector2(x, y)); v.Add(new Vector2(x + 1, y)); v.Add(new Vector2(x + 1, y + 1)); }
                else if (rot == 180f) { v.Add(new Vector2(x + 1, y)); v.Add(new Vector2(x + 1, y + 1)); v.Add(new Vector2(x, y + 1)); }
                else if (rot == 270f) { v.Add(new Vector2(x, y)); v.Add(new Vector2(x, y + 1)); v.Add(new Vector2(x + 1, y + 1)); }
            }
            return v;
        }

        private List<Vector2> GetCleanCorners(List<Vector2> hull)
        {
            if (hull.Count < 3) return hull;
            List<Vector2> corners = new List<Vector2>();
            for (int i = 0; i < hull.Count; i++)
            {
                Vector2 p1 = hull[(i + hull.Count - 1) % hull.Count];
                Vector2 p2 = hull[i];
                Vector2 p3 = hull[(i + 1) % hull.Count];
                if (Mathf.Abs(CrossProduct(p1, p2, p3)) > 0.01f) corners.Add(p2);
            }
            return corners;
        }

        //Thuat toan Convex Hull
        private List<Vector2> GetConvexHull(List<Vector2> points)
        {
            int n = points.Count;
            if (n <= 3) return points;
            points.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));

            List<Vector2> hull = new List<Vector2>();
            for (int i = 0; i < n; i++)
            {
                while (hull.Count >= 2 && CrossProduct(hull[hull.Count - 2], hull[hull.Count - 1], points[i]) <= 0)
                    hull.RemoveAt(hull.Count - 1);
                hull.Add(points[i]);
            }
            int lowerStart = hull.Count;
            for (int i = n - 2; i >= 0; i--)
            {
                while (hull.Count > lowerStart && CrossProduct(hull[hull.Count - 2], hull[hull.Count - 1], points[i]) <= 0)
                    hull.RemoveAt(hull.Count - 1);
                hull.Add(points[i]);
            }
            hull.RemoveAt(hull.Count - 1);
            return hull;
        }

        private float CrossProduct(Vector2 a, Vector2 b, Vector2 c) => (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);

        private float GetPolygonArea(List<Vector2> poly)
        {
            float area = 0;
            for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
                area += (poly[j].x + poly[i].x) * (poly[j].y - poly[i].y);
            return Mathf.Abs(area) / 2f;
        }
        #endregion
    }
}