using HEAL.HeuristicLib.Analysis;
using HEAL.HeuristicLib.Execution;
using HEAL.HeuristicLib.Operators;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.SearchSpaces;
using HEAL.HeuristicLib.States;

namespace HEAL.HeuristicLib.Analyzers;

public record BestMedianWorstEntry<T>(ISolution<T> Best, ISolution<T> Median, ISolution<T> Worst);

// public class MyAnalyzer<T> : IEvaluatorObserver<T>
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

public class BestMedianWorstAnalysis<T, TS, TP, TR>(IInterceptor<T, TS, TP, TR> interceptor)
  : IAnalyzer<IReadOnlyList<BestMedianWorstEntry<T>>, BestMedianWorstAnalysis<T, TS, TP, TR>.Instance>
  where TS : class, ISearchSpace<T>
  where TP : class, IProblem<T, TS>
  where TR : PopulationState<T>
{
  public IInterceptor<T, TS, TP, TR> Interceptor { get; } = interceptor;

  public Instance CreateAnalyzerInstance(Run run) => new(run, this);

  public sealed class Instance(Run run, BestMedianWorstAnalysis<T, TS, TP, TR> analyzer)
    : AnalyzerRunInstance<BestMedianWorstAnalysis<T, TS, TP, TR>, IReadOnlyList<BestMedianWorstEntry<T>>>(run, analyzer)
  {
    private readonly List<BestMedianWorstEntry<T>> bestSolutions = [];
    public IReadOnlyList<BestMedianWorstEntry<T>> BestSolutions => bestSolutions;

    public override void RegisterTaps(IAnalyzerTapRegistry taps)
    {
      taps.Register(analyzer.Interceptor, AfterInterception);
    }

    public void AfterInterception(TR newState, TR currentState, TR? previousState, TS searchSpace, TP problem)
    {
      var comp = problem.Objective.TotalOrderComparer is NoTotalOrderComparer ? new LexicographicComparer(problem.Objective.Directions) : problem.Objective.TotalOrderComparer;
      var ordered = currentState.Population.OrderBy(keySelector: x => x.ObjectiveVector, comp).ToArray();
      if (ordered.Length == 0) {
        bestSolutions.Add(null!);

        return;
      }

      bestSolutions.Add(new BestMedianWorstEntry<T>(ordered[0], ordered[ordered.Length / 2], ordered[^1]));
      PublishResult(bestSolutions.ToArray());
    }
  }

}
