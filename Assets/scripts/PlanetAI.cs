using UnityEngine;
using System.Collections.Generic;

public class PlanetAI : MonoBehaviour
{
	Planet thisPlanet = null;

	public int team = 0;
	public float connectionDuration = 0;

	protected float connectionTick = 0;

	// Use this for initialization
	void Start ()
	{
		thisPlanet = GetComponent<Planet>();
		connectionTick = Random.Range(-connectionDuration, 0);
	}

	// Update is called once per frame
	void Update ()
	{
		if(connectionDuration < 0)
		{
			Debug.LogError("Connection Duration must be > 0");
		}
		connectionTick += Time.fixedDeltaTime;
		if(connectionTick > connectionDuration)
		{
			connectionTick = 0;

			List<Planet> teamPlanets = new List<Planet>();
			for(int i = 0; i < Planet.AllPlanets.Count; i++)
			{
				if(Planet.AllPlanets[i].team == team)
				{
					teamPlanets.Add(Planet.AllPlanets[i]);
				}
			}

			for(int i = 0; i < teamPlanets.Count; i++)
			{
				thisPlanet = teamPlanets[i]; //TODO: pass this down
				if(Random.value < (Planet.AllPlanets.Count * 0.3f) / teamPlanets.Count) //randomly don't do anything
				{
					UpdateConnection();
				}
			}
		}
	}

	void UpdateConnection()
	{
		List<Planet> planets = thisPlanet.GetNearbyPlanets();
		if(TryReinforce(planets) == false)
		{
			if(TryAttack(planets) == false)
			{
				thisPlanet.SeverConnection();
			}
		}
	}

	bool TryReinforce(List<Planet> planets)
	{
		//if(thisPlanet.military.current > thisPlanet.threatLevel)
		{
			Planet threatendPlanet = null;
			float highestThreat = thisPlanet.threatLevel;
			for(int i = 0; i < planets.Count; i++)
			{
				Planet planet = planets[i];
				if(planet.team == thisPlanet.team && planet.threatLevel > highestThreat)
				{
					threatendPlanet = planet;
					highestThreat = planet.threatLevel;
				}
			}
			if(threatendPlanet != null)
			{
				float required = threatendPlanet.GetMilitaryRequired();
				float available = thisPlanet.GetMilitaryAvailable();
				float amountToSend = Mathf.Min(available, required);

				float rate = amountToSend / connectionDuration;

				int tier = GameConfig.GetBestConnectionTier(rate);
				if(tier >= 0)
				{
					thisPlanet.Connect(threatendPlanet, tier);
				}
				return true;
			}
		}
		return false;
	}

	bool TryAttack(List<Planet> planets)
	{
		//if(thisPlanet.military.current > thisPlanet.threatLevel)
		{
			Planet bestTarget = null;
			//default to current target
			if(thisPlanet.OutgoingConnection != null)
			{
				bestTarget = thisPlanet.OutgoingConnection.reciever;
			}
			float lowestUnits = float.MaxValue;
			for(int i = 0; i < planets.Count; i++)
			{
				Planet planet = planets[i];
				if((planet.team < 0 || planet.team != thisPlanet.team) && planet.military.current < lowestUnits)
				{
					bestTarget = planet;
					lowestUnits = planet.military.current;
				}
			}
			if(bestTarget != null)
			{
				float amountToSend = thisPlanet.GetMilitaryAvailable();
				float rate = amountToSend / connectionDuration;

				int tier = GameConfig.GetBestConnectionTier(rate);
				if(tier >= 0)
				{
					thisPlanet.Connect(bestTarget, tier);
					return true;
				}
			}
		}
		return false;
	}
}
