namespace ParcelEscape.Core
{
    public static class MoveExecutor
    {
        public static MoveExecutionResult Execute(BoardState board, MoveCommand command)
        {
            var validationResult = MoveValidator.Validate(board, command);
            if (validationResult.Status != MoveValidationStatus.Valid)
            {
                return new MoveExecutionResult(MoveExecutionStatus.ValidationFailed, null, validationResult, board);
            }

            board.TryGetPackageById(command.PackageId, out var escapedPackage);
            
            var newBoard = board.RemovePackage(command.PackageId);
            
            return new MoveExecutionResult(MoveExecutionStatus.Success, escapedPackage, validationResult, newBoard);
        }
    }
}
