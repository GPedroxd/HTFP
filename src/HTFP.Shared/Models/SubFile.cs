namespace HTFP.Shared.Models;

public sealed class SubFile
{
    private SubFile() { }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string Name { get; init; } = default!;
    public string Path { get; init; } = default!;
    public bool HasDivergentsOrders { get; private set; }
    public string? OutputPath { get; private set; }
    public Guid ReconciliationFileId { get; init; }
    public int TotalLines { get; init; }    
    internal IList<StatusRecord> _statusRecords = new List<StatusRecord>();
    public IReadOnlyCollection<StatusRecord> StatusRecords { get => _statusRecords.AsReadOnly(); }
    public string Status { get => _statusRecords.Last().Status.ToString(); private set { } }
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

    public SubFile(string name, string path, Guid reconciliationFileId, int totalLines)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Value cannot be null or empty.", nameof(name));

        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Value cannot be null or empty.", nameof(path));

        if (totalLines <= 0)
            throw new ArgumentOutOfRangeException(nameof(totalLines), "Total lines must be greater than zero.");

        Name = name;
        Path = path;
        ReconciliationFileId = reconciliationFileId;
        TotalLines = totalLines;
        
        _statusRecords.Add(new StatusRecord { Status = FileStatus.Created, Timestamp = DateTime.UtcNow, Message = "Sub-file record created." });
    }

    public void MarkAsProcessing()
    {
        StartProcessingDate = DateTime.UtcNow;

        _statusRecords.Add(new StatusRecord
        {
            Status = FileStatus.Processing,
            Timestamp = DateTime.UtcNow,
            Message = "Sub-file processing started."
        });
    }

    public void MarkasAsProcessed(int divergentOrdersCount)
    {
        HasDivergentsOrders = divergentOrdersCount > 0;

        _statusRecords.Add(new StatusRecord
        {
            Status = FileStatus.SuccessfullyProcessed,
            Timestamp = DateTime.UtcNow,
            Message = $"File processed with {divergentOrdersCount} divergent orders."
        });

        EndProcessingDate = DateTime.UtcNow;
        
        if (HasDivergentsOrders)
            OutputPath = $"{ReconciliationFileId}/subfilesOutput/{Name}-divergents.csv";
    }

    public void MarkAsPartiallyProcessed()
    {
        _statusRecords.Add(new StatusRecord
        {
            Status = FileStatus.PartiallyProcessed,
            Timestamp = DateTime.UtcNow,
            Message = "File processed but no orders were found."
        });

        EndProcessingDate = DateTime.UtcNow;

        if (HasDivergentsOrders)
            OutputPath = $"{ReconciliationFileId}/subfilesOutput/{Name}-divergents.csv";
    }

    public void SetAsError(string message)
    {
        _statusRecords.Add(new StatusRecord
        {
            Status = FileStatus.Error,
            Timestamp = DateTime.UtcNow,
            Message = message
        });

        EndProcessingDate = DateTime.UtcNow;
    }
}