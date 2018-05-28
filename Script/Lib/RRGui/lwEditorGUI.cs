// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain MINJARD <sminjard@bigpoint.net>
//
// Date: 2016/07/21

using UnityEngine;
using UnityEditor;

//! @class lwEditorGUI
//!
//! @brief	static class that holds custom controls
public static class lwEditorGUI
{	
	//! Create a size handle that allow you to resize an element on horizontal or vertical axis
	//!
	//!	@param	rect			position and size of the control
	//!	@param	fValue			current value of the control (the width or the height) that need to be resized
	//!	@param	bIsHorizontal	whether the control is an horizontal resizer or a vertical one
	//!
	//!	@return the new value of the control that need to be resized
	public static float SizeHandle( Rect rect, float fValue, bool bIsHorizontal )
	{
		int nControlID = GUIUtility.GetControlID( FocusType.Passive );

		MouseCursor mouseCursor = bIsHorizontal ? MouseCursor.ResizeHorizontal : MouseCursor.ResizeVertical;
		EditorGUIUtility.AddCursorRect( rect, mouseCursor );

		switch( Event.current.GetTypeForControl( nControlID ) )
		{
			case EventType.MouseUp:
				if( GUIUtility.hotControl==nControlID )
				{
					GUIUtility.hotControl = 0;
				}
				break;
			case EventType.MouseDown:
				if( rect.Contains( Event.current.mousePosition ) && Event.current.button==0 )
				{
					GUIUtility.hotControl = nControlID;
				}
				break;
		}

		if( Event.current.isMouse && GUIUtility.hotControl==nControlID && Event.current.type==EventType.MouseDrag )
		{
			fValue += bIsHorizontal ? Event.current.delta.x : Event.current.delta.y;
			Event.current.Use();
			GUI.changed = true;
		}

		return fValue;
	}

	//! Create a control that allow you to choose a folder and return its path
	//!
	//!	@param	rect				position and size of the control
	//!	@param	sLabel				label of the control
	//!	@param	sDirectoryPath		value of the control
	//!	@param	eFolderType			location type of the folder
	//!	@param	sBrowseWindowTitle	title of the folder selection window
	//!	@param	sBrowseDefaultName	default name of the folder in the folder selection window
	//!
	//!	@return	the path to the directory selected
	public static string FolderSelection( Rect rect, string sLabel, string sDirectoryPath, lwPathUtil.PathType eFolderType = lwPathUtil.PathType.Any, string sBrowseWindowTitle = "Choose a folder", string sBrowseDefaultName = "Folder" )
	{
		Vector2 v2ButtonSize = GUI.skin.button.CalcSize( FOLDER_SELECTION_BUTTON_LABEL );
		Rect labelRect = new Rect( rect.x, rect.y, rect.width - v2ButtonSize.x, rect.height );
		Rect buttonRect = new Rect( labelRect.x + labelRect.width, rect.y, v2ButtonSize.x, v2ButtonSize.y );
		EditorGUI.LabelField( labelRect, sLabel, sDirectoryPath );
		if( GUI.Button( buttonRect, FOLDER_SELECTION_BUTTON_LABEL ) )
		{
			string sAbsoluteSourceDirectoryPath;
			if( lwPathUtil.ToAbsolute( eFolderType, sDirectoryPath, out sAbsoluteSourceDirectoryPath )==false )
			{
				sAbsoluteSourceDirectoryPath = Application.dataPath;
			}
			
			string sSelectedAbsoluteDirectoryPath = EditorUtility.OpenFolderPanel( sBrowseWindowTitle, sAbsoluteSourceDirectoryPath, sBrowseDefaultName );
			// if the path is null, the user has cancelled, so we don't do anything
			if( sSelectedAbsoluteDirectoryPath!=null )
			{
				string sResultPath;
				if( lwPathUtil.ToRelative( eFolderType, sSelectedAbsoluteDirectoryPath, out sResultPath ) )
				{
					sDirectoryPath = sResultPath;
				}
			}
		}

		return sDirectoryPath;
	}

	//! Create a control that allow you to choose a file and return its path
	//!
	//!	@param	rect				position and size of the control
	//!	@param	sLabel				label of the control
	//!	@param	sFilePath			value of the control
	//!	@param	sFilters			array of filters to populate OpenFilePanelWithFilters method
	//!	@param	eFolderType			location type of the folder
	//!	@param	sBrowseWindowTitle	title of the folder selection window
	//!
	//!	@return	the path to the directory selected
	public static string FileSelection( Rect rect, string sLabel, string sFilePath, string[] sFilters, lwPathUtil.PathType eFileType = lwPathUtil.PathType.Any, string sBrowseWindowTitle = "Choose a file" )
	{
		lwTools.Assert( sFilters!=null && sFilters.Length>0 && ( sFilters.Length % 2 )==0 );
		
		Vector2 v2ButtonSize = GUI.skin.button.CalcSize( FOLDER_SELECTION_BUTTON_LABEL );
		Rect labelRect = new Rect( rect.x, rect.y, rect.width - v2ButtonSize.x, rect.height );
		Rect buttonRect = new Rect( labelRect.x + labelRect.width, rect.y, v2ButtonSize.x, v2ButtonSize.y );
		EditorGUI.LabelField( labelRect, sLabel, sFilePath );
		if( GUI.Button( buttonRect, FILE_SELECTION_BUTTON_LABEL ) )
		{
			string sAbsoluteSourceFilePath;
			string sAbsoluteSourceDirectoryPath;
			if( !string.IsNullOrEmpty( sFilePath ) && lwPathUtil.ToAbsolute( eFileType, sFilePath, out sAbsoluteSourceFilePath ) )
			{
				sAbsoluteSourceDirectoryPath = System.IO.Path.GetDirectoryName( sAbsoluteSourceFilePath );
			}
			else
			{
				sAbsoluteSourceDirectoryPath = Application.dataPath;
			}

			string sSelectedAbsoluteFilePath = EditorUtility.OpenFilePanelWithFilters( sBrowseWindowTitle, sAbsoluteSourceDirectoryPath, sFilters );
			// if the path is null, the user has cancelled, so we don't do anything
			if( sSelectedAbsoluteFilePath!=null )
			{
				string sResultPath;
				if( lwPathUtil.ToRelative( eFileType, sSelectedAbsoluteFilePath, out sResultPath ) )
				{
					sFilePath = sResultPath;
				}
			}
		}

		return sFilePath;
	}

	public static lwEnumFlag<t_Enum> EnumFlag<t_Enum>( Rect position, string sLabel, lwEnumFlag<t_Enum> value ) where t_Enum : struct, System.IConvertible
	{
		string[] sNamesArray = System.Enum.GetNames( typeof(t_Enum) );

		System.Text.StringBuilder sStringizedFlagValues = new System.Text.StringBuilder();
		int nFlagSetCounter = 0;
		for( int nIndex=0; nIndex<sNamesArray.Length; nIndex++ )
		{
			if( value[nIndex] )
			{
				if( nFlagSetCounter==0 )
				{
					sStringizedFlagValues.Append( '|' );
				}
				sStringizedFlagValues.Append( sNamesArray[nIndex] );
				nFlagSetCounter++;
			}
		}

		string sPopupLabel;
		if( nFlagSetCounter==0 )
		{
			sPopupLabel = "None";
		}
		else if( nFlagSetCounter==sNamesArray.Length )
		{
			sPopupLabel = "Everything";
		}
		else if( nFlagSetCounter>1 )
		{
			sPopupLabel = "Mixed...";
		}
		else
		{
			sPopupLabel = sStringizedFlagValues.ToString();
		}

		Rect controlRect = EditorGUI.PrefixLabel( position, new GUIContent( sLabel ) );
		if( GUI.Button( controlRect, sPopupLabel, "MiniPopup" ) )
		{
			GenericMenu contextualMenu = new GenericMenu();
			contextualMenu.AddItem( new GUIContent( "None" ), false, new GenericMenu.MenuFunction( () => value.SetAll( false ) ) );
			contextualMenu.AddItem( new GUIContent( "Everything" ), false, new GenericMenu.MenuFunction( () => value.SetAll( true ) ) );
			contextualMenu.AddSeparator( "" );

			for( int nIndex=0; nIndex<sNamesArray.Length; nIndex++ )
			{
				int nId = nIndex;
				contextualMenu.AddItem( new GUIContent( sNamesArray[nIndex] ), value[nIndex], new GenericMenu.MenuFunction( () => value[nId] = !value[nId] ) );
			}
			contextualMenu.ShowAsContext();
		}

		return value;
	}

#region Private
	#region Declarations
	internal static readonly GUIContent FILE_SELECTION_BUTTON_LABEL = new GUIContent( "Browse" );
	internal static readonly GUIContent FOLDER_SELECTION_BUTTON_LABEL = new GUIContent( "Browse" );
	#endregion
#endregion
}
