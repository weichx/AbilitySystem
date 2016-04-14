using UnityEngine;
using System.Collections;

public delegate void AbilityEvent(Ability2 ability);

public partial class Ability2 {

	public event AbilityEvent OnUse;
	public event AbilityEvent OnChargeConsumed;

	public event AbilityEvent OnCastStarted;
	public event AbilityEvent OnCastUpdated;
	public event AbilityEvent OnCastInterrupted;
	public event AbilityEvent OnCastCompleted;
	public event AbilityEvent OnCastCancelled;
	public event AbilityEvent OnCastFailed;
	public event AbilityEvent OnCastEnded;

	public event AbilityEvent OnChannelStart;
	public event AbilityEvent OnChannelUpdated;
	public event AbilityEvent OnChannelTick;
	public event AbilityEvent OnChannelInterrupted;
	public event AbilityEvent OnChannelCancelled;
	public event AbilityEvent OnChannelEnd;

}

