namespace ParcelEscape.Core
{
    public class MoveExecutionResult
    {
        public MoveExecutionStatus Status { get; }
        public PackageState? EscapedPackage { get; }
        public MoveValidationResult ValidationResult { get; }
        public BoardState ResultingBoard { get; }

        public MoveExecutionResult(MoveExecutionStatus status, PackageState? escapedPackage, MoveValidationResult validationResult, BoardState resultingBoard)
        {
            Status = status;
            EscapedPackage = escapedPackage;
            ValidationResult = validationResult;
            ResultingBoard = resultingBoard;
        }
    }
}
