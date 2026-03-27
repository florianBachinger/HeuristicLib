using HEAL.HeuristicLib.Analysis;
using HEAL.HeuristicLib.Execution;
using HEAL.HeuristicLib.Operators;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.SearchSpaces;
using HEAL.HeuristicLib.States;

namespace HEAL.HeuristicLib.Analyzers;

public record BestMedianWorstEntry<T>(ISolution<T> Best, ISolution<T> Median, ISolution<T> Worst);

public class BestMedianWorstAnalysis<T, TS, TP, TR> 
  : IAnalyzer<BestMedianWorstAnalysis<T, TS, TP, TR>.State>
  where TS : class, ISearchSpace<T>
  where TP : class, IProblem<T, TS>
  where TR : PopulationState<T>
{
  public BestMedianWorstAnalysis(IInterceptor<T, TS, TP, TR> interceptor)
  {
    Interceptor = interceptor;
  }

  public IInterceptor<T, TS, TP, TR> Interceptor { get; }

  public State CreateAnalyzerState(Run run) => new(Interceptor);

  public sealed class State : AnalyzerRunState
  {
    private readonly IInterceptor<T, TS, TP, TR> interceptor;
    
    private readonly List<BestMedianWorstEntry<T>> bestSolutions = [];
    public IReadOnlyList<BestMedianWorstEntry<T>> BestSolutions => bestSolutions;

    public State(IInterceptor<T, TS, TP, TR> interceptor)
    {
      this.interceptor = interceptor;
    }

    public override void RegisterObservations(IObservationRegistry observationRegistry)
    {
      observationRegistry.Add(interceptor, AfterInterception);
    }

    private void AfterInterception(TR newState, TR currentState, TR? previousState, TS searchSpace, TP problem)
    {
      var comp = problem.Objective.TotalOrderComparer is NoTotalOrderComparer ? new LexicographicComparer(problem.Objective.Directions) : problem.Objective.TotalOrderComparer;
      var ordered = currentState.Population.OrderBy(keySelector: x => x.ObjectiveVector, comp).ToArray();
      if (ordered.Length == 0) {
        bestSolutions.Add(null!);

        return;
      }

      bestSolutions.Add(new BestMedianWorstEntry<T>(ordered[0], ordered[ordered.Length / 2], ordered[^1]));
    }
  }
}
