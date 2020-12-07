using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charecter : MonoBehaviour
{
	public float FlySpeed = 1f;
	public GameObject camera;

	private Vector2 targetDirection;
	public Vector2 clampInDegrees = new Vector2(360, 180);
	public float sensitivity = 10f;
    public float maxYAngle = 80f;
    private Vector3 currentRotation;
    private bool isPause = false;
    
    private Game game;
    private Rigidbody body;
	private SyncServerDown syncPlayer;

    private void SendForce(Vector3 force)
    {
		if(game.isInit)
        {
            game.playerSync.transform.force = force;
        	string json = JsonUtility.ToJson(game.playerSync);
        	string request = "{\"msgid\":10003, \"client\":"+json+"}";
			syncPlayer.AddTempTransform(game.playerSync.transform);
			game.Send(request);	
		}   
    }

	// Start is called before the first frame update
	void Start()
    {
        currentRotation = transform.eulerAngles;

        game = GameObject.Find("Game").gameObject.GetComponent<Game>();
        body = GetComponent<Rigidbody>();
		syncPlayer = GetComponent<SyncServerDown>();
	}
	void Update()
	{

	}
    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.M))
            isPause = !isPause;

        if(!isPause)
        {

        	Vector3 force = Vector3.zero;

			if (Input.GetAxis("Vertical") != 0)
		    {
				body.velocity = Vector3.zero;
				Vector3 direction = camera.transform.TransformDirection(Vector3.forward);
				force += direction.normalized * Input.GetAxis("Vertical") * FlySpeed;
				body.AddForce(direction.normalized * Input.GetAxis("Vertical") * FlySpeed, ForceMode.Impulse);
			}
		   
		   
		    if (Input.GetAxis("Horizontal") != 0)
		    {
				body.velocity = Vector3.zero;
				Vector3 direction = camera.transform.TransformDirection(Vector3.right);
				force += direction.normalized * Input.GetAxis("Horizontal") * FlySpeed;
				body.AddForce(direction.normalized * Input.GetAxis("Horizontal") * FlySpeed, ForceMode.Impulse);
			}

			if(force != Vector3.zero)
				SendForce(force);

			if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
				body.velocity = Vector3.zero;

			//ensure these stay this way
			Cursor.lockState = CursorLockMode.Locked;
	        Cursor.visible = false;

	 

	        currentRotation.x += Input.GetAxis("Mouse X");
	        currentRotation.y -= Input.GetAxis("Mouse Y");
	        currentRotation.x = Mathf.Repeat(currentRotation.x, 360);
	        currentRotation.y = Mathf.Clamp(currentRotation.y, -maxYAngle, maxYAngle);
			camera.transform.rotation = Quaternion.Euler(currentRotation.y,currentRotation.x,0); 
        }
        else
        {
			if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
				body.velocity = Vector3.zero;

			
        	Cursor.lockState = CursorLockMode.None;
	        Cursor.visible = true;
        }

    }
}
