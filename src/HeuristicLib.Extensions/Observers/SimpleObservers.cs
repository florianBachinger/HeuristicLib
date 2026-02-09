using HEAL.HeuristicLib.Operators.Crossovers;
using HEAL.HeuristicLib.Operators.Evaluators;
using HEAL.HeuristicLib.Operators.Interceptors;
using HEAL.HeuristicLib.Operators.Mutators;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.SearchSpaces;
using HEAL.HeuristicLib.States;

namespace HEAL.HeuristicLib.Observers;

public interface IInterceptorObserver<TGenotype> :
  IInterceptorObserver<TGenotype, IAlgorithmState, ISearchSpace<TGenotype>, IProblem<TGenotype, ISearchSpace<TGenotype>>>
{ }

public interface IInterceptorObserver<TGenotype, in TState> :
  IInterceptorObserver<TGenotype, TState, ISearchSpace<TGenotype>, IProblem<TGenotype, ISearchSpace<TGenotype>>>
  where TState : class, IAlgorithmState
{ }

public interface IEvaluatorObserver<TGenotype> :
  IEvaluatorObserver<TGenotype, ISearchSpace<TGenotype>, IProblem<TGenotype, ISearchSpace<TGenotype>>>
{ }

public interface ICrossoverObserver<TGenotype> :
  ICrossoverObserver<TGenotype, ISearchSpace<TGenotype>, IProblem<TGenotype, ISearchSpace<TGenotype>>>
{ }

public interface IMutatorObserver<TGenotype> :
  IMutatorObserver<TGenotype, ISearchSpace<TGenotype>, IProblem<TGenotype, ISearchSpace<TGenotype>>>
{ }
