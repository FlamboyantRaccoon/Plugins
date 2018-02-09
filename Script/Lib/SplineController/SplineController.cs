using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum eOrientationMode { NODE = 0, TANGENT }

[AddComponentMenu("Splines/Spline Controller")]
[RequireComponent(typeof(SplineInterpolator))]
public class SplineController : MonoBehaviour
{
	public GameObject SplineRoot;
	public float Duration = 10;
	public eOrientationMode OrientationMode = eOrientationMode.NODE;
	public eWrapMode WrapMode = eWrapMode.ONCE;
	public bool AutoStart = true;
	public bool AutoClose = true;
	public bool HideOnExecute = true;
	public bool DisplayOnDrawGizmos = true;
	public int SegmentCount = 250;

	SplineInterpolator mSplineInterp;
	Transform[] mTransforms;

	void OnDrawGizmos()
	{
		if( DisplayOnDrawGizmos )
		{
			Transform[] trans = GetTransforms();
			if ( trans == null || trans.Length < 2)
				return;
	
			SplineInterpolator interp = GetComponent(typeof(SplineInterpolator)) as SplineInterpolator;
			SetupSplineInterpolator(interp, trans);
			interp.StartInterpolation(null, false, WrapMode);
	
			Vector3 prevPos = trans[0].localPosition;
			
			for (int c = 1; c <= SegmentCount; c++)
			{
				float currTime = c * Duration / SegmentCount;
				Vector3 currPos = interp.GetHermiteAtTime(currTime);
				float mag = (currPos-prevPos).magnitude * 2;
				Gizmos.color = new Color(mag, 0, 0, 1);
				Gizmos.DrawLine(prevPos, currPos);
				prevPos = currPos;
			}
		}
		else
		{
			/*
			if( mSplineInterp != null && mTransforms != null )
			{
				Vector3 prevPos = mTransforms[0].position;
			
				for (int c = 1; c <= SegmentCount; c++)
				{
					float currTime = c * Duration / SegmentCount;
					Vector3 currPos = mSplineInterp.GetHermiteAtTime(currTime);
					float mag = (currPos-prevPos).magnitude * 2;
					Gizmos.color = new Color(mag, 0, 0, 1);
					Gizmos.DrawLine(prevPos, currPos);
					prevPos = currPos;
				}
			}
			*/
		}
	}


	public void Init()
	{
		mSplineInterp = GetComponent(typeof(SplineInterpolator)) as SplineInterpolator;

		mTransforms = GetTransforms();

		if (HideOnExecute)
			DisableTransforms();

		if (AutoStart)
			FollowSpline();
		
	}

    public void Reset()
    {
        mSplineInterp.Reset();
    }

    void SetupSplineInterpolator(SplineInterpolator interp, Transform[] trans)
	{
		interp.Reset();

		float step = (AutoClose) ? Duration / trans.Length :
			Duration / (trans.Length - 1);

		int c;
		for (c = 0; c < trans.Length; c++)
		{
			if (OrientationMode == eOrientationMode.NODE)
			{
				interp.AddPoint(trans[c].localPosition, trans[c].rotation, step * c, new Vector2(0, 1));
			}
			else if (OrientationMode == eOrientationMode.TANGENT)
			{
				Quaternion rot;
				if (c != trans.Length - 1)
					rot = Quaternion.LookRotation(trans[c + 1].localPosition - trans[c].localPosition, trans[c].up);
				else if (AutoClose)
					rot = Quaternion.LookRotation(trans[0].localPosition - trans[c].localPosition, trans[c].up);
				else
					rot = trans[c].rotation;

				interp.AddPoint(trans[c].localPosition, rot, step * c, new Vector2(0, 1));
			}
		}

		if (AutoClose)
			interp.SetAutoCloseMode(step * c);
	}


	/// <summary>
	/// Returns children transforms, sorted by name.
	/// </summary>
	Transform[] GetTransforms()
	{
		if (SplineRoot != null)
		{
			List<Component> components = new List<Component>(SplineRoot.GetComponentsInChildren(typeof(Transform)));
		#if !UNITY_FLASH
			List<Transform> transforms = components.ConvertAll(c => (Transform)c);
		#else
			List<Transform> transforms = new List<Transform>();
			foreach( Component c in components )
			{
				transforms.Add( c.transform );
			}
		#endif

			transforms.Remove(SplineRoot.transform);
			transforms.Sort(delegate(Transform a, Transform b)
			{
				return a.name.CompareTo(b.name);
			});

			return transforms.ToArray();
		}

		return null;
	}

	/// <summary>
	/// Disables the spline objects, we don't need them outside design-time.
	/// </summary>
	void DisableTransforms()
	{
		if (SplineRoot != null)
		{
			SplineRoot.SetActive(false);
		}
	}


	/// <summary>
	/// Starts the interpolation
	/// </summary>
	void FollowSpline()
	{
		if (mTransforms.Length > 0)
		{
			SetupSplineInterpolator(mSplineInterp, mTransforms);
			mSplineInterp.StartInterpolation(null, true, WrapMode);
		}
	}
	
	/// <summary>
	/// GetPositionAtTime
	/// </summary>
	public Vector3 GetPositionAtTime( float fTime )
	{
		if ( mSplineInterp != null )
		{
			return mSplineInterp.GetHermiteAtTime( fTime );
		}
		return Vector3.zero;
	}

}