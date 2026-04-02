using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.SearchSpaces;
using HEAL.HeuristicLib.States;

namespace HEAL.HeuristicLib.Operators;

public interface ITerminator<TGenotype, in TSearchSpace, in TProblem, in TAlgorithmState>
  : IOperator<ITerminatorInstance<TGenotype, TSearchSpace, TProblem, TAlgorithmState>>
  where TSearchSpace : class, ISearchSpace<TGenotype>
  where TProblem : IProblem<TGenotype, TSearchSpace>
  where TAlgorithmState : IAlgorithmState;

public interface ITerminatorInstance<TGenotype, in TSearchSpace, in TProblem, in TAlgorithmState>
  : IOperatorInstance
  where TSearchSpace : class, ISearchSpace<TGenotype>
  where TProblem : IProblem<TGenotype, TSearchSpace>
  where TAlgorithmState : IAlgorithmState
{
  bool ShouldTerminate(TAlgorithmState state, TSearchSpace searchSpace, TProblem problem);
}
