using System;
using ParcelEscape.Core;

namespace ParcelEscape.Gameplay
{
    public class GameSession
    {
        public BoardState CurrentBoard { get; private set; }
        public InteractionState State { get; private set; }

        private readonly BoardState _initialBoard;

        public GameSession(BoardState initialBoard)
        {
            _initialBoard = initialBoard ?? throw new ArgumentNullException(nameof(initialBoard));
            CurrentBoard = _initialBoard;
            State = InteractionState.Ready;
        }

        public bool TryMovePackage(int packageId, out MoveExecutionResult result)
        {
            if (State != InteractionState.Ready)
            {
                result = null;
                return false;
            }

            var command = new MoveCommand(packageId);
            result = MoveExecutor.Execute(CurrentBoard, command);

            if (result.Status == MoveExecutionStatus.Success)
            {
                CurrentBoard = result.ResultingBoard;
                State = InteractionState.ResolvingMove;
            }

            return true;
        }

        public void CompleteMovePresentation()
        {
            State = InteractionState.Ready;
        }

        public void Restart()
        {
            CurrentBoard = _initialBoard;
            State = InteractionState.Ready;
        }
    }
}
