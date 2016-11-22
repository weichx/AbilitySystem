using System;

namespace com.ootii.Messages
{
    /// <summary>
    /// Defines the basic properties required for
    /// a message to be sent through the messenger
    /// </summary>
	public interface IMessage
	{
        /// <summary>
        /// Enumeration for the message type. We use strings so they can be any value.
        /// </summary>
        String Type { get; set; }

        /// <summary>
        /// Sender of the message
        /// </summary>
        object Sender { get; set; }

        /// <summary>
        /// Receiver of the message
        /// </summary>
        object Recipient { get; set; }

        /// <summary>
        /// Time in seconds to delay the processing of the message
        /// </summary>
        float Delay { get; set; }

        /// <summary>
        /// ID used to help define what the message is for
        /// </summary>
        int ID { get; set; }

        /// <summary>
        /// Core data of the message
        /// </summary>
        object Data { get; set; }

        /// <summary>
        /// Determines if the message was sent
        /// </summary>
        Boolean IsSent { get; set; }

        /// <summary>
        /// Determines if the message was handled
        /// </summary>
        Boolean IsHandled { get; set; }

        /// <summary>
        /// Used to ensure messages are sent next frame (when needed)
        /// </summary>
        int FrameIndex { get; set; }
		
		/// <summary>
		/// Clear this instance.
		/// </summary>
		void Clear();

        /// <summary>
        /// Used to release the message once it has been sent
        /// </summary>
        void Release();
    }
}
