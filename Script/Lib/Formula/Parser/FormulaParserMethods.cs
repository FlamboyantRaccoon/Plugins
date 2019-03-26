// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain Minjard <s.minjard@bigpoint.net>
//
// Date: 2017/03/09

using System.Collections.Generic;

namespace FormulaParser
{
	public static class Methods
	{
		/// <summary>
		/// Parse a formula and returns the root element of the formula.
		/// </summary>
		/// <param name="a_formulaText">The formula in a text format</param>
		/// <param name="a_symbolHandler">A container of all symbols defined for that formula</param>
		public static FormulaBlock ParseFormula(string a_formulaText, SymbolHandler a_symbolHandler)
		{
			lwTools.Assert(a_formulaText != null);
			return TryParseFormula(a_formulaText, a_symbolHandler, 0);
		}

		//! Check if the string given can be a variable name
		//!
		//!	@param	a_variableName	string to check
		//!
		//!	@return true if the name can be a variable, flase otherwise
		public static bool IsValidSymbolName(string a_variableName)
		{
			if(string.IsNullOrEmpty(a_variableName))
			{
				return false;
			}

			if((a_variableName[0] >= 'a'  &&  a_variableName[0] <= 'z')  ||  (a_variableName[0] >= 'A'  &&  a_variableName[0] <= 'Z'))
			{
				int charIndex = 1;
				while(charIndex < a_variableName.Length  &&  IsValidCharacterForSymbol(a_variableName[charIndex]))
				{
					++charIndex;
				}

				return charIndex == a_variableName.Length;
			}
			else
			{
				return false;
			}
		}

		private static FormulaBlock TryParseFormula(string a_formulaText, SymbolHandler a_symbolHandler, int a_metaArgumentCount)
		{
			List<FormulaElement> formulaElements = new List<FormulaElement>();

			// parser
			int charIndex = 0;
			while(charIndex < a_formulaText.Length)
			{
				FormulaVariable formulaVariable;
				FormulaMethod formulaMethod;
				FormulaOperator formulaOperator;
				FormulaBlock formulaBlock;
				if(TryParseConstant(a_formulaText, ref charIndex, out formulaVariable))
				{
					formulaElements.Add(formulaVariable);
				}
				else if(TryParseOperator(a_formulaText, ref charIndex, out formulaOperator))
				{
					formulaElements.Add(formulaOperator);
				}
				else if(TryParseBlock(a_formulaText, ref charIndex, out formulaBlock, a_symbolHandler, a_metaArgumentCount))
				{
					formulaElements.Add(formulaBlock);
				}
				else if(TryParseVariable(a_formulaText, ref charIndex, out formulaVariable, a_symbolHandler, a_metaArgumentCount))
				{
					formulaElements.Add(formulaVariable);
				}
				else if(TryParseMethod(a_formulaText, ref charIndex, out formulaMethod, a_symbolHandler, a_metaArgumentCount))
				{
					formulaElements.Add(formulaMethod);
				}
				else
				{
					string symbolName;
					if(a_symbolHandler != null  &&  TryParseSymbol(a_formulaText, charIndex, out symbolName, null))
					{
						throw new System.ArgumentException(string.Format("Invalid formula string : unknown symbol '{0}' while parsing '{1}'.", symbolName, a_formulaText));
					}
					else
					{
						throw new System.ArgumentException(string.Format("Invalid formula string : parsing failed for '{0}' at index {1}.", a_formulaText, charIndex));
					}
				}
			}

			if(formulaElements.Count == 0)
			{
				return null;
			}
			else
			{
				FormulaBlock formula = new FormulaBlock();

				// create the tree
				ConvertToSubTree_UnaryOperators(ref formulaElements);
				ConvertToSubTree_BinaryOperators(ref formulaElements);

				lwTools.Assert(formulaElements.Count == 1);

				formula.m_child = formulaElements[0];

				return formula;
			}
		}

		private static bool TryParseConstant(string a_formulaText, ref int a_charIndex, out FormulaVariable a_formulaConstant)
		{
			FormulaVariable.Type currentParsedVariableType = FormulaVariable.Type.None;
			string currentParsedVariable = "";

			bool hasParsingEnded = a_charIndex >= a_formulaText.Length;
			while(hasParsingEnded == false)
			{
				switch(a_formulaText[a_charIndex])
				{
					// variable
					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':			
					{
						switch(currentParsedVariableType)
						{
							case FormulaVariable.Type.None:
							{
								currentParsedVariableType = FormulaVariable.Type.ConstantInteger;
								currentParsedVariable += a_formulaText[a_charIndex];
							}
							break;
							case FormulaVariable.Type.ConstantInteger:
							case FormulaVariable.Type.ConstantDouble:
							{
								currentParsedVariable += a_formulaText[a_charIndex];
							}
							break;
							default: lwTools.AssertFormat(false, "Invalid variable type '{0}'.", currentParsedVariableType); break;
						}
					}
					break;
					case '.':
					case ',':
					{
						switch(currentParsedVariableType)
						{
							case FormulaVariable.Type.None: throw new System.ArgumentException(string.Format("Invalid formula string : character '{0}' at index {1} is not related to any variable.", a_formulaText[a_charIndex], a_charIndex));
							case FormulaVariable.Type.ConstantInteger:
							{
								currentParsedVariableType = FormulaVariable.Type.ConstantDouble;
								currentParsedVariable += a_formulaText[a_charIndex];
							}
							break;
							case FormulaVariable.Type.ConstantDouble: throw new System.ArgumentException(string.Format("Invalid formula string : character '{0}' at index {1} is related to a float variable that has a decimal separator already set.", a_formulaText[a_charIndex], a_charIndex));
							default: lwTools.AssertFormat(false, "Invalid variable type '{0}'.", currentParsedVariableType); break;
						}
					}
					break;
					default: hasParsingEnded = true; break;
				}

				if(hasParsingEnded == false)
				{
					++a_charIndex;
					hasParsingEnded |= a_charIndex >= a_formulaText.Length;
				}
			}

			if(currentParsedVariableType == FormulaVariable.Type.None)
			{
				a_formulaConstant = null;
				return false;
			}
			else
			{
				a_formulaConstant = new FormulaVariable{ m_type = currentParsedVariableType, m_variable = currentParsedVariable };
				return true;
			}
		}

		private static bool TryParseOperator(string a_formulaText, ref int a_charIndex, out FormulaOperator a_formulaOperator)
		{
			if(a_charIndex >= a_formulaText.Length)
			{
				a_formulaOperator = null;
				return false;
			}

			a_formulaOperator = null;
			switch(a_formulaText[a_charIndex])
			{
				case '+': a_formulaOperator = new FormulaOperator{ m_operator = Operator.Add }; break;
				case '-': a_formulaOperator = new FormulaOperator{ m_operator = Operator.Substract }; break;
				case '%': a_formulaOperator = new FormulaOperator{ m_operator = Operator.Modulo }; break;
				case '*': a_formulaOperator = new FormulaOperator{ m_operator = Operator.Multiply }; break;
				case '/': a_formulaOperator = new FormulaOperator{ m_operator = Operator.Divide }; break;
			}

			if(a_formulaOperator == null)
			{
				return false;
			}
			else
			{
				++a_charIndex;
				return true;
			}
		}

		private static bool TryParseBlock(string a_formulaText, ref int a_charIndex, out FormulaBlock a_formulaBlock, SymbolHandler a_symbolHandler, int a_metaArgumentCount)
		{
			if(a_charIndex >= a_formulaText.Length  ||  a_formulaText[a_charIndex] != '(')
			{
				a_formulaBlock = null;
				return false;
			}

			int parenthesisDepth = 1;
			int searchForClosingBracketCharIndex = a_charIndex + 1;
			while(searchForClosingBracketCharIndex < a_formulaText.Length  &&  parenthesisDepth > 0)
			{
				switch(a_formulaText[searchForClosingBracketCharIndex])
				{
					case '(': ++parenthesisDepth; break;
					case ')': --parenthesisDepth; break;
					default: break;
				}

				if(parenthesisDepth > 0)
				{
					++searchForClosingBracketCharIndex;
				}
			}

			if(parenthesisDepth != 0)
			{
				throw new System.ArgumentException(string.Format("Invalid formula string : the opening bracket at index {0} has no closing bracket related.", a_charIndex));
			}
			else
			{
				string subformula = a_formulaText.Substring(a_charIndex + 1, searchForClosingBracketCharIndex - 1 - a_charIndex);
				a_formulaBlock = TryParseFormula(subformula, a_symbolHandler, a_metaArgumentCount);
				a_charIndex = searchForClosingBracketCharIndex + 1;
				return true;
			}
		}

		private static bool TryParseVariable(string a_formulaText, ref int a_charIndex, out FormulaVariable a_formulaVariable, SymbolHandler a_symbolHandler, int metaVariableCount)
		{
			string symbolName;
			if(TryParseSymbol(a_formulaText, a_charIndex, out symbolName, InternalSymbols.metaFunctionVariableNames)
			   ||  TryParseSymbol(a_formulaText, a_charIndex, out symbolName, a_symbolHandler == null? null : a_symbolHandler.variableNames)
			   ||  TryParseSymbol(a_formulaText, a_charIndex, out symbolName, InternalSymbols.variableNames))
			{
				int endOfSymbolIndex = a_charIndex + symbolName.Length;
				if(endOfSymbolIndex == a_formulaText.Length  ||  a_formulaText[endOfSymbolIndex] != '(')
				{
					lwTools.Assert(endOfSymbolIndex <= a_formulaText.Length);
					a_formulaVariable = new FormulaVariable{ m_type = FormulaVariable.Type.Variable, m_variable = symbolName };
					a_charIndex = endOfSymbolIndex;
					return true;
				}
			}

			a_formulaVariable = null;
			return false;
		}

		private static bool TryParseMethod(string a_formulaText, ref int a_charIndex, out FormulaMethod a_formulaMethod, SymbolHandler a_symbolHandler, int a_metaArgumentCount)
		{
			if(TryParseMethod(a_formulaText, ref a_charIndex, out a_formulaMethod, a_symbolHandler, InternalSymbols.metaFunctionNames, InternalSymbols.GetMetaFunction, a_metaArgumentCount))
			{
				return true;
			}
			else if(TryParseMethod(a_formulaText, ref a_charIndex, out a_formulaMethod, a_symbolHandler, a_symbolHandler == null? null : a_symbolHandler.methodNames, a_symbolHandler == null? null : new System.Func<string, SymbolHandler.Method>(a_symbolHandler.GetMethod), a_metaArgumentCount))
			{
				return true;
			}
			else if(TryParseMethod(a_formulaText, ref a_charIndex, out a_formulaMethod, a_symbolHandler, InternalSymbols.methodNames, InternalSymbols.GetMethod, a_metaArgumentCount))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		private static bool TryParseMethod(string a_formulaText, ref int a_charIndex, out FormulaMethod a_formulaMethod, SymbolHandler a_symbolHandler, string[] a_methodNames, System.Func<string, SymbolHandler.Method> a_methodGetter, int a_metaArgumentCount)
		{
			string symbolName;
			if(TryParseSymbol(a_formulaText, a_charIndex, out symbolName, a_methodNames))
			{
				int endOfSymbolIndex = a_charIndex + symbolName.Length;
				if(endOfSymbolIndex < a_formulaText.Length  &&  a_formulaText[endOfSymbolIndex] == '(')
				{
					List<string> arguments = new List<string>();

					int argumentFirstCharIndex = endOfSymbolIndex + 1;
					int parenthesisDepth = 1;
					int searchForClosingBracketCharIndex = endOfSymbolIndex + 1;
					while(searchForClosingBracketCharIndex < a_formulaText.Length  &&  parenthesisDepth > 0)
					{
						switch(a_formulaText[searchForClosingBracketCharIndex])
						{
							case '(': ++parenthesisDepth; break;
							case ')': --parenthesisDepth; break;
							case ',':
							{
								if(parenthesisDepth == 1)
								{
									arguments.Add(a_formulaText.Substring(argumentFirstCharIndex, searchForClosingBracketCharIndex - argumentFirstCharIndex));
									argumentFirstCharIndex = searchForClosingBracketCharIndex + 1;
								}
							}
							break;
							default: break;
						}

						if(parenthesisDepth > 0)
						{
							++searchForClosingBracketCharIndex;
						}
					}

					if(parenthesisDepth != 0)
					{
						throw new System.ArgumentException(string.Format("Invalid formula string : the opening bracket at index {0} has no closing bracket related.", a_charIndex));
					}
					else
					{
						if(searchForClosingBracketCharIndex > argumentFirstCharIndex)
						{
							arguments.Add(a_formulaText.Substring(argumentFirstCharIndex, searchForClosingBracketCharIndex - argumentFirstCharIndex));
						}

						bool isValidMethod = true;
						int metaArgumentCount = 0;
						if(a_methodGetter != null)
						{
							SymbolHandler.Method methodDef = a_methodGetter(symbolName);
							isValidMethod = arguments.Count >= methodDef.minArgumentCount  &&  arguments.Count <= methodDef.maxArgumentCount;
							metaArgumentCount = methodDef.metaArgumentCount;
						}

						if(isValidMethod)
						{
							a_formulaMethod = new FormulaMethod(symbolName, metaArgumentCount);
							
							for(int argumentIndex = 0; argumentIndex < arguments.Count; ++argumentIndex)
							{
								FormulaBlock argumentBlock = TryParseFormula(arguments[argumentIndex], a_symbolHandler, argumentIndex < arguments.Count - 1? a_metaArgumentCount : a_metaArgumentCount + metaArgumentCount);
								a_formulaMethod.AddArgument(argumentBlock);
							}
							a_charIndex = searchForClosingBracketCharIndex + 1;
							return true;
						}
					}
				}
			}

			a_formulaMethod = null;
			return false;
		}

		private static bool TryParseSymbol(string a_formulaText, int a_charIndex, out string a_symbolName, string[] a_symbols)
		{
			if(a_charIndex < a_formulaText.Length
			   &&  (a_formulaText[a_charIndex] >= 'a'  &&  a_formulaText[a_charIndex] <= 'z')  ||  (a_formulaText[a_charIndex] >= 'A'  &&  a_formulaText[a_charIndex] <= 'Z'))
			{
				List<string> symbolNames = a_symbols == null? null : new List<string>(a_symbols);

				bool hasLoopEnded = false;
				int charIterator = a_charIndex;
				while(charIterator < a_formulaText.Length  &&  (symbolNames == null  ||  symbolNames.Count > 0)  &&  hasLoopEnded == false)
				{
					char currentChar = a_formulaText[charIterator];
					bool isValidCharacter = IsValidCharacterForSymbol(currentChar);

					if(isValidCharacter)
					{
						int charIndexInVariable = charIterator - a_charIndex;

						if(symbolNames != null)
						{
							// remove all variable that do not match the character
							int remainingSymbolIndex = 0;
							while(remainingSymbolIndex < symbolNames.Count)
							{
								if(symbolNames[remainingSymbolIndex].Length > charIndexInVariable  &&  symbolNames[remainingSymbolIndex][charIndexInVariable] == currentChar)
								{
									++remainingSymbolIndex;
								}
								else
								{
									symbolNames.RemoveAt(remainingSymbolIndex);
								}
							}
						}

						++charIterator;
					}
					else
					{
						hasLoopEnded = true;
					}
				}

				int symbolLength = charIterator - a_charIndex;
				bool isSymbolValid = true;

				if(symbolNames != null)
				{
					int potentialVariableIndex = 0;
					while(potentialVariableIndex < symbolNames.Count  &&  symbolNames[potentialVariableIndex].Length != symbolLength)
					{
						++potentialVariableIndex;
					}
					isSymbolValid = potentialVariableIndex < symbolNames.Count;
				}

				if(isSymbolValid)
				{
					a_symbolName = a_formulaText.Substring(a_charIndex, charIterator - a_charIndex);
					return true;
				}
				else
				{
					a_symbolName = null;
					return false;
				}
			}
			else
			{
				a_symbolName = null;
				return false;
			}
		}

		private static void ConvertToSubTree_UnaryOperators(ref List<FormulaElement> a_formulaElements)
		{
			int elementIndex = 0;
			while(elementIndex < a_formulaElements.Count)
			{
				FormulaOperator formulaOperator = a_formulaElements[elementIndex] as FormulaOperator;
				if(formulaOperator != null  &&  formulaOperator.m_operator == Operator.Substract)
				{
					if(elementIndex == 0  ||  a_formulaElements[elementIndex - 1] is FormulaOperator)
					{
						if(elementIndex < a_formulaElements.Count - 1  &&  a_formulaElements[elementIndex + 1] is FormulaVariable)
						{
							formulaOperator.AddChild(a_formulaElements[elementIndex + 1]);
							a_formulaElements.RemoveAt(elementIndex + 1);
						}
						else
						{
							throw new System.ArgumentException("Invalid formula string : an unary operator '-' has an invalid element following it.");
						}
					}
				}

				++elementIndex;
			}
		}

		private static void ConvertToSubTree_BinaryOperators(ref List<FormulaElement> a_formulaElements)
		{			
			Operator[][] binaryOperators = new Operator[][]
			{
				new Operator[]{ FormulaParser.Operator.Modulo, FormulaParser.Operator.Multiply, FormulaParser.Operator.Divide },
				new Operator[]{ FormulaParser.Operator.Add, FormulaParser.Operator.Substract }
			};

			for(int priorityIndex = 0; priorityIndex < binaryOperators.Length; ++priorityIndex)
			{
				int elementIndex = 1;
				while(elementIndex < a_formulaElements.Count - 1)
				{
					FormulaOperator formulaOperator = a_formulaElements[elementIndex] as FormulaOperator;
					if(formulaOperator != null  &&  System.Array.IndexOf(binaryOperators[priorityIndex], formulaOperator.m_operator) >= 0  &&  formulaOperator.childCount == 0)
					{
						formulaOperator.AddChild(a_formulaElements[elementIndex - 1]);
						formulaOperator.AddChild(a_formulaElements[elementIndex + 1]);

						a_formulaElements.RemoveAt(elementIndex + 1);
						a_formulaElements.RemoveAt(elementIndex - 1);
					}
					else
					{
						++elementIndex;
					}
				}
			}
		}

		public static bool IsValidCharacterForSymbol(char a_char)
		{
			return a_char == '_'
				||  (a_char >= 'a'  &&  a_char <= 'z')
				||  (a_char >= 'A'  &&  a_char <= 'Z')
				||  (a_char >= '0'  &&  a_char <= '9');
		}
	}
}
