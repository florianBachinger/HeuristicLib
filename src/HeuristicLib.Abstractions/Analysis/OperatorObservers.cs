using HEAL.HeuristicLib.Operators;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.SearchSpaces;
using HEAL.HeuristicLib.States;

namespace HEAL.HeuristicLib.Analysis;

public interface ICreatorObserver<in TG, in TS, in TP>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
{
  void AfterCreation(IReadOnlyList<TG> offspring, int count, TS searchSpace, TP problem);
}

public interface ICrossoverObserver<in TG, in TS, in TP>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
{
  void AfterCross(IReadOnlyList<TG> offspring, IReadOnlyList<IParents<TG>> parents, TS searchSpace, TP problem);
}

public interface IEvaluatorObserver<in TG, in TS, in TP>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
{
  void AfterEvaluation(IReadOnlyList<TG> genotypes, IReadOnlyList<ObjectiveVector> objectiveVectors, TS searchSpace, TP problem);
}

public interface IInterceptorObserver<in TG, in TS, in TP, in TR>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
  where TR : class, IAlgorithmState
{
  void AfterInterception(TR newState, TR currentState, TR? previousState, TS searchSpace, TP problem);
}

public interface IMutatorObserver<in TG, in TS, in TP>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
{
  void AfterMutate(IReadOnlyList<TG> offspring, IReadOnlyList<TG> parent, TS searchSpace, TP problem);
}

public interface IReplacerObserver<in TG, in TS, in TP>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
{
  void AfterReplacement(IReadOnlyList<ISolution<TG>> newPopulation, IReadOnlyList<ISolution<TG>> previousPopulation, IReadOnlyList<ISolution<TG>> offspringPopulation, Objective objective, TS searchSpace, TP problem);
}

public interface ISelectorObserver<in TG, in TS, in TP>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
{
  void AfterSelection(IReadOnlyList<ISolution<TG>> selected, IReadOnlyList<ISolution<TG>> population, Objective objective, int count, TS searchSpace, TP problem);
}

public interface ITerminatorObserver<in TG, in TR, in TS, in TP>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
  where TR : class, IAlgorithmState
{
  void AfterTerminationCheck(bool shouldTerminate, TR state, TS searchSpace, TP problem);
}

public interface IAnalyzerTapRegistry
{
  void Add<TExecutable, TObserver>(TExecutable executable, TObserver observer)
    where TExecutable : notnull
    where TObserver : class;
}

public sealed class ActionCreatorObserver<TG, TS, TP>(Action<IReadOnlyList<TG>, int, TS, TP> afterCreation) : ICreatorObserver<TG, TS, TP>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
{
  public void AfterCreation(IReadOnlyList<TG> offspring, int count, TS searchSpace, TP problem) => afterCreation(offspring, count, searchSpace, problem);
}

public sealed class ActionCrossoverObserver<TG, TS, TP>(Action<IReadOnlyList<TG>, IReadOnlyList<IParents<TG>>, TS, TP> afterCross) : ICrossoverObserver<TG, TS, TP>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
{
  public void AfterCross(IReadOnlyList<TG> offspring, IReadOnlyList<IParents<TG>> parents, TS searchSpace, TP problem) => afterCross(offspring, parents, searchSpace, problem);
}

public sealed class ActionEvaluatorObserver<TG, TS, TP>(Action<IReadOnlyList<TG>, IReadOnlyList<ObjectiveVector>, TS, TP> afterEvaluation) : IEvaluatorObserver<TG, TS, TP>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
{
  public void AfterEvaluation(IReadOnlyList<TG> genotypes, IReadOnlyList<ObjectiveVector> objectiveVectors, TS searchSpace, TP problem) => afterEvaluation(genotypes, objectiveVectors, searchSpace, problem);
}

public sealed class ActionInterceptorObserver<TG, TR, TS, TP>(Action<TR, TR, TR?, TS, TP> afterInterception) : IInterceptorObserver<TG, TS, TP, TR>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
  where TR : class, IAlgorithmState
{
  public void AfterInterception(TR newState, TR currentState, TR? previousState, TS searchSpace, TP problem) => afterInterception(newState, currentState, previousState, searchSpace, problem);
}

public sealed class ActionMutatorObserver<TG, TS, TP>(Action<IReadOnlyList<TG>, IReadOnlyList<TG>, TS, TP> afterMutate) : IMutatorObserver<TG, TS, TP>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
{
  public void AfterMutate(IReadOnlyList<TG> offspring, IReadOnlyList<TG> parent, TS searchSpace, TP problem) => afterMutate(offspring, parent, searchSpace, problem);
}

public sealed class ActionReplacerObserver<TG, TS, TP>(Action<IReadOnlyList<ISolution<TG>>, IReadOnlyList<ISolution<TG>>, IReadOnlyList<ISolution<TG>>, TS, TP> afterReplacement) : IReplacerObserver<TG, TS, TP>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
{
  public void AfterReplacement(IReadOnlyList<ISolution<TG>> newPopulation, IReadOnlyList<ISolution<TG>> previousPopulation, IReadOnlyList<ISolution<TG>> offspringPopulation, Objective objective, TS searchSpace, TP problem)
    => afterReplacement(newPopulation, previousPopulation, offspringPopulation, searchSpace, problem);
}

public sealed class ActionSelectorObserver<TG, TS, TP>(Action<IReadOnlyList<ISolution<TG>>, IReadOnlyList<ISolution<TG>>, Objective, int, TS, TP> afterSelection) : ISelectorObserver<TG, TS, TP>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
{
  public void AfterSelection(IReadOnlyList<ISolution<TG>> selected, IReadOnlyList<ISolution<TG>> population, Objective objective, int count, TS searchSpace, TP problem)
    => afterSelection(selected, population, objective, count, searchSpace, problem);
}

public sealed class TerminatorObserver<TG, TR, TS, TP>(Action<bool, TR, TS, TP> afterTerminateCheck) : ITerminatorObserver<TG, TR, TS, TP>
  where TS : class, ISearchSpace<TG>
  where TP : class, IProblem<TG, TS>
  where TR : class, IAlgorithmState
{
  public void AfterTerminationCheck(bool shouldTerminate, TR state, TS searchSpace, TP problem) => afterTerminateCheck(shouldTerminate, state, searchSpace, problem);
}

public static class AnalyzerTapRegistryExtensions
{
  extension(IAnalyzerTapRegistry taps)
  {
    public void Register<TG, TS, TP>(ICreator<TG, TS, TP> creator, Action<IReadOnlyList<TG>, int, TS, TP> afterCreation)
      where TS : class, ISearchSpace<TG>
      where TP : class, IProblem<TG, TS>
      => taps.Add(creator, new ActionCreatorObserver<TG, TS, TP>(afterCreation));

    public void Register<TG, TS, TP>(ICreator<TG, TS, TP> creator, Action<IReadOnlyList<TG>> afterCreation)
      where TS : class, ISearchSpace<TG>
      where TP : class, IProblem<TG, TS>
      => taps.Add(creator, new ActionCreatorObserver<TG, TS, TP>((offspring, _, _, _) => afterCreation(offspring)));

    public void Register<TG, TS, TP>(ICrossover<TG, TS, TP> crossover, Action<IReadOnlyList<TG>, IReadOnlyList<IParents<TG>>, TS, TP> afterCross)
      where TS : class, ISearchSpace<TG>
      where TP : class, IProblem<TG, TS>
      => taps.Add(crossover, new ActionCrossoverObserver<TG, TS, TP>(afterCross));

    public void Register<TG, TS, TP>(ICrossover<TG, TS, TP> crossover, Action<IReadOnlyList<TG>> afterCross)
      where TS : class, ISearchSpace<TG>
      where TP : class, IProblem<TG, TS>
      => taps.Add(crossover, new ActionCrossoverObserver<TG, TS, TP>((offspring, _, _, _) => afterCross(offspring)));

    public void Register<TG, TS, TP>(IEvaluator<TG, TS, TP> evaluator, Action<IReadOnlyList<TG>, IReadOnlyList<ObjectiveVector>, TS, TP> afterEvaluation)
      where TS : class, ISearchSpace<TG>
      where TP : class, IProblem<TG, TS>
      => taps.Add(evaluator, new ActionEvaluatorObserver<TG, TS, TP>(afterEvaluation));

    public void Register<TG, TS, TP>(IEvaluator<TG, TS, TP> evaluator, Action<IReadOnlyList<TG>, IReadOnlyList<ObjectiveVector>> afterEvaluation)
      where TS : class, ISearchSpace<TG>
      where TP : class, IProblem<TG, TS>
      => taps.Add(evaluator, new ActionEvaluatorObserver<TG, TS, TP>((genotypes, objectiveVectors, _, _) => afterEvaluation(genotypes, objectiveVectors)));

    public void Register<TG, TS, TP, TR>(IInterceptor<TG, TS, TP, TR> interceptor, Action<TR, TR, TR?, TS, TP> afterInterception)
      where TS : class, ISearchSpace<TG>
      where TP : class, IProblem<TG, TS>
      where TR : class, IAlgorithmState
      => taps.Add(interceptor, new ActionInterceptorObserver<TG, TR, TS, TP>(afterInterception));

    public void Register<TG, TS, TP, TR>(IInterceptor<TG, TS, TP, TR> interceptor, Action<TR> afterInterception)
      where TS : class, ISearchSpace<TG>
      where TP : class, IProblem<TG, TS>
      where TR : class, IAlgorithmState
      => taps.Add(interceptor, new ActionInterceptorObserver<TG, TR, TS, TP>((newState, _, _, _, _) => afterInterception(newState)));

    public void Register<TG, TS, TP>(IMutator<TG, TS, TP> mutator, Action<IReadOnlyList<TG>, IReadOnlyList<TG>, TS, TP> afterMutate)
      where TS : class, ISearchSpace<TG>
      where TP : class, IProblem<TG, TS>
      => taps.Add(mutator, new ActionMutatorObserver<TG, TS, TP>(afterMutate));

    public void Register<TG, TS, TP>(IMutator<TG, TS, TP> mutator, Action<IReadOnlyList<TG>> afterMutate)
      where TS : class, ISearchSpace<TG>
      where TP : class, IProblem<TG, TS>
      => taps.Add(mutator, new ActionMutatorObserver<TG, TS, TP>((offspring, _, _, _) => afterMutate(offspring)));

    public void Register<TG, TS, TP>(IReplacer<TG, TS, TP> replacer, Action<IReadOnlyList<ISolution<TG>>, IReadOnlyList<ISolution<TG>>, IReadOnlyList<ISolution<TG>>, TS, TP> afterReplacement)
      where TS : class, ISearchSpace<TG>
      where TP : class, IProblem<TG, TS>
      => taps.Add(replacer, new ActionReplacerObserver<TG, TS, TP>(afterReplacement));

    public void Register<TG, TS, TP>(IReplacer<TG, TS, TP> replacer, Action<IReadOnlyList<ISolution<TG>>> afterReplacement)
      where TS : class, ISearchSpace<TG>
      where TP : class, IProblem<TG, TS>
      => taps.Add(replacer, new ActionReplacerObserver<TG, TS, TP>((newPopulation, _, _, _, _) => afterReplacement(newPopulation)));

    public void Register<TG, TS, TP>(ISelector<TG, TS, TP> selector, Action<IReadOnlyList<ISolution<TG>>, IReadOnlyList<ISolution<TG>>, Objective, int, TS, TP> afterSelection)
      where TS : class, ISearchSpace<TG>
      where TP : class, IProblem<TG, TS>
      => taps.Add(selector, new ActionSelectorObserver<TG, TS, TP>(afterSelection));

    public void Register<TG, TS, TP>(ISelector<TG, TS, TP> selector, Action<IReadOnlyList<ISolution<TG>>> afterSelection)
      where TS : class, ISearchSpace<TG>
      where TP : class, IProblem<TG, TS>
      => taps.Add(selector, new ActionSelectorObserver<TG, TS, TP>((selected, _, _, _, _, _) => afterSelection(selected)));

    public void Register<TG, TR, TS, TP>(ITerminator<TG, TR, TS, TP> terminator, Action<bool, TR, TS, TP> afterTerminationCheck)
      where TS : class, ISearchSpace<TG>
      where TP : class, IProblem<TG, TS>
      where TR : class, IAlgorithmState
      => taps.Add(terminator, new TerminatorObserver<TG, TR, TS, TP>(afterTerminationCheck));

    public void Register<TG, TR, TS, TP>(ITerminator<TG, TR, TS, TP> terminator, Action<bool> afterTerminationCheck)
      where TS : class, ISearchSpace<TG>
      where TP : class, IProblem<TG, TS>
      where TR : class, IAlgorithmState
      => taps.Add(terminator, new TerminatorObserver<TG, TR, TS, TP>((shouldTerminate, _, _, _) => afterTerminationCheck(shouldTerminate)));
  }
}

