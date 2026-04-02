using HEAL.HeuristicLib.Execution;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.Random;
using HEAL.HeuristicLib.SearchSpaces;

namespace HEAL.HeuristicLib.Operators.Variations;

public record CrossoverOnlyVariation<TGenotype, TSearchSpace, TProblem>
  : IVariation<TGenotype, TSearchSpace, TProblem>
  where TSearchSpace : class, ISearchSpace<TGenotype>
  where TProblem : class, IProblem<TGenotype, TSearchSpace>
{
  public ICrossover<TGenotype, TSearchSpace, TProblem> Crossover { get; init; }

  public CrossoverOnlyVariation(ICrossover<TGenotype, TSearchSpace, TProblem> crossover)
  {
    Crossover = crossover;
  }

  public IVariationInstance<TGenotype, TSearchSpace, TProblem> CreateExecutionInstance(ExecutionInstanceRegistry instanceRegistry)
  {
    return new Instance(instanceRegistry.Resolve(Crossover));
  }

  public class Instance(ICrossoverInstance<TGenotype, TSearchSpace, TProblem> crossoverInstance) : IVariationInstance<TGenotype, TSearchSpace, TProblem>
  {
    public IReadOnlyList<TGenotype> Alter(IReadOnlyList<TGenotype> parent, IRandomNumberGenerator random, TSearchSpace searchSpace, TProblem problem)
    {
      if (parent.Count % 2 != 0)
        throw new ArgumentException("Crossover requires an even number of parents.", nameof(parent));

      var parentPairs = parent.ToParentPairs();
      return crossoverInstance.Cross(parentPairs, random, searchSpace, problem);
    }
  }
}
