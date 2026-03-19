using HEAL.HeuristicLib.Analysis;
using HEAL.HeuristicLib.Execution;
using HEAL.HeuristicLib.Operators;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.SearchSpaces;

namespace HEAL.HeuristicLib.Analyzers;

public class QualityCurveAnalysis<TGenotype, TSearchSpace, TProblem>(IEvaluator<TGenotype, TSearchSpace, TProblem> evaluator)
  : IAnalyzer<IReadOnlyList<(ISolution<TGenotype> best, int evalCount)>, QualityCurveAnalysis<TGenotype, TSearchSpace, TProblem>.Instance>
  where TSearchSpace : class, ISearchSpace<TGenotype>
  where TProblem : class, IProblem<TGenotype, TSearchSpace>
{
  public IEvaluator<TGenotype, TSearchSpace, TProblem> Evaluator { get; } = evaluator;

  public Instance CreateAnalyzerInstance(Run run) => new(run, this);

  public sealed class Instance(Run run, QualityCurveAnalysis<TGenotype, TSearchSpace, TProblem> analyzer)
    : AnalyzerRunInstance<QualityCurveAnalysis<TGenotype, TSearchSpace, TProblem>, IReadOnlyList<(ISolution<TGenotype> best, int evalCount)>>(run, analyzer)
  {
    public readonly List<(ISolution<TGenotype> best, int evalCount)> CurrentState = [];
    private ISolution<TGenotype>? best;
    private int evalCount;

    public override void RegisterTaps(IAnalyzerTapRegistry taps)
    {
      taps.Register(analyzer.Evaluator, AfterEvaluation);
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
        CurrentState.Add((best, evalCount));
        PublishResult(CurrentState.ToArray());
      }
    }
  }
}
