using UnityEngine;

public class SpawnPrefabOnAbilityEvent : AbilityComponent {

    public string prefabUrl;
    
    public override void OnCastCompleted() {
        GameObject spawned = ResourceManager.Spawn(prefabUrl);
        IAbilityContextAware[] components = spawned.GetComponents<IAbilityContextAware>();
        for(int i = 0; i < components.Length; i++) {
            components[i].SetAbilityContext(context);
        }
    }

}