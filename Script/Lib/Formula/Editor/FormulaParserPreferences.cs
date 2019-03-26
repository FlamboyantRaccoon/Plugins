// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain Minjard <s.minjard@bigpoint.net>
//
// Date: 2017/03/14

using UnityEngine;
using UnityEditor;

public class FormulaParserPreferences
{
	[InitializeOnLoadMethod]
	private static void RegisterPreferences()
	{
		lwToolPreferences.RegisterPreferences("Formula", OnGUI);
	}

	public static Color constantColor
	{
		get
		{
			Load();
			return ms_constantColor;
		}
	}

	public static Color variableColor
	{
		get
		{
			Load();
			return ms_variableColor;
		}
	}

	public static Color methodColor
	{
		get
		{
			Load();
			return ms_methodColor;
		}
	}

	public static void Load()
	{
		if( m_bIsLoaded )
		{
			return;
		}

		ms_constantColor = LoadColor("constant", Color.yellow);
		ms_variableColor = LoadColor("variable", Color.cyan);
		ms_methodColor = LoadColor("method", new Color(1.0f, 0.5f, 0.0f));

		m_bIsLoaded = true;
	}

	public static void Save()
	{
		lwTools.Assert( m_bIsLoaded );

		SaveColor("constant", ms_constantColor);
		SaveColor("variable", ms_variableColor);
		SaveColor("method", ms_methodColor);
	}

	public static void OnGUI()
	{
		Load();

		EditorGUI.BeginChangeCheck();
		{
			ms_constantColor = EditorGUILayout.ColorField("Constant", ms_constantColor);
			ms_variableColor = EditorGUILayout.ColorField("Variable", ms_variableColor);
			ms_methodColor = EditorGUILayout.ColorField("Method", ms_methodColor);
		}
		if(EditorGUI.EndChangeCheck())
		{
			Save();
		}
	}

	private static void SaveColor(string a_key, Color a_color)
	{
		EditorPrefs.SetFloat("Mozaic/Formula/" + a_key + "-red", a_color.r);
		EditorPrefs.SetFloat("Mozaic/Formula/" + a_key + "-green", a_color.g);
		EditorPrefs.SetFloat("Mozaic/Formula/" + a_key + "-blue", a_color.b);
	}

	private static Color LoadColor(string a_key, Color a_defaultColor)
	{
		float red = EditorPrefs.GetFloat("Mozaic/Formula/" + a_key + "-red", a_defaultColor.r);
		float green = EditorPrefs.GetFloat("Mozaic/Formula/" + a_key + "-green", a_defaultColor.g);
		float blue = EditorPrefs.GetFloat("Mozaic/Formula/" + a_key + "-blue", a_defaultColor.b);
		return new Color(red, green, blue);
	}

	private static bool m_bIsLoaded = false;

	private static Color ms_constantColor;
	private static Color ms_variableColor;
	private static Color ms_methodColor;
}
