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

// Support classes replicate the functionality of the original code, but in some cases they are
// substantially different architecturally. Although every effort is made to preserve the
// original architecture of the application in the converted project, the user should be aware that
// the primary goal of these support classes is to replicate functionality, and that at times
// the architecture of the resulting solution may differ somewhat.
using System.Threading;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     Support class used to handle threads.
    /// </summary>
    internal class ThreadClass
    {
        /// <summary>
        ///     The instance of System.Threading.Thread.
        /// </summary>
        private readonly Thread _threadField;

        /// <summary>
        ///     Initializes a new instance of the ThreadClass class.
        /// </summary>
        internal ThreadClass()
        {
            _threadField = new Thread(Run);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether or not a thread is a background thread.
        /// </summary>
        public bool IsBackground
        {
            set => _threadField.IsBackground = value;
        }

        protected bool IsStopping { get; private set; }

        /// <summary>
        ///     This method has no functionality unless the method is overridden.
        /// </summary>
        protected virtual void Run()
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
    }
}
