//using System;
//using UnityEngine;
//using UnityEditor;
//using System.Reflection;

//[CustomEditor(typeof(AbilityPrototype))]
//public class AbilityPrototypeEditor : Editor {

//    private bool overriddenRange = false;
//    private bool overriddenCastTime = false;
//    private bool overriddenTickTime = false;
//    private bool overriddenCooldown = false;
//    private bool overriddenResourceCost = false;
   
//    public void OnEnable() {
//        AbilityPrototype proto = target as AbilityPrototype;
//        //var ability = proto.GetComponent<Ability>();
//        //Type abilityType = ability.GetType();
//        //overriddenCastTime = IsOverridden(abilityType, "CreateCastTimeAttribute");
//        //overriddenTickTime = IsOverridden(abilityType, "CreateTickTimeAttribute");
//        //overriddenCooldown = IsOverridden(abilityType, "CreateCooldownAttribute");
//        //overriddenResourceCost = IsOverridden(abilityType, "CreateResourceCostAttribute");
//        //overriddenRange = IsOverridden(abilityType, "CreateRangeAttribute");
//    }

//    public override void OnInspectorGUI() {
//        AbilityPrototype proto = target as AbilityPrototype;

//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("UI Texture");
//        proto.uiSprite = (Sprite)EditorGUILayout.ObjectField(proto.uiSprite, typeof(Sprite), false);
//        EditorGUILayout.EndHorizontal();

//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("Cast Type");
//        proto.castType = (CastType)EditorGUILayout.EnumPopup(proto.castType);
//        EditorGUILayout.EndHorizontal();

//        switch (proto.castType) {
//            case CastType.Casted:
//                DrawCastTime(proto);
//                break;
//            case CastType.Channeled:
//                DrawCastTime(proto);
//                DrawTickTime(proto);
//                break;
//            case CastType.Instant:
//                break;
//        }
//        DrawRange(proto);
//        DrawCooldown(proto);
//        DrawResourceCost(proto);
//    }

//    private void DrawResourceCost(AbilityPrototype proto) {
//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("Resource Cost");
//        if (overriddenResourceCost) {
//            EditorGUILayout.LabelField("Formula");
//        }
//        else {
//            proto.resourceCost = EditorGUILayout.FloatField(proto.resourceCost);
//        }
//        EditorGUILayout.EndHorizontal();
//    }

//    private void DrawCooldown(AbilityPrototype proto) {
//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("Cooldown");
//        if (overriddenCooldown) {
//            EditorGUILayout.LabelField("Formula");
//        }
//        else {
//            proto.cooldown = EditorGUILayout.FloatField(proto.cooldown);
//        }
//        EditorGUILayout.EndHorizontal();
//    }

//    private void DrawRange(AbilityPrototype proto) {
//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("Range");
//        if (overriddenRange) {
//            EditorGUILayout.LabelField("Formula");
//        }
//        else {
//            proto.range = EditorGUILayout.FloatField(proto.range);
//        }
//        EditorGUILayout.EndHorizontal();
//    }

//    private void DrawCastTime(AbilityPrototype proto) {
//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("Cast Time");
//        if(overriddenCastTime) {
//            EditorGUILayout.LabelField("Formula");
//        }
//        else {
//            proto.castTime = EditorGUILayout.FloatField(proto.castTime);
//        }
//        EditorGUILayout.EndHorizontal();
//    }

//    private void DrawTickTime(AbilityPrototype proto) {
//        EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.LabelField("Tick Time");
//        if (overriddenTickTime) {
//            EditorGUILayout.LabelField("Formula");
//        }
//        else {
//            proto.tickTime = EditorGUILayout.FloatField(proto.tickTime);
//        }
//        EditorGUILayout.EndHorizontal();
//    }

//    private static bool IsOverridden(Type t, string methodName) {
//        var method = t.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
//        if (method == null) return false;
//        return method.DeclaringType != typeof(Ability);
//    }
//}