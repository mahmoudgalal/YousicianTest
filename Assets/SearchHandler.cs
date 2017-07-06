using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Experimental.Networking;
using UnityEngine.EventSystems;
//using JSON;
using SimpleJson;
/**
 * Simple dynamic listview to retreive json items from remote server.
 * 
 * Created by: Mahmoud Galal
 * */
public class SearchHandler : MonoBehaviour, IEndDragHandler
{
	public GameObject itemPrefab;
	public RectTransform scrollableContent;
    public RectTransform loadProgress;  
	public Text searchtext;
    public Text debugText;
	private const string SERVER_URL = "https://external.api.yle.fi/v1/programs/items.json";
	private const string APP_ID = "5be23877";
	private const string APP_KEY = "5ef0923e33ef49dbecd6b73699845462";
	private const string LIMIT_KEYWORD = "limit";
	private const string OFFSET_KEYWORD = "offset";
	private const string LANG_KEYWORD = "language";
	private const string SECRET_KEY = "34d88ba3004b5480";
	private const int PAGE_SIZE = 10;
	private const float SCROLL_THRESHOLD = 0.06f;
	private int pageIndex = 0;
	private bool requestFinished = true;
    private int angle = 0;
    private int items = 0;	   
	private string lastKeyword ="";
	// Use this for initialization
	void Start () {
        loadProgress.gameObject.SetActive(false);       
	}
	/**
	 * Called when text changes
	 */
	public void onValueChanged(string value){
		Debug.Log ("New value added:"+value);
		Debug.Log ("hellow ");
		lastKeyword = value.Trim ();
		if (lastKeyword.Length > 0) {
			if (requestFinished) {
				pageIndex = 0;
				items = 0;
				StartCoroutine (loadPage (pageIndex, lastKeyword)); 
			}
		} else {
			clearView(scrollableContent.gameObject.transform);
		}			
	}
    //Do this when the user stops dragging this UI Element.
    public void OnEndDrag(PointerEventData data)
    {
        Debug.Log("Stopped dragging " + this.name + "!");
    }
	/**
	 * Called when the user scrolls the list
	 * */
    public void onScrollPositionChanged(Vector2 pos){
        Debug.Log("Scroll Changed to:" + pos.ToString());
		if ((pos.y < SCROLL_THRESHOLD && pos.y > 0.0f) && requestFinished)
        {
			if(lastKeyword.Length>0){
				//StopCoroutine("loadPage");
            	StartCoroutine(loadPage(pageIndex++,lastKeyword));
			}
        }
    }
	/**
	 * A coroutin to download json data 
	 * */
	private IEnumerator loadPage(int page,string keyword){
		requestFinished = false;
        loadProgress.gameObject.SetActive(true);
		string nextUrl = SERVER_URL+"?"+"app_id=" + APP_ID + "&app_key=" + APP_KEY +"&"+LANG_KEYWORD+"=fi"+
			"&q="+keyword+"&"+LIMIT_KEYWORD+"="+PAGE_SIZE+"&"+OFFSET_KEYWORD+"="+(items);
		Debug.Log ("Loading URL:"+nextUrl +" for Keyword:"+keyword);
		UnityWebRequest www = new UnityWebRequest(nextUrl);
		www.downloadHandler = new DownloadHandlerBuffer ();
		yield return www.Send ();      
		requestFinished = true;
       
        loadProgress.gameObject.SetActive(false);
        if (page == 0)
        {            
			clearView(scrollableContent.gameObject.transform);
			items = 0;
        }
		if (www.isError) {
			Debug.Log ("Error: " + www.error);
			clearView(scrollableContent.gameObject.transform);
            GameObject newItem = Instantiate<GameObject>(itemPrefab);
            Text titleObject = newItem.GetComponentInChildren<Text>();
            titleObject.text = www.error;
            newItem.transform.SetParent(scrollableContent, false); 
			debugText.text = "No Items:1" ;
			items = 0;
			pageIndex = 0;
		} else {
			if(lastKeyword.Length != 0){
				//Text changed while downloading...
				if(!lastKeyword.Equals(keyword)){
					pageIndex = 0;
					items = 0;
					yield return StartCoroutine (loadPage (pageIndex, lastKeyword)); 
				}else{
				pageIndex++;
				string json = www.downloadHandler.text;
				Debug.Log ("Received data:" + json);

				var N = JSON.JSONObject.Parse(json);

				int itemCount = N["data"].Array.Length;
				Debug.Log("Found:"+itemCount+" Items");
				items+=itemCount;
				debugText.text = "No Items:" + (items);
				for(int i = 0;i<itemCount;i++){
						GameObject newItem = Instantiate<GameObject> (itemPrefab);
		                Text titleObject = newItem.GetComponentInChildren<Text>();
						//Finish title of the program
						string titleTxt = N["data"].Array[i].Obj.GetObject("title").GetString("fi");
						if( titleTxt.Trim().Length == 0 )
							titleTxt ="N/A";
						titleObject.text =  titleTxt;    
						Debug.Log("Item Title:"+titleTxt);
						newItem.transform.SetParent (scrollableContent,false);
					}
				}
			}else{
				items = 0;
				pageIndex = 0 ;
				debugText.text = "No Items:"+items;
			}
		}        

	}
	/**
	 * Removes all items in the supplied transform
	 */
	void clearView(Transform src){
		foreach (Transform t in src)
		{
			Destroy(t.gameObject);
			Destroy(t);
		}
	}
	// Update is called once per frame
	void Update () {
        angle++;
        angle %= 360;
        loadProgress.Rotate(0.0f, 0.0f, angle);
	}
}
