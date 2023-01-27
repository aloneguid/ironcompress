using System.IO.Compression;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using IronCompress;

BenchmarkRunner.Run<SpeedBenchmark>();

[ShortRunJob]
[MarkdownExporter]
[MemoryDiagnoser]
public class SpeedBenchmark {
    private byte[]? _inputBuffer;

    [GlobalSetup]
    public void Setup() {
        _inputBuffer = Encoding.UTF8.GetBytes(string.Join("", Enumerable.Repeat("great compression", 1000)));
    }

    [Benchmark]
    public void SnappyNative() {
        var iron = new Iron();
        iron.ForcePlatform = Platform.Native;

        iron.Compress(Codec.Snappy, _inputBuffer.AsSpan(), null);
    }

    [Benchmark]
    public void SnappyManaged() {
        var iron = new Iron();
        iron.ForcePlatform = Platform.Managed;

        iron.Compress(Codec.Snappy, _inputBuffer.AsSpan(), null);
    }
}
