using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FmodAnimationHelper : MonoBehaviour {

	public void PlaySound( string sSoundName )
    {
        Debug.LogError("FMOD is no more in shared, please copy this script in your project");
        //FMODUnity.RuntimeManager.PlayOneShot(sSoundName); // "event:/GoodMove");
    }

    public void PlaySoundWithFloatParameter(string sSoundName, string sParamName, float fValue )
    {
        Debug.LogError("FMOD is no more in shared, please copy this script in your project");
/*
        FMOD.Studio.EventInstance eventInstance = FMODUnity.RuntimeManager.CreateInstance(sSoundName);
        FMOD.Studio.ParameterInstance paramInstance;
        if ( eventInstance.getParameter(sParamName, out paramInstance) != FMOD.RESULT.OK )
        {
            Debug.LogError(sParamName + " parameter not found on " + sSoundName + " event ");
            return;
        }
        paramInstance.setValue(fValue);
        eventInstance.start();
        eventInstance.release();*/
    }
}
