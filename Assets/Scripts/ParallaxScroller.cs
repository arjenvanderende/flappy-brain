using UnityEngine;
using System.Collections;

public class ParallaxScroller : MonoBehaviour {

	[Range(0, 2)]
	public float scrollSpeed;

	// Use this for initialization
	void Start () {	
	}
	
	// Update is called once per frame
	void Update () {
		float offsetX = Mathf.Repeat (Time.time * scrollSpeed, 1);
		renderer.sharedMaterial.SetTextureOffset("_MainTex", new Vector2 (offsetX, 0));
	}
}
