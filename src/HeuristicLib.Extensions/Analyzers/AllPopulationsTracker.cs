using HEAL.HeuristicLib.Analysis;
using HEAL.HeuristicLib.Execution;
using HEAL.HeuristicLib.Operators.Interceptors;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.SearchSpaces;
using HEAL.HeuristicLib.States;

namespace HEAL.HeuristicLib.Analyzers;

public class AllPopulationsTracker<T>
  : IAnalyzer<IReadOnlyList<ISolution<T>[]>, AllPopulationsTracker<T>.Instance>,
    IInterceptorObserver<T, PopulationState<T>>
{
  public Instance CreateAnalyzerInstance(Run run) => new(run, this);

  public IInterceptorObserverInstance<T, ISearchSpace<T>, IProblem<T, ISearchSpace<T>>, PopulationState<T>> CreateExecutionInstance(ExecutionInstanceRegistry instanceRegistry)
    => instanceRegistry.Run.ResolveAnalyzer(this);

  public sealed class Instance(Run run, AllPopulationsTracker<T> analyzer)
    : AnalyzerRunInstance<AllPopulationsTracker<T>, IReadOnlyList<ISolution<T>[]>>(run, analyzer),
      IInterceptorObserverInstance<T, ISearchSpace<T>, IProblem<T, ISearchSpace<T>>, PopulationState<T>>
  {
    public List<ISolution<T>[]> AllSolutions { get; } = [];

    public void AfterInterception(PopulationState<T> newState, PopulationState<T> currentState, PopulationState<T>? previousState, ISearchSpace<T> searchSpace, IProblem<T, ISearchSpace<T>> problem)
    {
      AllSolutions.Add(currentState.Population.ToArray());
      PublishResult(AllSolutions.ToArray());
    }
  }
}
