using System;
using System.Collections.Generic;
using Intelligence;

[Serializable]
public class DecisionSet : EntitySystemBase {

	public string name;
	public List<Decision> decisions;

	public DecisionSet() {
		decisions = new List<Decision>();
	}

}