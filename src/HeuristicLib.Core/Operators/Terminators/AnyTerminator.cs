using Generator.Equals;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.SearchSpaces;
using HEAL.HeuristicLib.States;

namespace HEAL.HeuristicLib.Operators.Terminators;

[Equatable]
public partial record AnyTerminator<TGenotype, TAlgorithmState, TSearchSpace, TProblem>
  : CompositeTerminator<TGenotype, TAlgorithmState, TSearchSpace, TProblem>
  where TAlgorithmState : class, IAlgorithmState
  where TSearchSpace : class, ISearchSpace<TGenotype>
  where TProblem : class, IProblem<TGenotype, TSearchSpace>
{
  public AnyTerminator(params ImmutableArray<ITerminator<TGenotype, TAlgorithmState, TSearchSpace, TProblem>> terminators)
    : base(terminators)
  {
  }

  protected override bool ShouldTerminate(TAlgorithmState algorithmState,
    IReadOnlyList<ITerminatorInstance<TGenotype, TAlgorithmState, TSearchSpace, TProblem>> innerTerminators,
    TSearchSpace searchSpace, TProblem problem)
  {
    return innerTerminators.Any(t => t.ShouldTerminate(algorithmState, searchSpace, problem));
  }
}
