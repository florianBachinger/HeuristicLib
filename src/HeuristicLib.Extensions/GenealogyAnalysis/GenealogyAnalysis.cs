using HEAL.HeuristicLib.Analyzers;
using HEAL.HeuristicLib.Execution;
using HEAL.HeuristicLib.Operators.Crossovers;
using HEAL.HeuristicLib.Operators.Interceptors;
using HEAL.HeuristicLib.Operators.Mutators;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.Random;
using HEAL.HeuristicLib.SearchSpaces;
using HEAL.HeuristicLib.States;

namespace HEAL.HeuristicLib.GenealogyAnalysis;

public class GenealogyAnalysis<T>(IEqualityComparer<T>? equality = null, bool saveSpace = false) :
  ICrossoverObserver<T>,
  IMutatorObserver<T>,
  IInterceptorObserver<T, PopulationState<T>>
  where T : notnull
{
  public class Instance(IEqualityComparer<T>? equality = null, bool saveSpace = false) : ICrossoverObserverInstance<T, ISearchSpace<T>, IProblem<T, ISearchSpace<T>>>, IMutatorObserverInstance<T, ISearchSpace<T>, IProblem<T, ISearchSpace<T>>>, IInterceptorObserverInstance<T, ISearchSpace<T>, IProblem<T, ISearchSpace<T>>, PopulationState<T>>
  {
    public readonly GenealogyGraph<T> Graph = new(equality ?? EqualityComparer<T>.Default);

    public void AfterCross(IReadOnlyList<T> offspring, IReadOnlyList<IParents<T>> parents, ISearchSpace<T> searchSpace, IProblem<T, ISearchSpace<T>> problem)
    {
      foreach (var (parents1, child) in parents.Zip(offspring)) {
        Graph.AddConnection([parents1.Item1, parents1.Item2], child);
      }
    }

    public void AfterMutate(IReadOnlyList<T> offspring, IReadOnlyList<T> parent, ISearchSpace<T> searchSpace, IProblem<T, ISearchSpace<T>> problem)
    {
      foreach (var (parents1, child) in parent.Zip(offspring)) {
        Graph.AddConnection([parents1], child);
      }
    }

    public virtual void AfterInterception(PopulationState<T> newState, PopulationState<T> currentState, PopulationState<T>? previousState, ISearchSpace<T> searchSpace, IProblem<T, ISearchSpace<T>> problem)
    {
      var ordered = currentState.Population.OrderBy(keySelector: x => x.ObjectiveVector, problem.Objective.TotalOrderComparer).ToArray();
      Graph.SetAsNewGeneration(ordered.Select(x => x.Genotype), saveSpace);
    }
  }

  protected virtual Instance CreateExecutionInstance() => new(equality, saveSpace);

  ICrossoverObserverInstance<T, ISearchSpace<T>, IProblem<T, ISearchSpace<T>>>
    IExecutable<ICrossoverObserverInstance<T, ISearchSpace<T>, IProblem<T, ISearchSpace<T>>>>
    .CreateExecutionInstance(ExecutionInstanceRegistry instanceRegistry) => CreateExecutionInstance();

  IMutatorObserverInstance<T, ISearchSpace<T>, IProblem<T, ISearchSpace<T>>>
    IExecutable<IMutatorObserverInstance<T, ISearchSpace<T>, IProblem<T, ISearchSpace<T>>>>
    .CreateExecutionInstance(ExecutionInstanceRegistry instanceRegistry) => CreateExecutionInstance();

  IInterceptorObserverInstance<T, ISearchSpace<T>, IProblem<T, ISearchSpace<T>>, PopulationState<T>>
    IExecutable<IInterceptorObserverInstance<T, ISearchSpace<T>, IProblem<T, ISearchSpace<T>>, PopulationState<T>>>
    .CreateExecutionInstance(ExecutionInstanceRegistry instanceRegistry) => CreateExecutionInstance();
}

public class RankAnalysis<T>(IEqualityComparer<T>? equality = null) : GenealogyAnalysis<T>(equality)
  where T : notnull
{
  public new class Instance(IEqualityComparer<T>? equality = null) : GenealogyAnalysis<T>.Instance(equality)
  {
    public List<List<double>> Ranks { get; } = [];

    public override void AfterInterception(PopulationState<T> newState, PopulationState<T> currentState, PopulationState<T>? previousState, ISearchSpace<T> searchSpace, IProblem<T, ISearchSpace<T>> problem)
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

  protected override GenealogyAnalysis<T>.Instance CreateExecutionInstance() => new Instance();
}
