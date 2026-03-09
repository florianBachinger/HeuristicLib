using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Random;

namespace HEAL.HeuristicLib.Operators.Replacers;

public record ElitismReplacer<TGenotype>
  : StatelessReplacer<TGenotype>
{
  public int Elites { get; }

  public ElitismReplacer(int elites)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(elites);
    Elites = elites;
  }

  public override IReadOnlyList<ISolution<TGenotype>> Replace(IReadOnlyList<ISolution<TGenotype>> previousPopulation, IReadOnlyList<ISolution<TGenotype>> offspringPopulation, Objective objective, int count, IRandomNumberGenerator random)
  {
    var elitesPopulation = previousPopulation.OrderBy(p => p.ObjectiveVector, objective.TotalOrderComparer).Take(Elites);
    
    var remainingCount = count - Math.Min(previousPopulation.Count, Elites);
    
    var nonElites = offspringPopulation.Take(remainingCount);
    
    return elitesPopulation.Concat(nonElites).ToArray();
  }
}
