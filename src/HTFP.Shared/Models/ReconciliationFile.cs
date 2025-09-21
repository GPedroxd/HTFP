using Grpc.Core;

namespace HTFP.Shared.Models;

public sealed class ReconciliationFile
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string Name { get; init; } = default!;
    public string Path { get; init; } = default!;
    public string OutputPath { get; init; } = default!;
    public string SubfilePath { get; init; } = default!;
    private int _totalLines;
    public int TotalLines { get => _totalLines; private set => _totalLines = value; }
    private int _totalSubFiles;
    public int TotalSubFiles { get => _totalSubFiles; private set => _totalSubFiles = value; }     
    private IList<StatusRecord> _statusRecords = new List<StatusRecord>();
    public IReadOnlyCollection<StatusRecord> StatusRecords { get => _statusRecords.AsReadOnly(); }
    public FileStatus CurrentStatus { get => _statusRecords.Last().Status; }
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

    private ReconciliationFile() { }

    public ReconciliationFile(string name, string path)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Value cannot be null or empty.", nameof(name));

        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Value cannot be null or empty.", nameof(path));

        Name = name;
        Path = path;
        SubfilePath = $"{Id}/Subfiles/";
        OutputPath = $"{Id}/Output/";
        _statusRecords.Add(new StatusRecord { Status = FileStatus.Created, Timestamp = DateTime.UtcNow, Message = "File record created." });
    }

    public void IncrementSplitFileCount()
        => Interlocked.Increment(ref _totalSubFiles);

    public void IncrementLineCount(int count)
        => Interlocked.Add(ref _totalLines, count);

    public void SetAsSuccessfullyFinished()
    {
        if(_statusRecords.Last().Status == FileStatus.SuccessfullyProcessed)
            return;

        _statusRecords.Add(new StatusRecord { Status = FileStatus.SuccessfullyProcessed, Timestamp = DateTime.UtcNow, Message = "Reconciliation finished successfully." });
        EndProcessingDate = DateTime.UtcNow;
    }

    public void SetAsPartiallyProcessed()
    {
        if(_statusRecords.Last().Status == FileStatus.PartiallyProcessed)
            return;

        _statusRecords.Add(new StatusRecord { Status = FileStatus.PartiallyProcessed, Timestamp = DateTime.UtcNow, Message = "One or more subfiles are not processed correctly." });
        EndProcessingDate = DateTime.UtcNow;
    }

    public void SetAsError(string message)
    {
        if(string.IsNullOrEmpty(message))
            throw new ArgumentException("Value cannot be null or empty.", nameof(message));

        _statusRecords.Add(new StatusRecord { Status = FileStatus.Error, Timestamp = DateTime.UtcNow, Message = message });
        EndProcessingDate = DateTime.UtcNow;
    }

    public void SetAsProcessing()
    {
        if(_statusRecords.Last().Status == FileStatus.Processing)
            return;

        _statusRecords.Add(new StatusRecord { Status = FileStatus.Processing, Timestamp = DateTime.UtcNow, Message = "File is being processed." });
    }
}