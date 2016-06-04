
namespace Intelligence {

	public class UseSkillAction : CharacterAction<Context> {

		public AbilityCreator abilityCreator;
		public string abilityId;

		public override bool OnUpdate() {
			return entity.abilityManager.IsCasting;
		}

		public override void OnCancel() {
			entity.abilityManager.CancelCast();
		}

		public override void OnInterrupt() {
			entity.abilityManager.InterruptCast();
		}

	}

}