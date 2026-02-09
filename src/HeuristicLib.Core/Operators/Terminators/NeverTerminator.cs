namespace HEAL.HeuristicLib.Operators.Terminators;

public record class NeverTerminator<TGenotype>
  : StatelessTerminator<TGenotype>
{
  public override bool ShouldTerminate()
  {
    return false;
  }
}
