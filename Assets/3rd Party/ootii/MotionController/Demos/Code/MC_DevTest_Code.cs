using UnityEngine;
using com.ootii.Actors;

public class MC_DevTest_Code : MonoBehaviour
{
    // Current actor controller
    public ActorController Actor = null;

    // Camera we will move
    //private GameObject mCamera = null;

    // Store the start position
    private Vector3 mStartPosition = Vector3.zero;

    /// <summary>
    /// Used for initialization
    /// </summary>
    private void Awake()
    {
        //mCamera = GameObject.FindWithTag("MainCamera");
    }

    /// <summary>
    /// Called before the first update
    /// </summary>
    private void Start()
    {
        SetActor(Actor);
    }

    /// <summary>
    /// Update once per frame
    /// </summary>
    private void Update()
    {
    }

    private void OnGUI()
    {
        Rect lUpButtonRect = new Rect(5, 10, 50, 20);
        if (GUI.Button(lUpButtonRect, "Up 10"))
        {
            Vector3 lNewPosition = Actor._Transform.position;
            lNewPosition.y = lNewPosition.y + 10f;

            Actor._Transform.position = lNewPosition;
        }

        Rect lResetButtonRect = new Rect(5, 35, 50, 20);
        if (GUI.Button(lResetButtonRect, "Reset"))
        {
            Actor._Transform.position = mStartPosition;
        }
    }

    /// <summary>
    /// Sets the new actor  that is being controlled
    /// </summary>
    /// <param name="rActor"></param>
    public void SetActor(ActorController rActor)
    {
        if (rActor == null) { return; }

        if (Actor != null)
        {
            ActorDriver lOldDriver = Actor.gameObject.GetComponent<ActorDriver>();
            if (lOldDriver != null) { lOldDriver.IsEnabled = false; }
        }

        ActorDriver lNewDriver = rActor.gameObject.GetComponent<ActorDriver>();
        if (lNewDriver != null) { lNewDriver.IsEnabled = true; }

        Actor = rActor;

        mStartPosition = Actor._Transform.position;
    }
}
