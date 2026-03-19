using HEAL.HeuristicLib.Execution;

namespace HEAL.HeuristicLib.Analysis;

public interface IAnalyzer
{
  IAnalyzerRunInstance CreateAnalyzerInstance(Run run);
}

public interface IAnalyzer<out TResult> : IAnalyzer;

public interface IAnalyzer<TResult, out TAnalyzerRunInstance> : IAnalyzer<TResult>
  where TResult : notnull
  where TAnalyzerRunInstance : class, IAnalyzerRunInstance
{
  // ToDo: think if we ned an abstract system of "scope" instead of explicit Execution and Run scope that we currently have.
  new TAnalyzerRunInstance CreateAnalyzerInstance(Run run);

  IAnalyzerRunInstance IAnalyzer.CreateAnalyzerInstance(Run run) => CreateAnalyzerInstance(run);
}

public interface IAnalyzerRunInstance : IExecutionInstance
{
  void RegisterTaps(IAnalyzerTapRegistry taps);
}

public abstract class AnalyzerRunInstance<TAnalyzer, TResult>(Run run, TAnalyzer analyzer) : IAnalyzerRunInstance
  where TAnalyzer : class, IAnalyzer<TResult>
  where TResult : notnull
{
  protected Run Run { get; } = run;

  protected TAnalyzer Analyzer { get; } = analyzer;

  protected void PublishResult(TResult result)
  {
	  Run.SetResult(Analyzer, result);
  }

  public abstract void RegisterTaps(IAnalyzerTapRegistry taps);
}
