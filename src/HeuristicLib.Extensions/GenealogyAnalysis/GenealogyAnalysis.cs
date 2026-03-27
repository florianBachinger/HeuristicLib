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
  IAnalyzer<GenealogyAnalysis<T, TS, TP, TR>.State>
  where TS : class, ISearchSpace<T>
  where TP : class, IProblem<T, TS>
  where TR : PopulationState<T>
  where T : notnull
{
  public ICrossover<T, TS, TP>? Crossover { get; } = crossover;
  public IMutator<T, TS, TP>? Mutator { get; } = mutator;
  public IInterceptor<T, TS, TP, TR>? Interceptor { get; } = interceptor;

  public class State(Run run, GenealogyAnalysis<T, TS, TP, TR> analyzer, IEqualityComparer<T>? equality = null, bool saveSpace = false)
    : AnalyzerRunState<GenealogyAnalysis<T, TS, TP, TR>>(run, analyzer)
  {
    public GenealogyGraph<T> Graph { get; } = new(equality ?? EqualityComparer<T>.Default);

    public override void RegisterObservations(IObservationRegistry observationRegistry)
    {
      if (Analyzer.Crossover is not null) {
        observationRegistry.Add(Analyzer.Crossover, AfterCross);
      }

      if (Analyzer.Mutator is not null) {
        observationRegistry.Add(Analyzer.Mutator, AfterMutate);
      }

      if (Analyzer.Interceptor is not null) {
        observationRegistry.Add(Analyzer.Interceptor, AfterInterception);
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
    }
  }

  public virtual State CreateAnalyzerState(Run run) => new(run, this, equality, saveSpace);
}

public class RankAnalysis<T, TS, TP, TR>(
  ICrossover<T, TS, TP>? crossover = null,
  IMutator<T, TS, TP>? mutator = null,
  IInterceptor<T, TS, TP, TR>? interceptor = null,
  IEqualityComparer<T>? equality = null)
  : GenealogyAnalysis<T, TS, TP, TR>(crossover, mutator, interceptor, equality),
    IAnalyzer<RankAnalysis<T, TS, TP, TR>.State>
  where TS : class, ISearchSpace<T>
  where TP : class, IProblem<T, TS>
  where TR : PopulationState<T>
  where T : notnull
{
  public new class State(Run run, RankAnalysis<T, TS, TP, TR> analyzer, IEqualityComparer<T>? equality = null)
    : GenealogyAnalysis<T, TS, TP, TR>.State(run, analyzer, equality)
  {
    public List<List<double>> Ranks { get; } = [];

    public RankAnalysisResult<T> Result => new(Graph, Ranks.Select(x => (IReadOnlyList<double>)x.ToArray()).ToArray());

    public override void AfterInterception(TR newState, TR currentState, TR? previousState, TS searchSpace, TP problem)
    {
      base.AfterInterception(newState, currentState, previousState, searchSpace, problem);
      RecordRanks(Graph, Ranks);
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

  public new State CreateAnalyzerState(Run run) => new(run, this, equality);

  IAnalyzerRunState IAnalyzer.CreateAnalyzerState(Run run) => CreateAnalyzerState(run);
}
