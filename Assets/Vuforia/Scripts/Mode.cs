using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mode : MonoBehaviour {

	public Text mode;

	// Use this for initialization
	public void switchMode () {
		if(mode.text == "Dict"){
			mode.text = "Trans";
		}
		else{
			mode.text = "Dict";
		}
	}

	// Update is called once per frame
	public void Update () {

	}
}
