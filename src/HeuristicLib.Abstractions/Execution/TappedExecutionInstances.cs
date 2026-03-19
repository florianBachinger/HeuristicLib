using HEAL.HeuristicLib.Analysis;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Operators;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.Random;
using HEAL.HeuristicLib.SearchSpaces;
using HEAL.HeuristicLib.States;

namespace HEAL.HeuristicLib.Execution;

internal sealed class TappedCreatorInstance<TG, TS, TP>(ICreatorInstance<TG, TS, TP> inner, IReadOnlyList<ICreatorObserver<TG, TS, TP>> observers) : ICreatorInstance<TG, TS, TP>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
{
  public IReadOnlyList<TG> Create(int count, IRandomNumberGenerator random, TS searchSpace, TP problem)
  {
    var result = inner.Create(count, random, searchSpace, problem);
    foreach (var observer in observers) {
      observer.AfterCreation(result, count, searchSpace, problem);
    }

    return result;
  }
}

internal sealed class TappedCrossoverInstance<TG, TS, TP>(ICrossoverInstance<TG, TS, TP> inner, IReadOnlyList<ICrossoverObserver<TG, TS, TP>> observers) : ICrossoverInstance<TG, TS, TP>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
{
  public IReadOnlyList<TG> Cross(IReadOnlyList<IParents<TG>> parents, IRandomNumberGenerator random, TS searchSpace, TP problem)
  {
    var result = inner.Cross(parents, random, searchSpace, problem);
    foreach (var observer in observers) {
      observer.AfterCross(result, parents, searchSpace, problem);
    }

    return result;
  }
}

internal sealed class TappedEvaluatorInstance<TG, TS, TP>(IEvaluatorInstance<TG, TS, TP> inner, IReadOnlyList<IEvaluatorObserver<TG, TS, TP>> observers) : IEvaluatorInstance<TG, TS, TP>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
{
  public IReadOnlyList<ObjectiveVector> Evaluate(IReadOnlyList<TG> genotypes, IRandomNumberGenerator random, TS searchSpace, TP problem)
  {
    var result = inner.Evaluate(genotypes, random, searchSpace, problem);
    foreach (var observer in observers) {
      observer.AfterEvaluation(genotypes, result, searchSpace, problem);
    }

    return result;
  }
}

internal sealed class TappedInterceptorInstance<TG, TS, TP, TR>(IInterceptorInstance<TG, TS, TP, TR> inner, IReadOnlyList<IInterceptorObserver<TG, TS, TP, TR>> observers) : IInterceptorInstance<TG, TS, TP, TR>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
  where TR : class, IAlgorithmState
{
  public TR Transform(TR currentState, TR? previousState, TS searchSpace, TP problem)
  {
    var result = inner.Transform(currentState, previousState, searchSpace, problem);
    foreach (var observer in observers) {
      observer.AfterInterception(result, currentState, previousState, searchSpace, problem);
    }

    return result;
  }
}

internal sealed class TappedMutatorInstance<TG, TS, TP>(IMutatorInstance<TG, TS, TP> inner, IReadOnlyList<IMutatorObserver<TG, TS, TP>> observers) : IMutatorInstance<TG, TS, TP>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
{
  public IReadOnlyList<TG> Mutate(IReadOnlyList<TG> parent, IRandomNumberGenerator random, TS searchSpace, TP problem)
  {
    var result = inner.Mutate(parent, random, searchSpace, problem);
    foreach (var observer in observers) {
      observer.AfterMutate(result, parent, searchSpace, problem);
    }

    return result;
  }
}

internal sealed class TappedReplacerInstance<TG, TS, TP>(IReplacerInstance<TG, TS, TP> inner, IReadOnlyList<IReplacerObserver<TG, TS, TP>> observers) : IReplacerInstance<TG, TS, TP>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
{
  public IReadOnlyList<ISolution<TG>> Replace(IReadOnlyList<ISolution<TG>> previousPopulation, IReadOnlyList<ISolution<TG>> offspringPopulation, Objective objective, int count, IRandomNumberGenerator random, TS searchSpace, TP problem)
  {
    var result = inner.Replace(previousPopulation, offspringPopulation, objective, count, random, searchSpace, problem);
    foreach (var observer in observers) {
      observer.AfterReplacement(result, previousPopulation, offspringPopulation, objective, searchSpace, problem);
    }

    return result;
  }
}

internal sealed class TappedSelectorInstance<TG, TS, TP>(ISelectorInstance<TG, TS, TP> inner, IReadOnlyList<ISelectorObserver<TG, TS, TP>> observers) : ISelectorInstance<TG, TS, TP>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
{
  public IReadOnlyList<ISolution<TG>> Select(IReadOnlyList<ISolution<TG>> population, Objective objective, int count, IRandomNumberGenerator random, TS searchSpace, TP problem)
  {
    var result = inner.Select(population, objective, count, random, searchSpace, problem);
    foreach (var observer in observers) {
      observer.AfterSelection(result, population, objective, count, searchSpace, problem);
    }

    return result;
  }
}

internal sealed class TappedTerminatorInstance<TG, TR, TS, TP>(ITerminatorInstance<TG, TR, TS, TP> inner, IReadOnlyList<ITerminatorObserver<TG, TR, TS, TP>> observers) : ITerminatorInstance<TG, TR, TS, TP>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
  where TR : class, IAlgorithmState
{
  public bool ShouldTerminate(TR state, TS searchSpace, TP problem)
  {
    var result = inner.ShouldTerminate(state, searchSpace, problem);
    foreach (var observer in observers) {
      observer.AfterTerminationCheck(result, state, searchSpace, problem);
    }

    return result;
  }
}
