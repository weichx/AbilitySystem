using UnityEngine;

public class AnimatorCullingProxy : MonoBehaviour
{
    // Use this script for culling because it is more memory
    // efficient to disable the animator than to let it cull itself.
    public Animator Target;


    public void OnBecameVisible()
    {
        Target.enabled = true;
    }


    public void OnBecameInvisible()
    {
        Target.enabled = false;
    }
}
