using System;

namespace Intelligence {
	
	[Serializable]
	public class Decision {
		public string name;
		public string description;
		public CharacterAction action;
//		public ContextFactory<Context> contextFactory; todo -- move this to the DSE
		public DecisionScoreEvaluator dse;

    }
//
//	public class A {
//
//		public void Run(Context context) {
//			Run(context as T);
//		}
//
//	}
//
	/*

	
	action
	decisions are actions
		actions need context
		factory makes sense here
		dse 

	Type decisionType = typeof(Decision<>);

	Type genType = Type.MakeGenericType(new Type[] { typeof(Context) } );

	var decision = Activator.CreateInstance(typeof(genType)) as Decision<Context>;

	Action<Context>[] actions
		actions.Add();
		
	*/
}
