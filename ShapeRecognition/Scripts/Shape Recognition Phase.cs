using UnityEngine;

namespace ShapeRecognition
{
    public class ShapeRecognitionPhase : IGameStepPhase
    {
        [Header("Phase Root")]
        [SerializeField] private GameObject root; 

        [Header("Dependencies")]
        [SerializeField] private GridManager gridManager;
        [SerializeField] private bool autoFindDependencies = true;

        private void OnValidate()
        {
            if (root == null) root = gameObject;

            if (!autoFindDependencies) return;
            if (gridManager == null) gridManager = GetComponentInChildren<GridManager>();
        }

        public override bool IsFinish()
        {
            if (gridManager == null) return false;

            return gridManager.GetTotalCorrectShapes() >= 1;
        }

        public override void UpdateIntroDialog()
        {
            if (root != null) root.SetActive(true);
        }

        public override void OnStartGameStep()
        {
            if (root != null) root.SetActive(true);
            if (gridManager != null) gridManager.ResetGrid();
        }

        public override void CorrectAnswer()
        {
            Debug.Log("ShapeRecognition: Correct Answer Triggered");
        }

        public override void EndGameStep()
        {
            base.EndGameStep();

            int totalCorrect = 0;
            int totalWrong = 0;

            if (gridManager != null)
            {
                totalCorrect = gridManager.GetTotalCorrectShapes();

                totalWrong = (totalCorrect >= 1) ? 0 : 1;
            }

            if (GameStepPhaseManager.instance != null)
            {
                GameStepPhaseManager.instance.correctAnswers += totalCorrect;
                GameStepPhaseManager.instance.wrongAnswers += totalWrong;
            }

            HideGameStep();

            //Debug.Log("So cau dung: " + totalCorrect);
        }

        public override void HideGameStep()
        {
            if (root != null) root.SetActive(false);
            else gameObject.SetActive(false);
        }
    }
}