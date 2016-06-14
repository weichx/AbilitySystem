using UnityEngine;
using System.Collections.Generic;

namespace Intelligence {


    public struct QueuedAction {

        public readonly float timestamp;
        public readonly PlayerCharacterAction action;

        public QueuedAction(PlayerCharacterAction action) {
            this.action = action;
            timestamp = Time.realtimeSinceStartup;
        }

    }

    public struct InputAction {
        public readonly string actionId;
        public readonly KeyCode[] keys;

        public InputAction(string actionId, KeyCode[] keys) {
            this.actionId = actionId;
            this.keys = keys;
        }

    }

    public class PlayerIntelligenceController : MonoBehaviour {
        public Queue<QueuedAction> actionQueue;
        public Dictionary<string, PlayerCharacterAction> actionMap;
        public Dictionary<string, KeyCode[]> keyMap;
        public List<InputAction> inputActions;

        private PlayerCharacterAction currentAction;
        private Entity entity;
        private Context context;

        private static PlayerIntelligenceController instance;

        void Awake() {
            instance = instance ?? this;
            actionQueue = new Queue<QueuedAction>();
            entity = GetComponent<Entity>();
            context = new Context(entity);
            inputActions = new List<InputAction>(10);
            inputActions.Add(new InputAction("Actionbar0-1", new KeyCode[] { KeyCode.Alpha1 }));
            inputActions.Add(new InputAction("Actionbar0-2", new KeyCode[] { KeyCode.Alpha2 }));
            inputActions.Add(new InputAction("Actionbar0-3", new KeyCode[] { KeyCode.Alpha3 }));
            inputActions.Add(new InputAction("Actionbar0-4", new KeyCode[] { KeyCode.Alpha4 }));
            inputActions.Add(new InputAction("Actionbar0-5", new KeyCode[] { KeyCode.Alpha5 }));
            inputActions.Add(new InputAction("Actionbar0-6", new KeyCode[] { KeyCode.Alpha6 }));
            inputActions.Add(new InputAction("Actionbar0-7", new KeyCode[] { KeyCode.Alpha7 }));
            inputActions.Add(new InputAction("Actionbar0-8", new KeyCode[] { KeyCode.Alpha8 }));
            inputActions.Add(new InputAction("Actionbar0-9", new KeyCode[] { KeyCode.Alpha9 }));

            actionMap = new Dictionary<string, PlayerCharacterAction>();

        }

        void Start() {
            //todo this is temporary, later we should have things dragged onto the action
            //bar register themselves with the action map by the slot's action id

            PlayerSkillBook skillbook = entity.GetComponent<PlayerSkillBook>();
            if (skillbook == null) return;

            for (int i = 0; i < 9; i++) {
                if (skillbook.skillBookEntries.Length == i) return;
                SkillBookEntry entry = skillbook.skillBookEntries[i];
                if (entry == null) continue;
                string id = "Actionbar0-" + (i + 1);
                actionMap[id] = entry.action;
            }
        }



        private void CheckInput() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                if (currentAction != null) {
                    currentAction.OnCancel();
                    currentAction = null;
                    actionQueue.Clear();
                }
                return;
            }
            for (int i = 0; i < inputActions.Count; i++) {
                KeyCode[] keys = inputActions[i].keys;
                for (int j = 0; j < keys.Length; j++) {
                    //only hadles 1 key for now
                    if (Input.GetKeyDown(keys[j])) {
                        string actionId = inputActions[i].actionId;
                        PlayerCharacterAction action = actionMap.Get(actionId);
                        if (action != null) {
                            actionQueue.Enqueue(new QueuedAction(action));
                            return;
                        }
                    }
                }
            }
        }

        private PlayerCharacterAction Dequeue() {
            while (actionQueue.Count > 0) {
                var queuedAction = actionQueue.Dequeue();
                //if action sat in queue for longer than half a second, move along
                if (queuedAction.timestamp + 0.25f < Time.realtimeSinceStartup) {
                    continue;
                }
                var action = queuedAction.action;
                action.Setup(context);
                return action;
            }
            return null;
        }

        public void Update() {
            CheckInput();
            currentAction = currentAction ?? Dequeue();

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

        }

        public static void Enqueue(PlayerCharacterAction action) {
            instance.actionQueue.Enqueue(new QueuedAction(action));
        }

    }

}
