namespace HTFP.Shared.Models;

public sealed class SubFile
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string Name { get; init; } = default!;
    public Guid MainFileId { get; init; }
    public int TotalLines { get; private set; }
    public FileStatus Status { get; private set; } = FileStatus.Created;
    public DateTime? StartProcessingDate { get; private set; }
    public DateTime? EndProcessingDate { get; private set; }
    public TimeSpan? ProcessingTime
    {
        get
        {
            if (EndProcessingDate is null) return null;

            return StartProcessingDate - EndProcessingDate;
        }
    }
    public string ProcessingTimeInPlanText
    {
        get
        {
            if (ProcessingTime is null)
                return "Not finished yet.";

            return ProcessingTime.Value.ToString(@"hh\:mm\:ss");
        }
    }
}