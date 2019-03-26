// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain Minjard <s.minjard@bigpoint.net>
//
// Date: 2017/03/10

[System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
public class FormulaSymbolAttribute : System.Attribute
{
	public enum Type
	{
		Variable,
		Method,
	}
	
	public readonly string[] variableNames;
	public readonly FormulaParser.SymbolHandler.Method[] methodSignatures;

	public FormulaSymbolAttribute(Type a_symbolType, params string[] a_symbolNames)
	{
		switch(a_symbolType)
		{
			case Type.Variable:
			{
				variableNames = a_symbolNames;
				methodSignatures = new FormulaParser.SymbolHandler.Method[0];

#if UNITY_EDITOR
				for(int variableIndex = 0; variableIndex < variableNames.Length; ++variableIndex)
				{
					lwTools.Assert(FormulaParser.Methods.IsValidSymbolName(variableNames[variableIndex]));
				}
#endif // UNITY_EDITOR
			}
			break;
			case Type.Method:
			{
				variableNames = new string[0];
				methodSignatures = System.Array.ConvertAll(a_symbolNames, item => FormulaParser.SymbolHandler.Method.Parse(item));
			}
			break;
			default: lwTools.AssertFormat(false, "Invalid symbol type '{0}'.", a_symbolType); break;
		}
	}
}
