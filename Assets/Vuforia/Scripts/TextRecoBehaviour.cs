/*==============================================================================
Copyright (c) 2013-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Protected under copyright and other laws.
==============================================================================*/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Vuforia
{
    /// <summary>
    /// This is the main behaviour class that encapsulates text recognition behaviour.
    /// It just has to be added to a Vuforia-enabled Unity scene and will initialize the text tracker with the configured word list.
    /// Events for newly recognized or lost words will be called on registered ITextRecoEventHandlers
    /// </summary> 
    public class TextRecoBehaviour : TextRecoAbstractBehaviour
    {
    	public Image ROI;
    	private TextTracker textTracker;

	    void initTextTracker(){
	    	textTracker = (TextTracker)TrackerManager.Instance.GetTracker<TextTracker>(); 
			Rect regionOfInterestTracking =  new Rect(Screen.width * 0.1f, Screen.height * 0.1f, 
				                                          Screen.width * 0.8f, Screen.height * 0.4f);
			Rect regionOfInterestDetection = new Rect(Screen.width * 0.2f, Screen.height * 0.2f, 
				                                          Screen.width * 0.6f, Screen.height * 0.2f);
			textTracker.SetRegionOfInterest(regionOfInterestDetection, regionOfInterestTracking); 
	    }

	    void Start(){
            //initTextTracker();           	
	    }
    }
}
