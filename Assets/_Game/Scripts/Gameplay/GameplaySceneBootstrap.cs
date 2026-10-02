using UnityEngine;
using ParcelEscape.Core;

namespace ParcelEscape.Gameplay
{
    public class GameplaySceneBootstrap : MonoBehaviour
    {
        [SerializeField] private LevelDefinitionAsset levelDefinition;
        [SerializeField] private BoardPresenter boardPresenter;
        [SerializeField] private CameraSetup cameraSetup;
        [SerializeField] private PackageInputController packageInputController;
        [SerializeField, Min(1)] private int holdingCapacity = 4;

        private GameSession _gameSession;

        public void Configure(
            LevelDefinitionAsset definition,
            BoardPresenter presenter,
            CameraSetup setup)
        {
            levelDefinition = definition;
            boardPresenter = presenter;
            cameraSetup = setup;
        }

        private void Start()
        {
            if (levelDefinition == null)
            {
                Debug.LogError("LevelDefinitionAsset is missing.");
                return;
            }

            if (boardPresenter == null)
            {
                boardPresenter = new GameObject("BoardPresenter").AddComponent<BoardPresenter>();
            }

            if (cameraSetup != null)
            {
                cameraSetup.SetupForBoard(levelDefinition.width, levelDefinition.height, 1.0f);
            }

            if (packageInputController == null)
            {
                packageInputController = GetComponent<PackageInputController>();
                if (packageInputController == null)
                {
                    packageInputController = gameObject.AddComponent<PackageInputController>();
                }
            }

            Camera inputCamera = cameraSetup != null
                ? cameraSetup.GetComponent<Camera>()
                : Camera.main;
            packageInputController.Initialize(inputCamera);

            BoardState initialBoard = LevelDefinitionConverter.Convert(levelDefinition);
            DeliveryState initialDelivery = LevelDefinitionConverter.ConvertDelivery(
                levelDefinition,
                holdingCapacity);
            _gameSession = new GameSession(initialBoard, initialDelivery);

            boardPresenter.Initialize(_gameSession);
            boardPresenter.SpawnBoard(initialBoard);
        }

        public void RestartLevel()
        {
            if (_gameSession != null)
            {
                _gameSession.Restart();
                boardPresenter.SpawnBoard(_gameSession.CurrentBoard);
            }
        }
    }
}
