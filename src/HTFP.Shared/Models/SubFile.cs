namespace HTFP.Shared.Models;

public sealed class SubFile
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string Name { get; init; } = default!;
    public string Path { get; init; } = default!;
    public bool HasDivergentsOrders { get; private set; }
    public string? OutputPath { get; private set; } 
    public Guid ReconciliationFileId { get; init; }
    public int TotalLines { get; init; }
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

    public void MarkasAsProcessed(int divergentOrdersCount)
    {
        HasDivergentsOrders = divergentOrdersCount > 0;
        Status = FileStatus.SuccessfullyProcessed;
        EndProcessingDate = DateTime.UtcNow;

        if (HasDivergentsOrders)
            OutputPath = $"{ReconciliationFileId}/subfilesOutput/{Name}-divergents.csv";
    }
}