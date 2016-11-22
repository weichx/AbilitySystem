namespace Intelligence {

    public struct DecisionContextPair {

        public readonly Decision decision;
        public readonly Context context;

        public DecisionContextPair(Decision decision, Context context) {
            this.decision = decision;
            this.context = context;
        }

    }

}