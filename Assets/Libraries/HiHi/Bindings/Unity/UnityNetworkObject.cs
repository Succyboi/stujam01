﻿#if UNITY_EDITOR || UNITY_STANDALONE

using UnityEngine;
using System;

/*
 * ANTI-CAPITALIST SOFTWARE LICENSE (v 1.4)
 *
 * Copyright © 2023 Pelle Bruinsma
 * 
 * This is anti-capitalist software, released for free use by individuals and organizations that do not operate by capitalist principles.
 *
 * Permission is hereby granted, free of charge, to any person or organization (the "User") obtaining a copy of this software and associated documentation files (the "Software"), to use, copy, modify, merge, distribute, and/or sell copies of the Software, subject to the following conditions:
 * 
 * 1. The above copyright notice and this permission notice shall be included in all copies or modified versions of the Software.
 * 
 * 2. The User is one of the following:
 *    a. An individual person, laboring for themselves
 *    b. A non-profit organization
 *    c. An educational institution
 *    d. An organization that seeks shared profit for all of its members, and allows non-members to set the cost of their labor
 *    
 * 3. If the User is an organization with owners, then all owners are workers and all workers are owners with equal equity and/or equal vote.
 * 
 * 4. If the User is an organization, then the User is not law enforcement or military, or working for or under either.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT EXPRESS OR IMPLIED WARRANTY OF ANY KIND, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
namespace HiHi {
    public abstract class UnityNetworkObject : MonoBehaviour, INetworkObject {
        #region NetworkObject Implementation

        Action INetworkObject.OnOwnershipChanged { get; set; }
        Action INetworkObject.OnAbandonmentPolicyChanged { get; set; }

        ISpawnData INetworkObject.OriginSpawnData { get; set; }

        bool INetworkObject.Registered { get; set; }
        ushort INetworkObject.UniqueID { get; set; }

        ushort? INetworkObject.ownerID { get; set; }
        NetworkObjectAbandonmentPolicy INetworkObject.abandonmentPolicy { get; set; } = HiHiConfiguration.DEFAULT_ABANDONMENT_POLICY;
        System.Collections.Generic.Dictionary<byte, SyncObject> INetworkObject.syncObjects { get; set; }

        void INetworkObject.OnRegister() => OnRegister();
        void INetworkObject.OnUnregister() => OnUnregister();
        void INetworkObject.DestroyLocally() => Destroy(gameObject);
        void INetworkObject.Update() => UpdateInstance();

        #endregion

        #region Unity

        public INetworkObject NetworkObject => this as INetworkObject;

        public virtual void OnDestroy() {
            if (NetworkObject.Registered) {
                NetworkObject.UnRegister();
            }
        }

        protected virtual void OnRegister() { }
        protected virtual void OnUnregister() { }
        protected virtual void UpdateInstance() { }

        #endregion
    }
}

#endif