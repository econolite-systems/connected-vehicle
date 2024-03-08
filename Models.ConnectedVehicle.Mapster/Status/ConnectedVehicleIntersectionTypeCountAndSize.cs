// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Models.ConnectedVehicle.Status
{
    public class ConnectedVehicleIntersectionTypeCountAndSize : ConnectedVehicleMessageTypeCountAndSize
    {
        /// <summary>
        /// The unique id for the signal that comes from Mobility configuration and will be mapped via the JPO intersection id.
        /// </summary>
        public Guid SignalId { get; set; } = Guid.Empty;

        /// <summary>
        /// The name of the signal as entered in the Mobility configuration screen
        /// </summary>
        public string SignalName { get; set; } = string.Empty;

        //TODO:  ko - these are stored as strings but will need to be converted to ints for mapping puprposes?
        /// <summary>
        // Part of the interseciton id unique identifier that comes from JPO.  Intersection.Id.Id.  Will be used to map the JPO id to our signal id.
        /// </summary>
        public string IntersectionId { get; set; } = string.Empty;

        /// <summary>
        /// Part of the interseciton id unique identifier that comes from JPO.  Intersection.Id.Region.  Not sure if we need it but there just in case.
        /// </summary>
        public string IntersectionRegion { get; set; } = string.Empty;

        /// <summary>
        /// Part of the JPO intersection data.  Intersection.IntersectionState.name.  Could be used as a backup if our name is empty.
        /// </summary>
        public string IntersectionName { get; set; } = string.Empty;

    }
}
