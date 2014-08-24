using UnityEngine;
using System.Collections;

public class GameConfig
{
	public static readonly float[] connectionTiers = {10.0f,25.0f,50.0f};

	//returns best fit tier (floored), returns -1 if no valid tier
	public static int GetBestConnectionTier(float _rate)
	{
		int bestTier = -1;
		for(int i = 0; i < connectionTiers.Length; i++)
		{
			if(_rate < connectionTiers[i])
			{
				return bestTier;
			}
			bestTier = i;
		}
		return bestTier;
	}

}
