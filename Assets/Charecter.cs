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
    private GameObject Menu;
    
	// Start is called before the first frame update
	void Start()
    {
        currentRotation = transform.eulerAngles;
        Menu = GameObject.Find("Menu").gameObject;
	}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            isPause = !isPause;

        if(!isPause)
        {
        	Menu.SetActive(false);

			if (Input.GetAxis("Vertical") != 0)
		    {
				//camera.transform.Translate(Vector3.forward * FlySpeed * Input.GetAxis("Vertical"));
				//transform.Translate(Vector3.forward * FlySpeed * Input.GetAxis("Vertical"));
				GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
				Vector3 direction = camera.transform.TransformDirection(Vector3.forward);
				GetComponent<Rigidbody>().AddForce(direction.normalized * Input.GetAxis("Vertical") * FlySpeed, ForceMode.Impulse);
			}
		   
		   
		    if (Input.GetAxis("Horizontal") != 0)
		    {
				//camera.transform.Translate(Vector3.right * FlySpeed * Input.GetAxis("Horizontal"));
				//transform.Translate(Vector3.right * FlySpeed * Input.GetAxis("Horizontal"));
				GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
				Vector3 direction = camera.transform.TransformDirection(Vector3.right);
				GetComponent<Rigidbody>().AddForce(direction.normalized * Input.GetAxis("Horizontal") * FlySpeed, ForceMode.Impulse);
			}

			if(Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
				GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);

			//ensure these stay this way
			Cursor.lockState = CursorLockMode.Locked;
	        Cursor.visible = false;
	 
	/*        var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
	        var targetOrientation = Quaternion.Euler(targetDirection);
	 

	        var xRotation = Quaternion.AngleAxis(-mouseDelta.y, Vector3.right);
	        camera.transform.localRotation *= xRotation * targetOrientation;
	        var yRotation = Quaternion.AngleAxis(mouseDelta.x, transform.InverseTransformDirection(Vector3.up));
	        camera.transform.localRotation *= yRotation;
	*/

	        currentRotation.x += Input.GetAxis("Mouse X");
	        currentRotation.y -= Input.GetAxis("Mouse Y");
	        currentRotation.x = Mathf.Repeat(currentRotation.x, 360);
	        currentRotation.y = Mathf.Clamp(currentRotation.y, -maxYAngle, maxYAngle);
			//camera.transform.rotation = Quaternion.Euler(currentRotation.y,currentRotation.x,0);
			camera.transform.rotation = Quaternion.Euler(currentRotation.y,currentRotation.x,0); 


        }
        else
        {
			if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
				GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);

			Menu.SetActive(true);
        	Cursor.lockState = CursorLockMode.None;
	        Cursor.visible = true;
        }

    }
}
