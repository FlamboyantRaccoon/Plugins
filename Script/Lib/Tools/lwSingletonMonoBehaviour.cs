using UnityEngine;

// NOTE1 :	Used to avoid build error on Flash platform with Unity 4.2+
// NOTE2 :	Flash runtime doesn't seem to call Start() if instance is created by code
//			So we force a call to Start() on first Update() in this specific case
public class lwMonoBehaviour : MonoBehaviour
{
#if UNITY_FLASH && !UNITY_EDITOR
	private bool m_bFirstUpdate = true;
#endif
	
	protected virtual void Start()
	{
	}
	
	protected virtual void Update()
	{
#if UNITY_FLASH && !UNITY_EDITOR
		if( m_bFirstUpdate )
		{
			m_bFirstUpdate = false;
			Start();
		}
#endif
	}
}

public class lwSingletonMonoBehaviour<T> : lwMonoBehaviour where T : lwMonoBehaviour
{
	public static T instance
	{
		get
		{
			if( SearchInstance()==null )
			{
				CreateInstance();
			}
			return s_instance;
		}
	}
	
	private static T s_instance;

	private static void SetInstance( T newInstance )
	{
		if( newInstance!=null )
		{
			s_instance = newInstance;
			GameObject.DontDestroyOnLoad( s_instance.gameObject );
		}
	}

	public static T SearchInstance()
	{
		if( s_instance==null )
		{
			SetInstance( FindObjectOfType( typeof(T) ) as T );
		}
		return s_instance;
	}

	public static T CreateInstance()
	{
		if( s_instance==null )
		{
			GameObject go = new GameObject( typeof(T).ToString() );
			SetInstance( go.AddComponent<T>() );
		}
		return s_instance;
	}
	
	public static void DestroyInstance()
	{
		if( IsInstanceValid() )
		{
			GameObject.Destroy( s_instance.gameObject );
			FreeInstance();
		}
	}

	public static void FreeInstance()
	{
		s_instance = null;
	}

	public static bool IsInstanceValid()
	{
		return ( s_instance!=null );
	}

	public static bool IsInstanceEqualTo( T otherInstance )
	{
		return ( s_instance==otherInstance );
	}
}
