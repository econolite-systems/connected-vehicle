// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Models.ConnectedVehicle.Status.Db
{
    public class ConnectedVehicleAzureDailyTotalsDocument : ConnectedVehicleMessageCountAndSize
    {
        public DateTime dayOfYear { get; set; } 
    }

}

