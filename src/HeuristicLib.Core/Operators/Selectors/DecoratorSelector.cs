using HEAL.HeuristicLib.Execution;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.Random;
using HEAL.HeuristicLib.SearchSpaces;

namespace HEAL.HeuristicLib.Operators.Selectors;

public abstract record DecoratorSelector<TGenotype, TSearchSpace, TProblem, TState>
  : ISelector<TGenotype, TSearchSpace, TProblem>
  where TSearchSpace : class, ISearchSpace<TGenotype>
  where TProblem : class, IProblem<TGenotype, TSearchSpace>
{
  protected ISelector<TGenotype, TSearchSpace, TProblem> InnerSelector { get; }

  protected DecoratorSelector(ISelector<TGenotype, TSearchSpace, TProblem> innerSelector)
  {
    InnerSelector = innerSelector;
  }

  public ISelectorInstance<TGenotype, TSearchSpace, TProblem> CreateExecutionInstance(ExecutionInstanceRegistry instanceRegistry) =>
    new Instance(this, instanceRegistry.Resolve(InnerSelector), CreateInitialState());

  protected abstract TState CreateInitialState();

  protected abstract IReadOnlyList<ISolution<TGenotype>> Select(IReadOnlyList<ISolution<TGenotype>> population,
    Objective objective, int count, TState state, ISelectorInstance<TGenotype, TSearchSpace, TProblem> selectorForRemaining,
    IRandomNumberGenerator random, TSearchSpace searchSpace, TProblem problem);

  private sealed class Instance(DecoratorSelector<TGenotype, TSearchSpace, TProblem, TState> decorator,
    ISelectorInstance<TGenotype, TSearchSpace, TProblem> innerSelector, TState state)
    : ISelectorInstance<TGenotype, TSearchSpace, TProblem>
  {
    public IReadOnlyList<ISolution<TGenotype>> Select(IReadOnlyList<ISolution<TGenotype>> population, Objective objective, int count, IRandomNumberGenerator random, TSearchSpace searchSpace, TProblem problem)
    {
      return decorator.Select(population, objective, count, state, innerSelector, random, searchSpace, problem);
    }
  }
}

public abstract record DecoratorSelector<TGenotype, TSearchSpace, TProblem>
  : DecoratorSelector<TGenotype, TSearchSpace, TProblem, NoState>
  where TSearchSpace : class, ISearchSpace<TGenotype>
  where TProblem : class, IProblem<TGenotype, TSearchSpace>
{
  protected DecoratorSelector(ISelector<TGenotype, TSearchSpace, TProblem> innerSelector)
    : base(innerSelector)
  {
  }

  protected sealed override NoState CreateInitialState() => NoState.Instance;

  protected sealed override IReadOnlyList<ISolution<TGenotype>> Select(IReadOnlyList<ISolution<TGenotype>> population,
    Objective objective, int count, NoState state, ISelectorInstance<TGenotype, TSearchSpace, TProblem> innerSelector,
    IRandomNumberGenerator random, TSearchSpace searchSpace, TProblem problem)
    => Select(population, objective, count, innerSelector, random, searchSpace, problem);

  protected abstract IReadOnlyList<ISolution<TGenotype>> Select(IReadOnlyList<ISolution<TGenotype>> population,
    Objective objective, int count, ISelectorInstance<TGenotype, TSearchSpace, TProblem> innerSelector,
    IRandomNumberGenerator random, TSearchSpace searchSpace, TProblem problem);
}
