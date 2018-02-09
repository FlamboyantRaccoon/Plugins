#if UNITY_5 || UNITY_2017_1_OR_NEWER
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class lwLinesMesh 
{
	public sealed class LineData
	{
		public Vector3 v3Start;
		public Vector3 v3Stop;
		public Vector3[] v3PosArray;
		public Vector2[] uvs;
		public Vector2[] uv2s;
		public Vector2[] normals;
		public Color color;
		public float fWidth;
        // because sometime you just want some line to be continuous and not all the mesh
        public bool bUsePreviousLinePoints;
		
		private float m_fLength = -1;
		public float Length { get{ return m_fLength; } }
		
		public LineData()
		{
			v3Start = Vector3.zero;
			v3Stop = Vector3.zero;
			v3PosArray = null;
			uvs = null;
			color = Color.clear;
			fWidth = 1f;
            bUsePreviousLinePoints = false;
        }
		
		public LineData( Vector3 _v3Start, Vector3 _v3Stop, Color _color, float _fWidth, bool _bUsePreviousLinePoints )
		{
			v3Start = _v3Start;
			v3Stop = _v3Stop;
			v3PosArray = null;
			uvs = null;
			color = _color;
			fWidth = _fWidth;
            bUsePreviousLinePoints = _bUsePreviousLinePoints;
        }
		
		public LineData( Vector3 _v3Start, Vector3 _v3Stop, Color _color, float _fWidth, Vector2[] _uvs, Vector2[] _uv2s, Vector2[] _normals, bool _bUsePreviousLinePoints)
		{
			v3Start = _v3Start;
			v3Stop = _v3Stop;
			v3PosArray = null;
			uvs = _uvs;
			uv2s = _uv2s;
			normals = _normals;
			color = _color;
			fWidth = _fWidth;
            bUsePreviousLinePoints = _bUsePreviousLinePoints;
        }

		public LineData( Vector3[] _v3PosArray, Color _color, Vector2[] _uvs=null, Vector2[] _uv2s=null, Vector2[] _normals=null, bool _bUsePreviousLinePoints=false )
		{
			v3Start = Vector3.zero;
			v3Stop = Vector3.zero;
			v3PosArray = _v3PosArray;
			uvs = _uvs;
			uv2s = _uv2s;
			normals = _normals;
			color = _color;
			fWidth = 1f;
            bUsePreviousLinePoints = _bUsePreviousLinePoints;
        }
		
		public float ComputeLength()
		{
			m_fLength = Vector3.Distance( v3Start, v3Stop );
			return m_fLength;
		}
	}

	public float BorderSize
	{
		get { return m_fBorderSize; }
		set { m_fBorderSize = value; }
	}

	public bool UseContinuousLines
	{
		get { return m_bUseContinuousLines; }
		set { m_bUseContinuousLines = value; }
	}

	public bool UseLastPoints
	{
		get { return m_bUseLastPointsToContinue; }
		set { m_bUseLastPointsToContinue = value; }
	}
	
	public float outlineOffset { get { return m_fOutlineOffset; } set { m_fOutlineOffset = value; } }
	public bool loop { get { return m_bLoop; } set { m_bLoop = value; } }
	public List<LineData> lines { get { return m_lines; } set { m_lines = value; } }
	public Mesh mesh { get { return m_mesh; } }
	public System.Action<Mesh> onUpdateMesh				{ set { m_onUpdateMesh = value; } }
	

	public bool m_bMergeVertex = false; // TODO : Debug this option
	public int m_nVertexBySide = 2;
	public bool m_bDebugGenerateMesh = false;

	private float m_fBorderSize = 0f;
	private bool m_bUseContinuousLines = false;
	private bool m_bUseLastPointsToContinue = false;
	private float m_fOutlineOffset = 0f;
	private bool m_bLoop = false;
	private List<LineData> m_lines = null;
	private Mesh m_mesh;
	
	private System.Action<Mesh> m_onUpdateMesh;

	public int GetLineCount()
	{
		if( m_lines != null )
			return m_lines.Count;
		return 0;
	}

	public Vector3 GetLineStartPos( int nLineIndex )
	{
		if( m_lines==null || nLineIndex<0 || nLineIndex>=m_lines.Count )
			return Vector3.zero;

		return m_lines[nLineIndex].v3Start;
	}

	public Vector3 GetLineStopPos( int nLineIndex )
	{
		if( m_lines==null || nLineIndex<0 || nLineIndex>=m_lines.Count )
			return Vector3.zero;

		return m_lines[nLineIndex].v3Stop;
	}

	public void AddLine()
	{
		if( m_lines == null )
			m_lines = new List<LineData>();
		m_lines.Add( new LineData() );
	}

	public void AddLine( Vector3[] v3PosArray, Color color, Vector2[] uvs=null, Vector2[] uv2s=null, Vector2[] v2Normals=null, bool _bUsePreviousLinePoints = false)
	{
		if( m_lines == null )
			m_lines = new List<LineData>();
		m_lines.Add( new LineData( v3PosArray, color, uvs, uv2s, v2Normals, _bUsePreviousLinePoints) );
	}

	public void AddLine( Vector3 v3Start, Vector3 v3Stop, Color color, float fWidth, bool _bUsePreviousLinePoints )
	{
		if( m_lines == null )
			m_lines = new List<LineData>();
		m_lines.Add( new LineData( v3Start, v3Stop, color, fWidth, _bUsePreviousLinePoints) );
	}
	
	public void AddLine( Vector3 v3Start, Vector3 v3Stop, Color color, float fWidth, Vector2[] uvs, Vector2[] uv2s=null, Vector2[] v2Normals=null, bool _bUsePreviousLinePoints = false )
	{
		if( m_lines == null )
			m_lines = new List<LineData>();
		m_lines.Add( new LineData( v3Start, v3Stop, color, fWidth, uvs, uv2s, v2Normals, _bUsePreviousLinePoints) );
	}
	
	public void AddQuad( Vector3[] v3Quads, Color color, Vector2[] uvs )
	{
		if( m_lines == null )
			m_lines = new List<LineData>();
		m_lines.Add( new LineData( v3Quads, color, uvs ) );
	}

	public void InsertLine( int nIndex, Vector3 v3Start, Vector3 v3Stop, Color color, float fWidth, bool bUsePreviousLinePoints )
	{
		if( m_lines==null )
			m_lines = new List<LineData>();
		m_lines.Insert( nIndex, new LineData( v3Start, v3Stop, color, fWidth, bUsePreviousLinePoints) );
	}

	public void RemoveLine( int nLineIdx )
	{
		if( m_lines==null || nLineIdx<0 || nLineIdx>=m_lines.Count )
			return;

		m_lines.RemoveAt( nLineIdx );
	}
	
	public void RemoveLine( Vector3 v3Start, Vector3 v3Stop )
	{
		if( m_lines!=null )
		{
			int nLineIdx = -1;
			int nLineCount = m_lines.Count;
			// Search Specific Line
			for( int i=0; i<nLineCount; i++ )
			{
				LineData ld = m_lines[i];
				if( ld.v3Start==v3Start && ld.v3Stop==v3Stop )
				{
					nLineIdx = i;
					break;
				}
			}
			// Test if found
			if( nLineIdx >= 0 )
			{
				m_lines.RemoveAt( nLineIdx );
			}
		}
	}
	
	public void ApplyLines()
	{
		if( m_lines != null )
			SetArray( m_lines.ToArray() );
	}
	
	public void CreatePoolOfLines( int nLineCount )
	{
		m_lines = new List<LineData>( nLineCount );
		for( int i = 0; i < nLineCount; ++i )
			AddLine();
		ApplyLines();
	}
	
	public void ClearLines()
	{
		if( m_lines != null )
		{
			m_lines.Clear();
			m_lines = null;
		}
	}

	public void SetVertexPosition( int nIndex, Vector3 vPos )
	{
		if( nIndex<0 || nIndex>=m_lines.Count )
			return;

		m_lines[nIndex].v3Start = vPos;
		m_lines[(m_lines.Count+nIndex-1)%m_lines.Count].v3Stop = vPos;

		ApplyLines();
	}

    public void ChangeLinesColor(Color color)
    {
        int nCount = -1;
        if (m_lines != null)
            nCount = m_lines.Count;
        else if (m_mesh != null)
            nCount = m_mesh.colors.Length;

        for (int i = 0; i < nCount; i++)
        {
            ChangeLineColor(i, color);
        }
        ApplyLines();
    }

    public void ChangeLineColor(int nLineIdx, Color color)
    {
        if (m_lines != null)
        {
            if (nLineIdx >= 0 && nLineIdx < m_lines.Count)
                m_lines[nLineIdx].color = color;
        }
        if (m_mesh != null)
        {
            Color[] colors = m_mesh.colors;
            if (nLineIdx >= 0 && nLineIdx < colors.Length / 4)
            {
                colors[nLineIdx * 4 + 0] = color;
                colors[nLineIdx * 4 + 1] = color;
                colors[nLineIdx * 4 + 2] = color;
                colors[nLineIdx * 4 + 3] = color;
            }
            m_mesh.colors = colors;
        }
    }

    public void SetLineVertexPosition( int nLineId, bool bStart, Vector3 vPos )
	{
		if( nLineId<0 || nLineId>=m_lines.Count )
			return;

		if( bStart )
		{
			m_lines[nLineId].v3Start = vPos;
		}
		else
		{
			m_lines[nLineId].v3Stop = vPos;
		}

		ApplyLines();
	}

	public void SetArray( LineData[] ldArray )
	{
		if( m_mesh==null )
		{
			m_mesh = new Mesh();
		}
		else
		{
			m_mesh.Clear();
		}


		int nLen = ldArray.Length;
		if( nLen>0 )
		{
			int nIdxV = 0;
			int nIdxT = 0;

			int nVertexCount =  m_bMergeVertex ? ((nLen+1) * m_nVertexBySide) : (nLen * m_nVertexBySide * 2);
			int nQuadsByLine = m_nVertexBySide - 1;
			int nIndexByTriangle = 3;
			int nTrianglesByQuad = 2;
			int nTriangleIndexByQuad = nIndexByTriangle * nTrianglesByQuad;
			int nTrianglesByLine = nQuadsByLine * nTrianglesByQuad;
			int nTriangleCount = nLen * nTrianglesByLine;
			int nTriangleIndexCount = nTriangleCount * nIndexByTriangle;
			int nPointsByLine = m_nVertexBySide * 2;

			Vector3[] v3Vertices = new Vector3[nVertexCount];
			int[] nTriangles = new int[nTriangleIndexCount];
			Color[] cVertices = new Color[nVertexCount];
			Vector2[] v2TexCoord = new Vector2[nVertexCount];
			Vector2[] v2TexCoord2 = new Vector2[nVertexCount];
			Vector3[] v3Normals = new Vector3[nVertexCount];
			Vector3[] v3Quad = new Vector3[nPointsByLine];
			Vector2[] v2InterPos = new Vector2[m_nVertexBySide];
            int nVertexInc = m_bMergeVertex ? m_nVertexBySide : nPointsByLine;

            Vector3 v3LastStop = Vector3.zero;

			for( int i=0; i<nLen; i++ )
			{
				LineData ld = ldArray[i];
				bool bUsePosArray = ld.v3PosArray!=null;
				if( bUsePosArray )
					MakeQuad( v3Quad, ld.v3PosArray );
				else
					MakeQuad( v3Quad, ld.v3Start, ld.v3Stop, ld.fWidth, m_fBorderSize );
				
				if( ( m_bUseContinuousLines || ld.bUsePreviousLinePoints) && i!=0 )
				{
					if( m_bUseLastPointsToContinue )
					{
						if( !m_bMergeVertex )
						{
							for( int v = 0; v < m_nVertexBySide; ++v )
							{
								v3Quad[v].x = v3Vertices[nIdxV - m_nVertexBySide + v].x;
								v3Quad[v].y = v3Vertices[nIdxV - m_nVertexBySide + v].y;
							}

                            if( m_bLoop && i==nLen-1 )
                            {
                                for (int v = 0; v < m_nVertexBySide; ++v)
                                {
                                    v3Quad[v+nVertexInc - m_nVertexBySide].x = v3Vertices[v].x;
                                    v3Quad[v+nVertexInc - m_nVertexBySide].y = v3Vertices[v].y;
                                }
                            }
						}
					}
					else
					{
						if( v3LastStop == ld.v3Start )
						{
							bool bInterOk = true;
							for( int v = 0; v < m_nVertexBySide; ++v )
							{
								if( !LineIntersection( v3Vertices[nIdxV - 2 * m_nVertexBySide + v], v3Vertices[nIdxV - m_nVertexBySide + v], v3Quad[v], v3Quad[m_nVertexBySide + v], ref v2InterPos[v] ) )
								{
									bInterOk = false;
									break;
								}
							}

							if( bInterOk )
							{
								for( int v = 0; v < m_nVertexBySide; ++v )
								{
									v3Quad[v].x = v2InterPos[v].x;
									v3Quad[v].y = v2InterPos[v].y;
									v3Vertices[nIdxV - m_nVertexBySide+v] = v3Quad[v];
								}
								if( m_nVertexBySide==2 && TestTriangleFacesOrientation( v3Vertices, nIdxV - 4 ) && nIdxV>=8 )
								{
									Vector2 v2Inter = Vector2.zero;
									bool bInter = LineIntersection( v3Vertices[nIdxV - 8], v3Vertices[nIdxV - 6], v3Quad[0], v3Quad[2], ref v2Inter );
									if( bInter )
									{
										v3Vertices[nIdxV - 6] = v2Inter;
										v3Vertices[nIdxV - 4] = v2Inter;
										v3Vertices[nIdxV - 2] = v2Inter;
										v3Quad[0] = v2Inter;
									}
								}
							}
						}
						if( loop && i==nLen-1 )
						{
							bool bInterOk = true;
							for( int v = 0; v < m_nVertexBySide; ++v )
							{
								if( ! LineIntersection( v3Quad[v], v3Quad[m_nVertexBySide+v], v3Vertices[v], v3Vertices[m_nVertexBySide+v], ref v2InterPos[v] ) )
								{
									bInterOk = false;
									break;
								}
							}
							if( bInterOk )
							{
								for( int v = 0; v < m_nVertexBySide; ++v )
								{
									v3Quad[m_nVertexBySide + v].x = v2InterPos[v].x;
									v3Quad[m_nVertexBySide + v].y = v2InterPos[v].y;
									v3Vertices[v] = v3Quad[2+v];
								}

								if( m_nVertexBySide==2 && TestTriangleFacesOrientation( v3Vertices, nIdxV-4 ) )
								{
									Vector2 v2Inter = Vector2.zero;
									bool bInter = LineIntersection( v3Vertices[nIdxV-4], v3Vertices[nIdxV-2], v3Vertices[0], v3Vertices[2], ref v2Inter );
									if( bInter )
									{
										v3Vertices[nIdxV-2] = v2Inter;
										v3Vertices[0] = v2Inter;
										v3Quad[0] = v2Inter;
										v3Quad[2] = v2Inter;
									}
								}
							}
						}
					}
				}
				v3LastStop = ld.v3Stop;

				
				for( int p = 0; p<nVertexInc; ++p )
					v3Vertices[nIdxV + p] = v3Quad[p];

				if( ld.uvs!=null && ld.uvs.Length>=nVertexInc )
				{
					for( int p = 0; p<nVertexInc; ++p )
						v2TexCoord[nIdxV+p] = ld.uvs[p];
				}
				else
				{
					if( m_bMergeVertex )
					{
						for( int v = 0; v < m_nVertexBySide; ++v )
						{
							float fRatio = (float)v / (float)( m_nVertexBySide - 1 );
							v2TexCoord[nIdxV + v] = Vector2.up * fRatio + Vector2.right * (nIdxV/m_nVertexBySide);
						}
					}
					else
					{
						for( int v = 0; v < m_nVertexBySide; ++v )
						{
							float fRatio = (float)v / (float)( m_nVertexBySide - 1 );
							v2TexCoord[nIdxV + v] = Vector2.up * fRatio;
							v2TexCoord[nIdxV + v + m_nVertexBySide] = v2TexCoord[nIdxV + v] + Vector2.right;
						}
					}
				}

				if( ld.uv2s!=null && ld.uv2s.Length>=nVertexInc )
				{
					for( int p = 0; p<nVertexInc; ++p )
						v2TexCoord2[nIdxV+p] = ld.uv2s[p];
				}
				else
				{
					if( m_bMergeVertex )
					{
						for( int v = 0; v < m_nVertexBySide; ++v )
						{
							float fRatio = (float)v / (float)( m_nVertexBySide - 1 );
							v2TexCoord2[nIdxV + v] = Vector2.up * fRatio + Vector2.right * ( nIdxV / m_nVertexBySide );
						}
					}
					else
					{
						for( int v = 0; v < m_nVertexBySide; ++v )
						{
							float fRatio = (float)v / (float)( m_nVertexBySide - 1 );
							v2TexCoord2[nIdxV + v] = Vector2.up * fRatio;
							v2TexCoord2[nIdxV + v + m_nVertexBySide] = v2TexCoord2[nIdxV + v] + Vector2.right;
						}
					}
				}

				if( ld.normals!=null && ld.normals.Length>=nVertexInc )
				{
					for( int p = 0; p<nVertexInc; ++p )
						v3Normals[nIdxV+p] = ld.normals[p];
				}
				else
				{
					for( int p = 0; p<nVertexInc; ++p )
						v3Normals[nIdxV+p] = Vector3.back;
				}

				for( int p = 0; p<nVertexInc; ++p )
					cVertices[nIdxV+p] = ld.color;

				for( int q = 0; q < nQuadsByLine; ++q )
				{
					nTriangles[nIdxT + 0] = nIdxV + 0 + q;
					nTriangles[nIdxT + 1] = nIdxV + 1 + q;
					nTriangles[nIdxT + 2] = nIdxV + m_nVertexBySide + q;
					nTriangles[nIdxT + 3] = nIdxV + 1 + q;
					nTriangles[nIdxT + 4] = nIdxV + m_nVertexBySide + 1 + q;
					nTriangles[nIdxT + 5] = nIdxV + m_nVertexBySide + q;
					nIdxT += nTriangleIndexByQuad;
				}

				nIdxV += nVertexInc;
			}

			m_mesh.vertices = v3Vertices;
			m_mesh.triangles = nTriangles;
			m_mesh.uv = v2TexCoord;
			m_mesh.uv2 = v2TexCoord2;
			m_mesh.colors = cVertices;
			m_mesh.normals = v3Normals;

			if( m_onUpdateMesh!=null )
			{
				m_onUpdateMesh( m_mesh );
			}
		}
	}

	//public List<UIVertex> ConvertMesh( Vector3[] vertices, int[] triangles, Vector3[] normals, Vector2[] uv )
	//{
	//	List<UIVertex> vertexList = new List<UIVertex>(triangles.Length);
     
	//	UIVertex vertex;
	//	for (int i = 0; i < triangles.Length; i++)
	//	{
	//		vertex = new UIVertex();
	//		int triangle = triangles[i];
     
	//		vertex.position = vertices[triangle];
	//		vertex.uv0 = uv[triangle];
	//		vertex.normal = normals[triangle];
	//		vertex.color = Color.white;
     
	//		vertexList.Add(vertex);
     
	//		if (i % 3 == 0)
	//			vertexList.Add(vertex);
	//	}
     
	//	return vertexList;
	//}


	/// <summary>
	/// Lines the intersection. FROM http://forum.unity3d.com/threads/17384-Line-Intersection
	/// </summary>
	/// <returns>
	/// The intersection.
	/// </returns>
	/// <param name='p1'>
	/// If set to <c>true</c> p1.
	/// </param>
	/// <param name='p2'>
	/// If set to <c>true</c> p2.
	/// </param>
	/// <param name='p3'>
	/// If set to <c>true</c> p3.
	/// </param>
	/// <param name='p4'>
	/// If set to <c>true</c> p4.
	/// </param>
	/// <param name='intersection'>
	/// If set to <c>true</c> intersection.
	/// </param>
	public static bool LineIntersection( Vector2 p1,Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 intersection )
	{
		float Ax = p2.x-p1.x;
		float Bx = p3.x-p4.x;
		float Ay = p2.y-p1.y;
		float By = p3.y-p4.y;
		float Cx = p1.x-p3.x;
		float Cy = p1.y-p3.y;
		float d = By*Cx - Bx*Cy;	// alpha numerator
		//float e = Ax*Cy - Ay*Cx;	// beta numerator
		float f = Ay*Bx - Ax*By;	// both denominator
		if( f==0 ) return false;	// check if they are parallel
		float num = d*Ax;			// numerator
		intersection.x = p1.x + num / f;
		num = d*Ay;
		intersection.y = p1.y + num / f;
		return true;
	}
	

	private void Destroy()
	{
		ClearLines();
	}
	
	private void MakeQuad( Vector3[] v3Dest, Vector3 v3Start, Vector3 v3Stop, float fWidth, float fBorder )
	{
		Vector3 v3Normal = new Vector3( v3Stop.y-v3Start.y, v3Start.x-v3Stop.x, v3Stop.z-v3Start.z );
		v3Normal.Normalize();

		if( fBorder>0f )
		{
			Vector3 v3Tangent = new Vector3( -v3Normal.y, v3Normal.x, v3Normal.z );
			v3Start -= v3Tangent * fBorder;
			v3Stop += v3Tangent * fBorder;
		}

		float fHalfSize = fWidth * 0.5f;

		v3Dest[0] = v3Start + v3Normal * (fHalfSize + outlineOffset);
		v3Dest[1] = v3Start - v3Normal * (fHalfSize - outlineOffset);
		v3Dest[2] = v3Stop + v3Normal * (fHalfSize + outlineOffset);
		v3Dest[3] = v3Stop - v3Normal * (fHalfSize - outlineOffset);
	}

	private void MakeQuad( Vector3[] v3Dest, Vector3[] vSrc )
	{
		int nCount = Mathf.Min( v3Dest.Length, vSrc.Length );
		for( int i=0; i<nCount; ++i )
			v3Dest[i] = vSrc[i];
	}

	private bool TestTriangleFacesOrientation( Vector3[] v3Vertices, int nStartIdx )
	{
		Vector3 v3P0 = v3Vertices[nStartIdx+0];
		Vector3 v3P1 = v3Vertices[nStartIdx+1];
		Vector3 v3P2 = v3Vertices[nStartIdx+2];
		Vector3 v3P3 = v3Vertices[nStartIdx+3];
		float fCrossDot1 = Vector3.Cross( v3P1-v3P0, v3P2-v3P1 ).z;
		float fCrossDot2 = Vector3.Cross( v3P2-v3P1, v3P1-v3P3 ).z;
		if( Mathf.Sign( fCrossDot1 )!=Mathf.Sign( fCrossDot2 ) )
		{
			return true;
		}
		return false;
	}
}
#endif
