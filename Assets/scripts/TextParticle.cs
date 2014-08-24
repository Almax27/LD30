using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class TextParticle : MonoBehaviour
{
  private Animator animator;
  void Start()
  {
    animator = gameObject.GetComponent<Animator>();
  }
  [ContextMenu("PositiveGrowth")]
  public void PositiveGrowth()
  {
    animator.SetTrigger("PositiveGrowth");
  }
}
