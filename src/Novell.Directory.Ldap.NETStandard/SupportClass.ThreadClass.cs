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
// Novell.Directory.Ldap.SupportClass.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

// Support classes replicate the functionality of the original code, but in some cases they are
// substantially different architecturally. Although every effort is made to preserve the
// original architecture of the application in the converted project, the user should be aware that
// the primary goal of these support classes is to replicate functionality, and that at times
// the architecture of the resulting solution may differ somewhat.
//

using System.Threading;

namespace Novell.Directory.Ldap
{

    public partial class SupportClass
    {
        /*******************************/

        /// <summary>
        ///     Support class used to handle threads.
        /// </summary>
        public class ThreadClass : IThreadRunnable
        {
            /// <summary>
            ///     The instance of System.Threading.Thread.
            /// </summary>
            private Thread _threadField;

            /// <summary>
            ///     Initializes a new instance of the ThreadClass class.
            /// </summary>
            public ThreadClass()
            {
                _threadField = new Thread(Run);
            }

            /// <summary>
            ///     Initializes a new instance of the Thread class.
            /// </summary>
            /// <param name="name">The name of the thread.</param>
            public ThreadClass(string name)
            {
                _threadField = new Thread(Run);
                Name = name;
            }

            /// <summary>
            ///     Initializes a new instance of the Thread class.
            /// </summary>
            /// <param name="start">A ThreadStart delegate that references the methods to be invoked when this thread begins executing.</param>
            public ThreadClass(ThreadStart start)
            {
                _threadField = new Thread(start);
            }

            /// <summary>
            ///     Initializes a new instance of the Thread class.
            /// </summary>
            /// <param name="start">A ThreadStart delegate that references the methods to be invoked when this thread begins executing.</param>
            /// <param name="name">The name of the thread.</param>
            public ThreadClass(ThreadStart start, string name)
            {
                _threadField = new Thread(start);
                Name = name;
            }

            /// <summary>
            ///     Gets the current thread instance.
            /// </summary>
            public Thread Instance
            {
                get => _threadField;
                set => _threadField = value;
            }

            /// <summary>
            ///     Gets or sets the name of the thread.
            /// </summary>
            public string Name
            {
                get => _threadField.Name;
                set
                {
                    if (_threadField.Name == null)
                    {
                        _threadField.Name = value;
                    }
                }
            }

            /// <summary>
            ///     Gets a value indicating the execution status of the current thread.
            /// </summary>
            public bool IsAlive => _threadField.IsAlive;

            /// <summary>
            ///     Gets or sets a value indicating whether or not a thread is a background thread.
            /// </summary>
            public bool IsBackground
            {
                get => _threadField.IsBackground;
                set => _threadField.IsBackground = value;
            }

            public bool IsStopping { get; private set; }

            /// <summary>
            ///     This method has no functionality unless the method is overridden.
            /// </summary>
            public virtual void Run()
            {
            }

            /// <summary>
            ///     Causes the operating system to change the state of the current thread instance to ThreadState.Running.
            /// </summary>
            public void Start()
            {
                _threadField.Start();
            }

            ///// <summary>
            ///// Interrupts a thread that is in the WaitSleepJoin thread state
            ///// </summary>
            // public virtual void Interrupt()
            // {
            // threadField.Interrupt();
            // }

            public void Stop()
            {
                IsStopping = true;
            }

            /// <summary>
            ///     Blocks the calling thread until a thread terminates.
            /// </summary>
            public void Join()
            {
                _threadField.Join();
            }

            /// <summary>
            ///     Blocks the calling thread until a thread terminates or the specified time elapses.
            /// </summary>
            /// <param name="miliSeconds">Time of wait in milliseconds.</param>
            public void Join(int miliSeconds)
            {
                lock (this)
                {
                    _threadField.Join(miliSeconds * 10000);
                }
            }

            /// <summary>
            ///     Blocks the calling thread until a thread terminates or the specified time elapses.
            /// </summary>
            /// <param name="miliSeconds">Time of wait in milliseconds.</param>
            /// <param name="nanoSeconds">Time of wait in nanoseconds.</param>
            public void Join(int miliSeconds, int nanoSeconds)
            {
                lock (this)
                {
                    _threadField.Join(miliSeconds * 10000 + nanoSeconds * 100);
                }
            }

            /// <summary>
            ///     Obtain a String that represents the current Object.
            /// </summary>
            /// <returns>A String that represents the current Object.</returns>
            public override string ToString()
            {
                return "Thread[" + Name + "]";
            }

            /// <summary>
            ///     Gets the currently running thread.
            /// </summary>
            /// <returns>The currently running thread.</returns>
            public static ThreadClass Current()
            {
                var currentThread = new ThreadClass
                {
                    Instance = Thread.CurrentThread
                };
                return currentThread;
            }
        }        
    }
}