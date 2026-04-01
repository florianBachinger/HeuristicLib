using HEAL.HeuristicLib.Execution;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.SearchSpaces;
using HEAL.HeuristicLib.States;

namespace HEAL.HeuristicLib.Operators.Terminators;

public abstract record DecoratorTerminator<TGenotype, TAlgorithmState, TSearchSpace, TProblem, TState>
  : ITerminator<TGenotype, TAlgorithmState, TSearchSpace, TProblem>
  where TAlgorithmState : class, IAlgorithmState
  where TSearchSpace : class, ISearchSpace<TGenotype>
  where TProblem : class, IProblem<TGenotype, TSearchSpace>
{
  protected ITerminator<TGenotype, TAlgorithmState, TSearchSpace, TProblem> InnerTerminator { get; }
  
  protected DecoratorTerminator(ITerminator<TGenotype, TAlgorithmState, TSearchSpace, TProblem> innerTerminator)
  {
    InnerTerminator = innerTerminator;
  }
  
  public ITerminatorInstance<TGenotype, TAlgorithmState, TSearchSpace, TProblem> CreateExecutionInstance(ExecutionInstanceRegistry instanceRegistry) =>
    new Instance(this, instanceRegistry.Resolve(InnerTerminator), CreateInitialState());
  
  protected abstract TState CreateInitialState();
  
  protected abstract bool ShouldTerminate(TAlgorithmState algorithmState, TState state,
    ITerminatorInstance<TGenotype, TAlgorithmState, TSearchSpace, TProblem> innerTerminator,
    TSearchSpace searchSpace, TProblem problem);
  
  private sealed class Instance(DecoratorTerminator<TGenotype, TAlgorithmState, TSearchSpace, TProblem, TState> decorator,
    ITerminatorInstance<TGenotype, TAlgorithmState, TSearchSpace, TProblem> innerTerminator, TState terminatorState)
    : ITerminatorInstance<TGenotype, TAlgorithmState, TSearchSpace, TProblem>
  {
    public bool ShouldTerminate(TAlgorithmState state, TSearchSpace searchSpace, TProblem problem)
    {
      return decorator.ShouldTerminate(state, terminatorState, innerTerminator, searchSpace, problem);
    }
  }
}

public abstract record DecoratorTerminator<TGenotype, TAlgorithmState, TSearchSpace, TProblem>
  : DecoratorTerminator<TGenotype, TAlgorithmState, TSearchSpace, TProblem, NoState>
  where TAlgorithmState : class, IAlgorithmState
  where TSearchSpace : class, ISearchSpace<TGenotype>
  where TProblem : class, IProblem<TGenotype, TSearchSpace>
{
  protected DecoratorTerminator(ITerminator<TGenotype, TAlgorithmState, TSearchSpace, TProblem> innerTerminator)
    : base(innerTerminator)
  {
  }

  protected sealed override NoState CreateInitialState() => NoState.Instance;

  protected sealed override bool ShouldTerminate(TAlgorithmState algorithmState, NoState state,
    ITerminatorInstance<TGenotype, TAlgorithmState, TSearchSpace, TProblem> innerTerminator,
    TSearchSpace searchSpace, TProblem problem)
    => ShouldTerminate(algorithmState, innerTerminator, searchSpace, problem);

  protected abstract bool ShouldTerminate(TAlgorithmState algorithmState,
    ITerminatorInstance<TGenotype, TAlgorithmState, TSearchSpace, TProblem> innerTerminator,
    TSearchSpace searchSpace, TProblem problem);
}
