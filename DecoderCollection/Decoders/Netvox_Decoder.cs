// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace SensorDecoderModule.Classes
{
    using Newtonsoft.Json;
    using System;

    internal static partial class LoraDecoders
    {
        private static string Netvox_R311W_Sensor_Decoder(string devEUI, byte[] payload, byte fport)
        {
            if (payload.Length != 11)
                return JsonConvert.SerializeObject(new DecodePayloadResult().Error = "payload length mismatch.");

            NetvoxR311WResponse decoded = new NetvoxR311WResponse();

            byte[] tempArray = new byte[5];

            decoded.Version = payload[0];
            decoded.DeviceType = payload[1];
            decoded.ReportType = payload[2];
            decoded.Battery = Convert.ToSingle(payload[3]) / 10;
            decoded.Water1Leak = payload[4] == 1 ? true : false;
            decoded.Water2Leak = payload[5] == 1 ? true : false;

            tempArray[0] = payload[6];
            tempArray[1] = payload[7];
            tempArray[2] = payload[8];
            tempArray[3] = payload[9];
            tempArray[4] = payload[10];

            decoded.Reserved = ConversionHelper.ByteArrayToString(tempArray);

            return JsonConvert.SerializeObject(decoded);
        }

        private class NetvoxR311WResponse
        {
            [JsonProperty("version")]
            public int Version { get; set; }
            [JsonProperty("device_type")]
            public int DeviceType { get; set; }
            [JsonProperty("report_type")]
            public int ReportType { get; set; }
            [JsonProperty("battery")]
            public float Battery { get; set; }
            [JsonProperty("water_1_leak")]
            public bool Water1Leak { get; set; }
            [JsonProperty("water_2_leak")]
            public bool Water2Leak { get; set; }
            [JsonProperty("reserved")]
            public string Reserved { get; set; }
        }
    }
}
