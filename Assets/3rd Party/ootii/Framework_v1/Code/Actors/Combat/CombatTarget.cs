using UnityEngine;

namespace com.ootii.Actors.Combat
{
    /// <summary>
    /// Information about a combat target. Typically the results of a search.
    /// </summary>
    public struct CombatTarget
    {
        /// <summary>
        /// Gives us something to compare to
        /// </summary>
        public static CombatTarget EMPTY = new CombatTarget();

        /// <summary>
        /// Origin of the seek. Typically the attacker's combat center.
        /// </summary>
        public Vector3 SeekOrigin;

        /// <summary>
        /// Combatant that is the target
        /// </summary>
        public ICombatant Combatant;

        /// <summary>
        /// Collider that is the target
        /// </summary>
        public Collider Collider;

        /// <summary>
        /// Closest point on the target's collider's surface from the anchor
        /// </summary>
        public Vector3 ClosestPoint;

        /// <summary>
        /// Distance from the anchor to the closest point
        /// </summary>
        public float Distance;

        /// <summary>
        /// Direction (in world-space) from the anchor to the closest point
        /// </summary>
        public Vector3 Direction;

        /// <summary>
        /// Horizontal angle from the attacker's forward to the closest point
        /// </summary>
        public float HorizontalAngle;

        /// <summary>
        /// Vertical angle from the attacker's forward to the closest point
        /// </summary>
        public float VerticalAngle;

        /// <summary>
        /// Determines if two object are the same
        /// </summary>
        /// <param name="rOther">CombatHit to compare</param>
        /// <returns>Boolean if the combat hit values are equivallent</returns>
        public override bool Equals(object rOther)
        {
            return base.Equals(rOther);
        }

        /// <summary>
        /// Hash code for the instance
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Comparator
        /// </summary>
        public static bool operator ==(CombatTarget c1, CombatTarget c2)
        {
            return c1.Equals(c2);
        }

        /// <summary>
        /// Comparator
        /// </summary>
        public static bool operator !=(CombatTarget c1, CombatTarget c2)
        {
            return !c1.Equals(c2);
        }
    }
}
