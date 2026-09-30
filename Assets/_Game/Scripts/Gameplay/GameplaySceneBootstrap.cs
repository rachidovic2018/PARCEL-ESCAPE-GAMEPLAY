using UnityEngine;
using ParcelEscape.Core;

namespace ParcelEscape.Gameplay
{
    public class GameplaySceneBootstrap : MonoBehaviour
    {
        [SerializeField] private LevelDefinitionAsset levelDefinition;
        [SerializeField] private BoardPresenter boardPresenter;
        [SerializeField] private CameraSetup cameraSetup;

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

            BoardState initialBoard = LevelDefinitionConverter.Convert(levelDefinition);
            _gameSession = new GameSession(initialBoard);

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
