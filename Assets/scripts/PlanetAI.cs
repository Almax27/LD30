using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Planet))]
public class PlanetAI : MonoBehaviour 
{
	Planet thisPlanet = null;
	
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
			if(Random.value > 0.1f) //randomly don't do anything
			{
				UpdateConnection();
			}
		}
	}

	void UpdateConnection()
	{
		List<Planet> planets = thisPlanet.GetNearbyPlanets();
		if(Random.value > 0.5f)
		{
			TryAttack(planets);
		}
		else
		{
			TryReinforce(planets);
		}
	}

	void TryReinforce(List<Planet> planets)
	{
		if(thisPlanet.military.current > thisPlanet.threatLevel)
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
			if(threatendPlanet)
			{
				float required = threatendPlanet.GetMilitaryRequired();
				float available = thisPlanet.GetMilitaryAvailable();
				float amountToSend = Mathf.Min(available, required);

				float rate = amountToSend / connectionDuration;

				int tier = GameConfig.GetBestConnectionTier(rate);
				if(tier >= 0)
				{
					thisPlanet.Connect(threatendPlanet, tier, Planet.Connection.Type.REINFORCE);
				}
			}
		}
	}
	
	void TryAttack(List<Planet> planets)
	{
		if(thisPlanet.military.current > thisPlanet.threatLevel)
		{
			Planet bestTarget = null;
			float lowestUnits = float.MaxValue;
			for(int i = 0; i < planets.Count; i++)
			{
				Planet planet = planets[i];
				if(planet.team != thisPlanet.team && planet.military.current < lowestUnits)
				{
					bestTarget = planet;
					lowestUnits = planet.military.current;
				}
			}
			if(bestTarget)
			{
				float amountToSend = thisPlanet.GetMilitaryAvailable();
				float rate = amountToSend / connectionDuration;

				int tier = GameConfig.GetBestConnectionTier(rate);
				if(tier >= 0)
				{
					thisPlanet.Connect(bestTarget, tier, Planet.Connection.Type.ATTACK);
				}
			}
		}
	}
}
