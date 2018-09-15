/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

//
// Novell.Directory.Ldap.MessageAgent.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Threading;
using Novell.Directory.Ldap.Utilclass;

namespace Novell.Directory.Ldap
{
    internal sealed class MessageAgent
    {
        public DebugId DebugId { get; } = DebugId.ForType<MessageAgent>();
        private int _indexLastRead;
        private readonly MessageVector _messages = new MessageVector(5);

        /// <summary>
        ///     Get a list of message ids controlled by this agent.
        /// </summary>
        /// <returns>
        ///     an array of integers representing the message ids.
        /// </returns>
        internal int[] MessageIDs
        {
            get
            {
                var size = _messages.Count;
                var ids = new int[size];

                for (var i = 0; i < size; i++)
                {
                    var info = (Message)_messages[i];
                    ids[i] = info.MessageId;
                }

                return ids;
            }
        }

        /// <summary> Get a count of all messages queued.</summary>
        internal int Count
        {
            get
            {
                var count = 0;
                var msgs = _messages.ToArray();
                for (var i = 0; i < msgs.Length; i++)
                {
                    var m = (Message)msgs[i];
                    count += m.Count;
                }

                return count;
            }
        }

        /// <summary>
        ///     empty and return all messages owned by this agent.
        /// </summary>
        private object[] RemoveAll()
        {
            return _messages.RemoveAll();
        }

        /// <summary>
        ///     merges two message agents.
        /// </summary>
        /// <param name="fromAgent">
        ///     the agent to be merged into this one.
        /// </param>
        internal void Merge(MessageAgent fromAgent)
        {
            var msgs = fromAgent.RemoveAll();
            for (var i = 0; i < msgs.Length; i++)
            {
                _messages.Add(msgs[i]);
                ((Message)msgs[i]).Agent = this;
            }

            lock (_messages)
            {
                if (msgs.Length > 1)
                {
                    Monitor.PulseAll(_messages); // wake all threads waiting for messages
                }
                else if (msgs.Length == 1)
                {
                    Monitor.Pulse(_messages); // only wake one thread
                }
            }
        }

        /// <summary>
        ///     Wakes up any threads waiting for messages in the message agent.
        /// </summary>
        internal void SleepersAwake(bool all)
        {
            lock (_messages)
            {
                if (all)
                {
                    Monitor.PulseAll(_messages);
                }
                else
                {
                    Monitor.Pulse(_messages);
                }
            }
        }

        /// <summary>
        ///     Returns true if any responses are queued for any of the agent's messages
        ///     return false if no responses are queued, otherwise true.
        /// </summary>
        internal bool IsResponseReceived()
        {
            var size = _messages.Count;
            var next = _indexLastRead + 1;
            for (var i = 0; i < size; i++)
            {
                if (next == size)
                {
                    next = 0;
                }

                var info = (Message)_messages[next];
                if (info.HasReplies())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Returns true if any responses are queued for the specified msgId
        ///     return false if no responses are queued, otherwise true.
        /// </summary>
        internal bool IsResponseReceived(int msgId)
        {
            try
            {
                var info = _messages.FindMessageById(msgId);
                return info.HasReplies();
            }
            catch (FieldAccessException)
            {
                return false;
            }
        }

        /// <summary>
        ///     Abandon the request associated with MsgId.
        /// </summary>
        /// <param name="msgId">
        ///     the message id to abandon.
        /// </param>
        /// <param name="cons">
        ///     constraints associated with this request.
        /// </param>
        internal void Abandon(int msgId, LdapConstraints cons)
        {
            try
            {
                // Send abandon request and remove from connection list
                var info = _messages.FindMessageById(msgId);
                SupportClass.VectorRemoveElement(_messages, info); // This message is now dead
                info.Abandon(cons, null);
            }
            catch (FieldAccessException ex)
            {
                Logger.Log.LogWarning("Exception swallowed", ex);
            }
        }

        /// <summary> Abandon all requests on this MessageAgent.</summary>
        internal void AbandonAll()
        {
            var size = _messages.Count;

            for (var i = 0; i < size; i++)
            {
                var info = (Message)_messages[i];

                // Message complete and no more replies, remove from id list
                SupportClass.VectorRemoveElement(_messages, info);
                info.Abandon(null, null);
            }
        }

        /// <summary>
        ///     Indicates whether a specific operation is complete.
        /// </summary>
        /// <returns>
        ///     true if a specific operation is complete.
        /// </returns>
        internal bool IsComplete(int msgid)
        {
            try
            {
                var info = _messages.FindMessageById(msgid);
                if (!info.Complete)
                {
                    return false;
                }
            }
            catch (FieldAccessException ex)
            {
                // return true, if no message, it must be complete
                Logger.Log.LogWarning("Exception swallowed", ex);
            }

            return true;
        }

        /// <summary>
        ///     Send a request to the server.  A Message class is created
        ///     for the specified request which causes the message to be sent.
        ///     The request is added to the list of messages being managed by
        ///     this agent.
        /// </summary>
        /// <param name="conn">
        ///     the connection that identifies the server.
        /// </param>
        /// <param name="msg">
        ///     the LdapMessage to send.
        /// </param>
        /// <param name="timeOut">
        ///     the interval to wait for the message to complete or.
        ///     <code>null</code> if infinite.
        /// </param>
        internal void SendMessage(Connection conn, LdapMessage msg, int timeOut, BindProperties bindProps)
        {
            // creating a messageInfo causes the message to be sent
            // and a timer to be started if needed.
            var message = new Message(msg, timeOut, conn, this, bindProps);
            _messages.Add(message);
            message.SendMessage(); // Now send message to server
        }

        /// <summary>
        ///     Returns a response queued, or waits if none queued.
        /// </summary>
        internal object GetLdapMessage(int? msgId)
        {
            object rfcMsg;

            // If no messages for this agent, just return null
            if (_messages.Count == 0)
            {
                return null;
            }

            if (msgId.HasValue)
            {
                // Request messages for a specific ID
                try
                {
                    // Get message for this ID
                    var info = _messages.FindMessageById(msgId.Value);
                    rfcMsg = info.WaitForReply(); // blocks for a response
                    if (!info.AcceptsReplies() && !info.HasReplies())
                    {
                        // Message complete and no more replies, remove from id list
                        SupportClass.VectorRemoveElement(_messages, info);
                        info.Abandon(null, null); // Get rid of resources
                    }

                    return rfcMsg;
                }
                catch (FieldAccessException)
                {
                    // no such message id
                    return null;
                }
            }

            // A msgId was NOT specified, any message will do
            lock (_messages)
            {
                while (true)
                {
                    var next = _indexLastRead + 1;
                    for (var i = 0; i < _messages.Count; i++)
                    {
                        if (next >= _messages.Count)
                        {
                            next = 0;
                        }

                        var info = (Message)_messages[next];
                        _indexLastRead = next++;
                        rfcMsg = info.Reply;

                        // Check this request is complete
                        if (!info.AcceptsReplies() && !info.HasReplies())
                        {
                            // Message complete & no more replies, remove from id list
                            SupportClass.VectorRemoveElement(_messages, info); // remove from list
                            info.Abandon(null, null); // Get rid of resources

                            // Start loop at next message that is now moved
                            // to the current position in the Vector.
                            i -= 1;
                        }

                        if (rfcMsg != null)
                        {
                            // We got a reply
                            return rfcMsg;
                        }
                    } // end for loop */

                    // Messages can be removed in this loop, we we must
                    // check if any messages left for this agent
                    if (_messages.Count == 0)
                    {
                        return null;
                    }

                    // No data, wait for something to come in.
                    Monitor.Wait(_messages);
                } /* end while */
            } /* end synchronized */
        }
    }
}