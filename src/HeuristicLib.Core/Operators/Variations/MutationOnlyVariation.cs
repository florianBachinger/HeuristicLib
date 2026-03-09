using HEAL.HeuristicLib.Execution;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.Random;
using HEAL.HeuristicLib.SearchSpaces;

namespace HEAL.HeuristicLib.Operators.Variations;

public record MutationOnlyVariation<TGenotype, TSearchSpace, TProblem>
  : IVariation<TGenotype, TSearchSpace, TProblem>
  where TSearchSpace : class, ISearchSpace<TGenotype>
  where TProblem : class, IProblem<TGenotype, TSearchSpace>
{
  public IMutator<TGenotype, TSearchSpace, TProblem> Mutator { get; init; }

  public MutationOnlyVariation(IMutator<TGenotype, TSearchSpace, TProblem> mutator)
  {
    Mutator = mutator;
  }

  public IVariationInstance<TGenotype, TSearchSpace, TProblem> CreateExecutionInstance(ExecutionInstanceRegistry instanceRegistry)
  {
    return new Instance(instanceRegistry.Resolve(Mutator));
  }

  public class Instance(IMutatorInstance<TGenotype, TSearchSpace, TProblem> mutatorInstance) : IVariationInstance<TGenotype, TSearchSpace, TProblem>
  {
    public IReadOnlyList<TGenotype> Alter(IReadOnlyList<TGenotype> parent, IRandomNumberGenerator random, TSearchSpace searchSpace, TProblem problem)
    {
      return mutatorInstance.Mutate(parent, random, searchSpace, problem);
    }
  }
}
