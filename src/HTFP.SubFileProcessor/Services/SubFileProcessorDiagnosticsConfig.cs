using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace HTFP.SubFileProcessor.Services;

public sealed class SubFileProcessorDiagnosticsConfig
{
    public const string ServiceName = "HTFP-SubFileProcessor";

    public static readonly Meter Meter = new(ServiceName);
    public static Histogram<long> FileSize = Meter.CreateHistogram<long>("htfp.subfileprocessor.file.size", "bytes", "File size in bytes");
    public static Histogram<double> FileProcessTime = Meter.CreateHistogram<double>("htfp.subfileprocessor.file.processtime", "ms", "Time taken to process the file");
    public static Histogram<double> LineProcessTime = Meter.CreateHistogram<double>("htfp.subfileprocessor.line.processtime", "ms", "Time taken to process a line");
    public static ActivitySource ActivitySource = new(ServiceName);
}