using System;
using ParcelEscape.Core;

namespace ParcelEscape.Gameplay
{
    public class GameSession
    {
        public BoardState CurrentBoard { get; private set; }
        public DeliveryState CurrentDeliveryState { get; private set; }
        public InteractionState State { get; private set; }

        private readonly BoardState _initialBoard;
        private readonly DeliveryState _initialDeliveryState;

        public GameSession(
            BoardState initialBoard,
            DeliveryState initialDeliveryState)
        {
            _initialBoard = initialBoard ?? throw new ArgumentNullException(nameof(initialBoard));
            _initialDeliveryState = initialDeliveryState ??
                throw new ArgumentNullException(nameof(initialDeliveryState));
            CurrentBoard = _initialBoard;
            CurrentDeliveryState = _initialDeliveryState;
            State = InteractionState.Ready;
        }

        public bool TryMovePackage(int packageId, out GameSessionMoveResult result)
        {
            if (State != InteractionState.Ready)
            {
                result = null;
                return false;
            }

            var command = new MoveCommand(packageId);
            var boardMoveResult = MoveExecutor.Execute(CurrentBoard, command);

            if (boardMoveResult.Status != MoveExecutionStatus.Success)
            {
                result = GameSessionMoveResult.BoardMoveRejected(
                    boardMoveResult,
                    CurrentBoard,
                    CurrentDeliveryState);
                return true;
            }

            var routingResult = CurrentDeliveryState.RouteEscapedPackage(boardMoveResult);
            if (!routingResult.WasAccepted)
            {
                result = GameSessionMoveResult.DeliveryRejected(
                    boardMoveResult,
                    routingResult,
                    CurrentBoard,
                    CurrentDeliveryState);
                return true;
            }

            var promotionResult = routingResult.ResultingState.ResolveCompletedActiveTruck();
            CurrentBoard = boardMoveResult.ResultingBoard;
            CurrentDeliveryState = promotionResult.ResultingState;
            State = InteractionState.ResolvingMove;
            result = GameSessionMoveResult.Committed(
                boardMoveResult,
                routingResult,
                promotionResult,
                CurrentBoard,
                CurrentDeliveryState);
            return true;
        }

        public void CompleteMovePresentation()
        {
            State = InteractionState.Ready;
        }

        public void Restart()
        {
            CurrentBoard = _initialBoard;
            CurrentDeliveryState = _initialDeliveryState;
            State = InteractionState.Ready;
        }
    }
}
