using HEAL.HeuristicLib.Analysis;
using HEAL.HeuristicLib.Execution;
using HEAL.HeuristicLib.Operators;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.SearchSpaces;
using HEAL.HeuristicLib.States;

namespace HEAL.HeuristicLib.Analyzers;

public class BestMedianWorstPerEvaluationAnalysis<TGenotype, TSearchSpace, TProblem>
  (IEvaluator<TGenotype, TSearchSpace, TProblem> evaluator,
   IInterceptor<TGenotype, TSearchSpace, TProblem, PopulationState<TGenotype>> interceptor)
  : IAnalyzer<IReadOnlyList<(int evaluations, BestMedianWorstEntry<TGenotype> entry)>, BestMedianWorstPerEvaluationAnalysis<TGenotype, TSearchSpace, TProblem>.Instance>
  where TSearchSpace : class, ISearchSpace<TGenotype>
  where TProblem : class, IProblem<TGenotype, TSearchSpace>
{
  public IEvaluator<TGenotype, TSearchSpace, TProblem> Evaluator { get; } = evaluator;
  public IInterceptor<TGenotype, TSearchSpace, TProblem, PopulationState<TGenotype>> Interceptor { get; } = interceptor;

  public Instance CreateAnalyzerInstance(Run run) => new(run, this);

  public sealed class Instance(Run run, BestMedianWorstPerEvaluationAnalysis<TGenotype, TSearchSpace, TProblem> analyzer) :
    AnalyzerRunInstance<BestMedianWorstPerEvaluationAnalysis<TGenotype, TSearchSpace, TProblem>, IReadOnlyList<(int evaluations, BestMedianWorstEntry<TGenotype> entry)>>(run, analyzer)
  {
    private int currentEvaluationsCount;
    private readonly List<(int, BestMedianWorstEntry<TGenotype>)> bestSolutions = [];
    public IReadOnlyList<(int, BestMedianWorstEntry<TGenotype>)> BestSolutions => bestSolutions;

    public override void RegisterTaps(IAnalyzerTapRegistry taps)
    {
      taps.Register(Analyzer.Evaluator, AfterEvaluation);
      taps.Register(Analyzer.Interceptor, AfterInterception);
    }

    public void AfterEvaluation(IReadOnlyList<TGenotype> genotypes, IReadOnlyList<ObjectiveVector> objectiveVectors, TSearchSpace searchSpace, TProblem problem)
    {
      currentEvaluationsCount += genotypes.Count;
    }

    public void AfterInterception(PopulationState<TGenotype> newState, PopulationState<TGenotype> currentState, PopulationState<TGenotype>? previousState, TSearchSpace searchSpace, TProblem problem)
    {
      if (currentState.Population.Solutions.Length == 0)
        throw new InvalidOperationException("Population is empty, cannot determine best/median/worst solution.");

      var comp = problem.Objective.TotalOrderComparer is NoTotalOrderComparer ? new LexicographicComparer(problem.Objective.Directions) : problem.Objective.TotalOrderComparer;
      var ordered = currentState.Population.OrderBy(keySelector: x => x.ObjectiveVector, comp).ToArray();

      bestSolutions.Add((currentEvaluationsCount, new BestMedianWorstEntry<TGenotype>(ordered[0], ordered[ordered.Length / 2], ordered[^1])));
      PublishResult(bestSolutions.ToArray());
    }
  }
}
