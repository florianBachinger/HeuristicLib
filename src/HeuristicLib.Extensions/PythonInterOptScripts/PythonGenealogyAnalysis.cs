using HEAL.HeuristicLib.Algorithms;
using HEAL.HeuristicLib.Algorithms.Evolutionary;
using HEAL.HeuristicLib.Algorithms.LocalSearch;
using HEAL.HeuristicLib.Algorithms.MetaAlgorithms;
using HEAL.HeuristicLib.Analyzers;
using HEAL.HeuristicLib.Execution;
using HEAL.HeuristicLib.GenealogyAnalysis;
using HEAL.HeuristicLib.Genotypes.Trees;
using HEAL.HeuristicLib.Genotypes.Vectors;
using HEAL.HeuristicLib.Operators.Creators.PermutationCreators;
using HEAL.HeuristicLib.Operators.Creators.RealVectorCreators;
using HEAL.HeuristicLib.Operators.Creators.SymbolicExpressionTreeCreators;
using HEAL.HeuristicLib.Operators.Crossovers.PermutationCrossovers;
using HEAL.HeuristicLib.Operators.Crossovers.RealVectorCrossovers;
using HEAL.HeuristicLib.Operators.Crossovers.SymbolicExpressionTreeCrossovers;
using HEAL.HeuristicLib.Operators.Interceptors;
using HEAL.HeuristicLib.Operators.Mutators;
using HEAL.HeuristicLib.Operators.Mutators.PermutationMutators;
using HEAL.HeuristicLib.Operators.Mutators.RealVectorMutators;
using HEAL.HeuristicLib.Operators.Mutators.SymbolicExpressionTreeMutators;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.Random;
using HEAL.HeuristicLib.SearchSpaces;
using HEAL.HeuristicLib.SearchSpaces.Trees;
using HEAL.HeuristicLib.States;

#pragma warning disable S1104
#pragma warning disable S1104

namespace HEAL.HeuristicLib.PythonInterOptScripts;

public class PythonGenealogyAnalysis
{
  public delegate void GenerationCallback(object current);

  #region public methods
  #region BatchRuns
  private static ExperimentResult<T>[]
    RunConfigurableRepeated<T>(int repetitions, Func<int, ExperimentResult<T>> experiment, int seed)
  {
    return BatchExecution.Parallel<ExperimentResult<T>>(repetitions, r => experiment(r.NextInt()), RandomNumberGenerator.Create(seed), maxDegreeOfParallelism: -1)
                         .ToArray();
  }

  public static ExperimentResult<SymbolicExpressionTree>[] RunSymbolicRegressionConfigurable(string file, SymRegExperimentParameters parameters, int repetitions) =>
    RunConfigurableRepeated(
      repetitions,
      experiment: seed => RunSymbolicRegressionConfigurable(file, new SymRegExperimentParameters(parameters) { Seed = seed }),
      parameters.Seed);

  public static ExperimentResult<Permutation>[] RunTravelingSalesmanConfigurable(string file, TravelingSalesmanExperimentParameters parameters, int repetitions) =>
    RunConfigurableRepeated(
      repetitions,
      experiment: seed => RunTravelingSalesmanConfigurable(file, new TravelingSalesmanExperimentParameters(parameters) { Seed = seed }),
      parameters.Seed);

  public static ExperimentResult<RealVector>[] RunTestFunctionConfigurable(string file, TestFunctionExperimentParameters parameters, int repetitions) =>
    RunConfigurableRepeated(
      repetitions,
      experiment: seed => RunTestFunctionConfigurable(file, new TestFunctionExperimentParameters(parameters) { Seed = seed }),
      parameters.Seed);
  #endregion

  public static ExperimentResult<SymbolicExpressionTree> RunSymbolicRegressionConfigurable(
    string file,
    SymRegExperimentParameters parameters,
    GenerationCallback? callback = null)
  {
    parameters = new SymRegExperimentParameters(parameters) {
      Creator = parameters.Creator ?? new ProbabilisticTreeCreator(),
      Crossover = parameters.Crossover ?? new SubtreeCrossover(),
      Mutator = parameters.Mutator ?? CreateSymRegAllMutator()
    };
    var problem = ProblemGeneration.CreateSymbolicRegressionProblem(file, parameters);
    var actionCallback = callback is null ? null : new Action<PopulationState<SymbolicExpressionTree>>(callback);

    return RunAlgorithmConfigurable(problem, actionCallback, parameters);
  }

  public static ExperimentResult<Permutation> RunTravelingSalesmanConfigurable(
    string file,
    TravelingSalesmanExperimentParameters parameters,
    GenerationCallback? callback = null)
  {
    var problem = ProblemGeneration.CreateTravellingSalesmanProblem(file);

    parameters = new TravelingSalesmanExperimentParameters(parameters) {
      Creator = parameters.Creator ?? new RandomPermutationCreator(),
      Crossover = parameters.Crossover ?? new EdgeRecombinationCrossover(),
      Mutator = parameters.Mutator ?? new InversionMutator()
    };
    var actionCallback = callback is null ? null : new Action<PopulationState<Permutation>>(callback);

    return RunAlgorithmConfigurable(problem, actionCallback, parameters);
  }

  public static ExperimentResult<RealVector> RunTestFunctionConfigurable(
    string file,
    TestFunctionExperimentParameters parameters,
    GenerationCallback? callback = null)
  {
    parameters = new TestFunctionExperimentParameters(parameters) {
      Creator = parameters.Creator ?? new UniformDistributedCreator(),
      Crossover = parameters.Crossover ?? new SimulatedBinaryCrossover(),
      Mutator = parameters.Mutator ?? new GaussianMutator(1.0 / parameters.Dimension, 0.01)
    };
    var problem = ProblemGeneration.CreateTestFunctionProblem(parameters.Problem, parameters.Dimension, parameters.Instance);
    var actionCallback = callback is null ? null : new Action<PopulationState<RealVector>>(callback);

    return RunAlgorithmConfigurable(problem, actionCallback, parameters);
  }
  #endregion

  #region generic helpers
  public static ExperimentResult<T> RunAlgorithmConfigurable<T, TE>(
    IProblem<T, TE> problem,
    Action<PopulationState<T>>? callback,
    ExperimentParameters<T, TE> parameters) where T : notnull where TE : class, ISearchSpace<T>
  {
    //var terminator = new AfterIterationsTerminator<T>(parameters.Iterations);
    if (parameters.NoChildren < 0) {
      parameters.NoChildren = parameters.PopulationSize;
    }

    MyAnalyzers<T> analyzers = null!;
    Run run = null!;

    switch (parameters.AlgorithmName.ToLower()) {
      case "ga":
        var ga = GeneticAlgorithm.GetBuilder(parameters.Creator!, parameters.Crossover!, parameters.Mutator!);
        ga.PopulationSize = parameters.PopulationSize;
        ga.MutationRate = parameters.MutationRate;
        ga.Elites = parameters.Elites;
        if (parameters.Selector != null) {
          ga.Selector = parameters.Selector;
        }

        analyzers = AddAnalyzers(callback, ga, parameters);
        var gaRun = ga.Build().WithMaxIterations(parameters.Iterations).CreateRun(problem);
        gaRun.RunToCompletion(RandomNumberGenerator.Create(parameters.Seed));
        run = gaRun;

        break;
      case "es":
        var es = EvolutionStrategy.GetBuilder(parameters.Creator!, parameters.Mutator!);
        es.PopulationSize = parameters.PopulationSize;
        es.NumberOfChildren = parameters.NoChildren;
        es.Strategy = parameters.Strategy;
        //es.Terminator = terminator;
        if (parameters.Selector != null) {
          es.Selector = parameters.Selector;
        }

        if (parameters.WithCrossover) {
          es.Crossover = parameters.Crossover;
        }

        analyzers = AddAnalyzers(callback, es, parameters);

        var esRun = es.Build().WithMaxIterations(parameters.Iterations).CreateRun(problem);
        esRun.RunToCompletion(RandomNumberGenerator.Create(parameters.Seed));
        run = esRun;

        break;
      case "ls":
        var ls = HillClimber.GetBuilder(parameters.Creator!, parameters.Mutator!);
        ls.BatchSize = ls.MaxNeighbors = parameters.NoChildren;
        //ls.Terminator = terminator;

        // analyzers = AddAnalyzers(callback, ls, parameters);
        var lsRun = ls.Build().WithMaxIterations(parameters.Iterations).CreateRun(problem);
        lsRun.RunToCompletion(RandomNumberGenerator.Create(parameters.Seed));
        run = lsRun;

        break;
      case "nsga2":
        var nsga2 = NSGA2.GetBuilder(parameters.Creator!, parameters.Crossover!, parameters.Mutator!);
        nsga2.PopulationSize = parameters.PopulationSize;
        nsga2.MutationRate = parameters.MutationRate;
        if (parameters.Selector != null) {
          nsga2.Selector = parameters.Selector;
        }

        //nsga2.Terminator = terminator;
        analyzers = AddAnalyzers(callback, nsga2, parameters);
        var nsga2Run = nsga2.Build().WithMaxIterations(parameters.Iterations).CreateRun(problem);
        _ = nsga2Run.RunToCompletion(RandomNumberGenerator.Create(parameters.Seed));
        run = nsga2Run;
        break;
      default:
        throw new ArgumentException($"Algorithm '{parameters.AlgorithmName}' is not supported.");
    }

    return analyzers.ToExperimentResult(run);
  }

  private sealed record MyAnalyzers<T>(
    BestMedianWorstAnalysis<T> Qualities,
    RankAnalysis<T>? RankAnalysis,
    QualityCurveAnalysis<T> QualityCurve,
    AllPopulationsTracker<T>? AllPopulations)
    where T : notnull
  {
    public ExperimentResult<T> ToExperimentResult(Run run)
    {
      var qRes = run.GetResult(Qualities);
      var rankGraph = string.Empty;
      IReadOnlyList<List<double>> rankLines = [];
      if (RankAnalysis is not null && run.TryGetResult<RankAnalysisResult<T>>(RankAnalysis, out var rankResult)) {
        rankGraph = rankResult.Graph.ToGraphViz();
        rankLines = rankResult.Ranks.Select(x => x.ToList()).ToArray();
      }

      IReadOnlyList<ISolution<T>[]> apRes = [];
      if (AllPopulations is not null && run.TryGetResult<IReadOnlyList<ISolution<T>[]>>(AllPopulations, out var populations)) {
        apRes = populations;
      }

      var experimentResult = new ExperimentResult<T>(
        rankGraph,
        rankLines,
        qRes,
        apRes
      );
      return experimentResult;
    }
  }

  private static MyAnalyzers<T> AddAnalyzers<T, TE, TP, TRes, TA>(
    Action<TRes>? callback,
    AlgorithmBuilder<T, TE, TP, TRes, TA> builder,
    ExperimentParameters<T, TE> parameters)
    where T : notnull
    where TRes : PopulationState<T>
    where TE : class, ISearchSpace<T>
    where TP : class, IProblem<T, TE>
    where TA : IAlgorithm<T, TE, TP, TRes>
  {
    var qualities = new BestMedianWorstAnalysis<T>();
    builder.AttachAnalyzer(qualities);
    if (callback != null)
      builder.AttachObserver(new ActionInterceptorObserver<T, TE, TP, TRes>((y, _, _, _, _) => callback(y)));

    var rankAnalysis = parameters.TrackGenealogy ? new RankAnalysis<T>() : null;
    if (rankAnalysis is not null)
      builder.AttachAnalyzer(rankAnalysis);

    var qc = new QualityCurveAnalysis<T>();
    builder.AttachAnalyzer(qc);

    var apt = parameters.TrackPopulations ? new AllPopulationsTracker<T>() : null;
    if (apt is not null)
      builder.AttachAnalyzer(apt);

    return new MyAnalyzers<T>(qualities, rankAnalysis, qc, apt);
  }

  private static ChooseOneMutator<SymbolicExpressionTree, SymbolicExpressionTreeSearchSpace, IProblem<SymbolicExpressionTree, SymbolicExpressionTreeSearchSpace>> CreateSymRegAllMutator()
  {
    return ChooseOneMutator.Create(
      new ChangeNodeTypeManipulation(),
      new FullTreeShaker(),
      new OnePointShaker(),
      new RemoveBranchManipulation(),
      new ReplaceBranchManipulation());
  }
  #endregion
}
