using HEAL.HeuristicLib.States;

namespace HEAL.HeuristicLib.Operators.Interceptors;

public record class IdentityInterceptor<TG, TState> : StatelessInterceptor<TG, TState>
  where TState : class, IAlgorithmState
{
  public override TState Transform(TState currentState, TState? previousState) => currentState;
}
