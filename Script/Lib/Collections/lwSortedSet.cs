// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain MINJARD <sminjard@bigpoint.net>
//
// Date: 2016/06/08

#if !UNITY_FLASH || UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;

//! @sealed class lwSortedSet
//!
//! @brief	Sorted Set (based on C# collection but since it is not in the 2.0 subset)
public sealed class lwSortedSet<T>
{
	//! @class Enumerator
	//!
	//!	@brief	Enumerator structure of this collection
	public class Enumerator : IEnumerator<T>
	{
		//! Accessor to the current item of the collection
		public T Current
		{
			get{ return m_currentItem; }
		}

		object IEnumerator.Current
		{
			get{ return Current; }
		}

		//! Construct an enumerator based on the given collection
		//!
		//!	@param	collection	collection we enumerates
		public Enumerator( lwSortedSet<T> collection )
		{
			lwTools.Assert( collection!=null );
			m_collection = collection;
			Reset();
		}

		//! Move to the next item of the collection
		//!
		//!	@return true if the next item exists, false otherwise
		public bool MoveNext()
		{
			++m_nCurrentIndex;
			if( m_nCurrentIndex>=m_collection.Count )
			{
				return false;
			}
			else
			{
				m_currentItem = m_collection.m_internalList[m_nCurrentIndex];
				return true;
			}
		}

		//!	Reset the enumerator to its initial position
		public void Reset()
		{
			m_nCurrentIndex = -1;
			m_currentItem = default( T );
		}

		void IDisposable.Dispose() {}

#region Private
	#region Attributes
		private lwSortedSet<T> m_collection;
		private int m_nCurrentIndex;
		private T m_currentItem;
	#endregion
#endregion
	}

	// Constructors
	
	// Construct a sorted set with default values
	public lwSortedSet()
	{
		m_internalList = new List<T>();
		m_comparer = Comparer<T>.Default;
	}

	//! Construct a sorted set with a given capacity
	//!
	//!	@param	nCapacity	capacity of the collection at creation
	public lwSortedSet( int nCapacity )
	{
		m_internalList = new List<T>( nCapacity );
		m_comparer = Comparer<T>.Default;
	}

	//! Construct a sorted set with a given capacity and using a comparer instance
	//!
	//!	@param	nCapacity	capacity of the collection at creation
	//!	@param	comparer	Comparer instance to use
	public lwSortedSet( int nCapacity, IComparer<T> comparer )
	{
		m_internalList = new List<T>( nCapacity );
		m_comparer = comparer;
	}

	//Construct a sorted set using a comparer instance
	//!
	//!	@param	comparer	Comparer instance to use
	public lwSortedSet( IComparer<T> comparer )
	{
		m_internalList = new List<T>();
		m_comparer = comparer==null ? Comparer<T>.Default : comparer;
	}

	// Construct a sorted set with a given capacity and with an existing collection
	//!
	//!	@param	nCapacity	capacity of the collection at creation
	//!	@param	enumerable	collection source
	public lwSortedSet( int nCapacity, IEnumerable<T> enumerable )
	{
		m_internalList = new List<T>(nCapacity);
		m_comparer = Comparer<T>.Default;

		IEnumerator<T> enumerator = enumerable.GetEnumerator();
		while( enumerator.MoveNext() )
		{
			Add( enumerator.Current );
		}
	}

	// Construct a sorted set with an existing collection
	//!
	//!	@param	enumerable	collection source
	public lwSortedSet( IEnumerable<T> enumerable )
	{
		m_internalList = new List<T>();
		m_comparer = Comparer<T>.Default;

		IEnumerator<T> enumerator = enumerable.GetEnumerator();
		while( enumerator.MoveNext() )
		{
			Add( enumerator.Current );
		}
	}

	// Construct a sorted set with a given capacity with an existing collection using a comparer instance
	//!
	//!	@param	nCapacity	capacity of the collection at creation
	//!	@param	enumerable	collection source
	//!	@param	comparer	Comparer instance to use
	public lwSortedSet( int nCapacity, IEnumerable<T> enumerable, IComparer<T> comparer )
	{
		m_internalList = new List<T>(nCapacity);
		m_comparer = comparer==null? Comparer<T>.Default : comparer;

		IEnumerator<T> enumerator = enumerable.GetEnumerator();
		while( enumerator.MoveNext() )
		{
			Add( enumerator.Current );
		}
	}

	// Construct a sorted set with an existing collection using a comparer instance
	//!
	//!	@param	enumerable	collection source
	//!	@param	comparer	Comparer instance to use
	public lwSortedSet( IEnumerable<T> enumerable, IComparer<T> comparer )
	{
		m_internalList = new List<T>();
		m_comparer = comparer==null? Comparer<T>.Default : comparer;

		IEnumerator<T> enumerator = enumerable.GetEnumerator();
		while( enumerator.MoveNext() )
		{
			Add( enumerator.Current );
		}
	}

	// Properties

	//! Access to the comparer used
	public IComparer<T> Comparer
	{
		get{ return m_comparer; }
	}

	//! Access to the number of elements in the collection
	public int Count
	{
		get{ return m_internalList.Count; }
	}

	// Methods

	//! Add an item into the collection (items must be unique given the comparator used)
	//!
	//!	@param	item	the item to add
	//!
	//!	@return true if the element has been added, false otherwise
	public bool Add( T item )
	{
		int nInsertIndex = FindIndex( item );
		if( nInsertIndex<m_internalList.Count && m_comparer.Compare( m_internalList[nInsertIndex], item )==0 )
		{
			return false;
		}
		else
		{
			m_internalList.Insert( nInsertIndex, item );
			return true;
		}
	}

	//!	Add a collection of items into the collection (items must be unique given the comparator used)
	//!
	//!	@param	itemCollection	collection of items to add
	//!
	//!	@return	true if at least one item has been added, false otherwise
	public bool AddRange( IEnumerable<T> itemCollection )
	{
		bool bHasAtLeastOneItemAdded = false;
		IEnumerator<T> enumerator = itemCollection.GetEnumerator();
		while(enumerator.MoveNext())
		{
			bHasAtLeastOneItemAdded = Add(enumerator.Current)  ||  bHasAtLeastOneItemAdded;
		}
		return bHasAtLeastOneItemAdded;
	}

	//!	Clear the collection
	public void Clear()
	{
		m_internalList.Clear();
	}

	//!	Check if the collection contains a given item
	//!
	//!	@param	item	element to check
	//!
	//!	@return true if the element is contained inside the collection, false otherwise
	public bool Contains( T item )
	{
		int nIndex = FindIndex( item );
		return nIndex  < m_internalList.Count  &&  m_comparer.Compare( m_internalList[nIndex], item )==0;
	}

	//! Access an item from its index
	//!
	//!	@param	nIndex	index of the item to get
	public T this[ int nIndex ]
	{
		get
		{
			lwTools.Assert( nIndex >= 0 && nIndex < m_internalList.Count );
			return m_internalList[nIndex];
		}
	}

	//!	Creates an enumerator for the collection
	//!
	//!	@return the enumerator created
	public Enumerator GetEnumerator()
	{
		return new Enumerator( this );
	}

	//!	Copy the collection into a given array
	//!
	//!	@param	arrayArray	destination array (must be sized properly)
	public void CopyTo( T[] arrayArray )
	{
		m_internalList.CopyTo( arrayArray, 0 );
	}

	//!	Copy the collection into a given array from a specific index
	//!
	//!	@param	arrayArray	destination array (must be sized properly)
	//!	@param	nIndex		starting index of the elements to copy
	public void CopyTo( T[] arrayArray, int nIndex )
	{
		m_internalList.CopyTo( arrayArray, nIndex );
	}

	//!	Copy the collection into a given array from a specific index and for a specific number of elements
	//!
	//!	@param	arrayArray	destination array (must be sized properly)
	//!	@param	nIndex		starting index of the elements to copy
	//!	@param	nCount		number of elements to copy
	public void CopyTo( T[] arrayArray, int nIndex, int nCount )
	{
		m_internalList.CopyTo( nIndex, arrayArray, 0, nCount );
	}

	//!	Remove an element from the collection
	//!
	//!	@param	item	the element to remove
	//!
	//!	@return true if the element has been removed, false otherwise
	public bool Remove( T item )
	{
		int nRemoveIndex = FindIndex( item );
		if( nRemoveIndex<m_internalList.Count && m_comparer.Compare( m_internalList[nRemoveIndex], item )==0 )
		{
			m_internalList.RemoveAt( nRemoveIndex );
			return true;
		}
		else
		{
			return false;
		}
	}

	//!	Creates a new array from the collection
	//!
	//!	@return the new array
	public T[] ToArray()
	{
		return m_internalList.ToArray();
	}

#region Private
	#region Methods
	private int FindIndex( T item )
	{
		int nMaxIndex = m_internalList.Count;
		int nMinIndex = 0;

		while( nMinIndex<nMaxIndex )
		{
			int nMidIndex = ( nMaxIndex+nMinIndex )/2;

			int nCompareValue = m_comparer.Compare( item, m_internalList[nMidIndex] );
			lwTools.Assert( m_comparer.Compare( m_internalList[nMidIndex], item )==-nCompareValue, "Comparer used for lwSortedSet is not consistent with data." );
			if( nCompareValue<0 )
			{
				nMaxIndex = nMidIndex;
			}
			else if( nCompareValue>0 )
			{
				nMinIndex = nMidIndex+1;
			}
			else
			{
				nMinIndex = nMidIndex;
				nMaxIndex = nMidIndex;
			}
		}

		lwTools.Assert( nMinIndex==nMaxIndex );
		return nMinIndex;
	}
	#endregion

	#region Attributes
	private List<T> m_internalList;
	private IComparer<T> m_comparer;
	#endregion
#endregion
}
#endif
