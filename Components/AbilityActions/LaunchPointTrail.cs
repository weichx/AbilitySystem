//using UnityEngine;
//using AbilitySystem;

//[AddComponentMenu("Ability Actions/Launchers/Point Trail")]
//[RequireComponent(typeof(PointListTargetingStrategy))]
//public class LaunchPointTrail : AbilityAction {

//    public PointListTrail trailPrefab;

//    public override void OnCastCompleted() {
//        PointListTargetingStrategy strategy = ability.TargetingStrategy as PointListTargetingStrategy;
//        PointListTrail trailObj = Instantiate(trailPrefab, strategy.FirstPosition, Quaternion.identity) as PointListTrail;
//        trailObj.Initialize(strategy.pointList);    
//    }

//}