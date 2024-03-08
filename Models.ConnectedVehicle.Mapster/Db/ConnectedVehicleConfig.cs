// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Persistence.Common.Interfaces;

namespace Econolite.Ode.Models.ConnectedVehicle.Db
{
    public class ConnectedVehicleConfig : IIndexedEntity<Guid>
    {
        public ConnectedVehicleConfig()
        {

        }

        /// <summary>
        ///     Type of Online Storage: Size or Age
        /// </summary>
        public ConnectedVehicleStorageType OnlineStorageType { get; set; }

        /// <summary>
        ///     Type of Archive Storage: Size or Age
        /// </summary>
        public ConnectedVehicleStorageType ArchiveStorageType { get; set; }

        /// <summary>
        ///     Age of Online Data in Days. Valid range 0 - 365
        /// </summary>
        public short OnlineDays { get; set; }

        /// <summary>
        ///     Size of Online Data in bytes. Valid range 0 - 9,223,372,036,854,775,807
        /// </summary>
        public long OnlineSize { get; set; }

        /// <summary>
        ///     Age of Archived Data in Days. Valid range 0 - 365
        /// </summary>
        public short ArchivedDays { get; set; }

        /// <summary>
        ///     Size of Archived Data in bytes. Valid range 0 - 9,223,372,036,854,775,807
        /// </summary>
        public long ArchivedSize { get; set; }

        /// <summary>
        ///     Start time of Archive/Purge Data Operations
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        ///     End time of Archive/Purge Data Operations
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        ///     Id of the configuration
        /// </summary>
        public Guid Id { get; set; }
    }
}
