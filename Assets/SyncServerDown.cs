using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncServerDown : MonoBehaviour
{
	private Player player;
	private Rigidbody body;

    // Start is called before the first frame update
    void Start()
    {
        body = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(player != null)
        {
        	body.position = player.transform.position;
    		body.rotation = Quaternion.Euler(player.transform.rotation);
        }
    }

    public void SetPlayer(Player _p){ player = _p; }
}
