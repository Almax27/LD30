using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class Planet : MonoBehaviour 
{
	
	public int team = 0;
	public float unitsPerSecond = 0;
	public int maxUnits = 0;
	public int currentUnits = 0;
	public float aiTimeStep = 0;

	public float threatLevel = 0;

	public TextMesh debugText = null;
	public Renderer debugRenderer = null;
	
	protected float unitTick = 0;
	protected float aiTick = 0;


	static List<Planet> allPlanets = new List<Planet>();

	// Use this for initialization
	void Start () 
	{
		aiTick = Random.Range(0.0f, aiTimeStep);
		allPlanets.Add(this);

		List<Planet> planets = GetNearbyPlanets();
		UpdateThreatLevel(planets);
	}

	void OnDestroy()
	{
		allPlanets.Remove(this);
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		unitTick += Time.fixedDeltaTime;
		if (unitTick > 1.0f/unitsPerSecond) 
		{
			unitTick = 0;
			SpawnUnit();
		}

		aiTick += Time.fixedDeltaTime;
		if(aiTick > aiTimeStep)
		{
			aiTick = 0;
			OnUpdateAI();
		}

		if(debugText)
		{
			debugText.text = "Units: " + currentUnits + "\nThreat: " + threatLevel;
		}
		if(debugRenderer)
		{
			debugRenderer.material.color = team == 0 ? Color.blue : Color.red;
		}
	}

	[ContextMenu("RandomPlanet")]
	void RandomisePlanet()
	{
		unitsPerSecond = Random.Range(5.0f, 10.0f);
		maxUnits = Random.Range(50, 200);
		currentUnits = (int)Random.Range(maxUnits * 0.25f, maxUnits * 0.5f);
		aiTimeStep = Random.Range(0.0f, aiTimeStep);
	}

	void SpawnUnit()
	{
		if(currentUnits < maxUnits)
		{
			currentUnits += 1;
			if(currentUnits == maxUnits)
			{
				OnMaxUnitsReached();
			}
		}
	}

	void OnMaxUnitsReached()
	{
		Debug.Log("Capacity reached: " + currentUnits + "/" + maxUnits);
	}

	void OnUpdateAI()
	{
		List<Planet> planets = GetNearbyPlanets();
		UpdateThreatLevel(planets);
		if(Random.value > 0.5f)
		{
			if(Random.value > 0.5f)
				TryAttack(planets);
			else
				TryReinforce(planets);
		}
	}

	List<Planet> GetNearbyPlanets()
	{
		List<Planet> planets = new List<Planet>();
		SphereCollider sphere = collider as SphereCollider;
		for(int i = 0; i < allPlanets.Count; i++)
		{
			if(allPlanets[i] != this)
			{
				SphereCollider otherSphere = allPlanets[i].collider as SphereCollider;
				if(Vector3.Distance(otherSphere.transform.position, sphere.transform.position) < sphere.radius)
				{
					planets.Add(otherSphere.GetComponent<Planet>());
				}
			}
		}
		return planets;
	}

	public int GetUnitsAvailable()
	{
		int desired = (int)Mathf.Max (0.0f, (currentUnits - threatLevel));
		return (int)(Mathf.Min (desired, currentUnits) * Random.Range(0.5f,0.6f)); //can't spend more than you have
	}

	public int GetUnitsRequired()
	{
		int desired = (int)Mathf.Max (0.0f, (threatLevel - currentUnits));
		return Mathf.Min (desired, maxUnits - currentUnits); //can't require more than your capacity
	}


	void UpdateThreatLevel(List<Planet> planets)
	{
		threatLevel = -currentUnits;
		for(int i = 0; i < planets.Count; i++)
		{
			Planet planet = planets[i];
			if(planet.team != this.team)
			{
				threatLevel += planet.currentUnits;
			}
			else
			{
				threatLevel -= planet.currentUnits * 0.8f;
			}
		}
	}

	void TryReinforce(List<Planet> planets)
	{
		if(currentUnits > threatLevel)
		{
			Planet threatendPlanet = null;
			float highestThreat = threatLevel;
			for(int i = 0; i < planets.Count; i++)
			{
				Planet planet = planets[i];
				if(planet.team == team && planet.threatLevel > highestThreat)
				{
					threatendPlanet = planet;
					highestThreat = planet.threatLevel;
				}
			}
			if(threatendPlanet)
			{
				Debug.Log("Reinforcing...");
				int unitsRequired = threatendPlanet.GetUnitsRequired();
				int unitsAvailable = GetUnitsAvailable();
				int unitsToSpend = Mathf.Min(unitsAvailable, unitsRequired);

				//apply
				threatendPlanet.currentUnits += unitsToSpend;
				currentUnits -= unitsToSpend;
			}
		}
	}

	void TryAttack(List<Planet> planets)
	{
		if(currentUnits > threatLevel)
		{
			Planet bestTarget = null;
			int lowestUnits = int.MaxValue;
			for(int i = 0; i < planets.Count; i++)
			{
				Planet planet = planets[i];
				if(planet.team != this.team && planet.currentUnits < lowestUnits)
				{
					bestTarget = planet;
					lowestUnits = planet.currentUnits;
				}
			}
			if(bestTarget)
			{

				Debug.Log("Attacking...");
				int unitsToSpend = GetUnitsAvailable();

				//apply
				bestTarget.currentUnits -= unitsToSpend;
				currentUnits -= unitsToSpend;

				//change team and 'revive' units
				if(bestTarget.currentUnits < 0)
				{
					bestTarget.team = team;
					bestTarget.currentUnits *= -1;
				}
			}
		}
	}
}
