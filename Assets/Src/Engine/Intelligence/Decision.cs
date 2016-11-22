
namespace Intelligence {

    public class Decision {

		public string name;
		public string description;
	    public float weight;
		public CharacterAction action;
	    public ContextCollector contextCollector;
		public DecisionEvaluator evaluator;
        
    }

}
