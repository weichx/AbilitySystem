using System;
using UnityEngine;

namespace com.ootii.Actors.Combat
{
    /// <summary>
    /// An AttackStyle gives us details about a specific attack. We'll use
    /// these details to determine who can be hit and when.
    /// </summary>
    [Serializable]
    public class AttackStyle : ICombatStyle
    {
        /// <summary>
        /// Unique ID for the attack style (within the list)
        /// </summary>
        public string _Name = "";
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        /// <summary>
        /// Helps define the animation that is tied to the style
        /// </summary>
        public int _ParameterID = 0;
        public int ParameterID
        {
            get { return _ParameterID; }
            set { _ParameterID = value; }
        }

        /// <summary>
        /// Determines if the attack is able to be stopped
        /// </summary>
        public bool _IsInterruptible = true;
        public bool IsInterruptible
        {
            get { return _IsInterruptible; }
            set { _IsInterruptible = value; }
        }

        /// <summary>
        /// Direction of the attack relative to the character's forward
        /// </summary>
        public Vector3 _Forward = Vector3.forward;
        public Vector3 Forward
        {
            get { return _Forward; }
            set { _Forward = value; }
        }

        /// <summary>
        /// Horizontal field-of-attack centered on the Forward. This determines
        /// the horizontal range of the attack.
        /// </summary>
        public float _HorizontalFOA = 120f;
        public float HorizontalFOA
        {
            get { return _HorizontalFOA; }
            set { _HorizontalFOA = value; }
        }

        /// <summary>
        /// Vertical field-of-attack centered on the Forward. This determines
        /// the vertical range of the attack.
        /// </summary>
        public float _VerticalFOA = 90f;
        public float VerticalFOA
        {
            get { return _VerticalFOA; }
            set { _VerticalFOA = value; }
        }

        /// <summary>
        /// Minimum range for the attack (0 means use the combatant + weapon)
        /// </summary>
        public float _MinRange = 0f;
        public float MinRange
        {
            get { return _MinRange; }
            set { _MinRange = value; }
        }

        /// <summary>
        /// Maximum range for the attack (0 means use the combatant + weapon)
        /// </summary>
        public float _MaxRange = 0f;
        public float MaxRange
        {
            get { return _MaxRange; }
            set { _MaxRange = value; }
        }

        /// <summary>
        /// Amount to multiply the damage by
        /// </summary>
        public float _DamageModifier = 1f;
        public float DamageModifier
        {
            get { return _DamageModifier; }
            set { _DamageModifier = value; }
        }
    }
}
