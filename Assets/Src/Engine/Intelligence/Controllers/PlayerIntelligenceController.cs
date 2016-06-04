using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Intelligence {

    class PlayerIntelligenceController : MonoBehaviour {

        void Update() {
            /*
            
            if(Input.GetKey('SomeSkillButton')) {
                PlayerSkillDef psd = definition.skillSet.Get("SomeSkill");
                PlayerContext context = psd.contextBuilder.Create(); //1 context instance per use
                psd.contextBuilder.Start();
                psd.contextBuilder.Update();
                
                if(contextBuilder.IsReady()) {
                    psd.ability.Use(contextBuilder.GetContext()); //might be referenced by a spawned script so we cant destroy it
                    psd.contextBuilder.Reset();
                }
                else if(contextBuilder.IsCancelled()) {
                    activeAbility = null;
                    psd.contextBuilder.Reset();
                }
                
            }
            
            */
        }

    }

}
