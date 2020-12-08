using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnClicked : MonoBehaviour
{
    Vector3 position;
    private Gameplay gp;
    private Charecter chr;

    void OnMouseDown()
    {
        position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z); 
        gp.SetActiveMipMap(false);
        chr.MoveTo(position, gameObject.name);   
    }

    // Start is called before the first frame update
    void Start()
    {
        gp = GameObject.Find("Game").GetComponent<Gameplay>();
        chr = gp.player.GetComponent<Charecter>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
