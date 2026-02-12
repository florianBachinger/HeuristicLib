using HEAL.HeuristicLib.Execution;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.Random;
using HEAL.HeuristicLib.SearchSpaces;
using Microsoft.Extensions.Caching.Memory;

namespace HEAL.HeuristicLib.Operators.Evaluators;

public record CachingEvaluator<TGenotype, TSearchSpace, TProblem, TKey>
  : Evaluator<TGenotype, TSearchSpace, TProblem>
  where TSearchSpace : class, ISearchSpace<TGenotype>
  where TProblem : class, IProblem<TGenotype, TSearchSpace>
  where TGenotype : notnull
  where TKey : notnull
{
  protected readonly IEvaluator<TGenotype, TSearchSpace, TProblem> Evaluator;
  protected readonly Func<TGenotype, TKey> KeySelector;
  protected readonly long? SizeLimit;

  public CachingEvaluator(IEvaluator<TGenotype, TSearchSpace, TProblem> evaluator, Func<TGenotype, TKey> keySelector, long? sizeLimit = null)
  {
    this.Evaluator = evaluator;
    this.KeySelector = keySelector;
    this.SizeLimit = sizeLimit;
  }

  public override Instance CreateExecutionInstance(ExecutionInstanceRegistry instanceRegistry)
  {
    var evaluatorInstance = instanceRegistry.Resolve(Evaluator);
    return new Instance(evaluatorInstance, KeySelector, SizeLimit);
  }

  public new class Instance
    : Evaluator<TGenotype, TSearchSpace, TProblem>.Instance
  {
    protected readonly IEvaluatorInstance<TGenotype, TSearchSpace, TProblem> Evaluator;
    protected readonly Func<TGenotype, TKey> KeySelector;
    protected readonly MemoryCache Cache;

    public Instance(IEvaluatorInstance<TGenotype, TSearchSpace, TProblem> evaluator, Func<TGenotype, TKey> keySelector, long? sizeLimit)
    {
      this.Evaluator = evaluator;
      this.KeySelector = keySelector;
      var cacheOptions = new MemoryCacheOptions { SizeLimit = sizeLimit, TrackStatistics = true };
      Cache = new MemoryCache(cacheOptions);
    }

    public override IReadOnlyList<ObjectiveVector> Evaluate(
      IReadOnlyList<TGenotype> genotypes,
      IRandomNumberGenerator random,
      TSearchSpace searchSpace,
      TProblem problem)
    {
      var n = genotypes.Count;
      var results = new ObjectiveVector[n];

      // Distinct uncached items in evaluation order
      var uncachedGenotypes = new List<TGenotype>();
      var uncachedKeys = new List<TKey>();
      var uncachedMap = new Dictionary<TKey, (int j, List<int> indices)>();

      for (int i = 0; i < n; i++) {
        var g = genotypes[i];
        var key = KeySelector(g);

        if (Cache.TryGetValue(key, out ObjectiveVector? cached)) {
          results[i] = cached!;
          continue;
        }

        if (!uncachedMap.TryGetValue(key, out var entry)) {
          int j = uncachedGenotypes.Count;
          uncachedGenotypes.Add(g);
          uncachedKeys.Add(key);
          var indices = new List<int>(capacity: 1) { i };
          uncachedMap.Add(key, (j, indices));
        } else {
          entry.indices.Add(i);
        }
      }

      if (uncachedGenotypes.Count == 0)
        return results;

      var newObjectives = Evaluator.Evaluate(uncachedGenotypes, random, searchSpace, problem);

      for (int k = 0; k < uncachedKeys.Count; k++) {
        var key = uncachedKeys[k];
        var obj = newObjectives[k];

        Cache.Set(key, obj, new MemoryCacheEntryOptions { Size = 1 });
      }

      foreach (var (_, entry) in uncachedMap) {
        var obj = newObjectives[entry.j];
        foreach (var i in entry.indices)
          results[i] = obj;
      }

      return results;
    }
  }
}

public record CachingEvaluator<TGenotype, TSearchSpace, TProblem>
  : CachingEvaluator<TGenotype, TSearchSpace, TProblem, TGenotype>
  where TGenotype : notnull
  where TSearchSpace : class, ISearchSpace<TGenotype>
  where TProblem : class, IProblem<TGenotype, TSearchSpace>
{
  public CachingEvaluator(IEvaluator<TGenotype, TSearchSpace, TProblem> evaluator, long? sizeLimit = null) : base(evaluator, x => x, sizeLimit) { }
}

public static class CachedEvaluatorExtensions
{
  extension<TGenotype, TSearchSpace, TProblem>(IEvaluator<TGenotype, TSearchSpace, TProblem> evaluator)
    where TGenotype : notnull
    where TSearchSpace : class, ISearchSpace<TGenotype>
    where TProblem : class, IProblem<TGenotype, TSearchSpace>
  {
    public CachingEvaluator<TGenotype, TSearchSpace, TProblem, TKey> WithCache<TKey>(Func<TGenotype, TKey> keySelector, long? sizeLimit = null) where TKey : notnull
      => new(evaluator, keySelector, sizeLimit);

    public CachingEvaluator<TGenotype, TSearchSpace, TProblem> WithCache(long? sizeLimit = null)
      => new(evaluator, sizeLimit);
  }
}
