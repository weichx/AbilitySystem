using UnityEngine;

public class SpawnPrefabOnAbilityEvent : AbilityComponent {

    public string prefabUrl;
    
    public override void OnCastCompleted() {
        Debug.Log(ability.castTime.Value);
        GameObject spawned = ResourcePool.Spawn(prefabUrl);
        IAbilityContextAware[] components = spawned.GetComponents<IAbilityContextAware>();
        for(int i = 0; i < components.Length; i++) {
            components[i].SetAbilityContext(context);
        }
    }

}