using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FmodAnimationHelper : MonoBehaviour {

	public void PlaySound( string sSoundName )
    {
        FMODUnity.RuntimeManager.PlayOneShot(sSoundName); // "event:/GoodMove");
    }

    public void PlaySoundWithFloatParameter(string sSoundName, string sParamName, float fValue )
    {
        FMOD.Studio.EventInstance eventInstance = FMODUnity.RuntimeManager.CreateInstance(sSoundName);
        FMOD.Studio.ParameterInstance paramInstance;
        if ( eventInstance.getParameter(sParamName, out paramInstance) != FMOD.RESULT.OK )
        {
            Debug.LogError(sParamName + " parameter not found on " + sSoundName + " event ");
            return;
        }
        paramInstance.setValue(fValue);
        eventInstance.start();
        eventInstance.release();
    }
}
