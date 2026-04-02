using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.SearchSpaces;
using HEAL.HeuristicLib.States;

namespace HEAL.HeuristicLib.Operators;

public static class TerminatorExtension
{
  extension<TGenotype, TSearchSpace, TProblem, TAlgorithmState>(ITerminatorInstance<TGenotype, TSearchSpace, TProblem, TAlgorithmState> terminatorInstance)
    where TSearchSpace : class, ISearchSpace<TGenotype>
    where TProblem : IProblem<TGenotype, TSearchSpace>
    where TAlgorithmState : IAlgorithmState
  {
    public bool ShouldContinue(TSearchSpace searchSpace, TProblem problem, TAlgorithmState state)
    {
      return !terminatorInstance.ShouldTerminate(state, searchSpace, problem);
    }
  }
}

