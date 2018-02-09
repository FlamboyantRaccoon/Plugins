//#define DEBUG_CHEATCODES

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Cheat codes class
/// </summary>
[AddComponentMenu("LWS/Tools/Cheat Codes")]
public sealed class lwCheatCodes : MonoBehaviour
{
	public enum 		TriggerMode { Corners, Touch3 };
	public TriggerMode 	m_eTriggerMode = TriggerMode.Corners;

#if DEBUG_CHEATCODES
	public static string s_sDebugCode = string.Empty;
#endif

#if UNITY_IPHONE || UNITY_ANDROID
	// Keyboard iOS/Android
	private TouchScreenKeyboard m_tsKeyboard = null;
	private float m_fTouchesDuration = 0f;
#endif
	
	private string m_sKeys = "";
	private float m_fLastKeyTime;
	
	public delegate void CheatCodeDelegate( int nCode );
	private CheatCodeDelegate m_cheatCodeCbk = null;
	public delegate bool SpecialCheatCodeDelegate( string sLeftCode, string sRightCode );
	private SpecialCheatCodeDelegate m_specialCheatCodeCbk = null;

	public List<string> m_sCodeList = new List<string>();
	public Dictionary<string,int> m_dictSpecialCodes = new Dictionary<string,int>();
	public float m_fClearDelay = 1.0f;
	public bool m_bIsEnabled = true;
	private bool m_bWasEnabled = false;

	// Use this for initialization
	void Start()
	{
		m_fLastKeyTime = Time.realtimeSinceStartup;
		for( int i = 0; i < m_sCodeList.Count; ++i )
		{
			m_sCodeList[i] = m_sCodeList[i].ToLower();
		}
	}
	
	// Update is called once per frame
	void Update()
	{
		bool bWasEnabled = m_bWasEnabled;
		m_bWasEnabled = m_bIsEnabled;
		
		if( !m_bIsEnabled || !bWasEnabled )
			return;
		
#if UNITY_IPHONE || UNITY_ANDROID
		if( IsMobileKeyboardTriggered() )
		{
			OpenMobileKeyboard();
		}		
		UpdateMobileKeyboard();
#endif

#if UNITY_XBOX360 && !UNITY_EDITOR
		if( IsX360KeyboardTriggered() )
		{
			OpenX360Keyboard();
		}
#endif
		
		if( Input.anyKey )
		{
			m_fLastKeyTime = Time.realtimeSinceStartup;
			m_sKeys += Input.inputString.ToLower();
		}
		else if( Time.realtimeSinceStartup - m_fLastKeyTime > m_fClearDelay )
		{
			m_sKeys = "";
		}
		
		if( Input.anyKey )
		{
			if( TestInputKeys( m_sKeys ) )
				m_sKeys = "";
		}
	}
	
	/// <summary>
	/// Tests the input keys.
	/// </summary>
	/// <param name='sKeys'>
	/// S keys.
	/// </param>
	public bool TestInputKeys( string sKeys )
	{
#if DEBUG_CHEATCODES
		if( !string.IsNullOrEmpty( sKeys ) )
			s_sDebugCode = sKeys;
#endif
		for( int i = 0; i < m_sCodeList.Count; ++i )
		{
			if( m_sCodeList[i] == sKeys )
			{
				if( m_cheatCodeCbk != null )
					m_cheatCodeCbk( i );
				return true;
			}
		}
		foreach( KeyValuePair<string,int> kvp in m_dictSpecialCodes )
		{
			if( kvp.Value==sKeys.Length && kvp.Key==sKeys.Substring(0,kvp.Key.Length) )
			{
				if( m_specialCheatCodeCbk!=null )
				{
					bool bResult = false;
					System.Delegate[] delegates = m_specialCheatCodeCbk.GetInvocationList();
					for( int i=0; i<delegates.Length; i++ )
					{
						SpecialCheatCodeDelegate callback = delegates[i] as SpecialCheatCodeDelegate;
						bResult |= callback( kvp.Key, sKeys.Substring( kvp.Key.Length ) );
					}
					return bResult;
				}
			}
		}
		return false;
	}
	
	/// <summary>
	/// Gets the length of the keys.
	/// </summary>
	/// <returns>
	/// The keys length.
	/// </returns>
	public int GetKeysLength()
	{
		return m_sKeys.Length;
	}
	
	/// <summary>
	/// Sets the cheat code cbk.
	/// </summary>
	/// <param name='cbk'>
	/// Cbk.
	/// </param>
	public void SetCheatCodeCbk( CheatCodeDelegate cbk )
	{
		m_cheatCodeCbk = cbk;
	}
	
	/// <summary>
	/// Adds the cheat code cbk.
	/// </summary>
	/// <param name='cbk'>
	/// Cbk.
	/// </param>
	public void AddCheatCodeCbk( CheatCodeDelegate cbk )
	{
		m_cheatCodeCbk += cbk;
	}
	
	/// <summary>
	/// Removes the cheat code cbk.
	/// </summary>
	/// <param name='cbk'>
	/// Cbk.
	/// </param>
	public void RemoveCheatCodeCbk( CheatCodeDelegate cbk )
	{
		m_cheatCodeCbk -= cbk;
	}

	/// <summary>
	/// Adds the cheatcode cbk.
	/// </summary>
	public void AddSpecialCheatCodeCbk( SpecialCheatCodeDelegate cbk )
	{
		m_specialCheatCodeCbk += cbk;
	}

	/// <summary>
	/// Removes the cheatcode cbk.
	/// </summary>
	public void RemoveSpecialCheatCodeCbk( SpecialCheatCodeDelegate cbk )
	{
		m_specialCheatCodeCbk -= cbk;
	}
	
#if DEBUG_CHEATCODES
	/// <summary>
	/// Raises the GU event.
	/// </summary>
	void OnGUI()
	{
		if( !string.IsNullOrEmpty( s_sDebugCode ) )
		{
			GUI.Box( new Rect( 100, 100, 200, 50 ), s_sDebugCode );
		}
	}
#endif
	
#if UNITY_IPHONE || UNITY_ANDROID
	private bool IsMobileKeyboardTriggered()
	{
		if ( m_tsKeyboard == null )
		{
			switch ( m_eTriggerMode )
			{
			case TriggerMode.Corners:
				if( Input.touchCount == 2 && IsBottomLeftCornerTouched() && IsTopRightCornerTouched() )
				{
					if( m_fTouchesDuration == 0f )
						m_fTouchesDuration = Time.realtimeSinceStartup;
					else if( Time.realtimeSinceStartup - m_fTouchesDuration > 1f )
						return true;
				}
				else
					m_fTouchesDuration = 0f;
				break;
			case TriggerMode.Touch3:
				if( Input.touchCount == 3 )
				{
					if( m_fTouchesDuration == 0f )
						m_fTouchesDuration = Time.realtimeSinceStartup;
					else if( Time.realtimeSinceStartup - m_fTouchesDuration > 1f )
						return true;
				}
				else
					m_fTouchesDuration = 0f;
				break;
			}
		}
		return false;
	}
				
	private bool IsBottomLeftCornerTouched()
	{
		foreach( Touch touch in Input.touches )
		{
			if( touch.position.x < Screen.width * 0.2f && touch.position.y < Screen.height * 0.2f )
				return true;
		}
		return false;
	}
	
	private bool IsTopRightCornerTouched()
	{
		foreach( Touch touch in Input.touches )
		{
			if( touch.position.x > Screen.width * 0.8f && touch.position.y > Screen.height * 0.8f )
				return true;
		}
		return false;
	}
	
	public void OpenMobileKeyboard()
	{
		m_tsKeyboard = TouchScreenKeyboard.Open( "", TouchScreenKeyboardType.Default, false, false, false );
	}
	
	public void UpdateMobileKeyboard()
	{
		if( m_tsKeyboard != null && m_tsKeyboard.done )
		{
			TestInputKeys( m_tsKeyboard.text.ToLower() );
			m_tsKeyboard = null;
		}
	}
#endif
#if UNITY_XBOX360 && !UNITY_EDITOR
	public bool IsX360KeyboardTriggered()
	{
		return 	GamepadInput.GamePad.GetButton( GamepadInput.GamePad.Button.LeftShoulder, GamepadInput.GamePad.Index.Any ) && 
				GamepadInput.GamePad.GetButton( GamepadInput.GamePad.Button.RightShoulder, GamepadInput.GamePad.Index.Any ) &&
				GamepadInput.GamePad.GetButton( GamepadInput.GamePad.Button.Y, GamepadInput.GamePad.Index.Any );
	}

	public void OpenX360Keyboard()
	{
		X360Keyboard.OnResult = X360KeyboardResult;
		X360Keyboard.Show( 0, 128, "", "Title", "Desc" );
	}

	private void X360KeyboardResult( bool bOk, string sText )
	{
		if( bOk )
		{
			TestInputKeys( sText.ToLower() );
		}
	}
#endif
}
