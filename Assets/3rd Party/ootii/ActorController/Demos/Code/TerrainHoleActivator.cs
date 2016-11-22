using UnityEngine;
using com.ootii.Actors;

namespace com.ootii.Demos
{
    /// <summary>
    /// Script used to allow the actor to go through terrain (or any mesh)
    ///
    /// 1. Add terrain to your scene and put it on a "Terrain" layer. This can be
    ///    any Unity Layer. Just add per normal Unity layers.
    ///
    /// 2. Add a game object with a collider that will act like a door. Using a
    ///    cube works fine. Check "Is Trigger" on the collider.
    ///
    /// 3. Add this component to the "door". Set your terrain on the "Terrain" property.
    ///
    /// 4. Add a Rigid Body to the "door". Uncheck "Use Gravity" and check "Is Kinematic".
    ///
    /// 5. Add a plane or other mesh under the door and opening. This way when we disable collisions
    ///    on the terrain, your character won't fall through empty space.
    ///
    /// 6. On your Actor Controller, ensure your character's Collision Layers and
    ///    Grounding Layers include your new Terrain Layer from step #1.
    ///
    /// 7. Ensure you Actor Controller has "Use Grounding Layers" checked.
    ///
    /// 8. Add a Collider to your Actor Controller and adjust as needed. This can just be a small collider
    ///    or a full capsule collider... use as needed.
    /// 
    /// </summary>
    public class TerrainHoleActivator : MonoBehaviour
    {
        /// <summary>
        /// Terrain collider who is on a different layer
        /// </summary>
        public TerrainCollider Terrain = null;

        /// <summary>
        /// Tracks the layer the terrain is on
        /// </summary>
        private int mTerrainLayer = 0;

        /// <summary>
        /// Used for initialization
        /// </summary>
        void Start()
        {
            mTerrainLayer = Terrain.gameObject.layer;
        }

        /// <summary>
        /// Deactivate the collisions
        /// </summary>
        void OnTriggerEnter(Collider rCollider)
        {
            if (Terrain == null) { return; }
            if (mTerrainLayer == 0) { return; }
            if (rCollider.gameObject == null) { return; }

            ActorController lController = rCollider.gameObject.GetComponent<ActorController>();
            if (lController == null) { return; }

            int lInvertedMasks = ~lController.CollisionLayers;
            lController.CollisionLayers = ~(lInvertedMasks | (1 << mTerrainLayer));

            lInvertedMasks = ~lController.GroundingLayers;
            lController.GroundingLayers = ~(lInvertedMasks | (1 << mTerrainLayer));
        }

        /// <summary>
        /// Reactivate the collisions
        /// </summary>
        void OnTriggerExit(Collider rCollider)
        {
            if (Terrain == null) { return; }
            if (mTerrainLayer == 0) { return; }
            if (rCollider.gameObject == null) { return; }

            ActorController lController = rCollider.gameObject.GetComponent<ActorController>();
            if (lController == null) { return; }

            lController.CollisionLayers |= (1 << mTerrainLayer);
            lController.GroundingLayers |= (1 << mTerrainLayer);
        }
    }
}

