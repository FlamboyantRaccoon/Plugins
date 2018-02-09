using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class lwMemory<T> where T : Object
{
	public class MemoryData<U> where U : Object
	{
		public string m_sName;
		public long m_nBytes;

		public MemoryData( U t )
		{
			m_sName = t.name;
#if UNITY_5_6_OR_NEWER
			m_nBytes = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong( t );
#elif UNITY_5_5
			m_nBytes = UnityEngine.Profiling.Profiler.GetRuntimeMemorySize( t );
#else
			m_nBytes = Profiler.GetRuntimeMemorySize( t );
#endif
		}
	}

	private class MemoryDataComparer<U> : IComparer<MemoryData<U>> where U : Object
	{
		int IComparer<MemoryData<U>>.Compare( MemoryData<U> data1, MemoryData<U> data2 )
		{
			if( data1.m_nBytes!=data2.m_nBytes )
				return (int)( data2.m_nBytes-data1.m_nBytes );
			else
				return string.Compare( data2.m_sName, data1.m_sName );
		}
	}

	public MemoryData<T>[] dataArray { get { return m_memoryDataArray; } }
	public int nDataCount { get { return m_nMemoryCount; } }
	public long nDataSize { get { return m_nMemorySize; } }

	public static int MAX_LINE_COUNT = 10;

	private lwSortedSet<MemoryData<T>> m_sortedAssets;
	private MemoryData<T>[] m_memoryDataArray = new MemoryData<T>[MAX_LINE_COUNT];
	private int m_nMemoryCount;
	private long m_nMemorySize;
	
	public void Init()
	{
		m_sortedAssets = new lwSortedSet<MemoryData<T>>( new MemoryDataComparer<T>() );
	}

	public void Destroy()
	{
		if( m_sortedAssets!=null )
		{
			m_sortedAssets.Clear();
			m_sortedAssets = null;
		}
	}

	public void SnapMemory()
	{
		m_sortedAssets.Clear();
#if UNITY_EDITOR
		Object[] oPlayerSettings = Resources.FindObjectsOfTypeAll( typeof( PlayerSettings ) );
		Object[] oPlayerSettingsDep = EditorUtility.CollectDependencies( oPlayerSettings );
#endif
		T[] assetArray = (T[])Resources.FindObjectsOfTypeAll(typeof(T));
		m_nMemoryCount = assetArray.Length;
		m_nMemorySize = 0;
		for( int i=0; i<m_nMemoryCount; ++i )
		{
			T t = assetArray[i];
			MemoryData<T> data = new MemoryData<T>( t );
			//if( t.name.Contains( "Atlas" ) )
			//	Debug.Log( "Texture object " + t.name + " using: " + data.m_nBytes + "Bytes" );
			if( t.hideFlags == HideFlags.HideAndDontSave )
				continue;
#if UNITY_EDITOR
			if( IsObjectInArray( oPlayerSettingsDep, t ) )
				continue;
#endif
			if( m_sortedAssets.Add( data ) )
				m_nMemorySize += data.m_nBytes;
		}
		int nLineCount = Mathf.Min( m_nMemoryCount, MAX_LINE_COUNT );
		m_nMemoryCount = nLineCount;
		m_sortedAssets.CopyTo( m_memoryDataArray, 0, nLineCount );
	}

	private bool IsObjectInArray( Object[] oArray, Object o )
	{
		int nCount = oArray!=null ? oArray.Length : 0;
		for( int i = 0; i<nCount; ++i )
		{
			if( oArray[i]==o )
				return true;
		}
		return false;
	}
}
