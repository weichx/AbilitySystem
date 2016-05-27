
namespace Intelligence {

	//only here for type queries and storing lists of actions
	public abstract class CharacterAction {

		protected Entity entity;

		public void Run(Context context) {
			SetContext(context);
			OnStart();
		}
			
		public virtual void OnStart() {}

		public virtual bool OnUpdate() {
			return true;
		}

		public virtual void OnInterrupt() {

		}

		public virtual void OnCancel() {

		}

		public virtual void OnComplete() {

		}

		protected abstract void SetContext(Context context);

	}

	public abstract class CharacterAction<T> : CharacterAction where T : Context {
		
		protected T context;

		protected override void SetContext(Context context) {
			this.context = context as T;
		}

	}

	[System.Serializable]
	public class NoOpAction : CharacterAction {
	
		public string moarValues;
		protected override void SetContext(Context context) {}

	}
		
}