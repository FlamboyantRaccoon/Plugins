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
public sealed class lwPriorityQueue<T>
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
		public Enumerator( lwPriorityQueue<T> collection )
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
			if( m_nCurrentIndex>=m_collection.nCount )
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
		private lwPriorityQueue<T> m_collection;
		private int m_nCurrentIndex;
		private T m_currentItem;
	#endregion
#endregion
	}

	// Constructors

	// Construct a new collection
	public lwPriorityQueue()
	{
		m_internalList = new List<T>();
		m_priorityList = new List<int>();
	}

	// Properties

	//! Access to the number of elements inside the collection
	public int nCount
	{
		get{ return m_internalList.Count; }
	}

	// Methods

	//!	Add a new element with a given priority
	//!
	//!	@param	nPriority	priority of the element
	//!	@param	item		element to add
	public void Add( int nPriority, T item )
	{
		int nInsertIndex = FindIndex( nPriority );
		while( nInsertIndex<m_priorityList.Count && m_priorityList[nInsertIndex]==nPriority )
		{
			++nInsertIndex;
		}

		m_internalList.Insert( nInsertIndex, item );
		m_priorityList.Insert( nInsertIndex, nPriority );
	}

	//!	Get the first element of the queue
	//!
	//!	@return the first element of the queue
	public T Peek()
	{
		if( m_internalList.Count==0 )
		{
			throw new System.IndexOutOfRangeException( "Priority queue does not have any element." );
		}

		return m_internalList[0];
	}

	//!	Get the priority of the first element of the queue
	//!
	//!	@return the priority of the first element of the queue
	public int PeekPriority()
	{
		if( m_priorityList.Count==0 )
		{
			throw new System.IndexOutOfRangeException( "Priority queue does not have any element." );
		}

		return m_priorityList[0];
	}

	//!	Remove the first element of the queue
	//!
	//!	@return the first element (the one removed) of the queue
	public T Pop()
	{
		if( m_internalList.Count==0 )
		{
			throw new System.IndexOutOfRangeException( "Priority queue does not have any element." );
		}

		T returnedValue = m_internalList[0];
		m_internalList.RemoveAt( 0 );
		m_priorityList.RemoveAt( 0 );

		return returnedValue;
	}

	//! Clear the collection
	public void Clear()
	{
		m_internalList.Clear();
		m_priorityList.Clear();
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
	//!	@param	arrayArray	destination array
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

	//!	Creates a new array from the collection
	//!
	//!	@return the new array
	public T[] ToArray()
	{
		return m_internalList.ToArray();
	}

#region Private
	#region Methods
	private int FindIndex( int nPriority )
	{
		int nMaxIndex = m_internalList.Count;
		int nMinIndex = 0;

		while( nMinIndex<nMaxIndex )
		{
			int nMidIndex = ( nMaxIndex+nMinIndex )/2;

			if( nPriority<m_priorityList[nMidIndex] )
			{
				nMaxIndex = nMidIndex;
			}
			else if( nPriority>m_priorityList[nMidIndex] )
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
	private List<int> m_priorityList;
	#endregion
#endregion
}
#endif
