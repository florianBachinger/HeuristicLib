using HEAL.HeuristicLib.Genotypes.Vectors;
using HEAL.HeuristicLib.Random;

namespace HEAL.HeuristicLib.SearchSpaces.Vectors;

public record IntegerVectorSearchSpace : SearchSpace<IntegerVector>
{
  public IntegerVectorSearchSpace(int Length, IntegerVector Minimum, IntegerVector Maximum)
  {
    this.Length = Length;
    this.Minimum = Minimum;
    this.Maximum = Maximum;

    //TODO align maximum with step width
  }

  public IntegerVector Step { get; } = new(1); //TODO make configurable
  public int Length { get; init; }
  public IntegerVector Minimum { get; init; }
  public IntegerVector Maximum { get; init; }

  public override bool Contains(IntegerVector genotype)
  {
    return genotype.Count == Length
           && (genotype >= Minimum).All()
           && (genotype <= Maximum).All();
  }

  public static implicit operator RealVectorSearchSpace(IntegerVectorSearchSpace integerVectorSpace)
  {
    return new RealVectorSearchSpace(integerVectorSpace.Length, integerVectorSpace.Minimum, integerVectorSpace.Maximum);
  }

  public int FloorFeasible(double x, int dim)
  {
    return FloorFeasible(
      Minimum.Count == 1 ? Minimum[0] : Minimum[dim],
      Maximum.Count == 1 ? Maximum[0] : Maximum[dim],
      Step.Count == 1 ? Step[0] : Step[dim], x);
  }

  public int CeilingFeasible(double x, int dim)
  {
    return CeilingFeasible(
      Minimum.Count == 1 ? Minimum[0] : Minimum[dim],
      Maximum.Count == 1 ? Maximum[0] : Maximum[dim],
      Step.Count == 1 ? Step[0] : Step[dim], x);
  }

  public int RoundFeasible(double x, int dim)
  {
    return RoundFeasible(
      Minimum.Count == 1 ? Minimum[0] : Minimum[dim],
      Maximum.Count == 1 ? Maximum[0] : Maximum[dim],
      Step.Count == 1 ? Step[0] : Step[dim], x);
  }

  public IntegerVector RoundFeasible(RealVector x)
  {
    var res = new int[x.Count];
    for (var i = 0; i < res.Length; i++) {
      res[i] = RoundFeasible(x[i], i);
    }

    return res;
  }

  public const double Tolerance = 1e-12;

  // Largest feasible value <= x on grid, clamped to [minBound,maxBound]
  public static int FloorFeasible(int minBound, int maxBound, int step, double x)
  {
    if (x <= minBound)
      return minBound;
    if (x >= maxBound)
      return maxBound;

    // compute k = floor((x - minBound)/step)
    double t = (x - minBound) / step;
    int k = (int)Math.Floor(t + Tolerance);
    long v = minBound + (long)k * step;
    if (v < minBound)
      v = minBound;
    if (v > maxBound)
      v = maxBound;
    return (int)v;
  }

  // Smallest feasible value >= x on grid, clamped to [minBound,maxBound]
  public static int CeilingFeasible(int minBound, int maxBound, int step, double x)
  {
    if (x <= minBound)
      return minBound;
    if (x >= maxBound)
      return maxBound;

    // compute k = ceil((x - minBound)/step)
    double t = (x - minBound) / step;
    int k = (int)Math.Ceiling(t - Tolerance);
    long v = minBound + (long)k * step;
    if (v < minBound)
      v = minBound;
    if (v > maxBound)
      v = maxBound;
    return (int)v;
  }

  // Nearest feasible grid value to x (ties: away from zero-ish, but on a shifted grid)
  private static int RoundFeasible(int minBound, int maxBound, int step, double x)
  {
    if (x <= minBound)
      return minBound;
    if (x >= maxBound)
      return maxBound;

    double t = (x - minBound) / step;

    // round to nearest integer index
    int k = (int)Math.Round(t, MidpointRounding.AwayFromZero);

    long v = minBound + (long)k * step;
    if (v < minBound)
      v = minBound;
    if (v > maxBound)
      v = maxBound;
    return (int)v;
  }

  public int UniformRandom(IRandomNumberGenerator random, int idx)
  {
    int min = Minimum[Minimum.Count == 1 ? 0 : idx];
    int max = Maximum[Maximum.Count == 1 ? 0 : idx];
    int step = Step[Step.Count == 1 ? 0 : idx];

    // min/max are guaranteed feasible by the search space ctor.
    int nSteps = (max - min) / step; // inclusive count-1
    int k = random.NextInt(0, nSteps, true); // inclusive
    var v = min + k * step;
    return v;
  }

  public void Deconstruct(out int Length, out IntegerVector Minimum, out IntegerVector Maximum)
  {
    Length = this.Length;
    Minimum = this.Minimum;
    Maximum = this.Maximum;
  }
}
