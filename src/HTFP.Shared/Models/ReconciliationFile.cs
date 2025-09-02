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
    }

    public void IncrementSplitFileCount()
        => Interlocked.Increment(ref _totalSubFiles);

    public void IncrementLineCount(int count)
        => Interlocked.Add(ref _totalLines, count);

    public void SetAsFinished()
    {
        throw new NotImplementedException();
    }
}