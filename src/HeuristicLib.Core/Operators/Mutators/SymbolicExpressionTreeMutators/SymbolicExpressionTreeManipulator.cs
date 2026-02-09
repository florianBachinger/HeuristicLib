using HEAL.HeuristicLib.Genotypes.Trees;
using HEAL.HeuristicLib.SearchSpaces.Trees;

namespace HEAL.HeuristicLib.Operators.Mutators.SymbolicExpressionTreeMutators;

public abstract record class SymbolicExpressionTreeManipulator : SingleSolutionStatelessMutator<SymbolicExpressionTree, SymbolicExpressionTreeSearchSpace>;
