using System.Collections.Generic;
using UnityEngine;
using System;
using AbilitySystem;
using System.Text;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

[SelectionBase]
public class AIEntity : Entity {
    public TextAsset jsonFile;
    public Entity target;

    protected NavMeshAgent agent;
    private Timer influenceUpdateTimer;
    private InfluenceMapSection iMapSection;
    public AIActionEvaluator evaluator;
    private Animator animator;

    public override void Start() {
        base.Start();
        animator = GetComponent<Animator>();
        influenceUpdateTimer = new Timer(0.25f);
        agent = GetComponent<NavMeshAgent>();
        iMapSection = new InfluenceMapSection(9 * 9);
        AIAction[] actionPackage = null;
        if (jsonFile != null) {
            actionPackage = AIActionFactory.Create(this, jsonFile.text);
        }
        evaluator = new AIActionEvaluator(this, actionPackage);
    }

    public override void Update() {
        base.Update();
        evaluator.Update();
        iMapSection = InfluenceMapManager.Instance.UpdatePhysicalInfluence(transform.position, iMapSection);
        if (agent != null) {
            agent.speed = movementSpeed.Current;
        }
    }

    public void RefreshActionJson() {
        evaluator.Unload();
        AIAction[] actionPackage = AIActionFactory.Create(this, jsonFile.text);
        evaluator.AddActionPackage(actionPackage);
    }

#if UNITY_EDITOR
    public void OnGUI() {
        if(Selection.activeGameObject == gameObject && Application.isPlaying) {
            GUI.Box(new Rect(0, 0, 400, 600), "Action: " + evaluator.GetCurrentActionName());

            //evaluator.decisionLog.entries;
            //var results = evaluator.GetLastDecisionResults();
            //var rect = new Rect(0, 25, 400, 25);
            //for (int i = 0; i < results.Count; i++) {
            //    GUI.Label(rect, results[i].action.name + " : " + results[i].score);
            //    rect.y += 20f;
            //}
        }
    }
#endif

}

#if UNITY_EDITOR
[CustomEditor(typeof(AIEntity))]
public class AIEntityInspector : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if(GUILayout.Button("Refresh Action JSON")) {
            AIEntity agent = target as AIEntity;
            agent.RefreshActionJson();
        }
        if(Application.isPlaying && GUILayout.Button("Write Diagnostics")) {
            AIEntity agent = target as AIEntity;
            AIDecisionLog diagnostics = agent.evaluator.decisionLog;
            diagnostics.WriteToDisk(agent.name + "_AIDiagnostics.json");
        }
    }
}
#endif