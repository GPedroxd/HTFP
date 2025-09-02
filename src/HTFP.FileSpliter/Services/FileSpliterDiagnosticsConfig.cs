using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace HTFP.FileSpliter.Services;

public static class FileSpliterDiagnosticsConfig
{
    public const string ServiceName = "HTFP-FileSpliter";

    public static readonly Meter Meter = new(ServiceName);
    public static Histogram<long> FileSize = Meter.CreateHistogram<long>("htfp.filespliter.file.size", "bytes", "File size in bytes");
    public static Histogram<double> SplitTime = Meter.CreateHistogram<double>("htfp.filespliter.file.splittime", "ms", "Time taken to split the file");
    public static ActivitySource ActivitySource = new(ServiceName);
}