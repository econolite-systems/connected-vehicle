// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using System.Text;
using System.Text.Json;
using Econolite.Ode.Messaging;
using Econolite.Ode.Messaging.Elements;

namespace Econolite.Ode.Services.ConnectedVehicle.Messaging;

public class ConnectedVehicleConsumeResultFactory : IConsumeResultFactory<string, JsonDocument>
{
    public ConsumeResult<string, JsonDocument> BuildConsumeResult(Confluent.Kafka.ConsumeResult<byte[], byte[]> consumeResult)
    {
        var tenantid = Guid.Empty;
        if (consumeResult.Message.Headers.TryGetLastBytes(Consts.TENANT_ID_HEADER, out var buffer))
            Guid.TryParse(Encoding.UTF8.GetString(buffer), out tenantid);

        var type = Consts.TYPE_UNSPECIFIED;
        if (consumeResult.Message.Headers.TryGetLastBytes(Consts.TYPE_HEADER, out buffer))
            type = Encoding.UTF8.GetString(buffer);

        Guid? deviceid = default;
        if (consumeResult.Message.Headers.TryGetLastBytes(Consts.DEVICE_ID_HEADER, out buffer))
            if (Guid.TryParse(Encoding.UTF8.GetString(buffer), out var deviceidguid))
                deviceid = deviceidguid;
        return new ConsumeResult<string, JsonDocument>(tenantid, deviceid, type, consumeResult, _ => string.Empty, new JsonPayloadSpecialist<JsonDocument>());
    }
}