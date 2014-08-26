using UnityEngine;
using System.Collections;

public class RingController : MonoBehaviour {

	public Renderer ringBase = null;
	public Renderer ringOverlay1 = null;
	public Renderer ringOverlay2 = null;

	protected int team = -1;


	// Use this for initialization
	void Start () 
	{
		ringBase.transform.Rotate(Vector3.back, Random.Range(0.0f,360.0f));
		ringOverlay1.transform.Rotate(Vector3.back, Random.Range(0.0f,360.0f));
		ringOverlay2.transform.Rotate(Vector3.back, Random.Range(0.0f,360.0f));
	}
	
	// Update is called once per frame
	void Update () 
	{
		//make this object always face the camera
		transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
		                 Camera.main.transform.rotation * Vector3.up);

		//spin overlay1
		ringOverlay1.transform.Rotate(Vector3.back, 30 * Time.deltaTime);
		ringOverlay2.transform.Rotate(Vector3.back, -15 * Time.deltaTime);
	}

	public void SetTeam(int _team)
	{
		if( team != _team)
		{
			team = _team;

			Color color;
			if(team == 0)
			{
				color = Color.green;
			}
			else if(team == 1)
			{
				color = Color.red;
			}
			else 
			{
				color = Color.white;
			}
			ringBase.material.color = color;
			ringOverlay1.material.color = color;
			ringOverlay2.material.color = color;
		}
	}
}
