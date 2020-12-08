using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gameplay : MonoBehaviour
{
	public GameObject Map;
	public GameObject MinMap;
	private bool isMinMap = false;
	private bool isPause = false;
	private GameObject menu;
	
	public GameObject player;
	

    // Start is called before the first frame update
    void Start()
    {
        menu = GameObject.Find("Menu").gameObject;
        
    }

    // Update is called once per frame
    void Update()
    {
    	if (Input.GetKeyDown(KeyCode.Escape) && !isMinMap)
    	{
            isPause = !isPause;
            isMinMap = false;
    	}

        if(Input.GetKeyDown(KeyCode.M) && !isPause)
        	isMinMap = !isMinMap;

        if(isMinMap)
        {
        	Map.SetActive(false);
        	MinMap.SetActive(true);

        	Cursor.lockState = CursorLockMode.None;
	        Cursor.visible = true;
        }
        else
        {
        	Map.SetActive(true);
        	MinMap.SetActive(false);
        }

        menu.SetActive(isPause);

    }

    public void SetActiveMipMap(bool b)
    {
    	isMinMap = b;
    }

}
