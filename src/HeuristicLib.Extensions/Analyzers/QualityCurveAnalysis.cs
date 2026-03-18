using HEAL.HeuristicLib.Analysis;
using HEAL.HeuristicLib.Execution;
using HEAL.HeuristicLib.Operators.Evaluators;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.SearchSpaces;

namespace HEAL.HeuristicLib.Analyzers;

public class QualityCurveAnalysis<TGenotype>
  : IAnalyzer<IReadOnlyList<(ISolution<TGenotype> best, int evalCount)>, QualityCurveAnalysis<TGenotype>.Instance>,
    IEvaluatorObserver<TGenotype>
{
  public Instance CreateAnalyzerInstance(Run run) => new(run, this);

  public IEvaluatorObserverInstance<TGenotype, ISearchSpace<TGenotype>, IProblem<TGenotype, ISearchSpace<TGenotype>>> CreateExecutionInstance(ExecutionInstanceRegistry instanceRegistry)
    => instanceRegistry.Run.ResolveAnalyzer(this);

  public sealed class Instance(Run run, QualityCurveAnalysis<TGenotype> analyzer)
    : AnalyzerRunInstance<QualityCurveAnalysis<TGenotype>, IReadOnlyList<(ISolution<TGenotype> best, int evalCount)>>(run, analyzer),
      IEvaluatorObserverInstance<TGenotype, ISearchSpace<TGenotype>, IProblem<TGenotype, ISearchSpace<TGenotype>>>
  {
    public readonly List<(ISolution<TGenotype> best, int evalCount)> CurrentState = [];
    private ISolution<TGenotype>? best;
    private int evalCount;

    public void AfterEvaluation(IReadOnlyList<TGenotype> genotypes, IReadOnlyList<ObjectiveVector> objectiveVectors, ISearchSpace<TGenotype> searchSpace, IProblem<TGenotype, ISearchSpace<TGenotype>> problem)
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
