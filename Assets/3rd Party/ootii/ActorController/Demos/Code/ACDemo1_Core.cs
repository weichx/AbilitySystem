using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using com.ootii.Actors;
using com.ootii.Cameras;

public class ACDemo1_Core : MonoBehaviour
{
    // Current actor controller
    public ActorController Actor = null;

    // Camera we will move
    private GameObject mCamera = null;

    // Camera rig that can move the camera
    private FollowRig mCameraRig = null;

    // Description text
    private Text mActorText = null;

    // Timer to show description text
    private float mActorTextTimer = 0f;

    // Description per actor
    private Dictionary<string, string> mActorDescriptions = null;

    /// <summary>
    /// Used for initialization
    /// </summary>
    private void Awake()
    {
        mCamera = GameObject.FindWithTag("MainCamera");
        if (mCamera != null && mCamera.transform.parent != null)
        {
            mCameraRig = mCamera.transform.parent.gameObject.GetComponent<FollowRig>();
        }

        if (GameObject.Find("Actor Text") != null)
        {
            mActorText = GameObject.Find("Actor Text").GetComponent<Text>();
        }

        mActorDescriptions = new Dictionary<string, string>();
        mActorDescriptions.Add("Player Capsule", "Standard capsule settings that collides with objects and slides on slopes.");
        mActorDescriptions.Add("Player Sphere", "'Roller Ball' settings that collides with objects. Sphere based object that slides on slopes.");
        mActorDescriptions.Add("Player Ghost", "Ghost settings that goes through objects. He'll rise up on surfaces if possible.");
        mActorDescriptions.Add("Player Robot", "Standard character settings that collide with objects and slides on slopes. Does not have foot collisions for smooth stepping.");
        mActorDescriptions.Add("Player Spider", "Wall walker settings that collides with objects. Walk up to a wall and 'jump' to jump onto it.");
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
        if (mActorText != null && mActorText.enabled)
        {
            mActorTextTimer = mActorTextTimer + Time.deltaTime;
            if (mActorTextTimer > 5f)
            {
                mActorText.enabled = false;
            }
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

        if (mCameraRig != null)
        {
            mCameraRig.Anchor = Actor.transform;
        }
        else if (mCamera != null)
        {
            mCamera.transform.parent = Actor.transform;
            mCamera.transform.localPosition = new Vector3(0f, 1.5f, -4f);

            Vector3 lLookTarget = Actor.transform.position + (Actor.transform.up * 1f);
            mCamera.transform.rotation = Quaternion.LookRotation(lLookTarget - mCamera.transform.position, Actor.transform.up);
        }

        if (mActorText != null && mActorDescriptions.ContainsKey(rActor.gameObject.name))
        {
            mActorTextTimer = 0f;
            mActorText.text = mActorDescriptions[rActor.gameObject.name];
            mActorText.enabled = true;
        }
    }
}
