﻿using UnityEngine;
using System.Collections.Generic;

public class Planet : MonoBehaviour
{
	//class to describe connection between two worlds
	[System.Serializable]
	public class Connection
	{
		public Connection(Planet _sender, Planet _reciever, int _tier)
		{
			sender = _sender;
			reciever = _reciever;
			tier = _tier;
		}

		public Planet sender = null;
		public Planet reciever = null;
		public int tier = 0;

		public float rate { get { return GameConfig.connectionTiers[tier]; } }

		public void IncrementTier() { tier = (tier + 1) % GameConfig.connectionTiers.Length; }
	}

	//class to describe the state of a resource and encapsulate it's growth
	[System.Serializable]
	public class Resource
	{
		public float max = 0;
		public float current = 0;
		public float baseGrowth = 0; //units per second
		public float positiveGrowth = 0; //units per second
		public float negativeGrowth = 0; //units per second

		//returns true if current has changed
		public bool Update(float _dt)
		{
			float units = Mathf.Clamp (current + ((baseGrowth + positiveGrowth - negativeGrowth) * _dt), 0, max);
			if(units == current)
			{
				return false;
			}
			else
			{
				current = units;
				return true;
      		}
		}
	}
#region public members
	public int team = 0;
	public Resource military = new Resource();
	public float threatLevel = 0;
	
	public SphereCollider connectionArea = null;
	public TextMesh unitText = null;
  	public TextParticle textParticle = null;
	public TextParticle textParticleLoss = null;
	public Renderer debugRenderer = null;
#endregion

#region protected members
	protected ConnectionController connectionController = null;
  	protected float resourceInterval = 1;
  	protected float resourceTick = 0;

	protected List<Connection> incommingConnections = new List<Connection>();
	protected Connection outgoingConnection = null;
#endregion

#region public properties
	public Connection OutgoingConnection { get { return outgoingConnection; } }
	public static List<Planet> AllPlanets { get { return allPlanets; } }
#endregion

	static List<Planet> allPlanets = new List<Planet>();

	void Awake()
	{
		allPlanets.Add(this);
		connectionController = GetComponent<ConnectionController>();
	}
	void OnDestroy()
	{
		allPlanets.Remove(this);
	}

	// Use this for initialization
	void Start ()
	{
		List<Planet> planets = GetNearbyPlanets();
		UpdateThreatLevel(planets);
    	resourceTick = Random.Range(0.0f, 0.9f);
	}

	void Update()
	{
		List<Planet> planets = GetNearbyPlanets();
		UpdateThreatLevel(planets);
		UpdateResources(Time.deltaTime);

		//DEBUG GUI STUFF
		if(outgoingConnection != null)
		{
			Vector3 senderPos = outgoingConnection.sender.transform.position;
			Vector3 recieverPos = outgoingConnection.reciever.transform.position;
			Vector3 lerpPoint = Vector3.Lerp(senderPos, recieverPos, 0.4f);

			if(outgoingConnection.sender.team == outgoingConnection.reciever.team)
			{
				Debug.DrawLine (senderPos, lerpPoint, Color.blue);
			}
			else
			{
				Debug.DrawLine (senderPos, lerpPoint, Color.red);
			}
		}
		if(unitText)
		{
			if((int)military.current == (int)military.max)
			{
				unitText.text = "[" + ((int)military.current).ToString() + "]";
				unitText.color = Color.green;
			}
			else
			{
				float percent = military.current/military.max;
				unitText.text = ((int)military.current).ToString();
				unitText.color = Color.Lerp(Color.red, Color.green, percent);
			}

		}
		if(debugRenderer)
		{
			debugRenderer.material.color = team == 0 ? Color.blue : Color.red;
		}
	}

	[ContextMenu("RandomPlanet")]
	void RandomisePlanet()
	{
		military.baseGrowth = Random.Range(5.0f, 10.0f);
		military.max = Random.Range(50, 200);
		military.current = (int)Random.Range(military.max * 0.25f, military.max * 0.5f);
	}

	//TODO: optimise use, once per frame and access cache?
	public List<Planet> GetNearbyPlanets()
	{
		List<Planet> planets = new List<Planet>();
		for(int i = 0; i < allPlanets.Count; i++)
		{
			Planet planet = allPlanets[i];
			if(planet != this)
			{
				if(Vector3.Distance(planet.transform.position, this.transform.position) < connectionArea.radius)
				{
					planets.Add(planet);
				}
			}
		}
		return planets;
	}

	public float GetMilitaryAvailable()
	{
		float desired = Mathf.Max (0.0f, (military.current - threatLevel));
		return Mathf.Min (desired, military.current) * Random.Range(0.5f,0.6f); //can't spend more than you have
	}

	public float GetMilitaryRequired()
	{
		float desired = (int)Mathf.Max (0.0f, (threatLevel - military.current));
		return Mathf.Min (desired, military.max - military.current); //can't require more than your capacity
	}

	//update threat level based on given planet set
	void UpdateThreatLevel(List<Planet> planets)
	{
		threatLevel = -military.current;
		for(int i = 0; i < planets.Count; i++)
		{
			Planet planet = planets[i];
			if(planet.team != this.team)
			{
				threatLevel += planet.military.current;
			}
			else
			{
				threatLevel -= planet.military.current * 0.8f;
			}
		}
	}

	//create outgoing connection from this planet, will sever previous connection
	//all connections must be made through this call to correctly maintain references
	public void Connect(Planet _otherPlanet, int _tier)
	{
		//handle same target and type but new rate
		if(outgoingConnection != null &&
		   outgoingConnection.reciever == _otherPlanet &&
		   outgoingConnection.tier != _tier)
		{
			outgoingConnection.tier = _tier;
			return;
		}

		//cleanup previous connection
		SeverConnection();

		//create new connection
		this.outgoingConnection = new Connection(this, _otherPlanet, _tier);
		_otherPlanet.incommingConnections.Add(this.outgoingConnection);
		connectionController.CreateConnection(outgoingConnection);
	}

	public void SeverConnection()
	{
		if(this.outgoingConnection != null)
		{
			this.outgoingConnection.reciever.incommingConnections.Remove(this.outgoingConnection);
			this.outgoingConnection = null;
			connectionController.DestroyConnection();
		}
	}

	public void SeverAllConnections()
	{
		SeverConnection();
		List<Connection> temp = new List<Connection>(incommingConnections);
		foreach(Connection connection in temp)
		{
			connection.sender.SeverConnection();
		}
	}

	//update and apply resource growth
	void UpdateResources(float _dt)
	{
		//calculate effective growth rate based on incomming connections
		military.positiveGrowth = 0;
		military.negativeGrowth = 0;
		if(outgoingConnection != null)
		{
			military.negativeGrowth += outgoingConnection.rate;
		}
		foreach(Connection connection in incommingConnections)
		{
			if(connection.sender.team != connection.reciever.team)
			{
				military.negativeGrowth += connection.rate;
			}
			else
			{
				military.positiveGrowth += connection.rate;
			}
		}

		resourceTick += Time.deltaTime;
		if(resourceTick > resourceInterval)
		{
			resourceTick = 0;
			military.Update(resourceInterval);
			{
				float growth = military.baseGrowth + military.positiveGrowth - military.negativeGrowth;
				if(growth > 0)
				{
					textParticle.FirePositiveText("+" +  ((int)growth).ToString());
				}
				else if(growth < 0)
				{
					textParticleLoss.FireNegatveText(((int)growth).ToString());
				}
			}

			//handle 'death' and change to most agressive attacker's team
			if(military.current <= 0)
			{
				military.current = 0;

				if(incommingConnections.Count > 0)
				{
					//sort in descending rate order
					incommingConnections.Sort(delegate(Connection x, Connection y)
					                          {
						return -x.rate.CompareTo(y.rate); //CompareTo sorts in ascending to we use '-' to reverse it
					});
					this.team = incommingConnections[0].sender.team;
					//this.SeverAllConnections();
				}
				else
				{
					this.SeverConnection();
				}
			}
		}
	}
}
