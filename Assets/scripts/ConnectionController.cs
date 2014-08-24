using UnityEngine;
using System.Collections;

public class ConnectionController : MonoBehaviour
{

	public ConnectionLine connectionLinePrefab = null;

	protected ConnectionLine currentLine = null;

	void Start()
	{

	}

	void Update()
	{

	}

	public void CreateConnection(Planet.Connection _connection)
	{
		DestroyConnection();

		//create new connection
		if(_connection != null)
		{
			GameObject gobj = GameObject.Instantiate(connectionLinePrefab.gameObject) as GameObject;
			currentLine = gobj.GetComponent<ConnectionLine>();
			currentLine.Connect(_connection);
		}
	}

	public void DestroyConnection()
	{
		if(currentLine != null)
		{
			currentLine.SeverAndDestroy();
			currentLine = null;
		}
	}
}
