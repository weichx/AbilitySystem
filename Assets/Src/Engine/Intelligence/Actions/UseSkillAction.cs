
namespace Intelligence {

	public class UseSkillAction : CharacterAction<Context> {

		public AbilityCreator abilityCreator;
		public string abilityId;

		public override CharacterActionStatus OnUpdate() {
		    return entity.abilityManager.IsCasting ? CharacterActionStatus.Running : CharacterActionStatus.Completed;
		}

		//public override void OnCancel() {
		//	entity.abilityManager.CancelCast();
		//}

		//public override void OnInterrupt() {
		//	entity.abilityManager.InterruptCast();
		//}

	}

}