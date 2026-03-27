# Analyzer architecture

This page explains the intended analyzer system in HeuristicLib.

It builds on the existing ideas from [Observability & analysis](observability-and-analysis.md) and [Definition vs execution instances](execution-instances.md), but adds one crucial concept:

> Analyzer results belong to the **run**, not to the algorithm/operator definition and not to a short-lived operator execution instance.

## Why analyzers need their own architecture

Analyzer data has a different lifetime from normal operator or algorithm configuration.

- **Definitions** are declarative and re-usable.
  - They should contain configuration and graph structure only.
  - They must stay safe to re-use across multiple runs.
- **Execution instances** are runtime objects.
  - They may hold temporary state while something executes.
  - Their lifetime is controlled by `ExecutionInstanceRegistry` and by meta-algorithms such as `CycleAlgorithm`.
- **Analyzer results** are usually meaningful at the **run** level.
  - quality curves
  - genealogy graphs
  - per-iteration statistics
  - accumulated counters and traces

If analysis state were stored directly on a definition, re-running the same definition would mix old and new results.
If analysis state were stored only inside an observer/decorator execution instance, results would be tied to registry mechanics rather than to the logical run.

That is why HeuristicLib models analyzers as **definition-side hooks + run-scoped analyzer states + run-owned results**.

## The three layers of the analyzer system

The intended system has three layers.

### 1) Analyzer definition

An analyzer definition is a re-usable object that describes:

- what the analyzer observes
- how it hooks into the definition graph
- what result type it publishes
- how to create its runtime analyzer state for a run

All analyzers are run-scoped. The main contract is:

```csharp
public interface IAnalyzer<TResult, out TAnalyzerRunState> : IAnalyzer<TResult>
  where TResult : notnull
  where TAnalyzerRunState : class, IAnalyzerRunState
{
  TAnalyzerRunState CreateAnalyzerState(Run run);
}
```

An analyzer definition is still part of the **definition graph**.
It is configuration-time information, not run-time mutable state.

### 2) Hook definitions in the algorithm/operator graph

Analyzers still use the observable/decorator system as the place where they attach to execution.

Typical hook points are:

- `ObservableEvaluator<TG, TS, TP>`
- `ObservableInterceptor<TG, TS, TP, TR>`
- `ObservableMutator<TG, TS, TP>`
- `ObservableCrossover<TG, TS, TP>`
- `ObservableTerminator<TG, TR, TS, TP>`

At build time, an analyzer is attached through `AttachAnalyzer(...)`.
That method uses the analyzer's supported observer interfaces and decorates the right parts of the definition graph.

For example:

- a quality-curve analyzer needs evaluator hooks
- a genealogy analyzer may need crossover + mutator + iteration-end hooks
- a best/median/worst chart analyzer may need evaluator hooks for accumulation and interceptor hooks for iteration boundaries

### 3) Run-scoped analyzer state

At runtime, every analyzer gets **one analyzer state per run**.

That instance:

- receives callbacks from all hook points that belong to that analyzer
- stores temporary mutable analysis state
- publishes results into the `Run`

This is the run-scoped object:

```csharp
public interface IAnalyzerRunState : IExecutionInstance;
```

A small convenience base class is available:

```csharp
public abstract class AnalyzerRunState<TAnalyzer, TResult>(Run run, TAnalyzer analyzer)
  : IAnalyzerRunState
  where TAnalyzer : class, IAnalyzer<TResult>
  where TResult : notnull
{
  protected Run Run { get; } = run;
  protected TAnalyzer Analyzer { get; } = analyzer;

  protected void PublishResult(TResult result)
  {
    Run.SetResult(Analyzer, result);
  }
}
```

## Ownership and lifetimes

The analyzer architecture is easiest to understand by asking who owns what.

### Definition owns configuration

The analyzer definition owns:

- configuration
- identity
- hook requirements
- the result type it publishes

It does **not** own mutable run results.

### Analyzer state owns temporary mutable analysis state

The run-scoped analyzer state owns things like:

- current counters
- in-progress aggregation for the current iteration
- temporary buffers
- best-so-far values while evaluations happen
- a mutable genealogy graph being built during execution

This state exists only for the current run.

### Run owns published results

The `Run` is the canonical storage for published analyzer output.

Relevant APIs are:

```csharp
run.SetResult(analyzer, result);
run.TryGetResult(analyzer, out var result);
var result = run.GetResult(analyzer);
```

This makes analyzer results:

- independent of individual operator instances
- independent of `ExecutionInstanceRegistry` reuse details
- retrievable through a stable run-level API

## Runtime flow

The runtime flow looks like this.

### Build time

1. Create an analyzer definition.
2. Attach it to an algorithm builder via `AttachAnalyzer(...)`.
3. The builder decorates operators/interceptors/terminators as needed.

### Run creation

1. Create a `Run<TGenotype, TSearchSpace, TProblem, TState>`.
2. The run owns a root `ExecutionInstanceRegistry`.
3. The run also owns analyzer states and analyzer results.

### First hook invocation

When a decorated operator is instantiated, its observer side resolves the analyzer state through the run:

```csharp
instanceRegistry.Run.ResolveAnalyzer(this)
```

This guarantees:

- one analyzer state per analyzer definition per run
- the same analyzer state is shared across all hook points of that analyzer

### During execution

1. The operator or algorithm performs its normal work.
2. The observable wrapper invokes the analyzer hook.
3. The analyzer state updates its temporary state.
4. When a meaningful boundary is reached, it calls `PublishResult(...)`.

In the current design, publishing is typically done **incrementally**.
For many analyzers the natural boundary is the end of an iteration.

## Why the run is the right scope

`ExecutionInstanceRegistry` still matters, but it is not the right place to *own* analyzer results.

A registry controls the lifetime of operator/algorithm execution instances.
This is useful for:

- shared sub-graphs
- stateful operators
- meta-algorithm decisions about reuse or reset

But analyzers are usually intended to describe the full run.

For example, with `CycleAlgorithm`:

- operator execution instances may reset per cycle
- or may persist per inner algorithm
- but the analyzer result should usually still describe the whole run

By resolving analyzers through `Run`, HeuristicLib keeps analyzer scope stable even when execution registries change.

## Definition-side hooks vs execution-side logic

An analyzer usually has two responsibilities that should stay separate.

### Definition side: where to hook in

This side answers:

- Do I need `Evaluate(...)` callbacks?
- Do I need `AfterInterception(...)` callbacks?
- Do I need crossover/mutation/selection hooks?

This is handled by implementing the relevant observer interfaces and attaching through the builder.

### Execution side: what to do with the data

This side answers:

- What temporary state do I accumulate?
- When do I emit a new result snapshot?
- What result shape is stored on the run?

This is handled by the analyzer state.

## Example: iteration statistics analyzer

Suppose an analyzer wants to produce a best/average/worst chart per iteration.

It usually needs two hook families:

- **evaluator hook**
  - accumulate evaluation data while solutions are evaluated
- **interceptor hook**
  - detect the end of an iteration and publish one chart point

The architecture would look like this:

### Definition side

The analyzer definition:

- implements `IAnalyzer<..., ...>`
- implements `IEvaluatorObserver<...>`
- implements `IInterceptorObserver<...>`
- is attached through `AttachAnalyzer(...)`

### Runtime side

The analyzer state:

- receives `AfterEvaluation(...)`
- updates temporary counters/statistics
- receives `AfterInterception(...)`
- finalizes the current iteration snapshot
- calls `PublishResult(...)`

That is the intended pattern for analyzers that combine several hook points.

## Concrete examples in the codebase

### `QualityCurveAnalysis<TGenotype>`

This analyzer observes evaluation events and publishes a best-so-far quality curve.

Its analyzer state stores:

- current best solution
- current evaluation count
- current curve points

Whenever a new best solution is discovered, it publishes the updated curve to the run.

### `BestMedianWorstAnalysis<T>`

This analyzer observes iteration-boundary interception and publishes one entry per completed iteration:

- best solution
- median solution
- worst solution

### `GenealogyAnalysis<T>`

This analyzer combines several hook types:

- crossover hooks
- mutator hooks
- interceptor hooks

Its analyzer state builds a genealogy graph over time and publishes the evolving graph to the run.

## Retrieval model

Consumers should retrieve analyzer results from the run, not from the registry.

Preferred pattern:

```csharp
var analyzer = new QualityCurveAnalysis<MyGenotype>();
builder.AttachAnalyzer(analyzer);

var run = algorithm.CreateRun(problem);
var finalState = run.RunToCompletion(random);

var qualityCurve = run.GetResult(analyzer);
```

Avoid the old pattern of treating observer execution instances as the result container.
The registry is an implementation detail of execution instancing; the run is the public result scope.

## Design rules for analyzer authors

When implementing a new analyzer, follow these rules.

### Do

- keep analyzer definitions re-usable and configuration-only
- put mutable analysis state into the analyzer state
- publish results into `Run`
- attach through observable wrappers/decorators
- use iteration boundaries for live result publishing when the analysis is iteration-based

### Do not

- store mutable analysis results directly on definitions
- treat observable wrapper instances as the permanent home of results
- rely on registry reuse details for analyzer lifetime
- mutate algorithm outcomes from analyzer callbacks

## Relationship to observer wrappers

The analyzer system does **not** replace observable wrappers.
Instead:

- observable wrappers are the **hooking mechanism**
- run-scoped analyzers are the **architecture for owning analysis state and results**

This keeps concerns separate:

- wrappers define *where* callbacks happen
- analyzers define *how* analysis is accumulated and *where* it is stored

## Related pages

- [Observability & analysis](observability-and-analysis.md)
- [Definition vs execution instances](execution-instances.md)
- [Execution model](execution-model.md)
- [Algorithm state](algorithm-state.md)

