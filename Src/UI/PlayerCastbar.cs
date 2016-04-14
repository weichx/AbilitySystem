//using UnityEngine;
//using System.Collections;
//using AbilitySystem;
//using UnityEngine.UI;
//
//public class PlayerCastbar : MonoBehaviour {
//
//    protected Image castbarFill;
//    protected Image castbarOutline;
//    protected Text castBarText;
//
//    void Start() {
//        castbarFill = transform.Find("Castbar_Fill").GetComponent<Image>();
//        castbarOutline = transform.Find("Castbar_Outline").GetComponent<Image>();
//        castBarText = transform.Find("Castbar_Text").GetComponent<Text>();
//    }
//
//    void Update() {
//        var player = PlayerManager.playerEntity;
//        if (player == null) return;
//        if (player.IsCasting) {
//            if (player.ActiveAbility.IsChanneled) {
//                castbarOutline.enabled = true;
//                castbarFill.fillAmount = player.abilityManager.CastProgress;
//                float elapsed = player.abilityManager.ElapsedCastTime;
//                float total = player.abilityManager.TotalCastTime;
//                castBarText.text = "Channeling " + (total - elapsed).ToString("0.0");
//            }
//            else {
//                castbarOutline.enabled = true;
//                castbarFill.fillAmount = player.abilityManager.CastProgress;
//                string elapsed = player.abilityManager.ElapsedCastTime.ToString("0.0");
//                string total = player.abilityManager.TotalCastTime.ToString("0.0");
//                castBarText.text = "Casting " + elapsed;
//            }
//        }
//        else {
//            castbarOutline.enabled = false;
//            castbarFill.fillAmount = 0f;
//            castBarText.text = "";
//        }
//    }
//}
