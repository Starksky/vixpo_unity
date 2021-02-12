using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// обновление на стороне клиента
public class SyncServerDown : MonoBehaviour
{
    public float speed = 0.1f;
    private Player player;
	private Rigidbody body;
    private TextMesh name;
    private long currentPack = 0;

    // Start is called before the first frame update
    void Start()
    {
        body = gameObject.GetComponent<Rigidbody>();
        name = gameObject.transform.GetChild(0).gameObject.GetComponent<TextMesh>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(player != null)
        {
            name.text = player.transform.name;
            
            if (currentPack >= player.transform.pack) return;

            body.position = Vector3.Lerp(body.position, player.transform.position, speed);
            body.rotation = Quaternion.Lerp(body.rotation, Quaternion.Euler(player.transform.rotation), speed);
            currentPack = 0;
        }
    }

    public void SetPlayer(Player _p){ player = _p; }
    public void AddTempTransform(Object transform) {
        transform.pack = ++currentPack; 
    }
}
