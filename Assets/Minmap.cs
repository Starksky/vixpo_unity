using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minmap : MonoBehaviour
{
	public GameObject player;
	public GameObject pointPlayer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    	Vector3 position = pointPlayer.transform.localPosition;
    	position = new Vector3(player.transform.localPosition.x, position.y, player.transform.localPosition.z);
        pointPlayer.transform.localPosition = position;
    }
}
