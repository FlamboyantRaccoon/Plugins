// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain Minjard <s.minjard@bigpoint.net>
//
// Date: 2017/03/09

using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

//!	@class	FormulaEditorWindow
//!
//!	@brief	Editor window to edit a formula
public class FormulaEditorWindow : EditorWindow
{
	public static void Show(SerializedProperty formulaProperty, FormulaSymbolAttribute[] contextAttributes)
	{
		FormulaEditorWindow window = EditorWindow.GetWindow<FormulaEditorWindow>(true, "Equation Editor", true);
		window.minSize = new Vector2(300.0f, 300.0f);

		window.Edit(formulaProperty, contextAttributes);
	}

#region Unity callbacks
	private void OnGUI()
	{
		bool hasEquationChanged = false;
		EditorGUILayout.BeginHorizontal();
		{
			GUILayout.Label(" = ", GUILayout.ExpandWidth(false));
			EditorGUI.BeginChangeCheck();
			m_formulaText = GUILayout.TextArea(m_formulaText, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
			hasEquationChanged = EditorGUI.EndChangeCheck();

			if(hasEquationChanged)
			{
				m_formulaText = m_formulaText.Replace(" ", "");
			}

			TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);

			if(GUILayout.Button(EditorGUIUtility.FindTexture("d_AlphabeticalSorting"), GUILayout.ExpandWidth(false)))
			{				
				GenericMenu contextualMenu = new GenericMenu();

				// Variables
				string[] internalVariableNames = FormulaParser.InternalSymbols.variableNames;
				for(int variableIndex = 0; variableIndex < internalVariableNames.Length; ++variableIndex)
				{
					contextualMenu.AddItem(new GUIContent("Variables/" + internalVariableNames[variableIndex]), false, new GenericMenu.MenuFunction(lwCurry.Bind(InsertVariable, editor.cursorIndex, internalVariableNames[variableIndex])));
				}

				contextualMenu.AddSeparator("Variables/");

				string[] variableNames = m_formulaSymbols.variableNames;
				for(int variableIndex = 0; variableIndex < variableNames.Length; ++variableIndex)
				{
					contextualMenu.AddItem(new GUIContent("Variables/" + variableNames[variableIndex]), false, new GenericMenu.MenuFunction(lwCurry.Bind(InsertVariable, editor.cursorIndex, variableNames[variableIndex])));
				}

				// Methods
				FormulaParser.SymbolHandler.Method[] internalMethods = FormulaParser.InternalSymbols.methods;
				for(int methodIndex = 0; methodIndex < internalMethods.Length; ++methodIndex)
				{
					contextualMenu.AddItem(new GUIContent("Methods/" + internalMethods[methodIndex].sourceString), false, new GenericMenu.MenuFunction(lwCurry.Bind(InsertMethod, editor.cursorIndex, internalMethods[methodIndex])));
				}

				contextualMenu.AddSeparator("Methods/");

				string[] methodNames = m_formulaSymbols.methodNames;
				for(int methodIndex = 0; methodIndex < methodNames.Length; ++methodIndex)
				{
					FormulaParser.SymbolHandler.Method method = m_formulaSymbols.GetMethod(methodNames[methodIndex]);
					contextualMenu.AddItem(new GUIContent("Methods/" + method.sourceString), false, new GenericMenu.MenuFunction(lwCurry.Bind(InsertMethod, editor.cursorIndex, method)));
				}

				contextualMenu.ShowAsContext();
			}
		}
		EditorGUILayout.EndHorizontal();

		try
		{
			if(m_shouldForceEquationParsing  ||  hasEquationChanged  ||  m_formulaTree == null)
			{
				m_formulaTree = FormulaParser.Methods.ParseFormula(m_formulaText, m_formulaSymbols);
				m_formulaError = "";
				m_shouldForceEquationParsing = false;
			}
		}
		catch(System.Exception a_exception)
		{
			m_formulaError = a_exception.Message;
		}

		if(string.IsNullOrEmpty(m_formulaError) == false)
		{
			EditorGUILayout.HelpBox(m_formulaError, MessageType.Error);
		}

		Rect equationWindowRect = GUILayoutUtility.GetRect(position.width, position.width, 0, position.height, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
		if(m_formulaTree != null)
		{
			m_formulaScrollPosition = FormulaParser.UI.OnGUI(equationWindowRect, m_formulaScrollPosition, m_formulaTree);
		}

		OnEvaluationPanelGUI();
	}

	private void OnLostFocus()
	{
		Close();
	}

	private void OnDisable()
	{
		if(m_property != null)
		{
			m_property.stringValue = m_formulaText;
			m_property.serializedObject.ApplyModifiedProperties();
		}
	}

	private void InsertVariable(int position, string a_variableName)
	{
		m_formulaText = m_formulaText.Insert(position, a_variableName);
		m_shouldForceEquationParsing = true;
	}

	private void InsertMethod(int position, FormulaParser.SymbolHandler.Method a_method)
	{
		System.Text.StringBuilder methodInsertionText = new System.Text.StringBuilder();
		methodInsertionText.Append(a_method.name);
		methodInsertionText.Append("(");
		for(int argumentIndex = 1; argumentIndex < a_method.minArgumentCount; ++argumentIndex)
		{
			methodInsertionText.Append(",");
		}
		methodInsertionText.Append(")");

		m_formulaText = m_formulaText.Insert(position, methodInsertionText.ToString());
		m_shouldForceEquationParsing = true;
	}
#endregion

#region Private
	#region Methods
	private void Edit(SerializedProperty formulaProperty, FormulaSymbolAttribute[] contextAttributes)
	{
		m_property = formulaProperty.FindPropertyRelative("m_formulaString");

		m_formulaSymbols = new FormulaParser.SymbolHandler();
		for(int contextAttributeIndex = 0; contextAttributeIndex < contextAttributes.Length; ++contextAttributeIndex)
		{
			m_formulaSymbols.AddVariables(contextAttributes[contextAttributeIndex].variableNames);
			m_formulaSymbols.AddMethods(contextAttributes[contextAttributeIndex].methodSignatures);
		}

		m_formulaText = m_property.stringValue;
		m_formulaText = m_formulaText.Replace(" ", "");
		m_formulaError = "";
		m_formulaScrollPosition = Vector2.zero;

		m_shouldForceEquationParsing = true;

		m_evaluationPanelScrollPosition = Vector2.zero;

		m_evaluationEntries = new List<double[]>();
		m_evaluationEntries.Add(new double[m_formulaSymbols.symbolCount]);
		m_evaluationResults = new List<double>();
		m_evaluationResults.Add(0.0);
	}

	private void OnEvaluationPanelGUI()
	{
		string[] variableNames = m_formulaSymbols.variableNames;
		string[] methodNames = m_formulaSymbols.methodNames;
		
		EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));
		{
			if(GUILayout.Button("Evaluate", GUILayout.ExpandWidth(false)))
			{
				Evaluate();
			}
			
			m_evaluationPanelScrollPosition = EditorGUILayout.BeginScrollView(m_evaluationPanelScrollPosition, false, false);
			{
				EditorGUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(false));
				{
					for(int symbolIndex = 0; symbolIndex < m_formulaSymbols.symbolCount; ++symbolIndex)
					{
						EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(false));
						{
							string symbolName = symbolIndex < variableNames.Length? variableNames[symbolIndex] : (methodNames[symbolIndex - variableNames.Length] + "()");
							GUILayout.Label(symbolName, GUILayout.MaxWidth(150.0f));

							for(int entryLineIndex = 0; entryLineIndex < m_evaluationEntries.Count; ++entryLineIndex)
							{
								m_evaluationEntries[entryLineIndex][symbolIndex] = (double)EditorGUILayout.FloatField((float)m_evaluationEntries[entryLineIndex][symbolIndex], GUILayout.MaxWidth(150.0f));
							}
						}
						EditorGUILayout.EndVertical();
					}

					EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(false));
					{
						GUILayout.Label("Results", GUILayout.MaxWidth(150.0f));
						for(int entryLineIndex = 0; entryLineIndex < m_evaluationResults.Count; ++entryLineIndex)
						{
							GUILayout.Label(m_evaluationResults[entryLineIndex].ToString(), GUILayout.MaxWidth(150.0f));
						}
					}
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndScrollView();

			if(GUILayout.Button("New entry", GUILayout.ExpandWidth(false)))
			{
				m_evaluationEntries.Add(new double[m_formulaSymbols.symbolCount]);
				m_evaluationResults.Add(0.0);
			}
		}
		EditorGUILayout.EndVertical();
	}

	private void Evaluate()
	{
		FormulaEvaluator evaluator = new FormulaEvaluator();
		evaluator.AddResolverDelegate(0, OnEvaluationVariable);
		evaluator.AddResolverDelegate(0, OnEvaluationMethod);

		for(m_evaluationEntryIterator = 0; m_evaluationEntryIterator < m_evaluationEntries.Count; ++m_evaluationEntryIterator)
		{
			m_evaluationResults[m_evaluationEntryIterator] = evaluator.Evaluate(m_formulaTree);
		}
	}

	private bool OnEvaluationVariable(string a_symbolName, out double a_value)
	{
		int symbolIndex = System.Array.IndexOf(m_formulaSymbols.variableNames, a_symbolName);
		if(symbolIndex >= 0)
		{
			a_value = m_evaluationEntries[m_evaluationEntryIterator][symbolIndex];
			return true;
		}
		else
		{
			a_value = 0.0;
			return false;
		}
	}

	private bool OnEvaluationMethod(string a_symbolName, double[] a_arguments, out double a_value)
	{
		int symbolIndex = System.Array.IndexOf(m_formulaSymbols.methodNames, a_symbolName) + m_formulaSymbols.variableNames.Length;
		if(symbolIndex >= m_formulaSymbols.variableNames.Length)
		{
			a_value = m_evaluationEntries[m_evaluationEntryIterator][symbolIndex];
			return true;
		}
		else
		{
			a_value = 0.0;
			return false;
		}
	}
	#endregion

	#region Attributes
	private string m_formulaText;
	private SerializedProperty m_property;
	private Vector2 m_formulaScrollPosition;
	private bool m_shouldForceEquationParsing;

	private FormulaParser.SymbolHandler m_formulaSymbols;
	private FormulaParser.FormulaBlock m_formulaTree;
	private string m_formulaError;

	private Vector2 m_evaluationPanelScrollPosition;

	private int m_evaluationEntryIterator;
	private List<double[]> m_evaluationEntries = new List<double[]>();
	private List<double> m_evaluationResults = new List<double>();
	#endregion
#endregion
}
