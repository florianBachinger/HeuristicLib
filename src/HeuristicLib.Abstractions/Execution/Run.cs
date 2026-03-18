using System.Diagnostics.CodeAnalysis;
using HEAL.HeuristicLib.Algorithms;
using HEAL.HeuristicLib.Analysis;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.Random;
using HEAL.HeuristicLib.SearchSpaces;
using HEAL.HeuristicLib.States;

namespace HEAL.HeuristicLib.Execution;

public abstract class Run
{
  protected readonly ExecutionInstanceRegistry RootRegistry;

  protected readonly IDictionary<IAnalyzer, IAnalyzerRunInstance> AnalyzerInstances;

  protected readonly IDictionary<IAnalyzer, object> AnalyzerResults;

  protected Run()
  {
    RootRegistry = new ExecutionInstanceRegistry(this, parentRegistry: null);
    AnalyzerInstances = new Dictionary<IAnalyzer, IAnalyzerRunInstance>(ReferenceEqualityComparer.Instance);
    AnalyzerResults = new Dictionary<IAnalyzer, object>(ReferenceEqualityComparer.Instance);
  }
  
  public ExecutionInstanceRegistry CreateNewRegistry()
  {
    return new ExecutionInstanceRegistry(this, parentRegistry: null);
  }

  public ExecutionInstanceRegistry CreateChildRegistry()
  {
    return RootRegistry.CreateChildRegistry();
  }

  public TAnalyzerRunInstance ResolveAnalyzer<TResult, TAnalyzerRunInstance>(IAnalyzer<TResult, TAnalyzerRunInstance> analyzer)
    where TResult : notnull
    where TAnalyzerRunInstance : class, IAnalyzerRunInstance
  {
    if (AnalyzerInstances.TryGetValue(analyzer, out var analyzerInstance)) {
      return (TAnalyzerRunInstance)analyzerInstance;
    }

    var createdAnalyzerInstance = analyzer.CreateAnalyzerInstance(this);
    AnalyzerInstances[analyzer] = createdAnalyzerInstance;
    return createdAnalyzerInstance;
  }

  public void SetResult<TResult>(IAnalyzer<TResult> analyzer, TResult result)
    where  TResult : notnull
  {
    AnalyzerResults[analyzer] = result;
  }
  
  public bool TryGetResult<TResult>(IAnalyzer<TResult> analyzer, [MaybeNullWhen(false)] out TResult result)
    where TResult : notnull
  {
    if (AnalyzerResults.TryGetValue(analyzer, out var resultObj)) {
      result = (TResult)resultObj;
      return true;
    }
    result = default;
    return false;
  }
  
  public TResult GetResult<TResult>(IAnalyzer<TResult> analyzer) 
    where TResult : notnull
  {
     if (TryGetResult(analyzer, out var result)) {
       return result;
     }
     throw new KeyNotFoundException($"No result found for analyzer {analyzer}");
  }
  
  
}

public class Run<TGenotype, TSearchSpace, TProblem, TState> : Run
  where TSearchSpace : class, ISearchSpace<TGenotype>
  where TProblem : class, IProblem<TGenotype, TSearchSpace>
  where TState : class, IAlgorithmState
{
  public IAlgorithm<TGenotype, TSearchSpace, TProblem, TState> Algorithm { get; }

  public TProblem Problem { get; }

  public Run(IAlgorithm<TGenotype, TSearchSpace, TProblem, TState> algorithm, TProblem problem)
  {
    Algorithm = algorithm;
    Problem = problem;
  }

  public IAsyncEnumerable<TState> RunStreamingAsync(IRandomNumberGenerator random, TState? initialState = null, CancellationToken cancellationToken = default)
  {
    var instance = RootRegistry.Resolve(Algorithm);
    return instance.RunStreamingAsync(Problem, random, initialState, cancellationToken);
  }

  public async Task<TState> RunToCompletionAsync(IRandomNumberGenerator random, TState? initialState = null, CancellationToken cancellationToken = default)
  {
    return await RunStreamingAsync(random, initialState, cancellationToken).LastAsync(cancellationToken);
  }

  public IEnumerable<TState> RunStreaming(IRandomNumberGenerator random, TState? initialState = null, CancellationToken cancellationToken = default)
  {
    return RunStreamingAsync(random, initialState, cancellationToken).ToBlockingEnumerable(cancellationToken);
  }

  public TState RunToCompletion(IRandomNumberGenerator random, TState? initialState = null, CancellationToken cancellationToken = default)
  {
    return RunToCompletionAsync(random, initialState, cancellationToken).GetAwaiter().GetResult();
  }
}
