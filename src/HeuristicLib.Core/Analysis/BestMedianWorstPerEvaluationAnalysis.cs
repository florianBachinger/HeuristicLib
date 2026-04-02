using HEAL.HeuristicLib.Operators;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.SearchSpaces;
using HEAL.HeuristicLib.States;

namespace HEAL.HeuristicLib.Analysis;

public class BestMedianWorstPerEvaluationAnalysis<TGenotype, TSearchSpace, TProblem>
  : IAnalyzer<BestMedianWorstPerEvaluationAnalysis<TGenotype, TSearchSpace, TProblem>.State>
  where TSearchSpace : class, ISearchSpace<TGenotype>
  where TProblem : class, IProblem<TGenotype, TSearchSpace>
{
  public IEvaluator<TGenotype, TSearchSpace, TProblem> Evaluator { get; }
  public IInterceptor<TGenotype, TSearchSpace, TProblem, PopulationState<TGenotype>> Interceptor { get; }

  public BestMedianWorstPerEvaluationAnalysis(
    IEvaluator<TGenotype, TSearchSpace, TProblem> evaluator,
    IInterceptor<TGenotype, TSearchSpace, TProblem, PopulationState<TGenotype>> interceptor)
  {
    Evaluator = evaluator;
    Interceptor = interceptor;
  }

  public State CreateAnalyzerState() => new(this);

  public sealed class State(BestMedianWorstPerEvaluationAnalysis<TGenotype, TSearchSpace, TProblem> analyzer)
    : AnalyzerRunState<BestMedianWorstPerEvaluationAnalysis<TGenotype, TSearchSpace, TProblem>>(analyzer)
  {
    private int currentEvaluationsCount;
    private readonly List<(int evaluations, BestMedianWorstEntry<TGenotype> entry)> bestSolutions = [];

    public IReadOnlyList<(int evaluations, BestMedianWorstEntry<TGenotype> entry)> BestSolutions => bestSolutions;

    public override void RegisterObservations(IObservationRegistry observationRegistry)
    {
      observationRegistry.Add(Analyzer.Evaluator, AfterEvaluation);
      observationRegistry.Add(Analyzer.Interceptor, AfterInterception);
    }

    private void AfterEvaluation(IReadOnlyList<TGenotype> genotypes, IReadOnlyList<ObjectiveVector> objectiveVectors, TSearchSpace searchSpace, TProblem problem)
    {
      currentEvaluationsCount += genotypes.Count;
    }

    private void AfterInterception(PopulationState<TGenotype> newState, PopulationState<TGenotype> currentState, PopulationState<TGenotype>? previousState, TSearchSpace searchSpace, TProblem problem)
    {
      if (currentState.Population.Solutions.Length == 0) {
        throw new InvalidOperationException("Population is empty, cannot determine best/median/worst solution.");
      }

      var comp = problem.Objective.TotalOrderComparer is NoTotalOrderComparer ? new LexicographicComparer(problem.Objective.Directions) : problem.Objective.TotalOrderComparer;
      var ordered = currentState.Population.OrderBy(keySelector: x => x.ObjectiveVector, comp).ToArray();

      bestSolutions.Add((currentEvaluationsCount, new BestMedianWorstEntry<TGenotype>(ordered[0], ordered[ordered.Length / 2], ordered[^1])));
    }
  }
}
