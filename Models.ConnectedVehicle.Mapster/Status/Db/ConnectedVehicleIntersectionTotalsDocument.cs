// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Models.ConnectedVehicle.Status.Db
{
    public class ConnectedVehicleIntersectionTotalsDocument : ConnectedVehicleMessageTypeCountAndSize
    {
        public string InsersectionId { get; set; } = string.Empty;
        public string IntersectionRegion { get; set; } = string.Empty;
        public string IntersectionName { get; set; } = string.Empty;
    }

}

