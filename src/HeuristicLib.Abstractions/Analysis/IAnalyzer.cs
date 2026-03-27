using HEAL.HeuristicLib.Execution;

namespace HEAL.HeuristicLib.Analysis;

public interface IAnalyzer
{
  IAnalyzerRunState CreateAnalyzerState(Run run);
}

public interface IAnalyzer<out TAnalyzerRunState> : IAnalyzer
  where TAnalyzerRunState : class, IAnalyzerRunState
{
  // ToDo: think if we ned an abstract system of "scope" instead of explicit Execution and Run scope that we currently have.
  new TAnalyzerRunState CreateAnalyzerState(Run run);

  IAnalyzerRunState IAnalyzer.CreateAnalyzerState(Run run) => CreateAnalyzerState(run);
}

public interface IAnalyzerRunState : IExecutionInstance
{
  void RegisterObservations(IObservationRegistry observationRegistry);
}

public abstract class AnalyzerRunState : IAnalyzerRunState
{
  public abstract void RegisterObservations(IObservationRegistry observationRegistry);
}

public abstract class AnalyzerRunState<TAnalyzer>(Run run, TAnalyzer analyzer) : AnalyzerRunState
  where TAnalyzer : IAnalyzer
{
  protected Run Run { get; } = run;

  protected TAnalyzer Analyzer { get; } = analyzer;
}

