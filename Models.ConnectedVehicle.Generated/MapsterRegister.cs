// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using System.Reflection;
using Econolite.Ode.Models.ConnectedVehicle.Db;
using Mapster;

namespace Models.ConnectedVehicle
{
    public class MapsterRegister : ICodeGenerationRegister
    {
        public void Register(CodeGenerationConfig config)
        {
            config.AdaptTo("[name]Dto")
                .ApplyDefaultRule();

            config.AdaptFrom("[name]Add", MapType.Map)
                .ApplyDefaultRule()
                .ForType<ConnectedVehicleConfig>(cfg => { cfg.Ignore(connectedVehicle => connectedVehicle.Id); });

            config.AdaptFrom("[name]Update", MapType.MapToTarget)
                .ApplyDefaultRule();

            config.GenerateMapper("[name]Mapper")
                .ForType<ConnectedVehicleConfig>();
        }
    }

    internal static class RegisterExtensions
    {
        public static AdaptAttributeBuilder ApplyDefaultRule(this AdaptAttributeBuilder builder)
        {
            return builder
                .ForType<ConnectedVehicleConfig>()
                .ExcludeTypes(type => type.IsEnum)
                .AlterType(type => type.IsEnum || Nullable.GetUnderlyingType(type)?.IsEnum == true, typeof(string))
                .ShallowCopyForSameType(true);
        }
    }
}
