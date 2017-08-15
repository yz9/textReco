/*===============================================================================
Copyright (c) 2016 PTC Inc. All Rights Reserved.
Copyright (c) 2012-2015 Qualcomm Connected Experiences, Inc. All Rights Reserved.
Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;

[System.Serializable]
public class JsonText
{
    public string[] text;
    public string lang;
    public int code;
}

public class XmlText{
  public string fl;
  public int sn;
  public string dt;
  public string pr;
  public string vi;
}

/// <summary>
/// A custom event handler for TextReco-events
/// </summary>
public class TextEvent : MonoBehaviour, ITextRecoEventHandler, IVideoBackgroundEventHandler
{
    #region PRIVATE_MEMBERS
    // Size of text search area in percentage of screen
    private float mLoupeWidth = 0.15f;
    private float mLoupeHeight = 0.08f;

    // Line width of viusalized boxes around detected words
    private float mBBoxLineWidth = 3.0f;
    // Padding between detected words and visualized boxes
    private float mBBoxPadding = 0.0f;
    // Color of visualized boxes around detected words
    private Color32 mBBoxColor = new Color32(178, 163, 181, 1);

    private Rect mDetectionAndTrackingRect;
    private Texture2D mBoundingBoxTexture;
    private Material mBoundingBoxMaterial;

    private bool mIsInitialized;
    private bool mVideoBackgroundChanged;

    private readonly List<WordResult> mSortedWords = new List<WordResult>();


    [SerializeField]
    private Material boundingBoxMaterial = null;
    #endregion //PRIVATE_MEMBERS

    #region PRIVATE_MEMBER_VARIABLES

    private TrackableBehaviour mTrackableBehaviour;
    public Text translate;
    public Text originalText;
    public Text scan;
    public Text buttonText;
    //public UnityEngine.UI.Image output;
    private string myWord = null;
    private string myLang = "zh";
    List<string> values = new List<string>() {"zh", "ja", "fr", "es"};
    public Dropdown dropdownUI;

    JsonText myJson = new JsonText();
    XmlText myXml = new XmlText();
    public bool status;
    #endregion // PRIVATE_MEMBER_VARIABLES


    #region PUBLIC_MEMBERS
    //public Canvas textRecoCanvas;
    #endregion //PUBLIC_MEMBERS


    #region MONOBEHAVIOUR_METHODS
    public void Start()
    {
        // create the texture for bounding boxes
        mBoundingBoxTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        mBoundingBoxTexture.SetPixel(0, 0, mBBoxColor);
        mBoundingBoxTexture.Apply(false);

        mBoundingBoxMaterial = new Material(boundingBoxMaterial);
        mBoundingBoxMaterial.SetTexture("_MainTex", mBoundingBoxTexture);

        // register to TextReco events
        var trBehaviour = GetComponent<TextRecoBehaviour>();
        if (trBehaviour)
        {
            trBehaviour.RegisterTextRecoEventHandler(this);
        }

        // register for the OnVideoBackgroundConfigChanged event at the VuforiaBehaviour
        VuforiaARController.Instance.RegisterVideoBgEventHandler(this);
        translate.enabled = false;

        //StartCoroutine("lookUp");

    }

    void OnRenderObject()
    {
        DrawWordBoundingBoxes();
    }

    IEnumerator lookUp() {

     if(myWord != null){
       if(buttonText.text == "Dict"){
        myLang = values[dropdownUI.value];
        Debug.Log(myLang);
        string url = "https://translate.yandex.net/api/v1.5/tr.json/translate?key=trnsl.1.1.20170712T213228Z.50f66cb053334861.3305946c36e78fabf910df5491ed59c20967ebfd&lang=" + myLang + "&text=" + myWord;
        WWW www = new WWW(url);
        yield
        return www;

        if (www.error == null) {
         string json = www.text;
         //print(json);
         Debug.Log("Translate " + myWord + " ");
         myJson = JsonUtility.FromJson < JsonText > (json);
         setContent(myJson, null, myWord);
        } else {
         //error
        }
       }
      else{
        string learner = "http://www.dictionaryapi.com/api/v1/references/learners/xml/" + myWord + "?key=d8cdbef7-243f-4baa-9579-08c4335fe96d";
        WWW www = new WWW(learner);
        yield
        return www;

        if (www.error == null) {
         Debug.Log("Dictionary");
         string result = www.text;
         print(result);
         var doc = XDocument.Parse(result);
         var entries = doc.Root.Elements("entry");
         foreach(var entry in entries) {
          myXml.pr = (string) entry.Element("pr");
          myXml.fl = (string) entry.Element("fl");
          var defs = entry.Descendants("def");
          foreach(var def in defs) {
           if (def.Element("sn") != null)
            myXml.sn = (int) def.Element("sn");
           var dt = def.Element("dt");
           if (dt.Descendants("vi") != null)
            myXml.vi = (string) def.Element("vi");
            //dt.Descendants("vi").Remove();
           if (dt.Descendants("dx") != null)
            dt.Descendants("dx").Remove();
           myXml.dt = (string) dt;
           //Debug.Log("fl: " + myXml.fl + "sn: " + myXml.sn + "dt: " + myXml.dt);
           break;
          }
          break;
         }
         setContent(null, myXml, myWord);
        } //end if
        else {
         //error
        }
       }

     }



    }

    public void setContent(JsonText json, XmlText xml, string word) {
     originalText.text = word;
     if (buttonText.text == "Dict") {
      if (json != null) {
       translate.text = json.text[0];
       Debug.Log("Translate: (" + json.lang + ") " + json.text[0]);
       //  break;
      }
     } else {
      if (xml != null) {
       translate.text = "<b>" + word + "</b> [" + xml.pr + "] " + xml.fl + "\n <size=22>"
                        + xml.sn + " " + xml.dt + "\n <i>" + xml.vi + "</i></size>" ;
       Debug.Log("Dict");
       //break;
      }
     }
    }

    public void setActive(bool active) {
     originalText.enabled = active;
     translate.enabled = active;
     scan.enabled = !active;
     if (!active) {
      originalText.text = "";
      translate.text = "";
     }
     //output.enabled = active;
    }

    void Update()
    {
        if (mIsInitialized)
        {
            // Once the text tracker has initialized and every time the video background changed,
            // set the region of interest
            if (mVideoBackgroundChanged)
            {
                TextTracker textTracker = TrackerManager.Instance.GetTracker<TextTracker>();
                if (textTracker != null)
                {
                    CalculateLoupeRegion();
                    textTracker.SetRegionOfInterest(mDetectionAndTrackingRect, mDetectionAndTrackingRect);
                }
                mVideoBackgroundChanged = false;
            }

        }
        //if found
        if(status && myWord != null){
          StartCoroutine("lookUp");
          setActive(true);
        }else{
          StopCoroutine("lookUp");
          setActive(false);
        }

    }
    #endregion //MONOBEHAVIOUR_METHODS


    #region ITextRecoEventHandler_IMPLEMENTATION
    /// <summary>
    /// Called when text reco has finished initializing
    /// </summary>
    public void OnInitialized()
    {
        CalculateLoupeRegion();
        mIsInitialized = true;
    }

    /// <summary>
    /// This method is called whenever a new word has been detected
    /// </summary>
    /// <param name="wordResult">New trackable with current pose</param>
    public void OnWordDetected(WordResult wordResult)
    {
        var word = wordResult.Word;
        if (ContainsWord(word))
            Debug.LogError("Word was already detected before!");


        Debug.Log("Text: New word: " + wordResult.Word.StringValue + "(" + wordResult.Word.ID + ")");
        AddWord(wordResult);
        status = true;
        myWord = wordResult.Word.StringValue.ToLower();
        Debug.Log("myword is " + myWord + ".");
    }

    /// <summary>
    /// This method is called whenever a tracked word has been lost and is not tracked anymore
    /// </summary>
    public void OnWordLost(Word word)
    {
        if (!ContainsWord(word))
            Debug.LogError("Non-existing word was lost!");

        Debug.Log("Text: Lost word: " + word.StringValue + "(" + word.ID + ")");

        RemoveWord(word);
        status = false;
        myWord = null;
    }
    #endregion //PUBLIC_METHODS


    #region IVideoBackgroundEventHandler_IMPLEMENTATION
    // set a flag that the video background has changed. This means the region of interest has to be set again.
    public void OnVideoBackgroundConfigChanged()
    {
        mVideoBackgroundChanged = true;
    }
    #endregion // IVideoBackgroundEventHandler_IMPLEMENTATION


    #region PRIVATE_METHODS
    /// <summary>
    /// Draw a 3d bounding box around each currently tracked word
    /// </summary>
    private void DrawWordBoundingBoxes()
    {
        // render a quad around each currently tracked word
        foreach (var word in mSortedWords)
        {
            var pos = word.Position;
            var orientation = word.Orientation;
            var size = word.Word.Size;
            var pose = Matrix4x4.TRS(pos, orientation, new Vector3(size.x, 1, size.y));

            var cornersObject = new[]
                {
                    new Vector3(-0.5f, 0.0f, -0.5f), new Vector3(0.5f, 0.0f, -0.5f),
                    new Vector3(0.5f, 0.0f, 0.5f), new Vector3(-0.5f, 0.0f, 0.5f)
                };
            var corners = new Vector2[cornersObject.Length];
            for (int i = 0; i < cornersObject.Length; i++)
                corners[i] = Camera.current.WorldToScreenPoint(pose.MultiplyPoint(cornersObject[i]));

            DrawBoundingBox(corners);
        }
    }

    private void DrawBoundingBox(Vector2[] corners)
    {
        var normals = new Vector2[4];
        for (var i = 0; i < 4; i++)
        {
            var p0 = corners[i];
            var p1 = corners[(i + 1)%4];
            normals[i] = (p1 - p0).normalized;
            normals[i] = new Vector2(normals[i].y, -normals[i].x);
        }

        //add padding to inner corners
        corners = ExtendCorners(corners, normals, mBBoxPadding);
        //computer outer corners
        var outerCorners = ExtendCorners(corners, normals, mBBoxLineWidth);

        //create vertices in screen space
        var vertices = new Vector3[8];
        float depth = 1.02f * Camera.current.nearClipPlane;
        for (var i = 0; i < 4; i++)
        {
            vertices[i] = new Vector3(corners[i].x, corners[i].y, depth);
            vertices[i + 4] = new Vector3(outerCorners[i].x, outerCorners[i].y, depth);
        }

        //transform vertices into world space
        for (int i = 0; i < 8; i++)
            vertices[i] = Camera.current.ScreenToWorldPoint(vertices[i]);

        var mesh = new Mesh()
            {
                vertices = vertices,
                uv = new Vector2[8],
                triangles = new[]
                    {
                        0, 5, 4, 1, 5, 0,
                        1, 6, 5, 2, 6, 1,
                        2, 7, 6, 3, 7, 2,
                        3, 4, 7, 0, 4, 3
                    },
            };

        mBoundingBoxMaterial.SetPass(0);
        Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
        Destroy(mesh);
    }

    private static Vector2[] ExtendCorners(Vector2[] corners, Vector2[] normals, float extension)
    {
        //compute positions along the outer side of the boundary
        var linePoints = new Vector2[corners.Length * 2];
        for (var i = 0; i < corners.Length; i++)
        {
            var p0 = corners[i];
            var p1 = corners[(i + 1) % 4];

            var po0 = p0 + normals[i] * extension;
            var po1 = p1 + normals[i] * extension;
            linePoints[i * 2] = po0;
            linePoints[i * 2 + 1] = po1;
        }

        //compute corners of outer side of bounding box lines
        var outerCorners = new Vector2[corners.Length];
        for (var i = 0; i < corners.Length; i++)
        {
            var i2 = i * 2;
            outerCorners[(i + 1) % 4] = IntersectLines(linePoints[i2], linePoints[i2 + 1], linePoints[(i2 + 2) % 8],
                                             linePoints[(i2 + 3) % 8]);
        }
        return outerCorners;
    }

    /// <summary>
    /// Intersect the line p1-p2 with the line p3-p4
    /// </summary>
    private static Vector2 IntersectLines(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        var denom = (p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x);
        var x = ((p1.x * p2.y - p1.y * p2.x) * (p3.x - p4.x) - (p1.x - p2.x) * (p3.x * p4.y - p3.y * p4.x)) / denom;
        var y = ((p1.x * p2.y - p1.y * p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x * p4.y - p3.y * p4.x)) / denom;
        return new Vector2(x, y);
    }

    private void AddWord(WordResult wordResult)
    {
        //add new word into sorted list
        var cmp = new ObbComparison();
        int i = 0;
        while (i < mSortedWords.Count && cmp.Compare(mSortedWords[i], wordResult) < 0)
        {
            i++;
        }

        if (i < mSortedWords.Count)
        {
            mSortedWords.Insert(i, wordResult);
        }
        else
        {
            mSortedWords.Add(wordResult);
        }
    }

    private void RemoveWord(Word word)
    {
        for (int i = 0; i < mSortedWords.Count; i++)
        {
            if (mSortedWords[i].Word.ID == word.ID)
            {
                mSortedWords.RemoveAt(i);
                break;
            }
        }
    }

    private bool ContainsWord(Word word)
    {
        foreach (var w in mSortedWords)
            if (w.Word.ID == word.ID)
                return true;
        return false;
    }

    private void CalculateLoupeRegion()
    {
        // define area for text search
        var loupeWidth = mLoupeWidth * Screen.width;
        var loupeHeight = mLoupeHeight * Screen.height;
        var leftOffset = Screen.width / 2.0f - loupeWidth / 2.0f;
        var topOffset = Screen.height / 2.0f - loupeHeight / 2.0f;
        mDetectionAndTrackingRect = new Rect(leftOffset, topOffset, loupeWidth, loupeHeight);
				//Debug.Log(mDetectionAndTrackingRect);
		}
    #endregion //PRIVATE_METHODS
}
