// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace SensorDecoderModule.Classes
{
    using Newtonsoft.Json;
    using System;
    using System.Buffers.Binary;

    internal static partial class LoraDecoders
    {
        private static string Ascoel_CO868LR_Sensor_Decoder(string devEUI, byte[] payload, byte fport)
        {
            if (fport != 9)
                return JsonConvert.SerializeObject(new DecodePayloadResult().Error = "unsupported fport: " + fport);

            if (payload.Length != 11)
                return JsonConvert.SerializeObject(new DecodePayloadResult().Error = "payload length mismatch.");

            CO868LRResponse decoded = new CO868LRResponse();

            decoded.BatteryType = (payload[0] >> 7);
            decoded.BatteryLevel = (payload[0] & 0b01111111);

            byte[] tempArray = new byte[4];
            tempArray[0] = payload[1];
            tempArray[1] = payload[2];
            tempArray[2] = payload[3];
            tempArray[3] = payload[4];

            decoded.TValue = BitConverter.ToSingle(tempArray);

            tempArray[0] = payload[5];
            tempArray[1] = payload[6];
            tempArray[2] = payload[7];
            tempArray[3] = payload[8];

            decoded.RHValue = BitConverter.ToSingle(tempArray);

            tempArray = new byte[2];
            tempArray[0] = payload[9];
            tempArray[1] = payload[10];

            decoded.CO2Value = BinaryPrimitives.ReadUInt16BigEndian(tempArray);

            return JsonConvert.SerializeObject(decoded);
        }

        private class CO868LRResponse
        {
            [JsonProperty("battery_type")]
            public int BatteryType { get; set; }
            [JsonProperty("battery_level")]
            public int BatteryLevel { get; set; }
            [JsonProperty("t_value")]
            public float TValue { get; set; }
            [JsonProperty("rh_value")]
            public float RHValue { get; set; }
            [JsonProperty("co2_value")]
            public ulong CO2Value { get; set; }
        }
    }
}
