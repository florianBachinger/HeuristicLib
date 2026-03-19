using System.Diagnostics.CodeAnalysis;
using HEAL.HeuristicLib.Analysis;
using HEAL.HeuristicLib.Operators;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.SearchSpaces;
using HEAL.HeuristicLib.States;

namespace HEAL.HeuristicLib.Execution;

public class ExecutionInstanceRegistry
{
    // ToDo: do I really want to store this?
    private readonly Run run;
    public Run Run => run;
    
    private readonly ExecutionInstanceRegistry? parentRegistry;
    private readonly Dictionary<IExecutable<IExecutionInstance>, IExecutionInstance> registry = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<IExecutable<IExecutionInstance>, IExecutionInstance> untappedRegistry = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<IExecutable<IExecutionInstance>> untappedExecutables = new(ReferenceEqualityComparer.Instance);
    
    public ExecutionInstanceRegistry(Run run, ExecutionInstanceRegistry? parentRegistry = null)
    {
        this.run = run;
        this.parentRegistry = parentRegistry;
    }

    public ExecutionInstanceRegistry CreateChildRegistry()
    {
        return new ExecutionInstanceRegistry(run, this);
    }
    
    private bool TryResolve(IExecutable<IExecutionInstance> executable, [MaybeNullWhen(false)] out IExecutionInstance instance)
    {
        if (parentRegistry is not null && parentRegistry.TryResolve(executable, out instance)) {
            return true;
        }

        return registry.TryGetValue(executable, out instance);
    }

    private bool TryResolveUntapped(IExecutable<IExecutionInstance> executable, [MaybeNullWhen(false)] out IExecutionInstance instance)
    {
        if (parentRegistry is not null && parentRegistry.TryResolveUntapped(executable, out instance)) {
            return true;
        }

        return untappedRegistry.TryGetValue(executable, out instance);
    }
    
    public TExecutionInstance Resolve<TExecutionInstance>(IExecutable<TExecutionInstance> executable)
        where TExecutionInstance : class, IExecutionInstance
    {
        if (TryResolve(executable, out var instance)) {
            return (TExecutionInstance)instance;
        }

        if (!untappedExecutables.Contains((IExecutable<IExecutionInstance>)executable)
            && TryCreateTappedInstance(executable, out var tappedInstance)) {
            registry.Add((IExecutable<IExecutionInstance>)executable, tappedInstance);
            return tappedInstance;
        }
        
        instance = executable.CreateExecutionInstance(this);
        
        registry.Add(executable, instance);
        
        return (TExecutionInstance)instance;
    }

    public TExecutionInstance ResolveUntapped<TExecutionInstance>(IExecutable<TExecutionInstance> executable)
        where TExecutionInstance : class, IExecutionInstance
    {
        if (TryResolveUntapped(executable, out var instance)) {
            return (TExecutionInstance)instance;
        }

        var executableKey = (IExecutable<IExecutionInstance>)executable;
        untappedExecutables.Add(executableKey);
        try {
            instance = executable.CreateExecutionInstance(this);
        } finally {
            untappedExecutables.Remove(executableKey);
        }

        untappedRegistry.Add(executableKey, instance);
        return (TExecutionInstance)instance;
    }

    public IReadOnlyList<TObserver> GetObservers<TObserver>(object executable)
        where TObserver : class
        => run.GetAnalyzerTaps().GetObservers<TObserver>(executable);

    private bool TryCreateTappedInstance<TExecutionInstance>(IExecutable<TExecutionInstance> executable, [MaybeNullWhen(false)] out TExecutionInstance instance)
        where TExecutionInstance : class, IExecutionInstance
    {
        IExecutionInstance tappedInstance;
        if (TryCreateTappedInstanceCore((dynamic)executable!, out tappedInstance)) {
            instance = (TExecutionInstance)tappedInstance;
            return true;
        }

        instance = default;
        return false;
    }

    private bool TryCreateTappedInstanceCore<TG, TS, TP>(ICreator<TG, TS, TP> creator, [MaybeNullWhen(false)] out IExecutionInstance instance)
        where TS : class, ISearchSpace<TG>
        where TP : class, IProblem<TG, TS>
        => TryCreateTappedInstanceCore<ICreator<TG, TS, TP>, ICreatorInstance<TG, TS, TP>, ICreatorObserver<TG, TS, TP>>(creator, static (inner, observers) => new TappedCreatorInstance<TG, TS, TP>(inner, observers), out instance);

    private bool TryCreateTappedInstanceCore<TG, TS, TP>(ICrossover<TG, TS, TP> crossover, [MaybeNullWhen(false)] out IExecutionInstance instance)
        where TS : class, ISearchSpace<TG>
        where TP : class, IProblem<TG, TS>
        => TryCreateTappedInstanceCore<ICrossover<TG, TS, TP>, ICrossoverInstance<TG, TS, TP>, ICrossoverObserver<TG, TS, TP>>(crossover, static (inner, observers) => new TappedCrossoverInstance<TG, TS, TP>(inner, observers), out instance);

    private bool TryCreateTappedInstanceCore<TG, TS, TP>(IEvaluator<TG, TS, TP> evaluator, [MaybeNullWhen(false)] out IExecutionInstance instance)
        where TS : class, ISearchSpace<TG>
        where TP : class, IProblem<TG, TS>
        => TryCreateTappedInstanceCore<IEvaluator<TG, TS, TP>, IEvaluatorInstance<TG, TS, TP>, IEvaluatorObserver<TG, TS, TP>>(evaluator, static (inner, observers) => new TappedEvaluatorInstance<TG, TS, TP>(inner, observers), out instance);

    private bool TryCreateTappedInstanceCore<TG, TS, TP, TR>(IInterceptor<TG, TS, TP, TR> interceptor, [MaybeNullWhen(false)] out IExecutionInstance instance)
        where TS : class, ISearchSpace<TG>
        where TP : class, IProblem<TG, TS>
        where TR : class, IAlgorithmState
        => TryCreateTappedInstanceCore<IInterceptor<TG, TS, TP, TR>, IInterceptorInstance<TG, TS, TP, TR>, IInterceptorObserver<TG, TS, TP, TR>>(interceptor, static (inner, observers) => new TappedInterceptorInstance<TG, TS, TP, TR>(inner, observers), out instance);

    private bool TryCreateTappedInstanceCore<TG, TS, TP>(IMutator<TG, TS, TP> mutator, [MaybeNullWhen(false)] out IExecutionInstance instance)
        where TS : class, ISearchSpace<TG>
        where TP : class, IProblem<TG, TS>
        => TryCreateTappedInstanceCore<IMutator<TG, TS, TP>, IMutatorInstance<TG, TS, TP>, IMutatorObserver<TG, TS, TP>>(mutator, static (inner, observers) => new TappedMutatorInstance<TG, TS, TP>(inner, observers), out instance);

    private bool TryCreateTappedInstanceCore<TG, TS, TP>(IReplacer<TG, TS, TP> replacer, [MaybeNullWhen(false)] out IExecutionInstance instance)
        where TS : class, ISearchSpace<TG>
        where TP : class, IProblem<TG, TS>
        => TryCreateTappedInstanceCore<IReplacer<TG, TS, TP>, IReplacerInstance<TG, TS, TP>, IReplacerObserver<TG, TS, TP>>(replacer, static (inner, observers) => new TappedReplacerInstance<TG, TS, TP>(inner, observers), out instance);

    private bool TryCreateTappedInstanceCore<TG, TS, TP>(ISelector<TG, TS, TP> selector, [MaybeNullWhen(false)] out IExecutionInstance instance)
        where TS : class, ISearchSpace<TG>
        where TP : class, IProblem<TG, TS>
        => TryCreateTappedInstanceCore<ISelector<TG, TS, TP>, ISelectorInstance<TG, TS, TP>, ISelectorObserver<TG, TS, TP>>(selector, static (inner, observers) => new TappedSelectorInstance<TG, TS, TP>(inner, observers), out instance);

    private bool TryCreateTappedInstanceCore<TG, TR, TS, TP>(ITerminator<TG, TR, TS, TP> terminator, [MaybeNullWhen(false)] out IExecutionInstance instance)
        where TS : class, ISearchSpace<TG>
        where TP : class, IProblem<TG, TS>
        where TR : class, IAlgorithmState
        => TryCreateTappedInstanceCore<ITerminator<TG, TR, TS, TP>, ITerminatorInstance<TG, TR, TS, TP>, ITerminatorObserver<TG, TR, TS, TP>>(terminator, static (inner, observers) => new TappedTerminatorInstance<TG, TR, TS, TP>(inner, observers), out instance);

    private bool TryCreateTappedInstanceCore<TExecutable, TExecutionInstance, TObserver>(
        TExecutable executable,
        Func<TExecutionInstance, IReadOnlyList<TObserver>, IExecutionInstance> createTappedInstance,
        [MaybeNullWhen(false)] out IExecutionInstance instance)
        where TExecutable : class, IExecutable<TExecutionInstance>
        where TExecutionInstance : class, IExecutionInstance
        where TObserver : class
    {
        var observers = GetObservers<TObserver>(executable);
        if (observers.Count == 0) {
            instance = default;
            return false;
        }

        instance = createTappedInstance(ResolveUntapped(executable), observers);
        return true;
    }

    private static bool TryCreateTappedInstanceCore(object executable, [MaybeNullWhen(false)] out IExecutionInstance instance)
    {
        instance = default;
        return false;
    }
}
