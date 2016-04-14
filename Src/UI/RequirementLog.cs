using UnityEngine;
using UnityEngine.UI;

public class RequirementLog : MonoBehaviour {

    protected Text textComponent;
    protected Timer timer;
    protected string message;

    void Start() {
        textComponent = GetComponent<Text>();
        timer = new Timer();
    }

    public void SetMessage(string message, float timeout = 1.5f) {
        this.message = message;
        timer.Reset(timeout);
        textComponent.enabled = true;
        textComponent.text = message;
    }

    public void Update() {
        if (!timer.Ready) return;
        if (message != null && textComponent.enabled) {
            textComponent.enabled = false;
            message = null;
        }
    }
}