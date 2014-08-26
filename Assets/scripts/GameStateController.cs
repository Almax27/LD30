using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(GUITexture))]
public class GameStateController : MonoBehaviour {

	public enum State
	{
		NONE,
		SPLASH,
		PLAY,
		WIN,
		LOSE
	}

	public State state = State.NONE;
	public int playerTeam = 0;

	public GameObject gamePrefab = null;
	public GameObject gameInstance = null;

	public Texture2D splashTexture = null;
	public Texture2D winTexture = null;
	public Texture2D loseTexture = null;

	protected float gameEndTime = 0;
	protected State pendingState = State.NONE;
	protected float fadeTick = 0;

	// Use this for initialization
	void Awake () 
	{
		SetState(State.SPLASH);
	}
	
	// Update is called once per frame
	void Update () 
	{
		switch(state)
		{
		case State.SPLASH:
		{
			if(Input.anyKeyDown)
			{
				SetState(State.PLAY);
			}
			break;
		}
		case State.PLAY:
		{
			if(Input.GetKeyDown(KeyCode.Escape))
			{
				SetState(State.SPLASH);
				break;
			}
			List<int> teams = new List<int>();
			for(int i = 0; i < Planet.AllPlanets.Count; i++)
			{
				int team = Planet.AllPlanets[i].team;
				if(team >= 0 && teams.Contains(team) == false)
				{
					teams.Add(team);
				}
			}
			if(teams.Count == 1) //only one team remaining
			{
				if(teams.Contains(playerTeam))
				{
					SetState(State.WIN);
				}
				else
				{
					SetState(State.LOSE);
				}
			}
			break;
		}
		case State.WIN:
		case State.LOSE:
		{
			if(Input.anyKeyDown)
			{
				SetState(State.SPLASH);
			}

			break;
		}
		}

		if(state == State.WIN || state == State.LOSE)
		{
			float fadeDuration = 2.0f;
			fadeTick = Mathf.Min(fadeTick + Time.deltaTime, fadeDuration);
			Color c = guiTexture.color;
			c.a = fadeTick / fadeDuration;
			guiTexture.color = c;
		}
	}

	void SetState(State _state)
	{
		guiTexture.enabled = true;
		switch(_state)
		{
		case State.SPLASH:
		{
			if(state != _state)
			{
				if(gameInstance != null)
				{
					Destroy(gameInstance);
					gameInstance = null;
				}
			}
			guiTexture.texture = splashTexture;
			break;
		}
		case State.PLAY:
		{
			guiTexture.enabled = false;
			gameInstance = Instantiate(gamePrefab) as GameObject;
			break;
		}
		case State.WIN:
		{
			guiTexture.texture = winTexture;
			fadeTick= 0;
			break;
		}
		case State.LOSE:
		{
			guiTexture.texture = loseTexture;
			fadeTick = 0;
			break;
		}
		}
		state = _state;
	}
}
