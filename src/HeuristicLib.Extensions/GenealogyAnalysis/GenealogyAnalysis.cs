using HEAL.HeuristicLib.Analysis;
using HEAL.HeuristicLib.Execution;
using HEAL.HeuristicLib.Operators;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.SearchSpaces;
using HEAL.HeuristicLib.States;

namespace HEAL.HeuristicLib.GenealogyAnalysis;

public record RankAnalysisResult<T>(GenealogyGraph<T> Graph, IReadOnlyList<IReadOnlyList<double>> Ranks)
  where T : notnull;

public class GenealogyAnalysis<T, TS, TP, TR>(
  ICrossover<T, TS, TP>? crossover = null,
  IMutator<T, TS, TP>? mutator = null,
  IInterceptor<T, TS, TP, TR>? interceptor = null,
  IEqualityComparer<T>? equality = null,
  bool saveSpace = false) :
  IAnalyzer<GenealogyGraph<T>, GenealogyAnalysis<T, TS, TP, TR>.Instance>
  where TS : class, ISearchSpace<T>
  where TP : class, IProblem<T, TS>
  where TR : PopulationState<T>
  where T : notnull
{
  public ICrossover<T, TS, TP>? Crossover { get; } = crossover;
  public IMutator<T, TS, TP>? Mutator { get; } = mutator;
  public IInterceptor<T, TS, TP, TR>? Interceptor { get; } = interceptor;

  public class Instance(Run run, GenealogyAnalysis<T, TS, TP, TR> analyzer, IEqualityComparer<T>? equality = null, bool saveSpace = false)
    : AnalyzerRunInstance<GenealogyAnalysis<T, TS, TP, TR>, GenealogyGraph<T>>(run, analyzer)
  {
    public readonly GenealogyGraph<T> Graph = new(equality ?? EqualityComparer<T>.Default);

    public override void RegisterTaps(IAnalyzerTapRegistry taps)
    {
      if (analyzer.Crossover is not null) {
        taps.Register(analyzer.Crossover, AfterCross);
      }

      if (analyzer.Mutator is not null) {
        taps.Register(analyzer.Mutator, AfterMutate);
      }

      if (analyzer.Interceptor is not null) {
        taps.Register(analyzer.Interceptor, AfterInterception);
      }
    }

    public void AfterCross(IReadOnlyList<T> offspring, IReadOnlyList<IParents<T>> parents, TS searchSpace, TP problem)
    {
      foreach (var (parents1, child) in parents.Zip(offspring)) {
        Graph.AddConnection([parents1.Item1, parents1.Item2], child);
      }
    }

    public void AfterMutate(IReadOnlyList<T> offspring, IReadOnlyList<T> parent, TS searchSpace, TP problem)
    {
      foreach (var (parents1, child) in parent.Zip(offspring)) {
        Graph.AddConnection([parents1], child);
      }
    }

    public virtual void AfterInterception(TR newState, TR currentState, TR? previousState, TS searchSpace, TP problem)
    {
      var ordered = currentState.Population.OrderBy(keySelector: x => x.ObjectiveVector, problem.Objective.TotalOrderComparer).ToArray();
      Graph.SetAsNewGeneration(ordered.Select(x => x.Genotype), saveSpace);
      PublishResult(Graph);
    }
  }

  public virtual Instance CreateAnalyzerInstance(Run run) => new(run, this, equality, saveSpace);
}

public class RankAnalysis<T, TS, TP, TR>(
  ICrossover<T, TS, TP>? crossover = null,
  IMutator<T, TS, TP>? mutator = null,
  IInterceptor<T, TS, TP, TR>? interceptor = null,
  IEqualityComparer<T>? equality = null)
  : GenealogyAnalysis<T, TS, TP, TR>(crossover, mutator, interceptor, equality),
    IAnalyzer<RankAnalysisResult<T>, RankAnalysis<T, TS, TP, TR>.Instance>
  where TS : class, ISearchSpace<T>
  where TP : class, IProblem<T, TS>
  where TR : PopulationState<T>
  where T : notnull
{
  public new class Instance(Run run, RankAnalysis<T, TS, TP, TR> analyzer, IEqualityComparer<T>? equality = null)
    : GenealogyAnalysis<T, TS, TP, TR>.Instance(run, analyzer, equality)
  {
    public List<List<double>> Ranks { get; } = [];

    public override void AfterInterception(TR newState, TR currentState, TR? previousState, TS searchSpace, TP problem)
    {
      base.AfterInterception(newState, currentState, previousState, searchSpace, problem);
      RecordRanks(Graph, Ranks);
       Run.SetResult((RankAnalysis<T, TS, TP, TR>)Analyzer, new RankAnalysisResult<T>(Graph, Ranks.Select(x => (IReadOnlyList<double>)x.ToArray()).ToArray()));
    }

    private static void RecordRanks<TGenotype>(GenealogyGraph<TGenotype> graph, List<List<double>> ranks) where TGenotype : notnull
    {
      if (graph.Nodes.Count < 2) {
        return;
      }

      var line = graph.Nodes[^2].Values
                      .Where(x => x.Layer == 0)
                      .OrderBy(x => x.Rank)
                      .Select(node => node.GetAllDescendants().Where(x => x.Rank >= 0).Select(x => (double)x.Rank).DefaultIfEmpty(double.NaN).Average())
                      .ToList();
      if (line.Count > 0) {
        ranks.Add(line);
      }
    }
  }

  public new Instance CreateAnalyzerInstance(Run run) => new(run, this, equality);

  IAnalyzerRunInstance IAnalyzer.CreateAnalyzerInstance(Run run) => CreateAnalyzerInstance(run);
}
