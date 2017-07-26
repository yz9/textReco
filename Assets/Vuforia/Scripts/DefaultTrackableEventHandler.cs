/*==============================================================================
Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Protected under copyright and other laws.
==============================================================================*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using System.Xml.Serialization;
namespace Vuforia
{
    [System.Serializable]
    public class JsonText
    {
        public string[] text;
        public string lang;
        public int code;
    }

    /// <summary>
    /// A custom handler that implements the ITrackableEventHandler interface.
    /// </summary>
    public class DefaultTrackableEventHandler : MonoBehaviour,
                                                ITrackableEventHandler
    {
        #region PRIVATE_MEMBER_VARIABLES

        private TrackableBehaviour mTrackableBehaviour;
        public Text translate;
        public Text originalText;
        public Text scan;
        public Text buttonText;
        //public UnityEngine.UI.Image output;

        JsonText myObject = new JsonText();
        public bool status;


        #endregion // PRIVATE_MEMBER_VARIABLES



        #region UNTIY_MONOBEHAVIOUR_METHODS

        void Start()
        {
            mTrackableBehaviour = GetComponent<TrackableBehaviour>();
            if (mTrackableBehaviour)
            {
                mTrackableBehaviour.RegisterTrackableEventHandler(this);
            }
            translate.enabled = false;

        }

        #endregion // UNTIY_MONOBEHAVIOUR_METHODS



        #region PUBLIC_METHODS

        /// <summary>
        /// Implementation of the ITrackableEventHandler function called when the
        /// tracking state changes.
        /// </summary>
        public void OnTrackableStateChanged(
                                        TrackableBehaviour.Status previousStatus,
                                        TrackableBehaviour.Status newStatus)
        {
            if (newStatus == TrackableBehaviour.Status.DETECTED ||
                newStatus == TrackableBehaviour.Status.TRACKED ||
                newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
            {
                OnTrackingFound();
            }
            else
            {
                OnTrackingLost();
            }
        }

        #endregion // PUBLIC_METHODS


        IEnumerator Translate(string word){
            string getLangs = "https://https://dictionary.yandex.net/api/v1/dicservice.json/getLangs?key=dict.1.1.20170712T213201Z.011f10ef04c6cd2b.9429d4487d86ea0269e4039b4776af5d480b71e1&lang=fr&text" + word;
            string lookUp = "https://dictionary.yandex.net/api/v1/dicservice.json/lookup?key=dict.1.1.20170712T213201Z.011f10ef04c6cd2b.9429d4487d86ea0269e4039b4776af5d480b71e1&lang=en-fr&text=" + word;

            string url = "https://translate.yandex.net/api/v1.5/tr.json/translate?key=trnsl.1.1.20170712T213228Z.50f66cb053334861.3305946c36e78fabf910df5491ed59c20967ebfd&lang=zh&text=" + word;
            WWW www = new WWW(url);
            yield return www;

            string json = www.text;
            print(json);
            Debug.Log("Translate " + word + " ");
            myObject = JsonUtility.FromJson<JsonText>(json);
            setContent(myObject, word);

        }

        IEnumerator Dictionary(string word){
          string thesaurus = "http://www.dictionaryapi.com/api/v1/references/thesaurus/xml/" + word.ToLower() + "?key=5768a820-ba45-43c6-ac2b-4a04ee6d8527";

          string learner = "http://www.dictionaryapi.com/api/v1/references/learners/xml/apple?key=d8cdbef7-243f-4baa-9579-08c4335fe96d";
          WWW www = new WWW(thesaurus);
          yield return www;

          Debug.Log("Dictionary");
          string result = www.text;
          print(result);
        }

        public void setContent(JsonText json, string word){
          if(buttonText.text == "Dict"){
            originalText.text = word;
            translate.text = json.text[0];
            Debug.Log("Translate: (" + json.lang + ") " + json.text[0]);
          }
          else{
            translate.text = "dict hello";
            Debug.Log("Dict");
          }
        }

        public void setActive(bool active){
            originalText.enabled = active;
            translate.enabled = active;
            scan.enabled = !active;
            if(!active){
              originalText.text = "";
              translate.text = "";
            }
            //output.enabled = active;
        }

        public void Update(){
          //if button is pressed
          if(status){
            if(buttonText.text == "Trans"){
              StartCoroutine("Dictionary", mTrackableBehaviour.TrackableName);
            }
            else{
              StartCoroutine("Translate", mTrackableBehaviour.TrackableName.ToLower());
            }
            setActive(true);
          }else{
            setActive(false);
          }
        }


        #region PRIVATE_METHODS


        private void OnTrackingFound()
        {
            Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
            Collider[] colliderComponents = GetComponentsInChildren<Collider>(true);

            // Enable rendering:
            foreach (Renderer component in rendererComponents)
            {
                component.enabled = true;
            }

            // Enable colliders:
            foreach (Collider component in colliderComponents)
            {
                component.enabled = true;
            }

            Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " found");

            status = true;

        }

        private void OnTrackingLost()
        {
            Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
            Collider[] colliderComponents = GetComponentsInChildren<Collider>(true);

            // Disable rendering:
            foreach (Renderer component in rendererComponents)
            {
                component.enabled = false;
            }

            // Disable colliders:
            foreach (Collider component in colliderComponents)
            {
                component.enabled = false;
            }

            Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " lost");
            status = false;
        }

        #endregion // PRIVATE_METHODS
    }
}
