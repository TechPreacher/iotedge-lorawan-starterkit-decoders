// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace SensorDecoderModule.Classes
{
    using Newtonsoft.Json;
    using System;
    using System.Buffers.Binary;

    internal static partial class LoraDecoders
    {
        private static string Aaeon_LoRa_EndNode_Sensor_Decoder(string devEUI, byte[] payload, byte fport)
        {
            if (payload.Length != 12)
                return JsonConvert.SerializeObject(new DecodePayloadResult().Error = "payload length mismatch.");

            byte[] tempArray = new byte[2];

            AaeonEndNodeResponse decoded = new AaeonEndNodeResponse();

            decoded.NbrMex = payload[0];

            tempArray[0] = payload[1];
            tempArray[1] = payload[2];

            decoded.Temperature = Convert.ToSingle(BinaryPrimitives.ReadUInt16BigEndian(tempArray)) / 10;

            decoded.Humidity = payload[3];

            tempArray[0] = payload[4];
            tempArray[1] = payload[5];

            decoded.BatteryLevel = BinaryPrimitives.ReadUInt16BigEndian(tempArray);

            decoded.Barometer1 = payload[6];
            decoded.Barometer2 = payload[7];
            decoded.Barometer3 = payload[8];

            decoded.AccelerometerX = payload[9];
            decoded.AccelerometerY = payload[10];
            decoded.AccelerometerZ = payload[11];

            return JsonConvert.SerializeObject(decoded);
        }

        private class AaeonEndNodeResponse
        {
            [JsonProperty("nbr_mex")]
            public int NbrMex { get; set; }
            [JsonProperty("temperature")]
            public float Temperature { get; set; }
            [JsonProperty("humidity")]
            public int Humidity { get; set; }
            [JsonProperty("battery_level")]
            public int BatteryLevel { get; set; }
            [JsonProperty("barometer_1")]
            public int Barometer1 { get; set; }
            [JsonProperty("barometer_2")]
            public int Barometer2 { get; set; }
            [JsonProperty("barometer_3")]
            public int Barometer3 { get; set; }
            [JsonProperty("accelerometer_x")]
            public int AccelerometerX { get; set; }
            [JsonProperty("accelerometer_y")]
            public int AccelerometerY { get; set; }
            [JsonProperty("accelerometer_z")]
            public int AccelerometerZ { get; set; }
        }
    }
}
