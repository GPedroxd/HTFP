namespace HTFP.Shared.Models;

public enum FileStatus
{
    None = 0,
    Created = 1,
    Processing = 2,
    SuccessfullyProcessed = 3,
    PartiallyProcessed = 4,
    Error = 5
}