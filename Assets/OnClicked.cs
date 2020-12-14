using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnClicked : MonoBehaviour
{
    Vector3 position;
    private Gameplay gp;
    private Charecter chr;
    private Game game;
    
    void OnMouseDown()
    {
        game = GameObject.Find("Game").GetComponent<Game>();

        position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z); 
        gp.SetActiveMipMap(false);
        if(PlayerPrefs.GetInt("Server") != 0)
            chr.MoveTo(position, gameObject.name);   
        else
        {
            string request = "{\"msgid\":10002, \"name\":\""+gameObject.name+"\"}";
            game.Send(request);
        }
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
