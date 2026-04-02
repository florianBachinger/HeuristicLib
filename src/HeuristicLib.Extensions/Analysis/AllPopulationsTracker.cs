using HEAL.HeuristicLib.Operators;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.SearchSpaces;
using HEAL.HeuristicLib.States;

namespace HEAL.HeuristicLib.Analysis;

public class AllPopulationsTracker<T, TS, TP, TR>(IInterceptor<T, TS, TP, TR> interceptor)
  : IAnalyzer<AllPopulationsTracker<T, TS, TP, TR>.State>
  where TS : class, ISearchSpace<T>
  where TP : class, IProblem<T, TS>
  where TR : PopulationState<T>
{
  public IInterceptor<T, TS, TP, TR> Interceptor { get; } = interceptor;

  public State CreateAnalyzerState() => new(this);

  public sealed class State(AllPopulationsTracker<T, TS, TP, TR> analyzer)
    : AnalyzerRunState<AllPopulationsTracker<T, TS, TP, TR>>(analyzer)
  {
    private readonly List<ISolution<T>[]> allSolutions = [];

    public IReadOnlyList<ISolution<T>[]> AllSolutions => allSolutions;

    public override void RegisterObservations(IObservationRegistry observationRegistry)
    {
      observationRegistry.Add(Analyzer.Interceptor, AfterInterception);
    }

    public void AfterInterception(TR newState, TR currentState, TR? previousState, TS searchSpace, TP problem)
    {
      allSolutions.Add(currentState.Population.ToArray());
    }
  }
}
