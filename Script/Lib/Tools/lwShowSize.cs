using UnityEngine;
using System.Collections;

public sealed class lwShowSize : MonoBehaviour
{
	public Rect m_rectGui = new Rect( Screen.width * 0.5f - 300f, 5f, 600f, 30f );
    public GUISkin m_skin;

    void OnGUI()
	{
		GUIStyle style = GUI.skin.box;
        style.fontSize = 20;
        if (m_skin != null) GUI.skin = m_skin;
        GUI.Label( m_rectGui, "Screen size = " + Screen.width + "x" + Screen.height, style );
	}
}
