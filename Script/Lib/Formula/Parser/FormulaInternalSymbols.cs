// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain Minjard <s.minjard@bigpoint.net>
//
// Date: 2017/03/14

using UnityEngine;

using System.Collections.Generic;

namespace FormulaParser
{
	public static class InternalSymbols
	{
		// Variables
		public static readonly Dictionary<string, double> variables = new Dictionary<string, double>()
		{
			{ "PI", Mathf.PI },
			{ "PHI", (1 + System.Math.Sqrt(5)) / 2 }
		};

		public static string[] variableNames
		{
			get
			{
				if(ms_variableNames == null)
				{
					ms_variableNames = new string[variables.Count];
					variables.Keys.CopyTo(ms_variableNames, 0);
				}
				return ms_variableNames;
			}
		}

		// Meta Functions
		public static readonly SymbolHandler.Method[] metaFunctions = new SymbolHandler.Method[]
		{
			new SymbolHandler.Method("SUM", 3, 4, "SUM(3-4)", 1),
		};

		public static string[] metaFunctionNames
		{
			get
			{
				if(ms_metaFunctionNames == null)
				{
					ms_metaFunctionNames = System.Array.ConvertAll(metaFunctions, item => item.name);
				}
				return ms_metaFunctionNames;
			}
		}

		public static SymbolHandler.Method GetMetaFunction(string a_methodName)
		{
			if(ms_metaFunctions == null)
			{
				ms_metaFunctions = new Dictionary<string, SymbolHandler.Method>();
				for(int metaFunctionIndex = 0; metaFunctionIndex < metaFunctions.Length; ++metaFunctionIndex)
				{
					ms_metaFunctions.Add(metaFunctions[metaFunctionIndex].name, metaFunctions[metaFunctionIndex]);
				}
			}
			lwTools.Assert(ms_metaFunctions.ContainsKey(a_methodName));
			return ms_metaFunctions[a_methodName];
		}

		public static readonly string[] metaFunctionVariableNames = new string[]
		{
			"i", "j", "k", "a", "b", "c", "d", "e", "f", "g", "h", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"
		};

		// Methods
		public static readonly SymbolHandler.Method[] methods = new SymbolHandler.Method[]
		{
			SymbolHandler.Method.Parse("MIN(2-INF)"),
			SymbolHandler.Method.Parse("MAX(2-INF)"),
			SymbolHandler.Method.Parse("ABS(1)"),
			SymbolHandler.Method.Parse("COS(1)"),
			SymbolHandler.Method.Parse("SIN(1)"),
			SymbolHandler.Method.Parse("TAN(1)"),
			SymbolHandler.Method.Parse("POW(2)"),
			SymbolHandler.Method.Parse("SQRT(1)"),
			SymbolHandler.Method.Parse("INTDIV(2)"),
			SymbolHandler.Method.Parse("FLOOR(1)"),
			SymbolHandler.Method.Parse("CEIL(1)")
		};

		public static string[] methodNames
		{
			get
			{
				if(ms_methodNames == null)
				{
					ms_methodNames = System.Array.ConvertAll(methods, item => item.name);
				}
				return ms_methodNames;
			}
		}

		public static SymbolHandler.Method GetMethod(string a_methodName)
		{
			if(ms_methods == null)
			{
				ms_methods = new Dictionary<string, SymbolHandler.Method>();
				for(int methodIndex = 0; methodIndex < methods.Length; ++methodIndex)
				{
					ms_methods.Add(methods[methodIndex].name, methods[methodIndex]);
				}
			}
			lwTools.Assert(ms_methods.ContainsKey(a_methodName));
			return ms_methods[a_methodName];
		}

		public static bool TryMethodEvaluation(string a_methodName, double[] a_arguments, out double a_result)
		{
			SymbolHandler.Method method = GetMethod(a_methodName);
			lwTools.AssertFormat(a_arguments.Length >= method.minArgumentCount  &&  a_arguments.Length <= method.maxArgumentCount, "Invalid number of arguments for formula method '{0}'. {1} arguments given but the amount of argument must be in [{2}; {3}].", a_methodName, a_arguments.Length, method.minArgumentCount, method.maxArgumentCount);

			switch(a_methodName)
			{
				case "MIN":
				{
					a_result = lwMath.Min(a_arguments);
					return true;
				}
				case "MAX":
				{
					a_result = lwMath.Max(a_arguments);
					return true;
				}
				case "ABS":
				{
					a_result = lwMath.Abs(a_arguments[0]);
					return true;
				}
				case "COS":
				{
					a_result = lwMath.Cos(a_arguments[0]);
					return true;
				}
				case "SIN":
				{
					a_result = lwMath.Sin(a_arguments[0]);
					return true;
				}
				case "TAN":
				{
					a_result = lwMath.Tan(a_arguments[0]);
					return true;
				}
				case "POW":
				{
					a_result = lwMath.Pow(a_arguments[0], a_arguments[1]);
					return true;
				}
				case "SQRT":
				{
					a_result = lwMath.Sqrt(a_arguments[0]);
					return true;
				}
				case "INTDIV":
				{
					a_result = lwMath.IntegerDivision(a_arguments[0], a_arguments[1]);
					return true;
				}
				case "FLOOR":
				{
					a_result = lwMath.Floor(a_arguments[0]);
					return true;
				}
				case "CEIL":
				{
					a_result = lwMath.Ceil(a_arguments[0]);
					return true;
				}
				default:
				{
					lwTools.AssertFormat(false, "Formula internal method '{0}' is defined but the evaluation method is not set.", a_methodName);
					a_result = 0.0;
					return false;
				}
			}
		}

#region Private
		private static string[] ms_variableNames = null;
		private static string[] ms_metaFunctionNames = null;
		private static Dictionary<string, SymbolHandler.Method> ms_metaFunctions = null;
		private static string[] ms_methodNames = null;
		private static Dictionary<string, SymbolHandler.Method> ms_methods = null;
#endregion
	}
}