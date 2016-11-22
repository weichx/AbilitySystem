using UnityEngine;
using Intelligence;
using EntitySystem;

public class SpawnPrefab : AbilityComponent<Context> {

    public GameObject prefab;
    public bool spawnAtPoint = true;

    public override void OnCastCompleted() {
        if (prefab == null) return;
        MultiPointContext multiPoint = ability.GetContext<MultiPointContext>();
        GameObject spawned = null;
        Vector3 position = ability.Caster.transform.position;
        Quaternion rotation = Quaternion.identity;
        if (spawnAtPoint && multiPoint != null) {
            position = multiPoint.points[0];
        }
        spawned = Object.Instantiate(prefab, position, rotation) as GameObject;
        IContextAware[] components = spawned.GetComponents<IContextAware>();
        if (components != null) {
            for (int i = 0; i < components.Length; i++) {
                components[i].SetContext(ability.GetContext());
            }
        }
    }

}