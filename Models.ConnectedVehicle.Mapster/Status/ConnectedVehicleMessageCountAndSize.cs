// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Extensions;

namespace Econolite.Ode.Models.ConnectedVehicle.Status
{
    public class ConnectedVehicleMessageCountAndSize: ConnectedVehicleMessageCount
    {
        /// <summary>
        /// The total size in bytes for the given type
        /// </summary>
        public long ByteSize { get; set; }

        /// <summary>
        /// Converts the byte size to a string format B, KB, MB, GB, TB, PB, EB
        /// </summary>
        public string Size
        {
            get
            {
                return this.ByteSize.AdaptByteSizeToString();
            }
        }
    }
}
