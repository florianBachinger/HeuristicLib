using System.Collections;
using Generator.Equals;

namespace HEAL.HeuristicLib.Optimization;

public static class Population
{
  public static Population<TGenotype> From<TGenotype>(IReadOnlyList<TGenotype> genotypes, IReadOnlyList<ObjectiveVector> fitnesses) => new(genotypes, fitnesses);

  public static Population<TGenotype> From<TGenotype>(IEnumerable<ISolution<TGenotype>> solutions) => new(solutions.ToList());
}

[Equatable]
public partial record Population<TGenotype> : IISolutionLayout<TGenotype>
{
  [OrderedEquality]
  public IReadOnlyList<ISolution<TGenotype>> Solutions { get; init; }
  
  public Population(IReadOnlyList<TGenotype> genotypes, IReadOnlyList<ObjectiveVector> fitnesses)
    : this(ToSolutions(genotypes, fitnesses))
  {
  }
  public Population(params IReadOnlyList<ISolution<TGenotype>> Solutions)
  {
    this.Solutions = Solutions;
  }

  private static IReadOnlyList<ISolution<TGenotype>> ToSolutions(IReadOnlyList<TGenotype> genotypes, IReadOnlyList<ObjectiveVector> fitnesses)
  {
    if (genotypes.Count != fitnesses.Count) {
      throw new ArgumentException("Genotypes and fitnesses must have the same length.");
    }

    var solutions = genotypes.Zip(fitnesses).Select(x => Solution.From(x.First, x.Second));
    return solutions.ToArray();
  }

  public IEnumerator<ISolution<TGenotype>> GetEnumerator() => Solutions.GetEnumerator();

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  public IEnumerable<TGenotype> Genotypes => Solutions.Select(x => x.Genotype);
}
