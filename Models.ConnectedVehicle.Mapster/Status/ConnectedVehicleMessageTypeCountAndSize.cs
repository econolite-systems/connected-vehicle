// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Models.ConnectedVehicle.Status
{
    public class ConnectedVehicleMessageTypeCountAndSize: ConnectedVehicleMessageCountAndSize
    {
        /// <summary>
        /// The type of connected vehicle data.
        /// </summary>
        public ConnectedVehicleMessageTypeEnum Type { get; set; }
    }
}
