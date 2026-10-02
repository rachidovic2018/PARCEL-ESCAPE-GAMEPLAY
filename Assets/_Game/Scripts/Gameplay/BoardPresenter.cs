using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParcelEscape.Core;

namespace ParcelEscape.Gameplay
{
    public class BoardPresenter : MonoBehaviour
    {
        [SerializeField] private float cellSize = 1.0f;
        
        private GameSession _session;
        private List<GameObject> _spawnedObjects = new List<GameObject>();
        private Dictionary<int, PackageView> _packageViews = new Dictionary<int, PackageView>();

        public void Initialize(GameSession session)
        {
            _session = session;
        }

        public void SpawnBoard(BoardState board)
        {
            ClearBoard();

            foreach (var packageState in board.Packages)
            {
                GameObject pkgGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pkgGo.name = $"Package_{packageState.Id}";
                pkgGo.transform.SetParent(this.transform);
                pkgGo.transform.position = GridToWorld(packageState.Position, board.Width, board.Height);
                
                var boxCollider = pkgGo.GetComponent<BoxCollider>();
                if (boxCollider == null) pkgGo.AddComponent<BoxCollider>();

                var view = pkgGo.AddComponent<PackageView>();
                view.Initialize(packageState);
                view.OnTapped += HandlePackageTapped;

                _spawnedObjects.Add(pkgGo);
                _packageViews[packageState.Id] = view;
            }

            foreach (var blockerState in board.Blockers)
            {
                GameObject blkGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blkGo.name = $"Blocker_{blockerState.Id}";
                blkGo.transform.SetParent(this.transform);
                blkGo.transform.position = GridToWorld(blockerState.Position, board.Width, board.Height);
                
                var view = blkGo.AddComponent<BlockerView>();
                view.Initialize(blockerState.Position);

                _spawnedObjects.Add(blkGo);
            }
        }

        public void ClearBoard()
        {
            foreach (var obj in _spawnedObjects)
            {
                if (obj != null)
                {
                    var view = obj.GetComponent<PackageView>();
                    if (view != null)
                    {
                        view.OnTapped -= HandlePackageTapped;
                    }
                    Destroy(obj);
                }
            }
            _spawnedObjects.Clear();
            _packageViews.Clear();
        }

        private void HandlePackageTapped(int packageId)
        {
            if (_session == null || !_session.TryMovePackage(packageId, out var result))
            {
                return;
            }

            if (_packageViews.TryGetValue(packageId, out var view))
            {
                if (result.WasCommitted && result.EscapedPackage.HasValue)
                {
                    StartCoroutine(ResolveEscapeRoutine(view, result.EscapedPackage.Value.Direction));
                }
                else
                {
                    StartCoroutine(ResolveBlockedRoutine(view));
                }
            }
            else if (result.WasCommitted)
            {
                _session.CompleteMovePresentation();
            }
        }

        private IEnumerator ResolveEscapeRoutine(PackageView view, PackageDirection direction)
        {
            _packageViews.Remove(view.PackageId);
            _spawnedObjects.Remove(view.gameObject);
            
            yield return StartCoroutine(view.PlayEscapeAnimation(direction));

            _session.CompleteMovePresentation();
        }

        private IEnumerator ResolveBlockedRoutine(PackageView view)
        {
            yield return StartCoroutine(view.PlayBlockedFeedback());
        }

        private Vector3 GridToWorld(GridPosition gridPos, int boardWidth, int boardHeight)
        {
            float worldX = gridPos.X * cellSize - (boardWidth * cellSize / 2f) + cellSize / 2f;
            float worldZ = gridPos.Y * cellSize - (boardHeight * cellSize / 2f) + cellSize / 2f;
            float worldY = 0.5f; // Slightly above board
            
            return new Vector3(worldX, worldY, worldZ);
        }
    }
}
