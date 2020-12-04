using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// обновление на стороне клиента
public class SyncServerDown : MonoBehaviour
{
	private Player player;
	private Rigidbody body;
    private TextMesh name;
    private List<Object> tempTransforms;

    // Start is called before the first frame update
    void Start()
    {
        tempTransforms = new List<Object>();
        body = gameObject.GetComponent<Rigidbody>();
        name = gameObject.transform.GetChild(0).gameObject.GetComponent<TextMesh>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(player != null)
        {
            name.text = player.transform.name;

            body.position = Vector3.Lerp(body.position, player.transform.position, 0.02f);
            body.rotation = Quaternion.Lerp(body.rotation, Quaternion.Euler(player.transform.rotation), 0.02f);
        }
    }

    public void SetPlayer(Player _p){ player = _p; }
}
