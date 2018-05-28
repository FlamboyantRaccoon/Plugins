// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain MINJARD <sminjard@bigpoint.net>
//
// Date: 2016/07/21

using UnityEngine;
using UnityEditor;

//! @class LwGUILayout
//!
//! @brief	static class that holds custom controls using GUI Layouts
public static class lwEditorGUILayout
{	
	//! Create a size handle that allow you to resize an element on horizontal or vertical axis
	//!
	//!	@param	fValue			current value of the control (the width or the height) that need to be resized
	//!	@param	bIsHorizontal	whether the control is an horizontal resizer or a vertical one
	//!	@param	fHandleSize		size of the handle for the width or the height depending on bIsHorizontal
	//!
	//!	@return the new value of the control that need to be resized
	public static float SizeHandle( float fValue, bool bIsHorizontal, float fHandleSize = 10.0f )
	{		
		GUILayoutOption sizeOption = bIsHorizontal ? GUILayout.Width( fHandleSize ) : GUILayout.Height( fHandleSize );
		GUILayoutOption expandOption = bIsHorizontal ? GUILayout.ExpandHeight( true ) : GUILayout.ExpandWidth( true );
		Rect resizingRect = GUILayoutUtility.GetRect( GUIContent.none, GUI.skin.box, sizeOption, expandOption );
		return lwEditorGUI.SizeHandle( resizingRect, fValue, bIsHorizontal );
	}

	//! Create a control that allow you to choose a folder and return its path
	//!
	//!	@param	sLabel				label of the control
	//!	@param	sDirectoryPath		value of the control
	//!	@param	folderType			location type of the folder
	//!	@param	sBrowseWindowTitle	title of the folder selection window
	//!	@param	sBrowseDefaultName	default name of the folder in the folder selection window
	//!
	//!	@return	the path to the directory selected
	public static string FolderSelection( string sLabel, string sDirectoryPath, lwPathUtil.PathType folderType = lwPathUtil.PathType.Any, string sBrowseWindowTitle = "Choose a folder", string sBrowseDefaultName = "Folder" )
	{
		EditorGUILayout.BeginHorizontal( GUILayout.ExpandWidth( true ) );
		Vector2 labelSize = GUI.skin.label.CalcSize( new GUIContent( sDirectoryPath ) );
		Vector2 buttonSize = GUI.skin.button.CalcSize( lwEditorGUI.FOLDER_SELECTION_BUTTON_LABEL );
		float fControlHeight = Mathf.Max( buttonSize.y, labelSize.y );
		Rect controlRect = GUILayoutUtility.GetRect( EditorGUIUtility.labelWidth + buttonSize.x + labelSize.x, EditorGUIUtility.currentViewWidth, fControlHeight, fControlHeight, GUILayout.ExpandWidth (true ), GUILayout.ExpandHeight( false ) );
		EditorGUILayout.EndHorizontal();

		return lwEditorGUI.FolderSelection( controlRect, sLabel, sDirectoryPath, folderType, sBrowseWindowTitle, sBrowseDefaultName );
	}

	//! Create a control that allow you to choose a file and return its path
	//!
	//!	@param	sLabel				label of the control
	//!	@param	sFilePath			value of the control
	//!	@param	sFilters				array of filters to populate OpenFilePanelWithFilters method
	//!	@param	fileType			location type of the file
	//!	@param	sBrowseWindowTitle	title of the folder selection window
	//!
	//!	@return	the path to the file selected
	public static string FileSelection( string sLabel, string sFilePath, string[] sFilters, lwPathUtil.PathType fileType = lwPathUtil.PathType.Any, string sBrowseWindowTitle = "Choose a file" )
	{
		EditorGUILayout.BeginHorizontal( GUILayout.ExpandWidth( true ) );
		Vector2 labelSize = GUI.skin.label.CalcSize( new GUIContent( sFilePath ) );
		Vector2 buttonSize = GUI.skin.button.CalcSize( lwEditorGUI.FILE_SELECTION_BUTTON_LABEL );
		float fControlHeight = Mathf.Max( buttonSize.y, labelSize.y );
		Rect controlRect = GUILayoutUtility.GetRect( EditorGUIUtility.labelWidth + buttonSize.x + labelSize.x, EditorGUIUtility.currentViewWidth, fControlHeight, fControlHeight, GUILayout.ExpandWidth (true ), GUILayout.ExpandHeight( false ) );
		EditorGUILayout.EndHorizontal();

		return lwEditorGUI.FileSelection( controlRect, sLabel, sFilePath, sFilters, fileType, sBrowseWindowTitle );
	}

	public static lwEnumFlag<t_Enum> EnumFlag<t_Enum>( string sLabel, lwEnumFlag<t_Enum> value, params GUILayoutOption[] layoutOptions ) where t_Enum : struct, System.IConvertible
	{
		Rect position = GUILayoutUtility.GetRect( new GUIContent( sLabel ), "MiniPopup", layoutOptions );
		return lwEditorGUI.EnumFlag<t_Enum>( position, sLabel, value );
	}
}
