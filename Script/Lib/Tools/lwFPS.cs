using UnityEngine;

public sealed class lwFPS : MonoBehaviour 
{
	public float fps; // Current FPS
	public Rect m_rectGui = new Rect(5,5,100,20);
	public GUISkin m_skin;
	public float refreshDelay = 1f;

#if !UNITY_FLASH
	public float smoothFps; // Smoothed FPS
	public float minFps; // min FPS
	public float maxFps; // max FPS

	private const int LAST_FPS_COUNT = 50;	
	
	private int m_nLastFpsNum;
	private float[] m_fLastFpsArray;
	private float m_fCummulFps;
	private System.Diagnostics.Stopwatch m_timer;
	private long m_nLastTick;
#else
	public int UpdateInterval = 1;
	
	private float m_fLastInterval; // Last interval end time
	private int m_nFrames; // Frames over current interval
#endif
	private float m_fLastRefreshTime = 0f;
	private string m_sTextToShow = string.Empty;

	void Start()
	{
#if !UNITY_FLASH
		m_fLastFpsArray = new float[LAST_FPS_COUNT];
		m_fCummulFps = 0f;
		minFps = float.MaxValue;
		maxFps = float.MinValue;
		m_timer = new System.Diagnostics.Stopwatch();
		m_timer.Start();
#else		
		m_fLastInterval = Time.realtimeSinceStartup;
	    m_nFrames = 0;
#endif
	}

	void Update()
	{
#if !UNITY_FLASH
		long nCurrentTick = m_timer.ElapsedTicks;

		fps = (float)( (double)System.Diagnostics.Stopwatch.Frequency / (double)( nCurrentTick-m_nLastTick ) );
		m_nLastTick = nCurrentTick;
		
		m_fCummulFps += fps - m_fLastFpsArray[m_nLastFpsNum];
		
		smoothFps = m_fCummulFps / LAST_FPS_COUNT;
		
		m_fLastFpsArray[m_nLastFpsNum] = fps;
		m_nLastFpsNum++;
		if( m_nLastFpsNum==LAST_FPS_COUNT ) m_nLastFpsNum = 0;
		
		if( minFps>fps ) minFps = fps;
		if( maxFps<fps ) maxFps = fps;
#else
		m_nFrames++;
	    float fTimeNow = Time.realtimeSinceStartup;
	    if( fTimeNow>m_fLastInterval+UpdateInterval )
	    {
	        fps = m_nFrames / ( fTimeNow-m_fLastInterval );
	        m_nFrames = 0;
	        m_fLastInterval = fTimeNow;
	    }
#endif
		//Debug.Log( "fps " + fps + " smoothFps " + smoothFps + " minFps " + minFps + " maxFps " + maxFps );
	}
	
	public void ClearStats()
	{
#if !UNITY_FLASH
		minFps = float.MaxValue;
		maxFps = float.MinValue;
#endif
	}

	void OnGUI()
	{
		float fTime = Time.realtimeSinceStartup;
		if( m_fLastRefreshTime==0 || fTime - m_fLastRefreshTime>refreshDelay )
		{	
#if !UNITY_FLASH
			m_sTextToShow = string.Format( "i{0:f2} - a{1:f2}", fps, smoothFps );
#else
			m_sTextToShow = fps.ToString( "f2" );
#endif
			m_fLastRefreshTime = fTime;
		}
		if( m_skin!=null ) GUI.skin = m_skin;
		GUI.Label( m_rectGui, m_sTextToShow, GUI.skin.box );
	}
}
