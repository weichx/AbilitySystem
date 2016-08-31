using System;
using UnityEngine;

namespace com.ootii.Input
{
    /// <summary>
    /// Interface used to abstract how input is retrieved. Implement from this interface
    /// as needed in order to allow other objects  to process input using your input solution.
    /// </summary>
    public interface IInputSource
    {
        /// <summary>
        /// Helps users of the input source to determine if
        /// they are processing user input
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Set by an external object, it tracks the angle of the
        /// user input compared to the camera's forward direction
        /// Note that this info isn't always reliable as objects using it 
        /// before it's set it will get float.NaN.
        /// </summary>
        float InputFromCameraAngle { get; set; }

        /// <summary>
        /// Set by an external object, it tracks the angle of the
        /// user input compared to the avatars's forward direction
        /// Note that this info isn't always reliable as objects using it 
        /// before it's set it will get float.NaN.
        /// </summary>
        float InputFromAvatarAngle { get; set; }

        /// <summary>
        /// Retrieves horizontal movement from the the input
        /// </summary>
        float MovementX { get; }

        /// <summary>
        /// Retrieves vertical movement from the the input
        /// </summary>
        float MovementY { get; }

        /// <summary>
        /// Squared value of MovementX and MovementY (mX * mX + mY * mY)
        /// </summary>
        float MovementSqr { get; }

        /// <summary>
        /// Retrieves horizontal view movement from the the input
        /// </summary>
        float ViewX { get; }

        /// <summary>
        /// Retrieves vertical view movement from the the input
        /// </summary>
        float ViewY { get; }

        /// <summary>
        /// Determines if the player is currently able to look around
        /// </summary>
        bool IsViewingActivated { get; }

        /// <summary>
        /// Test if a specific key is pressed this frame.
        /// </summary>
        /// <param name="rKey"></param>
        /// <returns></returns>
        bool IsJustPressed(KeyCode rKey);

        /// <summary>
        /// Test if a specific key is pressed this frame.
        /// </summary>
        /// <param name="rKey">Integer representation of the key to test</param>
        /// <returns>Boolean that determines if the action just took place</returns>
        bool IsJustPressed(int rKey);

        /// <summary>
        /// Test if a specific action just occured this frame.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Boolean that determines if the action just took place</returns>
        bool IsJustPressed(string rAction);

        /// <summary>
        /// Test if a specific key is pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rKey">Unity key code that represents the key to test</param>
        /// <returns>Boolean that determines if the key is pressed</returns>
        /// <returns></returns>
        bool IsPressed(KeyCode rKey);

        /// <summary>
        /// Test if a specific key is pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rKey">Integer representation of the key to test</param>
        /// <returns>Boolean that determines if the key is pressed</returns>
        bool IsPressed(int rKey);

        /// <summary>
        /// Tests if a specific action is pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Boolean that determines if the action is taking place</returns>
        bool IsPressed(string rAction);

        /// <summary>
        /// Test if a specific key is released this frame.
        /// </summary>
        /// <param name="rKey"></param>
        /// <returns></returns>
        bool IsJustReleased(KeyCode rKey);

        /// <summary>
        /// Test if a specific key is released this frame.
        /// </summary>
        /// <param name="rKey">Integer representation of the key to test</param>
        /// <returns>Boolean that determines if the action just took place</returns>
        bool IsJustReleased(int rKey);

        /// <summary>
        /// Test if a specific action just occured this frame.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Boolean that determines if the action just took place</returns>
        bool IsJustReleased(string rAction);

        /// <summary>
        /// Test if a specific key is not pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rKey">Unity key code that represents the key to test</param>
        /// <returns>Boolean that determines if the key is pressed</returns>
        /// <returns></returns>
        bool IsReleased(KeyCode rKey);

        /// <summary>
        /// Test if a specific key is not pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rKey">Integer representation of the key to test</param>
        /// <returns>Boolean that determines if the key is pressed</returns>
        bool IsReleased(int rKey);

        /// <summary>
        /// Tests if a specific action is not pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Boolean that determines if the action is taking place</returns>
        bool IsReleased(string rAction);

        /// <summary>
        /// Test for a specific action value.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Float value as determined by the action</returns>
        float GetValue(int rKey);

        /// <summary>
        /// Test for a specific action value.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Float value as determined by the action</returns>
        float GetValue(string rAction);
    }
}
