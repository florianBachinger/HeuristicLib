using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Random;

namespace HEAL.HeuristicLib.Operators.Replacers;

public record CommaSelectionReplacer<TGenotype>
  : StatelessReplacer<TGenotype>
{
  public override IReadOnlyList<ISolution<TGenotype>> Replace(IReadOnlyList<ISolution<TGenotype>> previousPopulation, IReadOnlyList<ISolution<TGenotype>> offspringPopulation, Objective objective, int count, IRandomNumberGenerator random)
  {
    return offspringPopulation
      .OrderBy(p => p.ObjectiveVector, objective.TotalOrderComparer)
      .Take(count)
      .ToArray();
  }
}
