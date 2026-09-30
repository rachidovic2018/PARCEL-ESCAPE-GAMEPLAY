namespace ParcelEscape.Core
{
    public static class MoveValidator
    {
        public static MoveValidationResult Validate(BoardState board, MoveCommand command)
        {
            if (!board.TryGetPackageById(command.PackageId, out _))
            {
                return MoveValidationResult.Failure(MoveValidationStatus.PackageNotFound, $"Package with ID {command.PackageId} not found on the board.");
            }

            if (!board.IsPathClear(command.PackageId))
            {
                return MoveValidationResult.Failure(MoveValidationStatus.Blocked, $"Path is blocked for package {command.PackageId}.");
            }

            return MoveValidationResult.Success();
        }
    }
}
