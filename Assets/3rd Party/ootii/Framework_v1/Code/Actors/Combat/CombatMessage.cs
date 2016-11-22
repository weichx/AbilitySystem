using UnityEngine;
using com.ootii.Actors.LifeCores;
using com.ootii.Collections;
using com.ootii.Messages;

namespace com.ootii.Actors.Combat
{
    /// <summary>
    /// Message
    /// </summary>
    public class CombatMessage : IMessage
    {
        /// <summary>
        /// Message type to send to the MC
        /// </summary>
        public static int MSG_UNKNOWN = 0;
        public static int MSG_COMBATANT_CANCEL = 1;
        public static int MSG_COMBATANT_ATTACK = 2;
        public static int MSG_COMBATANT_BLOCK = 3;
        public static int MSG_COMBATANT_PARRY = 4;
        public static int MSG_COMBATANT_EVADE = 5;
        public static int MSG_ATTACKER_ATTACKED = 101;
        public static int MSG_DEFENDER_IGNORED = 102;
        public static int MSG_DEFENDER_BLOCKED = 103;
        public static int MSG_DEFENDER_PARRIED = 104;
        public static int MSG_DEFENDER_EVADED = 105;
        public static int MSG_DEFENDER_DAMAGED = 107;
        public static int MSG_DEFENDER_KILLED = 108;

        /// <summary>
        /// Combatant that represents the attacker
        /// </summary>
        public GameObject Attacker = null;

        /// <summary>
        /// Combatant that represents the defender
        /// </summary>
        public GameObject Defender = null;

        /// <summary>
        /// Weapon that did the damage
        /// </summary>
        public IWeaponCore Weapon = null;

        /// <summary>
        /// Combat style doing the attack
        /// </summary>
        public ICombatStyle CombatStyle = null;

        /// <summary>
        /// Amount of damage that occured
        /// </summary>
        public float Damage = 0f;

        /// <summary>
        /// Type of damage applied
        /// </summary>
        public int DamageType = 0;

        /// <summary>
        /// Vector that is the direction of the attack's velocity (in world-space)
        /// </summary>
        public Vector3 HitVector = Vector3.zero;

        /// <summary>
        /// Closest bone that was hit
        /// </summary>
        public Transform HitTransform = null;

        /// <summary>
        /// Point (in world-space) where the impact occured
        /// </summary>
        public Vector3 HitPoint = Vector3.zero;

        /// <summary>
        /// Direction (in local-space) that points to the impact hit point from the defender's combat origin
        /// </summary>
        public Vector3 HitDirection = Vector3.zero;

        /// <summary>
        /// Enumeration for the message type. We use strings so they can
        /// be any value.
        /// </summary>
        protected string mType = "";
        public string Type
        {
            get { return mType; }
            set { mType = value; }
        }

        /// <summary>
        /// Sender of the message
        /// </summary>
        protected object mSender = null;
        public object Sender
        {
            get { return mSender; }
            set { mSender = value; }
        }

        /// <summary>
        /// Receiver of the message
        /// </summary>
        protected object mRecipient = null;
        public object Recipient
        {
            get { return mRecipient; }
            set { mRecipient = value; }
        }

        /// <summary>
        /// Time in seconds to delay the processing of the message
        /// </summary>
        protected float mDelay = 0;
        public float Delay
        {
            get { return mDelay; }
            set { mDelay = value; }
        }

        /// <summary>
        /// ID used to help define what the message is for
        /// </summary>
        protected int mID = 0;
        public int ID
        {
            get { return mID; }
            set { mID = value; }
        }

        /// <summary>
        /// Core data of the message
        /// </summary>
        protected object mData = null;
        public object Data
        {
            get { return mData; }
            set { mData = value; }
        }

        /// <summary>
        /// Determines if the message was sent
        /// </summary>
        protected bool mIsSent = false;
        public bool IsSent
        {
            get { return mIsSent; }
            set { mIsSent = value; }
        }

        /// <summary>
        /// Determines if the message was handled
        /// </summary>
        protected bool mIsHandled = false;
        public bool IsHandled
        {
            get { return mIsHandled; }
            set { mIsHandled = value; }
        }

        /// <summary>
        /// Used to ensure frames are sent the next frame (when needed)
        /// </summary>
        protected int mFrameIndex = 0;
        public int FrameIndex
        {
            get { return mFrameIndex; }
            set { mFrameIndex = value; }
        }

        /// <summary>
        /// Clear this instance.
        /// </summary>
        public virtual void Clear()
        {
            mType = "";
            mSender = null;
            mRecipient = null;
            mID = 0;
            mData = null;
            mIsSent = false;
            mIsHandled = false;
            mDelay = 0.0f;

            Attacker = null;
            Defender = null;
            Weapon = null;
            CombatStyle = null;
            HitTransform = null;
        }

        /// <summary>
        /// Release this instance.
        /// </summary>
        public virtual void Release()
        {
            // We should never release an instance unless we're
            // sure we're done with it. So clearing here is fine
            Clear();

            // Reset the sent flags. We do this so messages are flagged as 'completed'
            // and removed by default.
            IsSent = true;
            IsHandled = true;

            // Make it available to others.
            if (this is CombatMessage)
            {
                sPool.Release(this);
            }
        }

        // ******************************** OBJECT POOL ********************************

        /// <summary>
        /// Allows us to reuse objects without having to reallocate them over and over
        /// </summary>
        private static ObjectPool<CombatMessage> sPool = new ObjectPool<CombatMessage>(40, 10);

        /// <summary>
        /// Pulls an object from the pool.
        /// </summary>
        /// <returns></returns>
        public static CombatMessage Allocate()
        {
            // Grab the next available object
            CombatMessage lInstance = sPool.Allocate();

            // Reset the sent flags. We do this so messages are flagged as 'completed'
            // by default.
            lInstance.IsSent = false;
            lInstance.IsHandled = false;

            // For this type, guarentee we have something
            // to hand back tot he caller
            if (lInstance == null) { lInstance = new CombatMessage(); }
            return lInstance;
        }

        /// <summary>
        /// Returns an element back to the pool.
        /// </summary>
        /// <param name="rEdge"></param>
        public static void Release(CombatMessage rInstance)
        {
            if (rInstance == null) { return; }

            // Reset the sent flags. We do this so messages are flagged as 'completed'
            // and removed by default.
            rInstance.IsSent = true;
            rInstance.IsHandled = true;

            // Make it available to others.
            sPool.Release(rInstance);
        }

        /// <summary>
        /// Returns an element back to the pool.
        /// </summary>
        /// <param name="rEdge"></param>
        public static void Release(IMessage rInstance)
        {
            if (rInstance == null) { return; }

            // We should never release an instance unless we're
            // sure we're done with it. So clearing here is fine
            rInstance.Clear();

            // Reset the sent flags. We do this so messages are flagged as 'completed'
            // and removed by default.
            rInstance.IsSent = true;
            rInstance.IsHandled = true;

            // Make it available to others.
            if (rInstance is CombatMessage)
            {
                sPool.Release((CombatMessage)rInstance);
            }
        }
    }
}
