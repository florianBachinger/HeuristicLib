using HEAL.HeuristicLib.Analyzers;
using HEAL.HeuristicLib.Optimization;

namespace HEAL.HeuristicLib.PythonInterOptScripts;

public record ExperimentResult<T>(
  string Graph,
  IReadOnlyList<List<double>> ChildRanks,
  IReadOnlyList<BestMedianWorstEntry<T>> BestMedianWorst,
  IReadOnlyList<ISolution<T>[]> AllPopulations);
