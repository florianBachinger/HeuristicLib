using System.Diagnostics.CodeAnalysis;

namespace HEAL.HeuristicLib.Execution;

public class ExecutionInstanceRegistry
{
    // ToDo: do I really want to store this?
    private readonly Run run;
    public Run Run => run;
    
    private readonly ExecutionInstanceRegistry? parentRegistry;
    private readonly Dictionary<IExecutable<IExecutionInstance>, IExecutionInstance> registry = new(ReferenceEqualityComparer.Instance);
    
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
    
    public TExecutionInstance Resolve<TExecutionInstance>(IExecutable<TExecutionInstance> executable)
        where TExecutionInstance : class, IExecutionInstance
    {
        if (TryResolve(executable, out var instance)) {
            return (TExecutionInstance)instance;
        }
        
        instance = executable.CreateExecutionInstance(this);
        
        registry.Add(executable, instance);
        
        return (TExecutionInstance)instance;
    }
}
