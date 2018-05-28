// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain MINJARD <sminjard@bigpoint.net>
//
// Date: 2016/05/27

using UnityEngine;
using UnityEditor;

//! @class lwEnumFlagPropertyDrawer
//!
//! @brief Property drawer for a lwEnumFlag class
[CustomPropertyDrawer( typeof( lwEnumFlagBaseClass ), true )]
public class lwEnumFlagPropertyDrawer : PropertyDrawer
{
	public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
	{
		EditorGUI.BeginProperty( position, label, property );

		SerializedProperty valuesArrayProperty = property.FindPropertyRelative( "m_nValues" );
		SerializedProperty namesArrayProperty = property.FindPropertyRelative( "m_enumNames" );

		System.Text.StringBuilder sStringizedFlagValues = new System.Text.StringBuilder();
		int nFlagSetCounter = 0;
		for( int nIndex = 0; nIndex<namesArrayProperty.arraySize; ++nIndex )
		{
			SerializedProperty nameProperty = namesArrayProperty.GetArrayElementAtIndex( nIndex );
			bool bIsChecked = lwEnumFlagEditorUtils.IsValueChecked( valuesArrayProperty, nIndex );
			if( bIsChecked )
			{
				if( nFlagSetCounter==0 )
				{
					sStringizedFlagValues.Append( '|' );
				}
				sStringizedFlagValues.Append( nameProperty.stringValue );
				++nFlagSetCounter;
			}
		}

		string sPopupLabel;
		if( nFlagSetCounter==0 )
		{
			sPopupLabel = "None";
		}
		else if( nFlagSetCounter==namesArrayProperty.arraySize )
		{
			sPopupLabel = "Everything";
		}
		else if( nFlagSetCounter > 1 )
		{
			sPopupLabel = "Mixed...";
		}
		else
		{
			sPopupLabel = sStringizedFlagValues.ToString();
		}

		Rect controlRect = EditorGUI.PrefixLabel( position, label );
		if( GUI.Button( controlRect, sPopupLabel, "MiniPopup" ) )
		{
			GenericMenu contextualMenu = new GenericMenu();
			contextualMenu.AddItem( new GUIContent( "None" ), false, new GenericMenu.MenuFunction( lwCurry.Bind(  lwEnumFlagEditorUtils.SetAsNone, valuesArrayProperty ) ) ); 
			contextualMenu.AddItem( new GUIContent( "Everything" ), false, new GenericMenu.MenuFunction( lwCurry.Bind(  lwEnumFlagEditorUtils.SetAsEverything, namesArrayProperty.arraySize, valuesArrayProperty ) ) );
			contextualMenu.AddSeparator( "" );

			for( int nIndex = 0; nIndex<namesArrayProperty.arraySize; ++nIndex )
			{
				SerializedProperty nameProperty = namesArrayProperty.GetArrayElementAtIndex( nIndex );
				bool bIsChecked = lwEnumFlagEditorUtils.IsValueChecked( valuesArrayProperty, nIndex );
				contextualMenu.AddItem( new GUIContent( nameProperty.stringValue ), bIsChecked, new GenericMenu.MenuFunction( lwCurry.Bind( lwEnumFlagEditorUtils.FlipFlag, valuesArrayProperty, nIndex ) ) );
			}
			contextualMenu.ShowAsContext();
		}

		EditorGUI.EndProperty();
	}
}

//! @class lwEnumFlagEditorUtils
//!
//!	@brief Utility class for editor methods
public static class lwEnumFlagEditorUtils
{
	//!	Set the property with the value given
	public static void Serialize<t_Enum>( ref SerializedProperty property, lwEnumFlag<t_Enum> value ) where t_Enum : struct, System.IConvertible
	{
		lwTools.Assert( property!=null );
		lwTools.Assert( value!=null );

		SerializedProperty internalArrayProperty = property.FindPropertyRelative( "m_nValues" );
		lwTools.Assert( internalArrayProperty!=null );
		if( internalArrayProperty.arraySize==0 )
		{
			internalArrayProperty.arraySize = lwEnumFlag<t_Enum>.ARRAY_LENGTH;

			SerializedProperty internalEnumNamesArray = property.FindPropertyRelative( "m_enumNames" );
			lwTools.Assert( internalEnumNamesArray.arraySize==0 ); // if the array of values have no element, this one should not have any too
			string[] enumNames = System.Enum.GetNames( typeof( t_Enum ) );
			internalEnumNamesArray.arraySize = enumNames.Length;
			for( int enumIndex = 0; enumIndex<enumNames.Length; ++enumIndex )
			{
				SerializedProperty enumNameProperty = internalEnumNamesArray.GetArrayElementAtIndex( enumIndex );
				enumNameProperty.stringValue = enumNames[enumIndex];
			}
		}
		lwTools.Assert( internalArrayProperty.arraySize==lwEnumFlag<t_Enum>.ARRAY_LENGTH );

		for( int nElementIndex = 0; nElementIndex<internalArrayProperty.arraySize; ++nElementIndex )
		{
			byte nValue = 0;

			int nFirstBitIndex = nElementIndex*sizeof( byte )*8;
			int nLastBitIndex = Mathf.Min( ( nElementIndex+1 )*sizeof( byte )*8, lwEnumFlag<t_Enum>.COUNT-1 );
			for( int nBitIndex = nFirstBitIndex; nBitIndex<=nLastBitIndex; ++nBitIndex )
			{
				if( value[nBitIndex] )
				{
					nValue |= ( byte )( 1 << ( nBitIndex-nFirstBitIndex ) );
				}
			}

			SerializedProperty elementProperty = internalArrayProperty.GetArrayElementAtIndex( nElementIndex );
			elementProperty.intValue = nValue;
		}
	}

	//! Get the value inside the property
	public static t_Result Deserialize<t_Result, t_Enum>( SerializedProperty property ) where t_Result : lwEnumFlag<t_Enum>, new() where t_Enum : struct, System.IConvertible
	{
		lwTools.Assert( property!=null );

		SerializedProperty internalArrayProperty = property.FindPropertyRelative( "m_nValues" );
		lwTools.Assert( internalArrayProperty!=null );
		lwTools.Assert( internalArrayProperty.arraySize==lwEnumFlag<t_Enum>.ARRAY_LENGTH );
		t_Result result = new t_Result();
		for( int nElementIndex = 0; nElementIndex<internalArrayProperty.arraySize; ++nElementIndex )
		{
			SerializedProperty elementProperty =internalArrayProperty.GetArrayElementAtIndex( nElementIndex );
			byte nValue = ( byte )elementProperty.intValue;

			int nFirstBitIndex = nElementIndex*sizeof( byte )*8;
			int nLastBitIndex = Mathf.Min( ( nElementIndex+1 )*sizeof( byte )*8, lwEnumFlag<t_Enum>.COUNT-1 );
			for( int nBitIndex = nFirstBitIndex; nBitIndex<=nLastBitIndex; ++nBitIndex )
			{
				result[nBitIndex] = ( nValue&( 1<<( nBitIndex-nFirstBitIndex ) ) ) != 0;
			}
		}
		return result;
	}

	public static bool IsValueChecked( SerializedProperty valuesArrayProperty, int nIndex )
	{
		int nArrayIndex = nIndex/( sizeof( byte ) * 8 );
		SerializedProperty elementProperty = valuesArrayProperty.GetArrayElementAtIndex( nArrayIndex );
		int nElementIndex = nIndex%( sizeof( byte ) * 8 );
		return ( elementProperty.intValue&( 1<<nElementIndex ) )!=0;
	}

	public static void SetAsNone( SerializedProperty valuesArrayProperty )
	{
		for( int nElementIndex = 0; nElementIndex<valuesArrayProperty.arraySize; ++nElementIndex )
		{
			SerializedProperty elementProperty = valuesArrayProperty.GetArrayElementAtIndex( nElementIndex );
			elementProperty.intValue = 0;
		}

		valuesArrayProperty.serializedObject.ApplyModifiedProperties();
	}

	public static void SetAsEverything( int nValueCount, SerializedProperty valuesArrayProperty )
	{
		for( int nElementIndex = 0; nElementIndex<valuesArrayProperty.arraySize; ++nElementIndex )
		{
			SerializedProperty elementProperty = valuesArrayProperty.GetArrayElementAtIndex( nElementIndex );

			byte value = byte.MaxValue;
			if( nElementIndex==valuesArrayProperty.arraySize-1 )
			{
				int nBitCount = nValueCount-( ( nValueCount/( sizeof( byte )*8 ) )*( sizeof( byte )*8 ) );
				value = ( byte )( ( 1<<nBitCount )-1 );
			}

			elementProperty.intValue = value;
		}

		valuesArrayProperty.serializedObject.ApplyModifiedProperties();
	}

	public static void FlipFlag( SerializedProperty valuesArrayProperty, int nIndex )
	{
		int nArrayIndex = nIndex/( sizeof( byte ) * 8 );
		SerializedProperty elementProperty = valuesArrayProperty.GetArrayElementAtIndex( nArrayIndex );
		int nElementIndex = nIndex%( sizeof( byte ) * 8 );

		if( ( elementProperty.intValue&( 1<<nElementIndex ) )!=0 )
		{
			elementProperty.intValue &= ~( 1<<nElementIndex );
		}
		else
		{
			elementProperty.intValue |= 1<<nElementIndex;
		}

		valuesArrayProperty.serializedObject.ApplyModifiedProperties();
	}
}