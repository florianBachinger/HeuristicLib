namespace HEAL.HeuristicLib.Random.Distributions;

public sealed class NormalDistribution(double mu = 0, double sigma = 1) : IDistribution<double>
{
  public double Sample(IRandomNumberGenerator rng) => NextDouble(rng, mu, sigma);

  public static double NextDouble(IRandomNumberGenerator random, double mu = 0, double sigma = 1)
  {
    double u;
    double s;
    do {
      u = (random.NextDouble() * 2) - 1;
      var v = (random.NextDouble() * 2) - 1;
      s = (u * u) + (v * v);
    } while (s is > 1 or 0);

    s = Math.Sqrt(-2.0 * Math.Log(s) / s);
    return mu + (sigma * u * s);
  }
}

public static class NormalExtensions
{
  public static double NextGaussian(this IRandomNumberGenerator rng, double mu = 0, double sigma = 1) => NormalDistribution.NextDouble(rng, mu, sigma);
}
