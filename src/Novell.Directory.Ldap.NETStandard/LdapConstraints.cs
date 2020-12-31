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

using System;
using System.Collections;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     Defines options controlling Ldap operations on the directory.
    ///     An LdapConstraints object is always associated with an LdapConnection
    ///     object; its values can be changed with LdapConnection.setConstraints, or
    ///     overridden by passing an LdapConstraints object to an operation.
    /// </summary>
    /// <seealso cref="LdapConnection.Constraints">
    /// </seealso>
    public class LdapConstraints : IDebugIdentifier
    {
        public virtual DebugId DebugId { get; } = DebugId.ForType<LdapConstraints>();
        private LdapControl[] _controls;
        private Hashtable _properties; // Properties
        private ILdapReferralHandler _refHandler;

        /// <summary>
        ///     Constructs a new LdapConstraints object that specifies the default
        ///     set of constraints.
        /// </summary>
        public LdapConstraints()
        {
        }

        /// <summary>
        ///     Constructs a new LdapConstraints object specifying constraints that
        ///     control wait time, and referral handling.
        /// </summary>
        /// <param name="msLimit">
        ///     The maximum time in milliseconds to wait for results.
        ///     The default is 0, which means that there is no
        ///     maximum time limit. This limit is enforced for an
        ///     operation by the API, not by the server.
        ///     The operation will be abandoned and terminated by the
        ///     API with a result code of LdapException.Ldap_TIMEOUT
        ///     if the operation exceeds the time limit.
        /// </param>
        /// <param name="doReferrals">
        ///     Determines whether to automatically follow
        ///     referrals or not. Specify true to follow
        ///     referrals automatically, and false to throw
        ///     an LdapReferralException if the server responds
        ///     with a referral. False is the default value.
        ///     The way referrals are followed automatically is
        ///     determined by the setting of the handler parameter.
        ///     It is ignored for asynchronous operations.
        /// </param>
        /// <param name="handler">
        ///     The custom authentication handler called when
        ///     LdapConnection needs to authenticate, typically on
        ///     following a referral.  A null may be specified to
        ///     indicate default authentication processing, i.e.
        ///     referrals are followed with anonymous authentication.
        ///     The handler object may be an implemention of either the
        ///     LdapBindHandler or LdapAuthHandler interface.
        ///     The implementation of these interfaces determines how
        ///     authentication is performed when following referrals.
        ///     It is ignored for asynchronous operations.
        /// </param>
        /// <param name="hop_limit">
        ///     The maximum number of referrals to follow in a
        ///     sequence during automatic referral following.
        ///     The default value is 10. A value of 0 means no limit.
        ///     The operation will be abandoned and terminated by the
        ///     API with a result code of
        ///     LdapException.REFERRAL_LIMIT_EXCEEDED if the
        ///     number of referrals in a sequence exceeds the limit.
        ///     It is ignored for asynchronous operations.
        /// </param>
        /// <seealso cref="LdapException.LdapTimeout">
        /// </seealso>
        /// <seealso cref="LdapException.ReferralLimitExceeded">
        /// </seealso>
        /// <seealso cref="LdapException.Referral">
        /// </seealso>
        /// <seealso cref="LdapReferralException">
        /// </seealso>
        /// <seealso cref="ILdapBindHandler">
        /// </seealso>
        /// <seealso cref="ILdapAuthHandler">
        /// </seealso>
        public LdapConstraints(int msLimit, bool doReferrals, ILdapReferralHandler handler, int hopLimit)
        {
            TimeLimit = msLimit;
            ReferralFollowing = doReferrals;
            _refHandler = handler;
            HopLimit = hopLimit;
        }

        /// <summary>
        ///     Returns the maximum number of referrals to follow during automatic
        ///     referral following.  The operation will be abandoned and terminated by
        ///     the API with a result code of LdapException.REFERRAL_LIMIT_EXCEEDED
        ///     if the number of referrals in a sequence exceeds the limit.
        ///     It is ignored for asynchronous operations.
        /// </summary>
        /// <returns>
        ///     The maximum number of referrals to follow in sequence.
        /// </returns>
        /// <seealso cref="HopLimit">
        /// </seealso>
        /// <seealso cref="LdapException.ReferralLimitExceeded">
        /// </seealso>
        /// <summary>
        ///     Sets the maximum number of referrals to follow in sequence during
        ///     automatic referral following.
        /// </summary>
        /// <param name="hop_limit">
        ///     The maximum number of referrals to follow in a
        ///     sequence during automatic referral following.
        ///     The default value is 10. A value of 0 means no limit.
        ///     The operation will be abandoned and terminated by the
        ///     API with a result code of
        ///     LdapException.REFERRAL_LIMIT_EXCEEDED if the
        ///     number of referrals in a sequence exceeds the limit.
        ///     It is ignored for asynchronous operations.
        /// </param>
        /// <seealso cref="LdapException.ReferralLimitExceeded">
        /// </seealso>
        public int HopLimit { get; set; } = 10;

        /// <summary>
        ///     Gets all the properties of the constraints object which has been
        ///     assigned with {@link #setProperty(String, Object)}.
        ///     A value of. <code>null</code> is returned if no properties are defined.
        /// </summary>
        /// <seealso cref="object">
        /// </seealso>
        /// <seealso cref="LdapConnection.GetProperty">
        /// </seealso>
        /// <summary>
        ///     Sets all the properties of the constraints object.
        /// </summary>
        /// <param name="props">
        ///     the properties represented by the Hashtable object to set.
        /// </param>
        internal Hashtable Properties
        {
            get => _properties;

            set => _properties = (Hashtable)value.Clone();
        }

        /// <summary>
        ///     Specified whether or not referrals are followed automatically.
        /// </summary>
        /// <returns>
        ///     True if referrals are followed automatically, or
        ///     false if referrals throw an LdapReferralException.
        /// </returns>
        /// <summary>
        ///     Specifies whether referrals are followed automatically or if
        ///     referrals throw an LdapReferralException.
        ///     Referrals of any type other than to an Ldap server (for example, a
        ///     referral URL other than ldap://something) are ignored on automatic
        ///     referral following.
        ///     The default is false.
        /// </summary>
        /// <param name="doReferrals">
        ///     True to follow referrals automatically.
        ///     False to throw an LdapReferralException if
        ///     the server returns a referral.
        /// </param>
        public bool ReferralFollowing { get; set; }

        /// <summary>
        ///     Returns the maximum number of milliseconds to wait for any operation
        ///     under these constraints.
        ///     If the value is 0, there is no maximum time limit on waiting
        ///     for operation results. The actual granularity of the timeout depends
        ///     platform.  This limit is enforced the the API on an
        ///     operation, not by the server.
        ///     The operation will be abandoned and terminated by the
        ///     API with a result code of LdapException.Ldap_TIMEOUT if the
        ///     operation exceeds the time limit.
        /// </summary>
        /// <returns>
        ///     The maximum number of milliseconds to wait for the operation.
        /// </returns>
        /// <seealso cref="LdapException.LdapTimeout">
        /// </seealso>
        /// <summary>
        ///     Sets the maximum number of milliseconds the client waits for
        ///     any operation under these constraints to complete.
        ///     If the value is 0, there is no maximum time limit enforced by the
        ///     API on waiting for the operation results. The actual granularity of
        ///     the timeout depends on the platform.
        ///     The operation will be abandoned and terminated by the
        ///     API with a result code of LdapException.Ldap_TIMEOUT if the
        ///     operation exceeds the time limit.
        /// </summary>
        /// <param name="msLimit">
        ///     The maximum milliseconds to wait.
        /// </param>
        /// <seealso cref="LdapException.LdapTimeout">
        /// </seealso>
        public int TimeLimit { get; set; }

        /// <summary>
        ///     Returns the controls to be sent to the server.
        /// </summary>
        /// <returns>
        ///     The controls to be sent to the server, or null if none.
        /// </returns>
        /// <seealso cref="Controls">
        /// </seealso>
        public LdapControl[] GetControls()
        {
            return _controls;
        }

        /// <summary>
        ///     Gets a property of the constraints object which has been
        ///     assigned with {@link #setProperty(String, Object)}.
        /// </summary>
        /// <param name="name">
        ///     Name of the property to be returned.
        /// </param>
        /// <returns>
        ///     the object associated with the property,
        ///     or. <code>null</code> if the property is not set.
        /// </returns>
        /// <seealso cref="object">
        /// </seealso>
        /// <seealso cref="LdapConnection.GetProperty">
        /// </seealso>
        public object GetProperty(string name)
        {
            if (_properties == null)
            {
                return null; // Requested property not available.
            }

            return _properties[name];
        }

        /// <summary>
        ///     Returns an object that can process authentication for automatic
        ///     referral handling.
        ///     It may be null.
        /// </summary>
        /// <returns>
        ///     An LdapReferralHandler object that can process authentication.
        /// </returns>
        /*package*/
        internal ILdapReferralHandler getReferralHandler()
        {
            return _refHandler;
        }

        /// <summary>
        ///     Sets a single control to be sent to the server.
        /// </summary>
        /// <param name="control">
        ///     A single control to be sent to the server or
        ///     null if none.
        /// </param>
        public void SetControls(LdapControl control)
        {
            if (control == null)
            {
                _controls = null;
                return;
            }

            _controls = new LdapControl[1];
            _controls[0] = (LdapControl)control.Clone();
        }

        /// <summary>
        ///     Sets controls to be sent to the server.
        /// </summary>
        /// <param name="controls">
        ///     An array of controls to be sent to the server or
        ///     null if none.
        /// </param>
        public void SetControls(LdapControl[] controls)
        {
            if (controls == null || controls.Length == 0)
            {
                _controls = null;
                return;
            }

            _controls = new LdapControl[controls.Length];
            for (var i = 0; i < controls.Length; i++)
            {
                _controls[i] = (LdapControl)controls[i].Clone();
            }
        }

        /// <summary>
        ///     Sets a property of the constraints object.
        ///     No property names have been defined at this time, but the
        ///     mechanism is in place in order to support revisional as well as
        ///     dynamic and proprietary extensions to operation modifiers.
        /// </summary>
        /// <param name="name">
        ///     Name of the property to set.
        /// </param>
        /// <param name="value">
        ///     Value to assign to the property.
        ///     property is not supported.
        ///     @throws NullPointerException if name or value are null.
        /// </param>
        /// <seealso cref="LdapConnection.GetProperty">
        /// </seealso>
        public void SetProperty(string name, object valueRenamed)
        {
            if (_properties == null)
            {
                _properties = new Hashtable();
            }

            _properties.Add(name, valueRenamed);
        }

        /// <summary>
        ///     Specifies the object that will process authentication requests
        ///     during automatic referral following.
        ///     The default is null.
        /// </summary>
        /// <param name="handler">
        ///     An object that implements LdapBindHandler or
        ///     LdapAuthHandler.
        /// </param>
        /// <seealso cref="ILdapAuthHandler">
        /// </seealso>
        /// <seealso cref="ILdapBindHandler">
        /// </seealso>
        public void setReferralHandler(ILdapReferralHandler handler)
        {
            _refHandler = handler;
        }

        /// <summary>
        ///     Clones an LdapConstraints object.
        /// </summary>
        /// <returns>
        ///     An LdapConstraints object.
        /// </returns>
        public object Clone()
        {
            try
            {
                var newObj = MemberwiseClone();
                if (_controls != null)
                {
                    ((LdapConstraints)newObj)._controls = new LdapControl[_controls.Length];
                    _controls.CopyTo(((LdapConstraints)newObj)._controls, 0);
                }

                if (_properties != null)
                {
                    ((LdapConstraints)newObj)._properties = (Hashtable)_properties.Clone();
                }

                return newObj;
            }
            catch (Exception ce)
            {
                throw new Exception("Internal error, cannot create clone", ce);
            }
        }
    }
}
