using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class ConnectionLine : MonoBehaviour 
{
	protected Planet.Connection connection = null;
	protected LineRenderer line = null;

	public Vector3 lineStart = Vector3.zero;
	public Vector3 lineEnd = Vector3.zero;

	public bool isConnected = false;
	public float animDuration = 0.5f;
	public float textureAnimRate_1 = 1.0f;
	public float textureAnimRate_2 = 0.5f;

	protected float animTick = 0;

	// Use this for initialization
	void Awake ()
	{
		line = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		line.SetVertexCount(2);

		animTick += isConnected ? Time.deltaTime : -Time.deltaTime;
		animTick = Mathf.Clamp(animTick, 0, animDuration);

		//if(animTick > 0 && animTick < animDuration)
		{
			float t = animTick / animDuration;
			line.SetPosition(0, lineStart);
			line.SetPosition(1, Vector3.Lerp(lineStart, lineEnd, t));
		}

		if(connection != null)
		{
			Vector2 textureOffset_1 = line.material.GetTextureOffset("_FlowTex1");
			textureOffset_1.x -= textureAnimRate_1 * Time.deltaTime * (connection.tier+0.5f);
			line.material.SetTextureOffset("_FlowTex1", textureOffset_1);
			
			Vector2 textureOffset_2 = line.material.GetTextureOffset("_FlowTex2");
			textureOffset_2.x -= textureAnimRate_2 * Time.deltaTime * (connection.tier+0.5f);
			line.material.SetTextureOffset("_FlowTex2", textureOffset_2);

			if(connection.sender.team == connection.reciever.team)
			{
				line.material.color = Color.green;
			}
			else
			{
				line.material.color = Color.red;
			}
		}
	}

	//perform sever animation and destroy self
	public void SeverAndDestroy()
	{
		Debug.Log ("Destoying connection from: " + connection.sender.name);
		//Debug.Break();

		connection = null;
		isConnected = false;
		Destroy(this.gameObject, animDuration);
	}

	//perform connect animation 
	public void Connect(Planet.Connection _connection)
	{
		Debug.Log ("Creating connection from: " + _connection.sender.name);
		//Debug.Break();

		connection = _connection;
		lineStart = connection.sender.transform.position;
		lineEnd = connection.reciever.transform.position;
		isConnected = true;

		Vector3 offset = Vector3.Cross((lineEnd - lineStart).normalized, Vector3.up);
		lineStart += offset * 0.2f;
		lineEnd += offset * 0.2f;

	}

	
}
