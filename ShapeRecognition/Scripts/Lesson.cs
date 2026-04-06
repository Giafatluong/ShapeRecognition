using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ShapeRecognition
{
    public enum ShapeTarget { Triangle, Square, Rectangle }

    public class Lesson : MonoBehaviour
    {
        [Header("Thiết lập câu hỏi")]
        public List<ShapeTarget> targetQuestions = new List<ShapeTarget>();

        [Header("Tham chiếu")]
        public GridManager gridManager;

        private int mistakeCount;
        private float lessonTimer;
        private bool isLessonPaused;
        private bool isAlreadyCorrect;

        void Start()
        {
            if (gridManager == null) gridManager = FindObjectOfType<GridManager>();
            if (gridManager != null) gridManager.OnGridChanged += AutoCheckShapes;
        }

        private void OnDestroy()
        {
            if (gridManager != null) gridManager.OnGridChanged -= AutoCheckShapes;
        }

        void Update()
        {
            if (!isLessonPaused) lessonTimer += Time.deltaTime;
        }

        private void AutoCheckShapes()
        {
            if (isLessonPaused || isAlreadyCorrect) return;

            gridManager.CheckPatterns();

            bool allPassed = true;
            foreach (ShapeTarget target in targetQuestions)
            {
                if (!CheckShapeStatus(target))
                {
                    allPassed = false;
                    break;
                }
            }

            if (allPassed && targetQuestions.Count > 0)
            {
                isAlreadyCorrect = true;
                if (GameStepPhaseManager.instance != null)
                {
                    GameStepPhaseManager.instance.UpdateCorrectResult(true);
                }
                Debug.Log("<color=green>SUCCESS: All targets met!</color>");
            }
        }

        private bool CheckShapeStatus(ShapeTarget target)
        {
            if (gridManager == null) return false;
            return target switch
            {
                ShapeTarget.Triangle => gridManager.isTriangle,
                ShapeTarget.Square => gridManager.isSquare,
                ShapeTarget.Rectangle => gridManager.isRectangle,
                _ => false
            };
        }

        public void EndLesson()
        {
            isLessonPaused = true;
            int finalCorrect = targetQuestions.Count(t => CheckShapeStatus(t));
            Debug.Log($"LESSON END: {finalCorrect}/{targetQuestions.Count} Correct");
        }

        public void PlusMistake()
        {
            mistakeCount++;
            if (GameStepPhaseManager.instance != null)
                GameStepPhaseManager.instance.UpdateCorrectResult(false);
        }

        public string GetLessonTime()
        {
            int minute = Mathf.FloorToInt(lessonTimer / 60);
            int second = Mathf.FloorToInt(lessonTimer % 60);
            return $"{minute:00}'{second:00}''";
        }
    }
}