using HEAL.HeuristicLib.Analysis;
using HEAL.HeuristicLib.Execution;
using HEAL.HeuristicLib.Operators;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.SearchSpaces;

namespace HEAL.HeuristicLib.Analyzers;

public class QualityCurveAnalysis<TGenotype, TSearchSpace, TProblem>(IEvaluator<TGenotype, TSearchSpace, TProblem> evaluator)
  : IAnalyzer<QualityCurveAnalysis<TGenotype, TSearchSpace, TProblem>.State>
  where TSearchSpace : class, ISearchSpace<TGenotype>
  where TProblem : class, IProblem<TGenotype, TSearchSpace>
{
  public IEvaluator<TGenotype, TSearchSpace, TProblem> Evaluator { get; } = evaluator;

  public State CreateAnalyzerState(Run run) => new(run, this);

  public sealed class State(Run run, QualityCurveAnalysis<TGenotype, TSearchSpace, TProblem> analyzer)
    : AnalyzerRunState<QualityCurveAnalysis<TGenotype, TSearchSpace, TProblem>>(run, analyzer)
  {
    private readonly List<(ISolution<TGenotype> best, int evalCount)> currentState = [];
    private ISolution<TGenotype>? best;
    private int evalCount;

    public IReadOnlyList<(ISolution<TGenotype> best, int evalCount)> CurrentState => currentState;

    public override void RegisterObservations(IObservationRegistry observationRegistry)
    {
      observationRegistry.Add(Analyzer.Evaluator, AfterEvaluation);
    }

    public void AfterEvaluation(IReadOnlyList<TGenotype> genotypes, IReadOnlyList<ObjectiveVector> objectiveVectors, TSearchSpace searchSpace, TProblem problem)
    {
      for (var i = 0; i < genotypes.Count; i++) {
        var genotype = genotypes[i];
        var q = objectiveVectors[i];
        evalCount++;

        if (best is not null) {
          var comp = problem.Objective.TotalOrderComparer;
          if (NoTotalOrderComparer.Instance.Equals(comp)) {
            comp = new LexicographicComparer(problem.Objective.Directions);
          }

          if (comp.Compare(q, best.ObjectiveVector) >= 0) {
            continue;
          }
        }

        best = new Solution<TGenotype>(genotype, q);
        currentState.Add((best, evalCount));
      }
    }
  }
}
