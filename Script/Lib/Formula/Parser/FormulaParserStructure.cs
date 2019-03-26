// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain Minjard <s.minjard@bigpoint.net>
//
// Date: 2017/03/09

using System.Collections.Generic;

namespace FormulaParser
{
	//!	@class	SymbolHandler
	//!
	//!	@brief	Contain all the symbols an equation may need
	public class SymbolHandler
	{
		//!	@class	Method
		//!
		//!	@brief	Definition of a method
		public class Method
		{
			public readonly string name;
			public readonly int minArgumentCount;
			public readonly int maxArgumentCount;

			public readonly string sourceString;

			public readonly int metaArgumentCount;

			public static Method Parse(string a_source)
			{
				int openBracketIndex = 0;
				while(openBracketIndex < a_source.Length  &&  a_source[openBracketIndex] != '(')
				{
					++openBracketIndex;
				}
				lwTools.Assert(openBracketIndex < a_source.Length);

				string methodName = a_source.Substring(0, openBracketIndex);
				lwTools.Assert(FormulaParser.Methods.IsValidSymbolName(methodName));

				int closeBracketIndex = a_source.Length - 1;
				lwTools.Assert(a_source[closeBracketIndex] == ')');

				int minArgCount;
				int maxArgCount;
				if(openBracketIndex < closeBracketIndex - 1)
				{
					string argumentsText = a_source.Substring(openBracketIndex + 1, closeBracketIndex - 1 - openBracketIndex);
					string[] splitArguments = argumentsText.Split('-');
					if(splitArguments.Length == 1)
					{
						if(splitArguments[0] == "INF")
						{
							minArgCount = 0;
							maxArgCount = int.MaxValue;
						}
						else
						{
							minArgCount = int.Parse(splitArguments[0]);
							maxArgCount = minArgCount;
						}
					}
					else
					{
						lwTools.Assert(splitArguments.Length == 2);
						minArgCount = int.Parse(splitArguments[0]);
						if(splitArguments[1] == "INF")
						{
							maxArgCount = int.MaxValue;
						}
						else
						{
							maxArgCount = int.Parse(splitArguments[1]);
						}
					}
				}
				else
				{
					minArgCount = 0;
					maxArgCount = 0;
				}
				lwTools.Assert(minArgCount >= 0);
				lwTools.Assert(minArgCount <= maxArgCount);

				return new Method(methodName, minArgCount, maxArgCount, a_source, 0);
			}

			internal Method(string a_methodName, int a_minArgumentCount, int a_maxArgumentCount, string a_sourceString, int a_metaArgumentCount)
			{
				name = a_methodName;
				minArgumentCount = a_minArgumentCount;
				maxArgumentCount = a_maxArgumentCount;
				sourceString = a_sourceString;
				metaArgumentCount = a_metaArgumentCount;
			}
		}

		public int symbolCount
		{
			get{ return m_variableNames.Count + m_methods.Count; }
		}

		public string[] variableNames
		{
			get{ return m_variableNames.ToArray(); }
		}

		public string[] methodNames
		{
			get
			{
				string[] result = new string[m_methods.Count];
				m_methods.Keys.CopyTo(result, 0);
				return result;
			}
		}

		public Method GetMethod(string a_methodName)
		{
			lwTools.Assert(m_methods.ContainsKey(a_methodName));
			return m_methods[a_methodName];
		}

		public void AddVariables(params string[] a_variableNames)
		{
			m_variableNames.AddRange(a_variableNames);
		}

		public void AddMethods(params Method[] a_methodNames)
		{
			for(int methodIndex = 0; methodIndex < a_methodNames.Length; ++methodIndex)
			{
				m_methods.Add(a_methodNames[methodIndex].name, a_methodNames[methodIndex]);
			}
		}

		private readonly List<string> m_variableNames = new List<string>();
		private readonly Dictionary<string, Method> m_methods = new Dictionary<string, Method>();
	}
	
	//!	@enum	Operator
	//!
	//!	@brief	Enumeration of all operators
	public enum Operator
	{
		// They need to be sorted by prevalence in ascending order
		Add,
		Substract,
		Modulo,
		Multiply,
		Divide,
	}

	//! @class	FormulaElement
	//!
	//!	@brief	Element of a formula
	public abstract class FormulaElement
	{
		public FormulaElement m_parent;
	}

	//!	@class	ForumlaBlock
	//!
	//!	@brief	Represent a formula (or a sub-formula due to brakets)
	public sealed class FormulaBlock : FormulaElement
	{
		public FormulaElement m_child;
	}

	//!	@class	FormulaOperator
	//!
	//!	@brief	Operator of a formula
	public sealed class FormulaOperator : FormulaElement
	{
		public Operator m_operator;

		public int childCount
		{
			get{ return m_children.Count; }
		}

		public FormulaElement this[int a_index]
		{
			get{ return m_children[a_index]; }
		}

		internal void AddChild(FormulaElement a_equationElement)
		{
			lwTools.Assert(a_equationElement.m_parent == null);
			a_equationElement.m_parent = this;
			m_children.Add(a_equationElement);
		}

		private List<FormulaElement> m_children = new List<FormulaElement>();
	}

	//!	@class	FormulaVariable
	//!
	//!	@brief	Variable of a formula
	public sealed class FormulaVariable : FormulaElement
	{
		public enum Type
		{
			None,
			ConstantInteger,
			ConstantDouble,
			Variable
		}

		public Type m_type;
		public string m_variable;
	}

	//! @class	FormulaMethod
	//!
	//!	@brief	Method used in a formula
	public sealed class FormulaMethod : FormulaElement
	{
		public readonly string methodName;
		public readonly int metaArgumentCount;

		public FormulaMethod(string a_name, int a_metaArgumentCount)
		{
			methodName = a_name;
			metaArgumentCount = a_metaArgumentCount;
		}

		public int argumentCount
		{
			get{ return m_arguments.Count; }
		}

		public FormulaBlock this[int a_index]
		{
			get{ return m_arguments[a_index]; }
		}

		internal void AddArgument(FormulaBlock a_equationElement)
		{
			lwTools.Assert(a_equationElement.m_parent == null);
			a_equationElement.m_parent = this;
			m_arguments.Add(a_equationElement);
		}

		private List<FormulaBlock> m_arguments = new List<FormulaBlock>();
	}
}
