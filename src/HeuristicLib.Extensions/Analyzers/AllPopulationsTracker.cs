using HEAL.HeuristicLib.Analysis;
using HEAL.HeuristicLib.Execution;
using HEAL.HeuristicLib.Operators;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.SearchSpaces;
using HEAL.HeuristicLib.States;

namespace HEAL.HeuristicLib.Analyzers;

public class AllPopulationsTracker<T, TS, TP, TR>(IInterceptor<T, TS, TP, TR> interceptor)
  : IAnalyzer<IReadOnlyList<ISolution<T>[]>, AllPopulationsTracker<T, TS, TP, TR>.Instance>
  where TS : class, ISearchSpace<T>
  where TP : class, IProblem<T, TS>
  where TR : PopulationState<T>
{
  public IInterceptor<T, TS, TP, TR> Interceptor { get; } = interceptor;

  public Instance CreateAnalyzerInstance(Run run) => new(run, this);

  public sealed class Instance(Run run, AllPopulationsTracker<T, TS, TP, TR> analyzer)
    : AnalyzerRunInstance<AllPopulationsTracker<T, TS, TP, TR>, IReadOnlyList<ISolution<T>[]>>(run, analyzer)
  {
    public List<ISolution<T>[]> AllSolutions { get; } = [];

    public override void RegisterTaps(IAnalyzerTapRegistry taps)
    {
      taps.Register(analyzer.Interceptor, AfterInterception);
    }

    public void AfterInterception(TR newState, TR currentState, TR? previousState, TS searchSpace, TP problem)
    {
      AllSolutions.Add(currentState.Population.ToArray());
      PublishResult(AllSolutions.ToArray());
    }
  }
}
