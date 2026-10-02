using System;
using ParcelEscape.Core;

namespace ParcelEscape.Gameplay
{
    public enum GameSessionMoveStatus
    {
        BoardMoveRejected,
        DeliveryRejected,
        Committed
    }

    public sealed class GameSessionMoveResult
    {
        public GameSessionMoveStatus Status { get; }
        public MoveExecutionResult BoardMoveResult { get; }
        public DeliveryRoutingResult DeliveryRoutingResult { get; }
        public TruckPromotionResult TruckPromotionResult { get; }
        public BoardState ResultingBoard { get; }
        public DeliveryState ResultingDeliveryState { get; }
        public bool WasCommitted => Status == GameSessionMoveStatus.Committed;
        public PackageState? EscapedPackage =>
            WasCommitted ? BoardMoveResult.EscapedPackage : (PackageState?)null;

        private GameSessionMoveResult(
            GameSessionMoveStatus status,
            MoveExecutionResult boardMoveResult,
            DeliveryRoutingResult deliveryRoutingResult,
            TruckPromotionResult truckPromotionResult,
            BoardState resultingBoard,
            DeliveryState resultingDeliveryState)
        {
            Status = status;
            BoardMoveResult = boardMoveResult ?? throw new ArgumentNullException(nameof(boardMoveResult));
            DeliveryRoutingResult = deliveryRoutingResult;
            TruckPromotionResult = truckPromotionResult;
            ResultingBoard = resultingBoard ?? throw new ArgumentNullException(nameof(resultingBoard));
            ResultingDeliveryState = resultingDeliveryState ??
                throw new ArgumentNullException(nameof(resultingDeliveryState));
        }

        internal static GameSessionMoveResult BoardMoveRejected(
            MoveExecutionResult boardMoveResult,
            BoardState resultingBoard,
            DeliveryState resultingDeliveryState)
        {
            return new GameSessionMoveResult(
                GameSessionMoveStatus.BoardMoveRejected,
                boardMoveResult,
                null,
                null,
                resultingBoard,
                resultingDeliveryState);
        }

        internal static GameSessionMoveResult DeliveryRejected(
            MoveExecutionResult boardMoveResult,
            DeliveryRoutingResult deliveryRoutingResult,
            BoardState resultingBoard,
            DeliveryState resultingDeliveryState)
        {
            return new GameSessionMoveResult(
                GameSessionMoveStatus.DeliveryRejected,
                boardMoveResult,
                deliveryRoutingResult,
                null,
                resultingBoard,
                resultingDeliveryState);
        }

        internal static GameSessionMoveResult Committed(
            MoveExecutionResult boardMoveResult,
            DeliveryRoutingResult deliveryRoutingResult,
            TruckPromotionResult truckPromotionResult,
            BoardState resultingBoard,
            DeliveryState resultingDeliveryState)
        {
            return new GameSessionMoveResult(
                GameSessionMoveStatus.Committed,
                boardMoveResult,
                deliveryRoutingResult,
                truckPromotionResult,
                resultingBoard,
                resultingDeliveryState);
        }
    }
}
