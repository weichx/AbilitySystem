using System;

[Serializable]
public class AIConsiderationLogEntry {
    public string name;
    public float input;
    public float output;
    public string curve;
    //public string type;
    //public float slope;
    //public float exp;
    //public float vShift;
    //public float hShift;
    //public float threshold;

    public AIConsiderationLogEntry(AIConsideration consideration, float input, float output) {
        name = consideration.name;
        this.input = input;
        this.output = output;
        curve = consideration.curve.ToShortString();
        //slope = consideration.curve.slope;
        //exp = consideration.curve.exp;
        //vShift = consideration.curve.vShift;
        //hShift = consideration.curve.hShift;
        //threshold = consideration.curve.threshold;
        //type = consideration.curve.type;
    }

}