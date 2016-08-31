using UnityEngine;
using System.Collections.Generic;
using Intelligence;
using Intelligence.Actions;
using UnityEngine.UI;

public class ActionTester1 : MonoBehaviour {
    public Queue<CharacterAction> actionQueue;
    public Transform waypoint0;
    public Entity meleeRangeTarget;
    public Text combatTextPrefab;

    private CharacterAction currentAction;

    void Start() {
        var entity = GetComponent<Entity>();

        actionQueue = new Queue<CharacterAction>();
        var goTo = new MoveToLocation();
        goTo.Setup(new PointContext(entity, waypoint0.position));
        actionQueue.Enqueue(goTo);

        var meleeRange = new MoveToMeleeRange();
        meleeRange.Setup(new SingleTargetContext(entity, meleeRangeTarget));
        actionQueue.Enqueue(meleeRange);
    }


    void Update() {

        if (currentAction == null && actionQueue.Count != 0) {
            currentAction = actionQueue.Dequeue();
            SpawnCombatText(currentAction.ToString());
        }

        if (currentAction != null) {

            CharacterActionStatus actionStatus = currentAction.OnUpdate();

            switch (actionStatus) {
                case CharacterActionStatus.Running:
                    break;
                case CharacterActionStatus.Cancelled:
                    currentAction.OnCancel();
                    currentAction.OnCleanup();
                    currentAction = null;
                    break;
                case CharacterActionStatus.Completed:
                    currentAction.OnComplete();
                    currentAction.OnCleanup();
                    currentAction = null;
                    break;
            }

        }
        else {

        }
    }

    private void SpawnCombatText(string text) {
        var canvas = GetComponentInChildren<Canvas>().transform;
        Text textElement = Instantiate(combatTextPrefab, Vector3.zero, Quaternion.identity) as Text;
        textElement.transform.SetParent(canvas);
        var rectTransform = textElement.GetComponent<RectTransform>();
        rectTransform.transform.localPosition = combatTextPrefab.transform.localPosition;
        rectTransform.transform.localRotation = combatTextPrefab.transform.localRotation;
        rectTransform.transform.localScale = combatTextPrefab.transform.localScale;
        textElement.text = text;
    }
}
