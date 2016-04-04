using UnityEngine;
using System.Collections;
using AbilitySystem;

public class CharacterAttributes : MonoBehaviour {

    public EntityAttribute strength;
    public EntityAttribute dexterity;
    public EntityAttribute intellect;
    public EntityAttribute charisma;
    public EntityAttribute speed;

    [Header("Saves")]
    public EntityAttribute willpower;
    public EntityAttribute fortitude;
    public EntityAttribute avoidance;

}
