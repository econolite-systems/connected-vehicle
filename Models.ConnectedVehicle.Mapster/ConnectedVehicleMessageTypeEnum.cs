// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Models.ConnectedVehicle
{
    public enum ConnectedVehicleMessageTypeEnum
    {
        /// <summary>
        /// Signal phase and timing message (SPAT) 
        /// </summary>
        SPAT,

        /// <summary>
        /// Basic safety message (BSM)
        /// </summary>
        BSM,

        /// <summary>
        /// Signal request message (SRM)
        /// </summary>
        SRM,

        /// <summary>
        /// Traveler information message (TIM)
        /// </summary>
        TIM
    }
}
