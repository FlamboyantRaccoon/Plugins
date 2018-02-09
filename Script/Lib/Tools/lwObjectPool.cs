using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Tools/Object Pool")]
public sealed class lwObjectPool<PoolObjectType> where PoolObjectType : MonoBehaviour
{
	private PoolObjectType m_prefab = null;
	private List<PoolObjectType> m_instanceList = null;
	private List<PoolObjectType> m_usedList = null;
	private Transform m_trContainer = null;

    #if UNITY_EDITOR
	// Only useful to name prefab instances in editor
	private string m_sName = "";
	private int m_nInstanceIndex = 0;
#endif
	
	// ------------------------------------------------------------------------------- //
	// Created 
	// ------------------------------------------------------------------------------- //
	/// <summary>
	/// Init the specified prefab and nCount.
	/// </summary>
	/// <param name='prefab'>
	/// Prefab.
	/// </param>
	/// <param name='nCount'>
	/// N count.
	/// </param>
	public void Init( PoolObjectType prefab, int nCount, Transform trContainer )
	{
		Init( prefab, nCount, trContainer, Vector3.zero );
	}
	
	public void Init( PoolObjectType prefab, int nCount, Transform trContainer, Vector3 v3Position )
	{
		if( prefab!=null && m_instanceList==null )
		{
			m_prefab = prefab;
			m_trContainer = trContainer;
			m_instanceList = new List<PoolObjectType>( nCount );
			m_usedList = new List<PoolObjectType>();
#if UNITY_EDITOR
			m_sName = prefab.name;
#endif
			for( int i=0; i<nCount; i++ )
			{
                PoolObjectType instance = GameObject.Instantiate( m_prefab, v3Position, Quaternion.identity ) as PoolObjectType;
#if UNITY_EDITOR
				m_nInstanceIndex = i;
				instance.name = m_sName + "_" + ( i+1 );
#endif
				PoolObject( instance );
				instance.gameObject.SetActive( false );
			}
		}
	}
	
	// ------------------------------------------------------------------------------- //
	// Created 24/01/13
	// ------------------------------------------------------------------------------- //
	/// <summary>
	/// Destroies the pool.
	/// </summary>
	public void Destroy()
	{
		if( m_instanceList!=null )
		{
			for( int i=0; i<m_instanceList.Count; i++ )
			{
				GameObject.Destroy( m_instanceList[i].gameObject );
			}
		}
		if( m_usedList!=null )
		{
			for( int i=0; i<m_usedList.Count; i++ )
			{
				GameObject.Destroy( m_usedList[i].gameObject );
			}
		}
		m_prefab = null;
		m_instanceList = null;
		m_usedList = null;
		m_trContainer = null;
	}
	
	// ------------------------------------------------------------------------------- //
	// Created 23/01/13
	// ------------------------------------------------------------------------------- //
	public PoolObjectType GetPrefab()
	{
		return m_prefab;
	}
	
	// ------------------------------------------------------------------------------- //
	// Created 23/01/13
	// ------------------------------------------------------------------------------- //
	/// <summary>
	/// Gets the pooled instance count.
	/// </summary>
	/// <returns>
	/// The pooled instance count.
	/// </returns>
	public int GetPooledInstanceCount()
	{
		if( m_instanceList!=null )
			return m_instanceList.Count;
		else
			return -1;
	}
	
	// ------------------------------------------------------------------------------- //
	// Created 24/01/13
	// ------------------------------------------------------------------------------- //
	/// <summary>
	/// Gets the used instance count.
	/// </summary>
	/// <returns>
	/// The used instance count.
	/// </returns>
	public int GetUsedInstanceCount()
	{
		if( m_usedList!=null )
			return m_usedList.Count;
		else
			return -1;
	}
	
	// ------------------------------------------------------------------------------- //
	// Created 23/01/13
	// ------------------------------------------------------------------------------- //
	/// <summary>
	/// Gets the instance.
	/// </summary>
	/// <returns>
	/// The instance.
	/// </returns>
	public PoolObjectType GetInstance()
	{
		return GetInstance( Vector3.zero, null );
	}

	public PoolObjectType GetInstance( Transform trans )
	{
		return GetInstance( Vector3.zero, trans );
	}

	public PoolObjectType GetInstance( Vector3 v3Position )
	{
		return GetInstance( v3Position, null );
	}

	public PoolObjectType GetInstance( Vector3 v3Position, Transform trans )
	{
		if( m_instanceList!=null && m_usedList!=null && m_prefab!=null )
		{
            PoolObjectType instance = null;
			if( m_instanceList.Count>0 )
			{
				instance = m_instanceList[0];
				m_instanceList.RemoveAt( 0 );
			}
			else
			{
				instance = GameObject.Instantiate( m_prefab, v3Position, Quaternion.identity ) as PoolObjectType;
#if UNITY_EDITOR
				instance.name = m_sName + "_" + ( GetUsedInstanceCount()+1 ).ToString();
				Debug.LogWarning( "Pool instantiation " + m_prefab + " Pool max = " + ( GetUsedInstanceCount()+1 ) );
#endif
			}
			m_usedList.Add( instance );

			if( instance==null )
			{
				Debug.LogWarning( "Pool instance is null!" );
				return null;
			}

			instance.transform.SetParent( trans );

            instance.transform.localPosition = v3Position;
			instance.gameObject.SetActive( true );
			return instance;
		}
		return null;
	}
	
	// ------------------------------------------------------------------------------- //
	// Created 23/01/13
	// ------------------------------------------------------------------------------- //
	/// <summary>
	/// Pools the object.
	/// </summary>
	/// <param name='instance'>
	/// Instance.
	/// </param>
	public void PoolObject(PoolObjectType instance )
	{
		if( m_instanceList!=null && instance!=null )
		{
			instance.gameObject.SetActive( false );
			lwTools.SetParent( instance.transform, m_trContainer, Vector3.zero, Vector3.one );
			instance.transform.localRotation = Quaternion.identity;
#if UNITY_EDITOR
			if( m_prefab!=null ) instance.name = string.Concat( m_sName, string.Concat( "_", m_nInstanceIndex ) );
			m_nInstanceIndex++;
#endif
			m_instanceList.Add( instance );
			m_usedList.Remove( instance );
		}
	}
	
	// ------------------------------------------------------------------------------- //
	// Created 19/03/13
	// ------------------------------------------------------------------------------- //
	public PoolObjectType[] CopyInstanceArray()
	{
		if( m_instanceList!=null )
			return m_instanceList.ToArray();
		else
			return null;
	}
	
	// ------------------------------------------------------------------------------- //
	// Created 19/03/13
	// ------------------------------------------------------------------------------- //
	public PoolObjectType[] CopyUsedInstanceArray()
	{
		if( m_usedList!=null )
			return m_usedList.ToArray();
		else
			return null;
	}
	
	public List<PoolObjectType> UsedInstanceArray
	{
		get
		{
			return m_usedList;
		}
	}
}
