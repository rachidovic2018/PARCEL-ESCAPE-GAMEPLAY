namespace ParcelEscape.Core
{
    public class MoveValidationResult
    {
        public MoveValidationStatus Status { get; }
        public string Reason { get; }

        private MoveValidationResult(MoveValidationStatus status, string reason)
        {
            Status = status;
            Reason = reason;
        }

        public static MoveValidationResult Success()
        {
            return new MoveValidationResult(MoveValidationStatus.Valid, string.Empty);
        }

        public static MoveValidationResult Failure(MoveValidationStatus status, string reason)
        {
            return new MoveValidationResult(status, reason);
        }
    }
}
