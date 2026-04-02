using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.SearchSpaces;
using HEAL.HeuristicLib.States;

namespace HEAL.HeuristicLib.Operators;

public interface IInterceptor<TGenotype, in TSearchSpace, in TProblem, TAlgorithmState>
  : IOperator<IInterceptorInstance<TGenotype, TSearchSpace, TProblem, TAlgorithmState>>
  where TAlgorithmState : class, IAlgorithmState
  where TSearchSpace : class, ISearchSpace<TGenotype>
  where TProblem : class, IProblem<TGenotype, TSearchSpace>;

public interface IInterceptorInstance<TGenotype, in TSearchSpace, in TProblem, TAlgorithmState>
  : IOperatorInstance
  where TAlgorithmState : class, IAlgorithmState
  where TSearchSpace : class, ISearchSpace<TGenotype>
  where TProblem : class, IProblem<TGenotype, TSearchSpace>
{
  // ToDo: think about really providing the previous state, as it implies some form of iteration and state storage (if the interceptor really needs the previous state, it can be stateful and store it on its own).
  TAlgorithmState Transform(TAlgorithmState currentState, TAlgorithmState? previousState, TSearchSpace searchSpace, TProblem problem);
}
