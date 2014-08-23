using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class Connection : MonoBehaviour
{
  public float pulseTime = 0;

  private LineRenderer line;
  private float pulseTick = 0.0f;

  public void Start()
  {
    line = gameObject.GetComponent<LineRenderer>();
  }
  [ContextMenu("Pulse")]
  public void Pulse()
  {
    pulseTick = 0;
  }
	// Update is called once per frame
	void Update ()
  {
    pulseTick += Time.deltaTime;
    float divTime = pulseTime/3;

    if(pulseTick < divTime)
    {
      line.SetWidth(Mathf.Lerp(1, 2, pulseTick/divTime),1);
    }
    else if(pulseTick > divTime && pulseTick < divTime*2)
    {
      line.SetWidth(Mathf.Lerp(2, 1, (pulseTick-divTime)/divTime),Mathf.Lerp(1, 2, (pulseTick-divTime)/divTime));
    }
    else if(pulseTick > divTime*2 && pulseTick < divTime*3)
    {
      line.SetWidth(1, Mathf.Lerp(2, 1, (pulseTick-divTime*2)/divTime));
    }
    else
      line.SetWidth(1,1);

	}
}
