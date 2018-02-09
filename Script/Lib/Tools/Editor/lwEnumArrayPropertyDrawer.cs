// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain MINJARD <sminjard@bigpoint.net>
//
// Date: 2016/05/27

using UnityEngine;
using UnityEditor;

//! @class lwEnumArrayPropertyDrawer
//!
//! @brief Property drawer for a lwEnumArray class
[CustomPropertyDrawer( typeof( lwEnumArrayBaseClass ), true )]
public class lwEnumArrayPropertyDrawer : PropertyDrawer
{
	public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
	{
		EditorGUI.BeginProperty( position, label, property );

		property.isExpanded = EditorGUI.Foldout( new Rect( position.x, position.y, position.width, 16.0f ), property.isExpanded, label );
		if( property.isExpanded )
		{
			++EditorGUI.indentLevel;

			SerializedProperty valuesArrayProperty = property.FindPropertyRelative( "m_internalArray" );
			SerializedProperty namesArrayProperty = property.FindPropertyRelative( "m_enumNames" );

			float fTop = position.y+16.0f;
			for( int nIndex = 0; nIndex<namesArrayProperty.arraySize; ++nIndex )
			{
				GUIContent elementLabel = new GUIContent( namesArrayProperty.GetArrayElementAtIndex( nIndex ).stringValue );
				SerializedProperty elementProperty = valuesArrayProperty.GetArrayElementAtIndex( nIndex );

				float fPropertyHeight = EditorGUI.GetPropertyHeight( elementProperty, elementLabel );
				Rect elementRect = new Rect( position.x, fTop, position.width, fPropertyHeight ); 
				EditorGUI.PropertyField( elementRect, elementProperty, elementLabel, true );

				fTop += fPropertyHeight;
			}

			--EditorGUI.indentLevel;
		}

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
	{
		float fHeight = 16.0f;
		if( property.isExpanded )
		{
			SerializedProperty valuesArrayProperty = property.FindPropertyRelative( "m_internalArray" );
			SerializedProperty namesArrayProperty = property.FindPropertyRelative( "m_enumNames" );

			for( int nIndex = 0; nIndex<namesArrayProperty.arraySize; ++nIndex )
			{
				fHeight += EditorGUI.GetPropertyHeight( valuesArrayProperty.GetArrayElementAtIndex( nIndex ), new GUIContent( namesArrayProperty.GetArrayElementAtIndex( nIndex ).stringValue ) );
			}
		}

		return fHeight;
	}
}
