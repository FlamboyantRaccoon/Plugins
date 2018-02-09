using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(lwMinMaxVectorAttribute))]
public class lwMinMaxVectorAttributeEditor : PropertyDrawer
{
	private const int TEXT_FIELD_WIDTH = 100;

	public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
	{
		EditorGUI.BeginProperty( position, label, property );
			
		lwMinMaxVectorAttribute thisAttribute = (lwMinMaxVectorAttribute)attribute;

		position = EditorGUI.PrefixLabel( position, label );
		
		if( property.propertyType==SerializedPropertyType.Vector2 )
		{
			float fMinValue = property.vector2Value.x;
			float fMaxValue = property.vector2Value.y;

			Rect rtSlider = new Rect( position.x, position.y, position.width - 2 * TEXT_FIELD_WIDTH, EditorGUIUtility.singleLineHeight );
			Rect rtMinField = new Rect( position.x + position.width - 2 * TEXT_FIELD_WIDTH, position.y, TEXT_FIELD_WIDTH, EditorGUIUtility.singleLineHeight );
			Rect rtMaxField = new Rect( position.x + position.width - 1 * TEXT_FIELD_WIDTH, position.y, TEXT_FIELD_WIDTH, EditorGUIUtility.singleLineHeight );

			EditorGUI.BeginChangeCheck();
			EditorGUI.MinMaxSlider( rtSlider, ref fMinValue, ref fMaxValue, thisAttribute.fMinValue, thisAttribute.fMaxValue );
			if( EditorGUI.EndChangeCheck() )
			{
				if( thisAttribute.bUseInteger )
				{
					fMinValue = Mathf.Round( fMinValue );
					fMaxValue = Mathf.Round( fMaxValue );
				}
				property.vector2Value = new Vector2( fMinValue, fMaxValue );
			}

			EditorGUI.BeginChangeCheck();
			fMinValue = EditorGUI.DelayedFloatField( rtMinField, fMinValue );
			if( EditorGUI.EndChangeCheck() )
			{
				if( thisAttribute.bUseInteger )
				{
					fMinValue = Mathf.Round( Mathf.Clamp( fMinValue, thisAttribute.fMinValue, fMaxValue ) );
				}
				property.vector2Value = new Vector2( fMinValue, fMaxValue );
			}

			EditorGUI.BeginChangeCheck();
			fMaxValue = EditorGUI.DelayedFloatField( rtMaxField, fMaxValue );
			if( EditorGUI.EndChangeCheck() )
			{
				if( thisAttribute.bUseInteger )
				{
					fMaxValue = Mathf.Round( Mathf.Clamp( fMaxValue, fMinValue, thisAttribute.fMaxValue ) );
				}
				property.vector2Value = new Vector2( fMinValue, fMaxValue );
			}
		}
		else
		{
			GUI.Label( position, "'lwMinMaxVectorAttribute' can only be used with Vector2." );
		}

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
	{
		return EditorGUIUtility.singleLineHeight;
	}
}
