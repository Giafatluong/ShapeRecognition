using UnityEngine;

namespace ShapeRecognition
{
    public class DragObject : MonoBehaviour
    {
        private Vector3 offset;
        [HideInInspector] public bool isDrag;
        [HideInInspector] public Vector3 startPos;

        private SnapObjects snapObjects;
        private GridManager gridManager;
        private Zone zone; 

        public GameObject trianglePrefab;

        private float minX, maxX, minY, maxY;
        private int rotationIndex = 0;
        private float fixedZ;

        void Awake()
        {
            snapObjects = FindObjectOfType<SnapObjects>();
            gridManager = FindObjectOfType<GridManager>();
            zone = FindObjectOfType<Zone>();

            if (gridManager != null)
            {
                fixedZ = gridManager.transform.position.z;

                minX = gridManager.transform.position.x;
                minY = gridManager.transform.position.y;
                maxX = minX + (gridManager.width * gridManager.size);
                maxY = minY + (gridManager.height * gridManager.size);
            }
            else
            {
                fixedZ = transform.position.z;
            }

            rotationIndex = Mathf.RoundToInt(transform.eulerAngles.z / 90f) % 4;
        }

        private void OnMouseDown()
        {
            isDrag = true;
            startPos = transform.position;
            offset = transform.position - GetMousePos();
            if (gridManager != null) gridManager.RemoveFromGrid(this);
        }

        private void OnMouseDrag()
        {
            if (!isDrag) return;

            Vector3 currentMousePos = GetMousePos();
            Vector3 newPos = currentMousePos + offset;

            newPos.z = fixedZ;
            transform.position = newPos;

            if (Input.GetKeyDown(KeyCode.R))
            {
                RotateObject();
            }
            if (snapObjects != null)
            {
                if (IsInsideGrid(newPos))
                    snapObjects.UpdateGhost(newPos, transform.rotation, transform);
                else
                    snapObjects.DestroyGhost();
            }
        }

        private void OnMouseUp()
        {
            isDrag = false;
            Vector3 currentPos = transform.position;
            Vector3 finalSnapPos = snapObjects.CalculateSnapPos(currentPos);

            if (zone != null && zone.isInsideArea(currentPos))
            {
                if (IsInsideGrid(startPos))
                {
                    gameObject.SetActive(false);
                    snapObjects.DestroyGhost();
                    return;
                }
                else
                {
                    transform.position = startPos;
                }
            }
            else if (IsInsideGrid(finalSnapPos) && snapObjects.CanPlaceAt(finalSnapPos, transform))
            {
                transform.position = finalSnapPos;
                gridManager.AddToGrid(finalSnapPos, this);

                if (zone != null && zone.isInsideArea(startPos))
                {
                    if (trianglePrefab != null)
                    {
                        GameObject go = Instantiate(trianglePrefab, startPos, Quaternion.identity);
                        Vector3 temp = go.transform.position;
                        temp.z = fixedZ;
                        go.transform.position = temp;
                    }
                }
            }
            else
            {
                transform.position = startPos;
                if (IsInsideGrid(startPos))
                {
                    gridManager.AddToGrid(startPos, this);
                }
            }

            if (snapObjects != null) snapObjects.DestroyGhost();
        }

        private void RotateObject()
        {
            rotationIndex = (rotationIndex + 1) % 4;
            transform.rotation = Quaternion.Euler(0, 0, rotationIndex * 90f);
        }

        public bool IsInsideGrid(Vector3 pos)
        {
            return (pos.x >= minX && pos.x <= maxX && pos.y >= minY && pos.y <= maxY);
        }

        private Vector3 GetMousePos()
        {
            Vector3 mousePoint = Input.mousePosition;
            mousePoint.z = Mathf.Abs(Camera.main.transform.position.z - fixedZ);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePoint);
            worldPos.z = fixedZ;
            return worldPos;
        }
    }
}