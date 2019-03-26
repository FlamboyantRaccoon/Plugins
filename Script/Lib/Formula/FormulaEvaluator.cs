// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain Minjard <s.minjard@bigpoint.net>
//
// Date: 2017/03/14

using UnityEngine;

using System.Collections.Generic;

//!	@class	FormulaEvaluator
//!
//!	@brief	Evaluator of a formula
public class FormulaEvaluator 
{
	public delegate bool VariableResolverDelegate(string a_variableName, out double a_variableValue);
	public delegate bool MethodResolverDelegate(string a_methodName, double[] a_arguments, out double a_variableValue);

	public FormulaEvaluator()
	{
		m_metaFunctionStack = new Stack<FormulaParser.FormulaMethod>();
		m_metaFunctionFirstArgumentIndex = new Stack<int>();
		m_metaArgumentValues = new List<double>();
		
		m_variableResolverQueue = new lwPriorityQueue<VariableResolverDelegate>();
		m_methodResolverQueue = new lwPriorityQueue<MethodResolverDelegate>();

		// add default resolvers
		m_variableResolverQueue.Add(int.MinValue, TryGetMetaArgumentValue);
		m_variableResolverQueue.Add(int.MaxValue, FormulaParser.InternalSymbols.variables.TryGetValue);
		m_methodResolverQueue.Add(int.MaxValue, FormulaParser.InternalSymbols.TryMethodEvaluation);
	}

	public double Evaluate(FormulaParser.FormulaBlock a_formula)
	{
		lwTools.Assert(a_formula != null);

		return EvaluateAbstract(a_formula.m_child);
	}

	public void AddResolverDelegate(int a_priority, VariableResolverDelegate a_resolverDelegate)
	{
		lwTools.Assert(a_priority != int.MinValue);
		lwTools.Assert(a_priority != int.MaxValue);
		lwTools.Assert(a_resolverDelegate != null);

		m_variableResolverQueue.Add(a_priority, a_resolverDelegate);
	}

	public void AddResolverDelegate(int a_priority, MethodResolverDelegate a_resolverDelegate)
	{
		lwTools.Assert(a_priority != int.MinValue);
		lwTools.Assert(a_priority != int.MaxValue);
		lwTools.Assert(a_resolverDelegate != null);

		m_methodResolverQueue.Add(a_priority, a_resolverDelegate);
	}

	private double Evaluate(FormulaParser.FormulaVariable a_variable)
	{
		switch(a_variable.m_type)
		{
			case FormulaParser.FormulaVariable.Type.ConstantInteger: return (double)int.Parse(a_variable.m_variable);
			case FormulaParser.FormulaVariable.Type.ConstantDouble: return double.Parse(a_variable.m_variable);
			case FormulaParser.FormulaVariable.Type.Variable:
			{
				double result = 0.0;
				bool hasSymbolBeenFilled = false;
				lwPriorityQueue<VariableResolverDelegate>.Enumerator resolverDelegateEnumerator = m_variableResolverQueue.GetEnumerator();
				while(resolverDelegateEnumerator.MoveNext()  &&  hasSymbolBeenFilled == false)
				{
					hasSymbolBeenFilled = resolverDelegateEnumerator.Current(a_variable.m_variable, out result);
				}

				lwTools.AssertFormat(hasSymbolBeenFilled, "Symbol for variable '{0}' not resolved during formula evaluation.", a_variable.m_variable);
				return result;
			}
			default: lwTools.AssertFormat(false, "Invalid Formula variable type '{0}'.", a_variable.m_type); return 0.0f;
		}
	}

	private double Evaluate(FormulaParser.FormulaOperator a_operator)
	{
		double[] childrenResults = new double[a_operator.childCount];
		for(int childIndex = 0; childIndex < a_operator.childCount; ++childIndex)
		{
			childrenResults[childIndex] = EvaluateAbstract(a_operator[childIndex]);
		}
		
		switch(a_operator.m_operator)
		{
			case FormulaParser.Operator.Add:
			{
				lwTools.Assert(a_operator.childCount == 2);
				return childrenResults[0] + childrenResults[1];
			}
			case FormulaParser.Operator.Substract:
			{
				if(a_operator.childCount == 1)
				{
					return -childrenResults[0];
				}
				else
				{
					lwTools.Assert(a_operator.childCount == 2);
					return childrenResults[0] - childrenResults[1];
				}
			}
			case FormulaParser.Operator.Modulo:
			{
				lwTools.Assert(a_operator.childCount == 2);
				return childrenResults[0] % childrenResults[1];
			}
			case FormulaParser.Operator.Multiply:
			{
				lwTools.Assert(a_operator.childCount == 2);
				return childrenResults[0] * childrenResults[1];
			}
			case FormulaParser.Operator.Divide:
			{
				lwTools.Assert(a_operator.childCount == 2);
				return childrenResults[0] / childrenResults[1];
			}
			default: lwTools.AssertFormat(false, "Invalid formula operator '{0}'.", a_operator.m_operator); return 0.0f;
		}
	}

	private double Evaluate(FormulaParser.FormulaMethod a_method)
	{
		if(a_method.metaArgumentCount == 0)
		{
			double[] argumentResults = new double[a_method.argumentCount];
			for(int childIndex = 0; childIndex < a_method.argumentCount; ++childIndex)
			{
				argumentResults[childIndex] = EvaluateAbstract(a_method[childIndex]);
			}

			double result = 0.0;
			bool hasSymbolBeenFilled = false;
			lwPriorityQueue<MethodResolverDelegate>.Enumerator resolverDelegateEnumerator = m_methodResolverQueue.GetEnumerator();
			while(resolverDelegateEnumerator.MoveNext()  &&  hasSymbolBeenFilled == false)
			{
				hasSymbolBeenFilled = resolverDelegateEnumerator.Current(a_method.methodName, argumentResults, out result);
			}

			lwTools.AssertFormat(hasSymbolBeenFilled, "Symbol for method '{0}' not resolved during formula evaluation.", a_method.methodName);
			return result;
		}
		else
		{
			double result;

			PushMetaFunction(a_method);
			
			// Meta functions
			switch(a_method.methodName)
			{
				case "SUM":
				{
					lwTools.Assert(a_method.argumentCount >= 3  &&  a_method.argumentCount <= 4);
					double metaArgumentBegin = EvaluateAbstract(a_method[0]);
					double metaArgumentEnd = EvaluateAbstract(a_method[1]);

					double increment = a_method.argumentCount == 3? 1.0 : EvaluateAbstract(a_method[2]);
					lwTools.AssertFormat(increment != 0.0, "Invalid increment for method 'SUM' : {0}.", increment);

					result = 0.0;

					SetMetaFunctionArgument(0, metaArgumentBegin);
					while((increment > 0  &&  GetMetaFunctionArgument(0) <= metaArgumentEnd)  ||  (increment < 0  &&  GetMetaFunctionArgument(0) >= metaArgumentEnd))
					{
						result += EvaluateAbstract(a_method[a_method.argumentCount - 1]);
						
						SetMetaFunctionArgument(0, GetMetaFunctionArgument(0) + increment);
					}
				}
				break;
				default: lwTools.AssertFormat(false, "Invalid formula meta function '{0}'.", a_method.methodName); result = 0.0; break;
			}

			PopMetaFunction(a_method);

			return result;
		}
	}

	private double EvaluateAbstract(FormulaParser.FormulaElement a_element)
	{
		if(a_element is FormulaParser.FormulaBlock)
		{
			return Evaluate((FormulaParser.FormulaBlock)a_element);
		}
		else if(a_element is FormulaParser.FormulaVariable)
		{
			return Evaluate((FormulaParser.FormulaVariable)a_element);
		}
		else if(a_element is FormulaParser.FormulaOperator)
		{
			return Evaluate((FormulaParser.FormulaOperator)a_element);
		}
		else if(a_element is FormulaParser.FormulaMethod)
		{
			return Evaluate((FormulaParser.FormulaMethod)a_element);
		}
		else
		{
			lwTools.AssertFormat(false, "Invalid formula element '{0}'.", a_element.GetType().Name);
			return 0.0f;
		}
	}

	private bool TryGetMetaArgumentValue(string a_metaArgumentSymbol, out double value)
	{
		int metaArgumentIndex = System.Array.IndexOf(FormulaParser.InternalSymbols.metaFunctionVariableNames, a_metaArgumentSymbol);
		if(metaArgumentIndex >= 0)
		{
			lwTools.Assert(metaArgumentIndex < m_metaArgumentValues.Count);
			value = m_metaArgumentValues[metaArgumentIndex];
			return true;
		}
		else
		{
			value = 0.0;
			return false;
		}
	}

	private void PushMetaFunction(FormulaParser.FormulaMethod a_method)
	{
		lwTools.Assert(a_method.metaArgumentCount > 0);
		int previousMetaFunctionArgumentIndex = m_metaFunctionStack.Count == 0? 0 : m_metaFunctionStack.Peek().metaArgumentCount;
			
		m_metaFunctionStack.Push(a_method);
		m_metaFunctionFirstArgumentIndex.Push(m_metaFunctionFirstArgumentIndex.Count == 0? 0 : m_metaFunctionFirstArgumentIndex.Peek() + previousMetaFunctionArgumentIndex);
		for(int metaArgumentIndex = 0; metaArgumentIndex < a_method.metaArgumentCount; ++metaArgumentIndex)
		{
			m_metaArgumentValues.Add(0.0);
		}
	}

	private void PopMetaFunction(FormulaParser.FormulaMethod a_method)
	{
		lwTools.Assert(m_metaFunctionStack.Peek() == a_method);
		lwTools.Assert(m_metaArgumentValues.Count >= a_method.metaArgumentCount);
		m_metaArgumentValues.RemoveRange(m_metaArgumentValues.Count - a_method.metaArgumentCount, a_method.metaArgumentCount);
		m_metaFunctionFirstArgumentIndex.Pop();
		m_metaFunctionStack.Pop();
	}

	private void SetMetaFunctionArgument(int a_argumentIndex, double a_value)
	{
		int firstMetaArgumentIndex = m_metaFunctionFirstArgumentIndex.Peek();
		m_metaArgumentValues[firstMetaArgumentIndex + a_argumentIndex] = a_value;
	}

	private double GetMetaFunctionArgument(int a_argumentIndex)
	{
		int firstMetaArgumentIndex = m_metaFunctionFirstArgumentIndex.Peek();
		return m_metaArgumentValues[firstMetaArgumentIndex + a_argumentIndex];
	}

	private Stack<FormulaParser.FormulaMethod> m_metaFunctionStack;
	private Stack<int> m_metaFunctionFirstArgumentIndex;
	private List<double> m_metaArgumentValues;

	private lwPriorityQueue<VariableResolverDelegate> m_variableResolverQueue;
	private lwPriorityQueue<MethodResolverDelegate> m_methodResolverQueue;
}
