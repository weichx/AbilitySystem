using UnityEngine;
using Intelligence;

public class PlayerAction_UseSkill : PlayerCharacterAction {

    private Ability ability;
    private ContextCreationStatus status;
    private PlayerAbilityContextCreator contextCreator;

    public PlayerAction_UseSkill(Ability ability, PlayerAbilityContextCreator contextCreator) {
        this.ability = ability;
        this.contextCreator = contextCreator;
    }

    public override void OnStart() {
        status = ContextCreationStatus.Building;
        contextCreator.Setup(entity, ability);
    }

    public override CharacterActionStatus OnUpdate() {

        if (status == ContextCreationStatus.Building) {

            status = contextCreator.UpdateContext();
            if (status == ContextCreationStatus.Completed) {
                ability.Use(contextCreator.GetContext());
                contextCreator.Reset();
            }
            else if(status == ContextCreationStatus.Cancelled) {
                contextCreator.Reset();
            }

        }
        else {
            ability.UpdateCast();
        }

        switch (status) {

            case ContextCreationStatus.Building:
                return CharacterActionStatus.Running;

            case ContextCreationStatus.Completed:
                return ability.IsCasting ? CharacterActionStatus.Running : CharacterActionStatus.Completed;

            case ContextCreationStatus.Cancelled:
                return CharacterActionStatus.Cancelled;

            default:
                return CharacterActionStatus.Cancelled;
        }
    }

    public override void OnCancel() {
        contextCreator.Reset();
        if (ability.IsCasting) ability.CancelCast();
    }

    public override void OnInterrupt() {
        contextCreator.Reset();
        if (ability.IsCasting) ability.CancelCast();
    }
}