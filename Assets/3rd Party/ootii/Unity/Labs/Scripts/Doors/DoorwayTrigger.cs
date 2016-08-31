using UnityEngine;
using UnityEngine.Events;

public class DoorwayTrigger : MonoBehaviour
{
    public UnityEvent OnPlayerProceedForward;
    public UnityEvent OnPlayerProceedBackward;


    public void OnTriggerExit(Collider other)
    {
        // Only execute the function if the script is enable and it's the player's collider.
        if (!enabled || !other.CompareTag("Player")) return;

        // Create a vector that will point from the doorway to the player in the door's local coordinates.
        var exitVec = transform.InverseTransformPoint(other.transform.position);

        // The z component of this vector determines which direction the player is leaving the door's trigger.
        if (exitVec.z > 0)
        {
            OnPlayerProceedForward.Invoke();
        }
        else
        {
            OnPlayerProceedBackward.Invoke();
        }
    }
}
