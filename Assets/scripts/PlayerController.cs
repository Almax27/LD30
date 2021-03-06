﻿using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
	public LayerMask planetLayerMask;
	public int team = 0;
	public ConnectionLine connectionLine = null;

	public Planet selectedPlanet = null;

	protected Vector2 mouseDownPos = new Vector2();
	protected bool isPlacingConnection = false;

	// Use this for initialization
	void Start ()
	{

	}

	// Update is called once per frame
	void Update ()
	{
		if(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
		{
			Debug.Log("mouseDown: " + Input.mousePosition);
			mouseDownPos = Input.mousePosition;
			Ray ray = Camera.main.ScreenPointToRay(mouseDownPos);
			RaycastHit hitInfo;
			if(Physics.Raycast(ray, out hitInfo, 1000, planetLayerMask))
			{
				Planet planet = hitInfo.collider.GetComponent<Planet>();
				if(planet.team == this.team)
				{
					SelectPlanet(planet);
				}
			}
		}
		else if(Input.GetMouseButtonUp(0) && selectedPlanet != null)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo;
			if(Physics.Raycast(ray, out hitInfo, float.MaxValue, planetLayerMask))
			{
				Planet planet = hitInfo.collider.GetComponent<Planet>();

				if(Vector3.Distance(selectedPlanet.transform.position, planet.transform.position) < selectedPlanet.connectionArea.radius)
				{
					//if released on a different planet, attempt connection
					if(planet != selectedPlanet)
					{
						SetConnection(selectedPlanet, planet);
					}
					//if released on the same planet, cycle connection tier
					else if(selectedPlanet.OutgoingConnection != null)
					{
						selectedPlanet.OutgoingConnection.IncrementTier();
					}
				}
				else
				{
					selectedPlanet.SeverConnection();
				}
			}
			else
			{
				selectedPlanet.SeverConnection();
			}
			UnselectPlanet();
			isPlacingConnection = false;
		}
		else if(Input.GetMouseButtonUp(1) && selectedPlanet != null)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo;
			if(Physics.Raycast(ray, out hitInfo, float.MaxValue, planetLayerMask))
			{
				selectedPlanet.SeverConnection();
				UnselectPlanet();
				isPlacingConnection = false;
			}
		}

		float deadZone = 5;
		if(!isPlacingConnection && selectedPlanet && Input.GetMouseButton(0) && Vector2.Distance(mouseDownPos, Input.mousePosition) > deadZone)
		{
			isPlacingConnection = true;
		}

		if(Input.GetMouseButton(0))
		{
			Plane xzPlane = new Plane(Vector3.up, Vector3.zero);

			float mouseDownDist = 0;
			Ray mouseDownRay = Camera.main.ScreenPointToRay(mouseDownPos);
			xzPlane.Raycast(mouseDownRay, out mouseDownDist);

			float mouseDist = 0;
			Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			xzPlane.Raycast(mouseRay, out mouseDist);

			if(connectionLine != null && isPlacingConnection)
			{
				connectionLine.gameObject.SetActive(true);

				connectionLine.lineStart = selectedPlanet.transform.position;
				connectionLine.lineEnd = mouseRay.GetPoint(mouseDist);

				connectionLine.SendMessage("Update");

				//clamp length
				Vector3 dist = connectionLine.lineEnd - connectionLine.lineStart;
				if(dist.sqrMagnitude > selectedPlanet.connectionArea.radius * selectedPlanet.connectionArea.radius)
				{
					dist = dist.normalized * selectedPlanet.connectionArea.radius;
					connectionLine.lineEnd = connectionLine.lineStart + dist;
				}
			}

			//debug
			Debug.DrawLine(mouseDownRay.origin, mouseDownRay.GetPoint(mouseDownDist), Color.white);
			Debug.DrawLine(mouseRay.origin, mouseRay.GetPoint(mouseDist), Color.white);
			Debug.DrawLine(mouseDownRay.GetPoint(mouseDownDist), mouseRay.GetPoint(mouseDist), Color.white);
		}

		if(connectionLine != null)
		{
			connectionLine.gameObject.SetActive(isPlacingConnection);
		}
	}

	void SelectPlanet(Planet planet)
	{
		UnselectPlanet();
		if(planet != null)
		{
			Debug.Log("Selected Planet: " + planet.name);
			selectedPlanet = planet;
		}
	}

	void UnselectPlanet()
	{
		if(selectedPlanet != null)
		{
			Debug.Log("Unselected Planet: " + selectedPlanet.name);
			selectedPlanet = null;
		}
	}

	void SetConnection(Planet from, Planet to)
	{
		//apply connection here
		Debug.Log("Connection made: " + from.name + " -> " + to.name);
		if(from.team != to.team)
		{
			from.Connect(to, 1);
		}
		else
		{
			from.Connect(to, 1);
		}
	}

	/*
	//GUI space selection arrow
	void OnGUI()
	{
		if(selectedPlanet && isPlacingConnection)
		{
			Vector3 planetPos = GUIUtility.ScreenToGUIPoint( Camera.main.WorldToScreenPoint(selectedPlanet.transform.position));
			planetPos.y = Camera.main.pixelHeight - planetPos.y;
			Vector3 mousePos = GUIUtility.ScreenToGUIPoint( Input.mousePosition );
			mousePos.y = Camera.main.pixelHeight - mousePos.y;

			GuiHelper.DrawLine(planetPos, mousePos, connectionLineTexture, 100);
		}
	}
	*/
}
