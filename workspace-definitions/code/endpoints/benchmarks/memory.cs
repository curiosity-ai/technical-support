[endpoint: Curiosity.Endpoints.Path("benchmarks/memory")]
[endpoint: Curiosity.Endpoints.ReadOnly]
[endpoint: Curiosity.Endpoints.AccessMode("AdminOnly")]

const int BandwidthMemSize = 512 * 1_024 * 1_024; //512MB
const int LatencyMemSize = 64 * 1_024 * 1_024; // 64MB
const int BandwidthArraySize = BandwidthMemSize / sizeof(int);
const int LatencyArraySize = LatencyMemSize / sizeof(int);

const int NumIterations = 8;

var threads = Enumerable.Range(1, Environment.ProcessorCount)
                 .Select(c => (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)c))
                 .Distinct()
                 .ToArray();

return threads.Select(c => RunTest($"Bandwidth MB/s {c} thread{(c > 1 ? "s" : "")}", () => MeasureBandwidth(c)))
       .Concat(threads.Select(c => RunTest($"Latency ns {c} thread{(c > 1 ? "s" : "")}", () => MeasureLatency(c))));

BenchmarkResult RunTest(string label, Func<double> benchmarkFunc)
{
    benchmarkFunc(); //Warm-up run
    var results = new List<double>();
    for (int i = 0; i < NumIterations; i++)
    {
        RelayStatusAsync($"Running: {label} iteration {i + 1}/{NumIterations}");
        Logger.LogInformation("Running: {0} iteration {1}/{2}", label, i + 1, NumIterations);
        GCHelper.CompactCollect();
        var result = benchmarkFunc();
        results.Add(result);
    }

    var avg = results.Average();
    var std = Math.Sqrt(results.Select(val => (val - avg) * (val - avg)).Sum() / NumIterations);

    return new BenchmarkResult
    {
        Label = label,
        Avg = avg,
        Std = std,
        Max = results.Max(),
        Min = results.Min(),
        Results = results.ToArray()
    };
}

double MeasureBandwidth(int coreCount)
{
    var mre = new ManualResetEvent(false);
    var threads = Enumerable.Range(0, coreCount).Select(v => new Thread(() =>
    {
        var array = new int[BandwidthArraySize / coreCount];
        mre.WaitOne();
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = i;
        }
    })).ToArray();

    Array.ForEach(threads, t => t.Start());

    var sw = ValueStopwatch.StartNew();
    mre.Set();
    Array.ForEach(threads, t => t.Join());
    double mb = (BandwidthArraySize / coreCount) * coreCount * sizeof(int) / (1024.0 * 1024.0);
    return mb / sw.GetElapsedTime().TotalSeconds; // MB/s
}

double MeasureLatency(int coreCount)
{
    int[] array = Enumerable.Range(0, LatencyArraySize).ToArray();
    int accessesPerThread = LatencyArraySize / coreCount;
    var mre = new ManualResetEvent(false);
    var threads = Enumerable.Range(0, coreCount).Select(v => new Thread(() =>
    {
        var rng = new Random(v);
        int sum = 0;
        mre.WaitOne();
        for (int i = 0; i < accessesPerThread; i++)
        {
            int index = rng.Next(LatencyArraySize);
            sum += array[index];
        }
    })).ToArray();

    Array.ForEach(threads, t => t.Start());
    var sw = ValueStopwatch.StartNew();
    mre.Set();
    Array.ForEach(threads, t => t.Join());
    return sw.GetElapsedTime().TotalNanoseconds / (accessesPerThread * coreCount); // ns per access
}

class BenchmarkResult
{
    public string Label { get; set; }
    public double Avg { get; set; }
    public double Std { get; set; }
    public double Max { get; set; }
    public double Min { get; set; }
    public double[] Results { get; set; }
}
