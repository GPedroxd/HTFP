namespace HTFP.Shared.Models;

public sealed class MainFile
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string FilePath { get; init; } = default!;
    public int TotalLines { get; private set; }
    public int TotalSubFiles { get; private set; }
    public FileStatus Status { get; private set; } = FileStatus.Created;
    public DateTime StartProcessingDate { get; private set; } = DateTime.UtcNow;
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

    public void SetAsFinished()
    {
        
    }
}