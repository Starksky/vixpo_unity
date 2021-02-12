using UnityEngine;
using System.Collections;

public class testScript : MonoBehaviour {
    
	void Start () {
        WebCamDevice[] devices = WebCamTexture.devices;
        Debug.Log("Number of web cams connected: " + devices.Length);
        Renderer rend = GetComponent<Renderer>();

        WebCamTexture mycam = new WebCamTexture();
        string camName = devices[1].name;
        Debug.Log("The webcam name is " + camName);
        mycam.deviceName = camName;
        rend.material.mainTexture = mycam;

        mycam.Play();
	}    	
}
