using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class languageSelection : MonoBehaviour {

	public Dropdown dropdown;
	List<string> options = new List<string>() {"EN - ZH", "EN - JA", "EN - FR", "EN - ES"};
	List<string> values = new List<string>() {"zh", "ja", "fr", "es"};
	void Start(){
		dropdown.AddOptions(options);
	}

	public void value(int index){
		//Debug.Log(values[index]);
	}

}
