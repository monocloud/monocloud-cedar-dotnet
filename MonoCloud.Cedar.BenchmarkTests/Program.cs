using BenchmarkDotNet.Running;
using MonoCloud.Cedar.BenchmarkTests;

BenchmarkSwitcher.FromAssembly(typeof(AuthorizationBenchmark).Assembly).Run(args);
