using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(TextMesh))]
public class TextParticle : MonoBehaviour
{
  private Animator animator;
  private TextMesh textMesh;

  void Start()
  {
    animator = gameObject.GetComponent<Animator>();
    textMesh = gameObject.GetComponent<TextMesh>();
  }

  [ContextMenu("PositiveGrowth")]
  public void PositiveGrowth()
  {
    animator.SetTrigger("PositiveGrowth");
  }
  
  public void FireParticleText(string text)
  {
    textMesh.text = text;
    animator.SetTrigger("PositiveGrowth");
  }
}
