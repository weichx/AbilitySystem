//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;

//public class KeyChord {
//    KeyCode[] keys;
//}

//public class ActionbarSlot : MonoBehaviour {
//    public string entityId;
//    public Ability ability;
//    public KeyCode hotKey;

//    private string abilityId;

//    void Start() {
//        //ability = entity.GetAbility(abilityId);
//    }

//    void Update() {
//        if (ability == null) {
//            LoadAbility();
//            return;
//        }
//        //return;
//        //todo this should be implemented using key chords in a tree 
//        //if (ability.Usable() && Input.GetKey(hotKey)) {
//        //    ability.Use();
//        //}
//        //todo only allow 1 ability to run at a time
//        if (Input.GetKeyDown(KeyCode.Alpha1)) {
//         //   ability.Use();
//        }
//    }

//    public void SetAbility(string abilityId) {
//        this.abilityId = abilityId;//todo get ability
//    }

//    private void LoadAbility() {
//        //if (abilityId == null) return;
//        ////todo this goes away,use entity database
//        //var playerObj = GameObject.FindGameObjectWithTag("Player");
//        //ability = playerObj.GetComponent<AbilityManager>().GetAbility(abilityId);
//        //if (ability == null) return;
//        //GetComponent<Image>().sprite = ability.uiSprite;
//    }
//}