using UnityEngine;
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
		public bool Update(float _dt, bool _allowBaseGrowth)
		{
			float units = Mathf.Clamp (current + (((_allowBaseGrowth ? baseGrowth : 0.0f) + positiveGrowth - negativeGrowth) * _dt), 0.0f, max);
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

	public GameObject[] planetModelPrefabs = new GameObject[0];
	public GameObject currentModel = null;
	public Vector3 modelRotAxis = Vector3.zero;
	public RingController ring = null;
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

		//pick a random skin
		RandomiseSkin();

		float axisDevience = 0.1f;
		modelRotAxis = new Vector3(0, Random.Range(-axisDevience,axisDevience), 1);
		modelRotAxis.Normalize();
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
		if(currentModel != null)
		{
			currentModel.transform.Rotate(modelRotAxis, 20 * Time.deltaTime);
		}

		if(ring != null)
		{
			ring.SetTeam(team);
		}
	}

	[ContextMenu("RandomPlanet")]
	void RandomisePlanet()
	{
		military.baseGrowth = Random.Range(5.0f, 10.0f);
		military.max = Random.Range(50, 200);
		military.current = (int)Random.Range(military.max * 0.25f, military.max * 0.5f);
	}

	[ContextMenu("RandomSkin")]
	void RandomiseSkin()
	{
		if(planetModelPrefabs.Length == 0)
		{
			Debug.LogError("Failed to randomise planet skin: No models given");
			return;
		}

		//destroy current
		if(currentModel != null)
		{
			DestroyImmediate(currentModel);
			currentModel = null;
		}
		//create new
		currentModel = GameObject.Instantiate(planetModelPrefabs[Random.Range(0,planetModelPrefabs.Length)]) as GameObject;
		currentModel.transform.parent = this.transform;
		currentModel.transform.localPosition = Vector3.zero;

		currentModel.transform.Rotate(Random.Range(0.0f,360.0f),Random.Range(0.0f,360.0f),Random.Range(0.0f,360.0f));
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
		float available = Mathf.Max (0.0f, (military.current - threatLevel)) * Random.Range(0.5f,3.0f);
		return Mathf.Min (available, military.current); //can't spend more than you have
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

			float delta = 0;
			if(planet.team != this.team)
			{
				delta += planet.military.current * 0.8f;// * Random.Range(0.5f,1.0f);
			}
			threatLevel += delta;
		}
		threatLevel = Mathf.Max(0, threatLevel);
	}

	//create outgoing connection from this planet, will sever previous connection
	//all connections must be made through this call to correctly maintain references
	public void Connect(Planet _otherPlanet, int _tier)
	{
		//handle same target and type but new rate
		if(outgoingConnection != null &&
		   outgoingConnection.reciever == _otherPlanet)
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
			military.negativeGrowth += Mathf.Max(0, outgoingConnection.rate);
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
			if(military.Update(resourceInterval, this.team >= 0))
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
				this.SeverConnection();
			}
		}
	}
}
