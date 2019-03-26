// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain Minjard <s.minjard@bigpoint.net>
//
// Date: 2017/03/09

using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

namespace FormulaParser
{
	public static class UI
	{
		public static Vector2 OnGUI(Rect a_rect, Vector2 a_scrollPosition, FormulaBlock a_formulaBlock)
		{
			EquationElementRect equationRect = new EquationElementRect();
			FillStructure(a_formulaBlock, ref equationRect);

			GUIContent equalityContent = new GUIContent("=");
			Vector2 equalitySize = GUI.skin.label.CalcSize(equalityContent);

			Rect viewRect = new Rect(0, 0, equationRect.sizeWithChildren.x + equalitySize.x, Mathf.Max(equationRect.sizeWithChildren.y, equalitySize.y));
			a_scrollPosition = GUI.BeginScrollView(a_rect, a_scrollPosition, viewRect, false, false);
			{				
				float posX;
				if(viewRect.width >= a_rect.width)
				{
					posX = equalitySize.x + equationRect.sizeWithChildren.x * 0.5f;
				}
				else
				{
					posX = equalitySize.x + a_rect.width * 0.5f;
				}
				float posY = (a_rect.height - equationRect.sizeWithChildren.y + equationRect.pivot.y) * 0.5f;

				Rect equalityRect = new Rect(posX - equationRect.sizeWithChildren.x * 0.5f - equalitySize.x, posY - equalitySize.y * 0.5f, equalitySize.x, equalitySize.y);
				GUI.Label(equalityRect, equalityContent);

				UnityEditor.Handles.BeginGUI();
				ShowStructure(a_formulaBlock, equationRect, posX, posY);
				UnityEditor.Handles.EndGUI();
			}
			GUI.EndScrollView();

			return a_scrollPosition;
		}

#region Private
		private class EquationElementRect
		{
			internal Vector2 position;
			internal Vector2 size;
			internal Vector2 sizeWithChildren;
			internal Vector2 anchor;
			internal Vector2 pivot;

			internal List<EquationElementRect> childrenRect = new List<EquationElementRect>();
		}

		private static void FillStructure(FormulaVariable a_formulaVariable, ref EquationElementRect a_rect)
		{
			a_rect.size = GUI.skin.label.CalcSize(new GUIContent(a_formulaVariable.m_variable));
			a_rect.sizeWithChildren = a_rect.size;
			a_rect.anchor = Vector2.zero;
			a_rect.pivot = a_rect.size * 0.5f;
		}

		private static void FillStructure(FormulaOperator a_formulaOperator, ref EquationElementRect a_rect)
		{
			for(int childIndex = 0; childIndex < a_formulaOperator.childCount; ++childIndex)
			{
				EquationElementRect childRect = new EquationElementRect();
				FillStructureAbstract(a_formulaOperator[childIndex], ref childRect);
				a_rect.childrenRect.Add(childRect);
			}

			if(a_formulaOperator.m_operator == Operator.Divide)
			{
				a_rect.size = new Vector2(0.0f, divisionHeight);
				a_rect.sizeWithChildren = a_rect.size;

				for(int childIndex = 0; childIndex < a_formulaOperator.childCount; ++childIndex)
				{
					a_rect.sizeWithChildren.y += a_rect.childrenRect[childIndex].sizeWithChildren.y;
					a_rect.sizeWithChildren.x = Mathf.Max(a_rect.sizeWithChildren.x, a_rect.childrenRect[childIndex].sizeWithChildren.x);
				}
				a_rect.sizeWithChildren.x += 2.0f * divisionMargin;
				a_rect.size.x = a_rect.sizeWithChildren.x;

				a_rect.childrenRect[0].position.y = -(a_rect.size.y + a_rect.childrenRect[0].sizeWithChildren.y) * 0.5f;
				a_rect.childrenRect[0].position.x = 0.0f;
				a_rect.childrenRect[1].position.y = (a_rect.size.y + a_rect.childrenRect[1].sizeWithChildren.y) * 0.5f;
				a_rect.childrenRect[1].position.x = 0.0f;

				a_rect.anchor = Vector2.zero;
				a_rect.pivot.x = a_rect.sizeWithChildren.x * 0.5f;
				a_rect.pivot.y = a_rect.childrenRect[0].position.y + divisionHeight * 0.5f;
			}
			else
			{				
				string operatorText;
				switch(a_formulaOperator.m_operator)
				{
					case Operator.Add: operatorText = " + "; break;
					case Operator.Substract: operatorText = a_formulaOperator.childCount == 1? "-" : " - "; break;
					case Operator.Modulo: operatorText = " % "; break;
					case Operator.Multiply: operatorText = " * "; break;
					default: lwTools.Assert(false); operatorText = ""; break;
				}

				a_rect.size = GUI.skin.label.CalcSize(new GUIContent(operatorText));

				a_rect.sizeWithChildren = a_rect.size;
				for(int childIndex = 0; childIndex < a_formulaOperator.childCount; ++childIndex)
				{
					a_rect.sizeWithChildren.x += a_rect.childrenRect[childIndex].sizeWithChildren.x;
					a_rect.sizeWithChildren.y = Mathf.Max(a_rect.sizeWithChildren.y, a_rect.childrenRect[childIndex].sizeWithChildren.y);
				}

				if(a_formulaOperator.m_operator == Operator.Substract  &&  a_formulaOperator.childCount == 1)
				{
					a_rect.childrenRect[0].position.x = (a_rect.size.x + a_rect.childrenRect[0].sizeWithChildren.x) * 0.5f;
					a_rect.childrenRect[0].position.y = 0.0f;

					// Reduce the size for an unary operator to improve visual
					float sizeReduction = a_rect.size.x * 0.40f;	// this value is arbitrary
					a_rect.childrenRect[0].position.x -= sizeReduction;
					a_rect.childrenRect[0].sizeWithChildren.x -= sizeReduction;

					a_rect.anchor = new Vector2(a_rect.childrenRect[0].sizeWithChildren.x * 0.5f, 0.0f);
					a_rect.pivot.x = a_rect.size.x * 0.5f;
					a_rect.pivot.y = a_rect.sizeWithChildren.y * 0.5f;
				}
				else
				{
					a_rect.childrenRect[0].position.x = -(a_rect.size.x + a_rect.childrenRect[0].sizeWithChildren.x) * 0.5f;
					a_rect.childrenRect[0].position.y = 0.0f;
					a_rect.childrenRect[1].position.x = (a_rect.size.x + a_rect.childrenRect[1].sizeWithChildren.x) * 0.5f;
					a_rect.childrenRect[1].position.y = 0.0f;
					a_rect.anchor.x = (a_rect.childrenRect[0].sizeWithChildren.x + a_rect.size.x + a_rect.childrenRect[1].sizeWithChildren.x) * 0.5f - a_rect.childrenRect[0].sizeWithChildren.x - a_rect.size.x * 0.5f;
					a_rect.anchor.y = 0.0f;
					a_rect.pivot.x = a_rect.childrenRect[0].sizeWithChildren.x + a_rect.size.x * 0.5f;
					a_rect.pivot.y = a_rect.sizeWithChildren.y * 0.5f;
				}
			}
		}

		private static void FillStructure(FormulaBlock a_formulaBlock, ref EquationElementRect a_rect)
		{
			EquationElementRect childRect = new EquationElementRect();
			FillStructureAbstract(a_formulaBlock.m_child, ref childRect);
			a_rect.childrenRect.Add(childRect);

			Vector2 openingBracketSize;
			Vector2 closingBracketSize;
			bool hasRoundBrackets = ShouldEncapsulateBlockWithRoundBrackets(a_formulaBlock);
			if(hasRoundBrackets)
			{
				openingBracketSize = GUI.skin.label.CalcSize(new GUIContent("("));
				closingBracketSize = GUI.skin.label.CalcSize(new GUIContent(")"));
			}
			else
			{
				openingBracketSize = Vector2.zero;
				closingBracketSize = Vector2.zero;
			}
			a_rect.size = new Vector2(openingBracketSize.x + closingBracketSize.x, Mathf.Max(openingBracketSize.y, closingBracketSize.y));

			a_rect.sizeWithChildren = new Vector2(a_rect.size.x + childRect.sizeWithChildren.x, Mathf.Max(a_rect.size.y, childRect.sizeWithChildren.y));

			childRect.position = Vector2.zero;

			a_rect.anchor = Vector2.zero;
			a_rect.pivot.x = childRect.pivot.x + openingBracketSize.x;
			a_rect.pivot.y = a_rect.sizeWithChildren.y * 0.5f;
		}

		private static void FillStructure(FormulaMethod a_formulaMethod, ref EquationElementRect a_rect)
		{
			for(int argumentIndex = 0; argumentIndex < a_formulaMethod.argumentCount; ++argumentIndex)
			{
				EquationElementRect childRect = new EquationElementRect();
				FillStructure(a_formulaMethod[argumentIndex], ref childRect);
				a_rect.childrenRect.Add(childRect);
			}

			Vector2 symbolSize = GUI.skin.label.CalcSize(new GUIContent(a_formulaMethod.methodName));

			Vector2 openingBracketSize = GUI.skin.label.CalcSize(new GUIContent("("));
			Vector2 closingBracketSize = GUI.skin.label.CalcSize(new GUIContent(")"));
			Vector2 separatorBracketSize = GUI.skin.label.CalcSize(new GUIContent(", "));

			a_rect.size = new Vector2(symbolSize.x + openingBracketSize.x + closingBracketSize.x + Mathf.Max(0.0f, separatorBracketSize.x * (a_formulaMethod.argumentCount - 1)), Mathf.Max(symbolSize.y, openingBracketSize.y, closingBracketSize.y, a_formulaMethod.argumentCount > 1? separatorBracketSize.y : 0.0f));

			a_rect.sizeWithChildren = a_rect.size;
			for(int argumentIndex = 0; argumentIndex < a_formulaMethod.argumentCount; ++argumentIndex)
			{				
				a_rect.sizeWithChildren.x += a_rect.childrenRect[argumentIndex].sizeWithChildren.x;
				a_rect.sizeWithChildren.y = Mathf.Max(a_rect.sizeWithChildren.y, a_rect.childrenRect[argumentIndex].sizeWithChildren.y);
			}

			float xOffset = symbolSize.x + openingBracketSize.x;
			for(int argumentIndex = 0; argumentIndex < a_formulaMethod.argumentCount; ++argumentIndex)
			{
				a_rect.childrenRect[argumentIndex].position = new Vector2(-a_rect.sizeWithChildren.x * 0.5f + xOffset + a_rect.childrenRect[argumentIndex].sizeWithChildren.x * 0.5f, 0.0f);

				xOffset += a_rect.childrenRect[argumentIndex].sizeWithChildren.x;
				if(argumentIndex < a_formulaMethod.argumentCount - 1)
				{
					xOffset += separatorBracketSize.x;
				}
			}

			a_rect.anchor.x = 0.0f;
			a_rect.anchor.y = 0.0f;
			a_rect.pivot.x = 0.0f;
			a_rect.pivot.y = 0.0f;
		}

		private static void FillStructureAbstract(FormulaElement a_formulaElement, ref EquationElementRect a_rect)
		{
			if(a_formulaElement is FormulaVariable)
			{
				FillStructure((FormulaVariable)a_formulaElement, ref a_rect);
			}
			else if(a_formulaElement is FormulaOperator)
			{
				FillStructure((FormulaOperator)a_formulaElement, ref a_rect);
			}
			else if(a_formulaElement is FormulaBlock)
			{
				FillStructure((FormulaBlock)a_formulaElement, ref a_rect);
			}
			else if(a_formulaElement is FormulaMethod)
			{
				FillStructure((FormulaMethod)a_formulaElement, ref a_rect);
			}
			else
			{
				lwTools.Assert(false);
			}
		}

		private static void ShowStructure(FormulaVariable a_formulaVariable, EquationElementRect a_rect, float a_posX, float a_posY)
		{
			float posX;
			float posY;
			GetWorldPosition(out posX, out posY, a_posX, a_posY, a_rect);

			Color oldColor = GUI.contentColor;
			GUI.contentColor = a_formulaVariable.m_type == FormulaVariable.Type.Variable? FormulaParserPreferences.variableColor : FormulaParserPreferences.constantColor;
				
			Rect labelRect = new Rect(posX - a_rect.size.x * 0.5f, posY - a_rect.size.y * 0.5f, a_rect.size.x, a_rect.size.y);
			GUI.Label(labelRect, a_formulaVariable.m_variable);

			GUI.contentColor = oldColor;
		}

		private static void ShowStructure(FormulaOperator a_formulaOperator, EquationElementRect a_rect, float a_posX, float a_posY)
		{
			float posX;
			float posY;
			GetWorldPosition(out posX, out posY, a_posX, a_posY, a_rect);
			
			if(a_formulaOperator.m_operator == Operator.Divide)
			{
				Vector2 firstPoint = new Vector2(posX - a_rect.size.x * 0.5f, posY);
				Vector2 lastPoint = new Vector2(posX + a_rect.size.x * 0.5f, posY);
				UnityEditor.Handles.DrawLine(firstPoint, lastPoint);
			}
			else
			{
				string displayedOperator;
				switch(a_formulaOperator.m_operator)
				{
					case Operator.Add: displayedOperator = " + "; break;
					case Operator.Substract: displayedOperator = a_formulaOperator.childCount == 1? "-" : " - "; break;
					case Operator.Modulo: displayedOperator = " % "; break;
					case Operator.Multiply: displayedOperator = " * "; break;
					default: lwTools.Assert(false); displayedOperator = ""; break;
				}

				Rect labelRect = new Rect(posX - a_rect.size.x * 0.5f, posY - a_rect.size.y * 0.5f, a_rect.size.x, a_rect.size.y);
				GUI.Label(labelRect, displayedOperator);
			}

			for(int childIndex = 0; childIndex < a_formulaOperator.childCount; ++childIndex)
			{
				ShowStructureAbstract(a_formulaOperator[childIndex], a_rect.childrenRect[childIndex], posX, posY);
			}
		}

		private static void ShowStructure(FormulaBlock a_formulaBlock, EquationElementRect a_rect, float a_posX, float a_posY)
		{
			float posX;
			float posY;
			GetWorldPosition(out posX, out posY, a_posX, a_posY, a_rect);

			bool hasRoundBrackets = ShouldEncapsulateBlockWithRoundBrackets(a_formulaBlock);
			if(hasRoundBrackets)
			{
				GUIContent openingBracket = new GUIContent("(");
				Vector2 openingBracketSize = GUI.skin.label.CalcSize(openingBracket);
				Rect openingBracketRect = new Rect(posX - a_rect.childrenRect[0].sizeWithChildren.x * 0.5f - openingBracketSize.x, posY - openingBracketSize.y * 0.5f, openingBracketSize.x, openingBracketSize.y); 
				GUI.Label(openingBracketRect, openingBracket);

				GUIContent closingBracket = new GUIContent(")");
				Vector2 closingBracketSize = GUI.skin.label.CalcSize(closingBracket);
				Rect closingBracketRect = new Rect(posX + a_rect.childrenRect[0].sizeWithChildren.x * 0.5f, posY - closingBracketSize.y * 0.5f, closingBracketSize.x, closingBracketSize.y); 
				GUI.Label(closingBracketRect, closingBracket);
			}

			ShowStructureAbstract(a_formulaBlock.m_child, a_rect.childrenRect[0], posX, posY);
		}

		private static void ShowStructure(FormulaMethod a_formulaMethod, EquationElementRect a_rect, float a_posX, float a_posY)
		{
			float posX;
			float posY;
			GetWorldPosition(out posX, out posY, a_posX, a_posY, a_rect);

			Color oldColor = GUI.contentColor;
			GUI.contentColor = FormulaParserPreferences.methodColor;

			GUIContent methodNameContent = new GUIContent(a_formulaMethod.methodName);
			Vector2 methodNameSize = GUI.skin.label.CalcSize(methodNameContent);
			Rect labelRect = new Rect(posX - a_rect.sizeWithChildren.x * 0.5f, posY - a_rect.size.y * 0.5f, methodNameSize.x, methodNameSize.y);
			GUI.Label(labelRect, methodNameContent);

			GUI.contentColor = oldColor;

			GUIContent openingBracket = new GUIContent("(");
			Vector2 openingBracketSize = GUI.skin.label.CalcSize(openingBracket);
			Rect openingBracketRect = new Rect(labelRect.x + labelRect.width, posY - openingBracketSize.y * 0.5f, openingBracketSize.x, openingBracketSize.y); 
			GUI.Label(openingBracketRect, openingBracket);

			float xOffset = openingBracketRect.x + openingBracketRect.width;
			for(int argumentIndex = 1; argumentIndex < a_formulaMethod.argumentCount; ++argumentIndex)
			{
				xOffset += a_rect.childrenRect[argumentIndex - 1].sizeWithChildren.x;

				GUIContent separatorContent = new GUIContent(", ");
				Vector2 separatorSize = GUI.skin.label.CalcSize(separatorContent);
				Rect separatorRect = new Rect(xOffset, posY - a_rect.size.y * 0.5f, separatorSize.x, separatorSize.y);
				GUI.Label(separatorRect, separatorContent);

				xOffset += separatorSize.x;
			}
			if(a_formulaMethod.argumentCount > 0)
			{
				xOffset += a_rect.childrenRect[a_formulaMethod.argumentCount - 1].sizeWithChildren.x;
			}

			GUIContent closingBracket = new GUIContent(")");
			Vector2 closingBracketSize = GUI.skin.label.CalcSize(closingBracket);
			Rect closingBracketRect = new Rect(xOffset, posY - closingBracketSize.y * 0.5f, closingBracketSize.x, closingBracketSize.y); 
			GUI.Label(closingBracketRect, closingBracket);

			for(int argumentIndex = 0; argumentIndex < a_formulaMethod.argumentCount; ++argumentIndex)
			{
				ShowStructure(a_formulaMethod[argumentIndex], a_rect.childrenRect[argumentIndex], posX, posY);
			}
		}

		private static void ShowStructureAbstract(FormulaElement a_formulaElement, EquationElementRect a_rect, float a_posX, float a_posY)
		{			
			if(a_formulaElement is FormulaVariable)
			{
				ShowStructure((FormulaVariable)a_formulaElement, a_rect, a_posX, a_posY);
			}
			else if(a_formulaElement is FormulaOperator)
			{
				ShowStructure((FormulaOperator)a_formulaElement, a_rect, a_posX, a_posY);
			}
			else if(a_formulaElement is FormulaBlock)
			{
				ShowStructure((FormulaBlock)a_formulaElement, a_rect, a_posX, a_posY);
			}
			else if(a_formulaElement is FormulaMethod)
			{
				ShowStructure((FormulaMethod)a_formulaElement, a_rect, a_posX, a_posY);
			}
			else
			{
				lwTools.Assert(false);
			}
		}

		private static void GetWorldPosition(out float a_posX, out float a_posY, float a_parentPosX, float a_parentPosY, EquationElementRect a_rect)
		{
			a_posX = a_parentPosX + a_rect.position.x - a_rect.anchor.x;
			a_posY = a_parentPosY + a_rect.position.y - a_rect.anchor.y;
		}

		private static bool ShouldEncapsulateBlockWithRoundBrackets(FormulaBlock a_formulaBlock)
		{
			if(a_formulaBlock.m_child is FormulaOperator)
			{
				FormulaOperator childOperator = (FormulaOperator)a_formulaBlock.m_child;
				if(a_formulaBlock.m_parent is FormulaOperator)
				{
					FormulaOperator parentOperator = (FormulaOperator)a_formulaBlock.m_parent;
					if((parentOperator.m_operator == Operator.Modulo  ||  parentOperator.m_operator == Operator.Multiply)
					   &&  childOperator.m_operator != Operator.Divide)
					{
						return true;
					}
				}
			}

			return false;
		}

		private const float divisionHeight = 5.0f;
		private const float divisionMargin = 10.0f;
#endregion
	}
}