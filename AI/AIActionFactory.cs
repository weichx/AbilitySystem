using UnityEngine;
using AbilitySystem;
using MiniJSON;

public static class AIActionFactory {

    //todo check for parameter attributes on target objects and ensure they are set if required

    public static AIAction[] Create(Entity agent, string json) {

        return Json.Deserialize<AIAction[]>(json);

        //var root = JSON.Parse(json);
        //var actions = root["actions"].AsArray;
        //var actionList = new AIAction[actions.Count];

        //for (var i = 0; i < actions.Count; i++) {

        //    var node = actions[i];
        //    var type = TypeCache.GetType(node["typeName"].Value);
        //    var considerations = node["considerations"].AsArray;
        //    var requirements = node["requirements"].AsArray;

        //    var actionInstance = JsonUtility.FromJson(node.ToJSON(0), type) as AIAction;
        //    actionInstance.Initialize(agent);
        //    actionList[i] = actionInstance;
        //    actionInstance.considerations = new AIConsideration[considerations.Count];
        //    actionInstance.requirements = new AIRequirement[requirements.Count];

        //    for (int j = 0; j < considerations.Count; j++) {
        //        var considerationJSON = considerations[j];
        //        var considerationType = TypeCache.GetType(considerationJSON["typeName"].Value);
        //        if (considerationType == null) {
        //            Debug.LogError("Cant find AI consideration type named `" + considerationJSON["typeName"].Value + "`");
        //        }
        //        else {
        //            //todo just keep on with simple json & reflection
        //            AIConsideration considerationInstance = JsonUtility.FromJson(considerationJSON.ToJSON(0), considerationType) as AIConsideration;
        //            CreateCurve(considerationInstance, considerationJSON);
        //            actionInstance.considerations[j] = considerationInstance;
        //        }
        //    }

        //    if (requirements == null) break;

        //    for(int j = 0; j < requirements.Count; j++) {
        //        var requirementJSON = requirements[j];
        //        var requirementType = TypeCache.GetType(requirementJSON["typeName"].Value);
        //        if (requirementType == null) {
        //            Debug.LogError("Cant find AI requirement type named `" + requirementJSON["typeName"].Value + "`");
        //        }
        //        else {
        //            //todo just keep on with simple json & reflection
        //            AIRequirement requirementInstance = JsonUtility.FromJson(requirementJSON.ToJSON(0), requirementType) as AIRequirement;
        //            actionInstance.requirements[j] = requirementInstance;
        //        }
        //    }
        //}

        //return actionList;
    }

    //todo just check for curve being a string. if string check for greater than sign + number for threshold
    //private static void CreateCurve(AIConsideration consideration, JSONNode json) {
    //    ResponseCurve curve = consideration.curve;
    //    if (curve == null) {
    //        JSONNode curvePreset = json["presetCurve"];
    //        JSONNode thresholdNode = json["presetCurveThreshold"];

    //        if (curvePreset != null) {
    //            consideration.curve = ResponseCurve.GetPreset(curvePreset.Value);
    //        }

    //        Debug.Assert(consideration.curve != null, "Consideration cannot have a null curve");
    //        if (thresholdNode != null) {
    //            consideration.curve.threshold = thresholdNode.AsFloat;
    //        }
    //    }
    //    consideration.curve.SetCurveType(consideration.curve.type);
    //}
}