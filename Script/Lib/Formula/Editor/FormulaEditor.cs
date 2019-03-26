// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain Minjard <s.minjard@bigpoint.net>
//
// Date: 2017/03/09

using UnityEngine;
using UnityEditor;

//!	@class	EquationEditor
//!
//!	@brief	Property drawer for structure Equation
[CustomPropertyDrawer(typeof(Formula))]
public class FormulaEditor : PropertyDrawer
{
	public override void OnGUI(Rect a_position, SerializedProperty a_property, GUIContent a_label)
	{
		EditorGUI.BeginProperty(a_position, a_label, a_property);

		Rect controlRect = EditorGUI.PrefixLabel(a_position, a_label);
		SerializedProperty textProperty = a_property.FindPropertyRelative("m_formulaString");
		GUI.Label(controlRect, "= " + textProperty.stringValue);

		switch(Event.current.type)
		{
			case EventType.MouseDown:
			{
				if(Event.current.button == 0  &&  controlRect.Contains(Event.current.mousePosition))
				{
					FormulaSymbolAttribute[] contextAttributes = (FormulaSymbolAttribute[])fieldInfo.GetCustomAttributes(typeof(FormulaSymbolAttribute), false);
					FormulaEditorWindow.Show(a_property, contextAttributes);
				}
			}
			break;
		}

		EditorGUI.EndProperty();
	}
}
