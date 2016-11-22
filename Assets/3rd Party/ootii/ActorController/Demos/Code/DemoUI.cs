using UnityEngine;
using com.ootii.Actors;

public class DemoUI : MonoBehaviour
{
    Vector3 mStartPosition = Vector3.zero;
    Quaternion mStartRotation = Quaternion.identity;

    // Use this for i nitialization
    void Start()
    {
        GameObject lAvatar = GameObject.Find("DefaultAvatar");
        if (lAvatar != null)
        {
            mStartPosition = lAvatar.transform.position;
            mStartRotation = lAvatar.transform.rotation;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 190, 100, 20), "Drop"))
        {
            GameObject lAvatar = GameObject.Find("DefaultAvatar");
            if (lAvatar != null)
            {
                Vector3 lPosition = lAvatar.transform.position;
                lPosition.y = 0f;

                lAvatar.transform.position = lPosition;
            }
        }

        if (GUI.Button(new Rect(10, 220, 100, 20), "Reset"))
        {
            GameObject lAvatar = GameObject.Find("DefaultAvatar");
            if (lAvatar != null)
            {
                lAvatar.transform.position = mStartPosition;
                lAvatar.transform.rotation = mStartRotation;

                ActorController lController = lAvatar.GetComponent<ActorController>();
                if (lController != null)
                {
                    lController.SetRotation(mStartRotation);
                }
            }
        }
    }
}
