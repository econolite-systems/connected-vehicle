// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Models.ConnectedVehicle.Status
{
    public class ConnectedVehicleRepositoryTypeCountAndSize: ConnectedVehicleMessageCountAndSize
    {
        /// <summary>
        /// The type of connected vehicle repository
        /// </summary>
        public ConnectedVehicleRepositoryTypeEnum Type { get; set; }
    }
}
