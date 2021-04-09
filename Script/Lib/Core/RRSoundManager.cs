using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RRSoundManager : lwSingletonMonoBehaviour<RRSoundManager>
{
    public enum SoundType
    {
        Amb = 0,
        Sfx
    }

    public delegate float GetSoundTypeVolumeDlg(SoundType soundType);

    public float EVENT_AMB_VOLUME = 0.3f;
    public class AmbianceData
    {
        public AudioSource m_source;
        public Coroutine m_fadeRoutine;
        public float m_fCurrentVolume;
    }

    [SerializeField]
    protected float m_ambianceFadeDuration = 0.5f;
    [SerializeField]
    protected int m_sfXSourceMax = 8;

    public GetSoundTypeVolumeDlg getSoundTypeVolumeDlg { get; set; }

    protected AudioSource[] m_sfxSources;
    protected Dictionary<string, AmbianceData> m_ambianceDico;

    private Dictionary<string, int> m_persistentSound;


    public int PlaySound(string sFileName)
    {
        //Debug.Log("PlaySound " + sFileName);
        int index = GetFreeAudioSourceIndex();
        if (index >= 0)
        {
            AudioSource audioSource = m_sfxSources[index];
            audioSource.clip = Resources.Load<AudioClip>("Audios/" + sFileName);
            audioSource.volume = GetVolume( SoundType.Sfx);
            m_sfxSources[index].loop = false;
            audioSource.Play();
        }
        return index;
    }



    public void UpdateAmbVolume(float previousVolume, float newVolume)
    {
        foreach (KeyValuePair<string, AmbianceData> pair in m_ambianceDico)
        {
            pair.Value.m_source.volume = pair.Value.m_fCurrentVolume * newVolume;
        }
    }

    public void PlayPersistentSound(string sSoundName, string sPath="")
    {
        //Debug.Log("PlayPersistentSoundStatic : " + sSoundName);
        if (m_persistentSound == null)
        {
            m_persistentSound = new Dictionary<string, int>();
        }

        int sound;
        if (m_persistentSound.TryGetValue(sSoundName, out sound))
        {
            return;
        }

        if( string.IsNullOrEmpty( sPath) )
        {
            sPath = sSoundName;
        }
        sound = PlaySound(sPath);
        if( sound >= 0)
        {
            m_sfxSources[sound].loop = true;
            m_persistentSound.Add(sSoundName, sound);
        }
    }

    public void StopPersistentSound(string sSoundName)
    {
        //Debug.Log("StopPersistentSoundStatic : " + sSoundName);

        if (m_persistentSound == null)
        {
            return;
        }

        int sound;
        if (m_persistentSound.TryGetValue(sSoundName, out sound))
        {
            m_sfxSources[sound].Stop();
            m_sfxSources[sound].loop = false;
            m_persistentSound.Remove(sSoundName);
        }
    }

    public void PausePersistentSound(string sSoundName)
    {
        //Debug.Log("StopPersistentSoundStatic : " + sSoundName);

        if (m_persistentSound == null)
        {
            return;
        }

        int sound;
        if (m_persistentSound.TryGetValue(sSoundName, out sound))
        {
            m_sfxSources[sound].Pause();
        }
    }

    public void ResumePersistentSound(string sSoundName)
    {
        //Debug.Log("StopPersistentSoundStatic : " + sSoundName);

        if (m_persistentSound == null)
        {
            return;
        }

        int sound;
        if (m_persistentSound.TryGetValue(sSoundName, out sound))
        {
            m_sfxSources[sound].UnPause();
        }
    }

    protected void StartAmbiance(AmbianceData ambianceData)
    {
        if (ambianceData.m_fadeRoutine != null)
        {
            StopCoroutine(ambianceData.m_fadeRoutine);
        }
        ambianceData.m_fadeRoutine = StartCoroutine(FadeRoutine(ambianceData, 0f, 1f, m_ambianceFadeDuration));
    }

    protected void StopAmbiance(AmbianceData ambianceData)
    {
        if (ambianceData.m_fadeRoutine != null)
        {
            StopCoroutine(ambianceData.m_fadeRoutine);
        }
        ambianceData.m_fadeRoutine = StartCoroutine(FadeRoutine(ambianceData, 1f, 0f, m_ambianceFadeDuration));
    }

    protected IEnumerator FadeRoutine(AmbianceData ambianceData, float start, float end, float duration)
    {
        float fAmbVolume = GetVolume(SoundType.Amb);
        if (!ambianceData.m_source.isPlaying) // start == 0 )
        {
            ambianceData.m_source.Play();
        }
        ambianceData.m_source.volume = start * fAmbVolume;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            ambianceData.m_fCurrentVolume = Mathf.Lerp(start, end, Mathf.Clamp01(elapsedTime / duration));
            ambianceData.m_source.volume = ambianceData.m_fCurrentVolume * fAmbVolume;
        }
    }

    protected virtual void Awake()
    {
        m_sfxSources = new AudioSource[m_sfXSourceMax];
        m_ambianceDico = new Dictionary<string, AmbianceData>();
    }


    protected int GetFreeAudioSourceIndex()
    {
        bool found = false;
        int index = 0;

        while (!found && index < m_sfxSources.Length)
        {
            if (m_sfxSources[index] == null || ( !m_sfxSources[index].isPlaying && !m_sfxSources[index].loop))
            {
                found = true;
            }
            else
            {
                index++;
            }
        }

        if (found)
        {
            if (m_sfxSources[index] == null)
            {
                m_sfxSources[index] = gameObject.AddComponent<AudioSource>();
            }
            return index;
        }
        return -1;
    }

    private float GetVolume( SoundType soundType )
    {
        if( getSoundTypeVolumeDlg!=null )
        {
            return getSoundTypeVolumeDlg(soundType);
        }
        return 1f;
    }
}
