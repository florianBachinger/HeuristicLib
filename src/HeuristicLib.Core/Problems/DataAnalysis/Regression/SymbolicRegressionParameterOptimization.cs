using System.Diagnostics;
using HEAL.HeuristicLib.Genotypes.Trees;
using HEAL.HeuristicLib.Problems.DataAnalysis.Regression.Evaluators;
using HEAL.HeuristicLib.Problems.DataAnalysis.Symbolic;
using HEAL.HeuristicLib.SearchSpaces.Trees.SymbolicExpressionTree.Symbols.Math;
using HEAL.HeuristicLib.SearchSpaces.Trees.SymbolicExpressionTree.Symbols.Math.Variables;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;

namespace HEAL.HeuristicLib.Problems.DataAnalysis.Regression;

public static class SymbolicRegressionParameterOptimization
{
  public delegate void PFunc(double[] c, double[] x, ref double fx, object o);

  public delegate void PGrad(double[] c, double[] x, ref double fx, double[] grad, object o);

  public static readonly PearsonR2Evaluator[] Evaluator = [new()];

  public static double OptimizeParameters(ISymbolicDataAnalysisExpressionTreeInterpreter interpreter,
                                          SymbolicExpressionTree tree,
                                          RegressionProblemData problemData,
                                          IReadOnlyList<int> rows,
                                          int maxIterations,
                                          bool updateVariableWeights = true,
                                          double lowerEstimationLimit = double.MinValue,
                                          double upperEstimationLimit = double.MaxValue,
                                          bool updateParametersInTree = true,
                                          Action<double[], double, object>? iterationCallback = null,
                                          EvaluationsCounter? counter = null)
  {
    // Numeric parameters in the tree become variables for parameter optimization.
    // Variables in the tree become parameters (fixed values) for parameter optimization.
    // For each parameter (variable in the original tree) we store the 
    // variable name, variable value (for factor vars) and lag as a DataForVariable object.
    // A dictionary is used to find parameters

    if (!TreeToAutoDiffTermConverter.TryConvertToAutoDiff(tree, updateVariableWeights, out var parameters, out var initialParameters, out var func, out var funcGrad)) {
      throw new NotSupportedException("Could not optimize parameters of symbolic expression tree due to not supported symbols used in the tree.");
    }

    ArgumentNullException.ThrowIfNull(parameters);
    ArgumentNullException.ThrowIfNull(initialParameters);
    ArgumentNullException.ThrowIfNull(func);
    ArgumentNullException.ThrowIfNull(funcGrad);

    if (parameters.Count == 0) {
      return 0.0; // constant expressions always have an R� of 0.0 
    }

    var parameterEntries = parameters.ToArray(); // order of entries must be the same for x

    // extract initial parameters

    var c = (double[])initialParameters.Clone();

    interpreter.GetSymbolicExpressionTreeValues(tree, problemData.Dataset, rows);

    // var model = new BoundedSymbolicRegressionModel(tree, interpreter, lowerEstimationLimit, upperEstimationLimit); // applyLinearScaling, lowerEstimationLimit, upperEstimationLimit;
    var model = new SymbolicRegressionModel(tree, interpreter);
    var originalQuality = problemData.Evaluate(model, rows, Evaluator, lowerEstimationLimit, upperEstimationLimit)[0];

    counter ??= new EvaluationsCounter();
    var rowEvaluationsCounter = new EvaluationsCounter();

    var ds = problemData.Dataset;
    var x = new double[rows.Count, parameters.Count];
    var row = 0;
    foreach (var r in rows) {
      var col = 0;
      foreach (var info in parameterEntries) {
        if (ds.VariableHasType<double>(info.VariableName)) {
          x[row, col] = ds.GetDoubleValue(info.VariableName, r + info.Lag);
        } else if (ds.VariableHasType<string>(info.VariableName)) {
          x[row, col] = ds.GetStringValue(info.VariableName, r) == info.VariableValue ? 1 : 0;
        } else {
          throw new InvalidProgramException("found a variable of unknown type");
        }

        col++;
      }

      row++;
    }

    var y = ds.GetDoubleValues(problemData.TargetVariable, rows).ToArray();
    var n = x.GetLength(0);
    var m = x.GetLength(1);
    var k = c.Length;

    var functionCx1Func = CreatePFunc(func);
    var functionCx1Grad = CreatePGrad(funcGrad);

    double[] cOpt = [];
    var status = ExitCondition.None;
    bool success = true;

    try {
      (cOpt, status) = MathNetLevenbergMarquardt(maxIterations, n, y, m, x, func, rowEvaluationsCounter, funcGrad, c.ToArray(), k);
    } catch (NonConvergenceException) {
      success = false;
    } catch (ArgumentException) {
      success = false;
    }

    if (status is ExitCondition.InvalidValues) {
      return originalQuality;
    }

    counter.FunctionEvaluations += rowEvaluationsCounter.FunctionEvaluations / n;
    counter.GradientEvaluations += rowEvaluationsCounter.GradientEvaluations / n;

    //optimizer detected  NAN / INF  in  the target function and/ or gradient
    if (!success) {
      return originalQuality;
    }

    double quality;
    UpdateParameters(tree, cOpt, updateVariableWeights);
    try {
      quality = problemData.Evaluate(model, rows, Evaluator)[0];
    } catch (InvalidOperationException) {
      // this happens when the new parameters produce invalid results (e.g. catastrophic cancellation)
      UpdateParameters(tree, initialParameters, updateVariableWeights);
      return originalQuality;
    }

    var improvement = originalQuality - quality <= 0.001 && !double.IsNaN(quality);

    if (!improvement) {
      UpdateParameters(tree, initialParameters, updateVariableWeights); // reset tree parameters
      return originalQuality;
    }

    if (!updateParametersInTree)
      UpdateParameters(tree, initialParameters, updateVariableWeights); // reset tree parameters
    return quality;
  }

  private static void UpdateParameters(SymbolicExpressionTree tree, double[] parameters, bool updateVariableWeights)
  {
    var i = 0;
    foreach (var node in tree.Root.IterateNodesPrefix()) {
      switch (node) {
        case NumberTreeNode { Parent.Symbol: Power } numberTreeNode when (numberTreeNode.Parent?[1] ?? null) == numberTreeNode:
          continue; // exponents in powers are not optimized (see TreeToAutoDiffTermConverter)
        case NumberTreeNode numberTreeNode:
          numberTreeNode.Value = parameters[i++];

          break;
        case VariableTreeNodeBase variableTreeNodeBase when updateVariableWeights:
          variableTreeNodeBase.Weight = parameters[i++];

          break;
        case VariableTreeNodeBase: {
          if (node is FactorVariableTreeNode { Weights: not null } factorVarTreeNode) {
            for (var j = 0; j < factorVarTreeNode.Weights.Length; j++) {
              factorVarTreeNode.Weights[j] = parameters[i++];
            }
          }

          break;
        }
      }
    }
  }

  private static PFunc CreatePFunc(TreeToAutoDiffTermConverter.ParametricFunction func)
  {
    return (c, x, ref fx, o) => {
      fx = func(c, x);
      var counter = (EvaluationsCounter)o;
      counter.FunctionEvaluations++;
    };
  }

  private static PGrad CreatePGrad(TreeToAutoDiffTermConverter.ParametricFunctionGradient funcGrad)
  {
    return (c, x, ref fx, grad, o) => {
      var tuple = funcGrad(c, x);
      fx = tuple.Item2;
      Array.Copy(tuple.Item1, grad, grad.Length);
      var counter = (EvaluationsCounter)o;
      counter.GradientEvaluations++;
    };
  }

  public static bool CanOptimizeParameters(SymbolicExpressionTree tree) => TreeToAutoDiffTermConverter.IsCompatible(tree);
}
