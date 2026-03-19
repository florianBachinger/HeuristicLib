using System.Collections;
using HEAL.HeuristicLib.Analysis;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Operators;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.SearchSpaces;
using HEAL.HeuristicLib.States;

namespace HEAL.HeuristicLib.Execution;

internal sealed class AnalyzerTapRegistry : IAnalyzerTapRegistry
{
  private readonly Dictionary<object, List<object>> observers = new(ReferenceEqualityComparer.Instance);

  public void Add<TExecutable, TObserver>(TExecutable executable, TObserver observer)
    where TExecutable : notnull
    where TObserver : class
    => Store(executable, observer);

  public IReadOnlyList<TObserver> GetObservers<TObserver>(object executable)
    where TObserver : class
  {
    var untypedObservers = (IEnumerable)GetObserversCore((dynamic)executable);
    return untypedObservers.Cast<object>().Cast<TObserver>().ToArray();
  }

  private IReadOnlyList<ICreatorObserver<TG, TS, TP>> GetObserversCore<TG, TS, TP>(ICreator<TG, TS, TP> creator)
    where TS : class, ISearchSpace<TG>
    where TP : class, IProblem<TG, TS>
    => Get(creator).Select(AdaptCreator<TG, TS, TP>).ToArray();

  private IReadOnlyList<ICrossoverObserver<TG, TS, TP>> GetObserversCore<TG, TS, TP>(ICrossover<TG, TS, TP> crossover)
    where TS : class, ISearchSpace<TG>
    where TP : class, IProblem<TG, TS>
    => Get(crossover).Select(AdaptCrossover<TG, TS, TP>).ToArray();

  private IReadOnlyList<IEvaluatorObserver<TG, TS, TP>> GetObserversCore<TG, TS, TP>(IEvaluator<TG, TS, TP> evaluator)
    where TS : class, ISearchSpace<TG>
    where TP : class, IProblem<TG, TS>
    => Get(evaluator).Select(AdaptEvaluator<TG, TS, TP>).ToArray();

  private IReadOnlyList<IInterceptorObserver<TG, TS, TP, TR>> GetObserversCore<TG, TS, TP, TR>(IInterceptor<TG, TS, TP, TR> interceptor)
    where TS : class, ISearchSpace<TG>
    where TP : class, IProblem<TG, TS>
    where TR : class, IAlgorithmState
    => Get(interceptor).Select(AdaptInterceptor<TG, TS, TP, TR>).ToArray();

  private IReadOnlyList<IMutatorObserver<TG, TS, TP>> GetObserversCore<TG, TS, TP>(IMutator<TG, TS, TP> mutator)
    where TS : class, ISearchSpace<TG>
    where TP : class, IProblem<TG, TS>
    => Get(mutator).Select(AdaptMutator<TG, TS, TP>).ToArray();

  private IReadOnlyList<IReplacerObserver<TG, TS, TP>> GetObserversCore<TG, TS, TP>(IReplacer<TG, TS, TP> replacer)
    where TS : class, ISearchSpace<TG>
    where TP : class, IProblem<TG, TS>
    => Get(replacer).Select(AdaptReplacer<TG, TS, TP>).ToArray();

  private IReadOnlyList<ISelectorObserver<TG, TS, TP>> GetObserversCore<TG, TS, TP>(ISelector<TG, TS, TP> selector)
    where TS : class, ISearchSpace<TG>
    where TP : class, IProblem<TG, TS>
    => Get(selector).Select(AdaptSelector<TG, TS, TP>).ToArray();

  private IReadOnlyList<ITerminatorObserver<TG, TR, TS, TP>> GetObserversCore<TG, TR, TS, TP>(ITerminator<TG, TR, TS, TP> terminator)
    where TS : class, ISearchSpace<TG>
    where TP : class, IProblem<TG, TS>
    where TR : class, IAlgorithmState
    => Get(terminator).Select(AdaptTerminator<TG, TR, TS, TP>).ToArray();

  public static IReadOnlyList<object> GetObserversCore(object executable) => [];

  private void Store(object executable, object observer)
  {
    if (!observers.TryGetValue(executable, out var observerList)) {
      observerList = [];
      observers[executable] = observerList;
    }

    observerList.Add(observer);
  }

  private IReadOnlyList<object> Get(object executable)
  {
    return observers.TryGetValue(executable, out var observerList)
      ? observerList
      : [];
  }

  private static ICreatorObserver<TG, TS, TP> AdaptCreator<TG, TS, TP>(object observer)
    where TS : class, ISearchSpace<TG>
    where TP : class, IProblem<TG, TS>
    => observer as ICreatorObserver<TG, TS, TP>
       ?? new ActionCreatorObserver<TG, TS, TP>((offspring, count, searchSpace, problem) => ((dynamic)observer).AfterCreation(offspring, count, (dynamic)searchSpace, (dynamic)problem));

  private static ICrossoverObserver<TG, TS, TP> AdaptCrossover<TG, TS, TP>(object observer)
    where TS : class, ISearchSpace<TG>
    where TP : class, IProblem<TG, TS>
    => observer as ICrossoverObserver<TG, TS, TP>
       ?? new ActionCrossoverObserver<TG, TS, TP>((offspring, parents, searchSpace, problem) => ((dynamic)observer).AfterCross(offspring, parents, (dynamic)searchSpace, (dynamic)problem));

  private static IEvaluatorObserver<TG, TS, TP> AdaptEvaluator<TG, TS, TP>(object observer)
    where TS : class, ISearchSpace<TG>
    where TP : class, IProblem<TG, TS>
    => observer as IEvaluatorObserver<TG, TS, TP>
       ?? new ActionEvaluatorObserver<TG, TS, TP>((genotypes, objectiveVectors, searchSpace, problem) => ((dynamic)observer).AfterEvaluation(genotypes, objectiveVectors, (dynamic)searchSpace, (dynamic)problem));

  private static IInterceptorObserver<TG, TS, TP, TR> AdaptInterceptor<TG, TS, TP, TR>(object observer)
    where TS : class, ISearchSpace<TG>
    where TP : class, IProblem<TG, TS>
    where TR : class, IAlgorithmState
    => observer as IInterceptorObserver<TG, TS, TP, TR>
       ?? new ActionInterceptorObserver<TG, TR, TS, TP>((newState, currentState, previousState, searchSpace, problem) => ((dynamic)observer).AfterInterception((dynamic)newState, (dynamic)currentState, previousState is null ? null : (dynamic)previousState, (dynamic)searchSpace, (dynamic)problem));

  private static IMutatorObserver<TG, TS, TP> AdaptMutator<TG, TS, TP>(object observer)
    where TS : class, ISearchSpace<TG>
    where TP : class, IProblem<TG, TS>
    => observer as IMutatorObserver<TG, TS, TP>
       ?? new ActionMutatorObserver<TG, TS, TP>((offspring, parent, searchSpace, problem) => ((dynamic)observer).AfterMutate(offspring, parent, (dynamic)searchSpace, (dynamic)problem));

  private static IReplacerObserver<TG, TS, TP> AdaptReplacer<TG, TS, TP>(object observer)
    where TS : class, ISearchSpace<TG>
    where TP : class, IProblem<TG, TS>
    => observer as IReplacerObserver<TG, TS, TP>
       ?? new ActionReplacerObserver<TG, TS, TP>((newPopulation, previousPopulation, offspringPopulation, searchSpace, problem) => ((dynamic)observer).AfterReplacement(newPopulation, previousPopulation, offspringPopulation, default(Objective)!, (dynamic)searchSpace, (dynamic)problem));

  private static ISelectorObserver<TG, TS, TP> AdaptSelector<TG, TS, TP>(object observer)
    where TS : class, ISearchSpace<TG>
    where TP : class, IProblem<TG, TS>
    => observer as ISelectorObserver<TG, TS, TP>
       ?? new ActionSelectorObserver<TG, TS, TP>((selected, population, objective, count, searchSpace, problem) => ((dynamic)observer).AfterSelection(selected, population, objective, count, (dynamic)searchSpace, (dynamic)problem));

  private static ITerminatorObserver<TG, TR, TS, TP> AdaptTerminator<TG, TR, TS, TP>(object observer)
    where TS : class, ISearchSpace<TG>
    where TP : class, IProblem<TG, TS>
    where TR : class, IAlgorithmState
    => observer as ITerminatorObserver<TG, TR, TS, TP>
       ?? new TerminatorObserver<TG, TR, TS, TP>((shouldTerminate, state, searchSpace, problem) => ((dynamic)observer).AfterTerminationCheck(shouldTerminate, (dynamic)state, (dynamic)searchSpace, (dynamic)problem));
}




