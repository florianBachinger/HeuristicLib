using HEAL.HeuristicLib.Execution;
using HEAL.HeuristicLib.Operators.Interceptors;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.SearchSpaces;
using HEAL.HeuristicLib.States;

namespace HEAL.HeuristicLib.Analyzers;

public record BestMedianWorstEntry<T>(ISolution<T> Best, ISolution<T> Median, ISolution<T> Worst);

// public class MyAnalyzer<T> : IEvaluatorObserver<T> where T : class
// {
//  public void AfterEvaluation(IReadOnlyList<T> genotypes,
//                              IReadOnlyList<ObjectiveVector> values,
//                              ISearchSpace<T> searchSpace,
//                              IProblem<T, ISearchSpace<T>> problem)
//  {
//    throw new NotImplementedException();
//  }

//  //public static void test()
//  //{
//  //  var p = new TestFunctionProblem(new AckleyFunction(2));
//  //  IEvaluator<RealVector, RealVectorSearchSpace, TestFunctionProblem> eval = null!;
//  //  var countr = new MyAnalyzer<RealVector>();
//  //  IEvaluator<RealVector, RealVectorSearchSpace, TestFunctionProblem> wrapped = countr.WrapEvaluator(eval);
//  //  wrapped.Evaluate([new RealVector([1,2])], RandomNumberGenerator.Create(0, RandomProfile.NoRandom), p.SearchSpace, p);
//  //  IEvaluator<RealVector, ISearchSpace<RealVector>, IProblem<RealVector, ISearchSpace<RealVector>>> x = eval;
//  //}
// }

public class BestMedianWorstAnalysis<T> : IInterceptorObserver<T, PopulationState<T>>
{

  public IInterceptorObserverInstance<T, ISearchSpace<T>, IProblem<T, ISearchSpace<T>>, PopulationState<T>> CreateExecutionInstance(ExecutionInstanceRegistry instanceRegistry) => new Instance();

  public sealed class Instance : IInterceptorObserverInstance<T, ISearchSpace<T>, IProblem<T, ISearchSpace<T>>, PopulationState<T>>
  {
    private readonly List<BestMedianWorstEntry<T>> bestSolutions = [];
    public IReadOnlyList<BestMedianWorstEntry<T>> BestSolutions => bestSolutions;

    public void AfterInterception(PopulationState<T> newState, PopulationState<T> currentState, PopulationState<T>? previousState, ISearchSpace<T> searchSpace, IProblem<T, ISearchSpace<T>> problem)
    {
      var comp = problem.Objective.TotalOrderComparer is NoTotalOrderComparer ? new LexicographicComparer(problem.Objective.Directions) : problem.Objective.TotalOrderComparer;
      var ordered = currentState.Population.OrderBy(keySelector: x => x.ObjectiveVector, comp).ToArray();
      if (ordered.Length == 0) {
        bestSolutions.Add(null!);

        return;
      }

      bestSolutions.Add(new BestMedianWorstEntry<T>(ordered[0], ordered[ordered.Length / 2], ordered[^1]));
    }
  }

}
