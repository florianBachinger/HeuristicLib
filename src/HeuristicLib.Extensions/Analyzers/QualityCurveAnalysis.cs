using HEAL.HeuristicLib.Execution;
using HEAL.HeuristicLib.Operators.Evaluators;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.Random;
using HEAL.HeuristicLib.SearchSpaces;

namespace HEAL.HeuristicLib.Analyzers;

public class QualityCurveAnalysis<TGenotype> : IEvaluatorObserver<TGenotype> where TGenotype : class
{
  public IEvaluatorObserverInstance<TGenotype, ISearchSpace<TGenotype>, IProblem<TGenotype, ISearchSpace<TGenotype>>> CreateExecutionInstance(ExecutionInstanceRegistry instanceRegistry)
    => new Instance();

  public sealed class Instance : IEvaluatorObserverInstance<TGenotype, ISearchSpace<TGenotype>, IProblem<TGenotype, ISearchSpace<TGenotype>>>
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
      }
    }
  }
}
