// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace SensorDecoderModule.Classes
{
    using System;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json;

    internal static partial class LoraDecoders
    {
        private static string Nke_Sensor_Decoder(string devEUI, byte[] payload, byte fport)
        {
            // Decode an uplink message from a buffer
            // (array) of bytes to an object of fields.

            // Sample Response:
            /*
            {
	            lora: {
		            port: 125,
		            payload: '110A04020000290998'
	            },
	            zclheader: {
		            report: 'standard',
		            endpoint: 0,
		            cmdID: '0x0A',
		            clusterdID: '0x0402',
		            attributID: '0x0000'
	            },
	            data: {
		            temperature: 24.56
	            }
            }
            */

            dynamic decoded = new JObject();
            decoded.lora = new JObject();

            decoded.lora.port = fport;
            decoded.lora.payload = ConversionHelper.ByteArrayToString(payload);

            if (fport == 125)
            {
                //batch
                var batch = ((payload[0] & 0x01) == 1) ? false : true;

                //trame standard
                if (batch == false)
                {
                    decoded.zclheader = new JObject();
                    decoded.zclheader.report = "standard";

                    //endpoint
                    decoded.zclheader.endpoint = ((payload[0] & 0xE0) >> 5) | ((payload[0] & 0x06) << 2);

                    //command ID
                    var cmdId = payload[1];
                    var cmdIdString = "0x" + cmdId.ToString("X2");
                    decoded.zclheader.cmdID = cmdIdString;

                    //Cluster ID
                    var clusterdId = (payload[2] * 256 + payload[3]);
                    var clusterdIdString = "0x" + clusterdId.ToString("X4");
                    decoded.zclheader.clusterdID = clusterdIdString;

                    // decode report and read atrtribut response
                    if ((cmdId == 0x0a) || (cmdId == 0x01))
                    {
                        decoded.data = new JObject();

                        //Attribut ID 
                        var attributId = payload[4] * 256 + payload[5];
                        var attributIdString = "0x" + attributId.ToString("X4");
                        decoded.zclheader.attributID = attributIdString;

                        //data index start
                        int index = 0;

                        if (cmdId == 0x0a)
                        {
                            index = 7;
                        }

                        if (cmdId == 0x01)
                        {
                            index = 8;
                            decoded.zclheader.status = payload[6];
                        }

                        //temperature
                        if ((clusterdId == 0x0402) && (attributId == 0x0000))
                        {
                            decoded.data.temperature = Convert.ToDouble(payload[index] * 256 + payload[index + 1]) / 100; // removed UintToInt conversion
                        }

                        //humidity
                        if ((clusterdId == 0x0405) && (attributId == 0x0000))
                        {
                            decoded.data.humidity = Convert.ToDouble(payload[index] * 256 + payload[index + 1]) / 100;
                        }

                        //binary input counter
                        if ((clusterdId == 0x000f) && (attributId == 0x0402))
                        {
                            decoded.data.counter = Convert.ToInt32(payload[index] * 256 * 256 * 256 + payload[index + 1] * 256 * 256 + payload[index + 2] * 256 + payload[index + 3]);
                        }

                        // binary input present value
                        if ((clusterdId == 0x000f) && (attributId == 0x0055))
                        {
                            decoded.data.pin_state = (payload[index] == 1) ? true : false;
                        }

                        //multistate output
                        if ((clusterdId == 0x0013) && (attributId == 0x0055))
                        {
                            decoded.data.value = payload[index];
                        }

                        // on/off present value
                        if ((clusterdId == 0x0006) && (attributId == 0x0000))
                        {
                            decoded.data.state = (payload[index] == 1) ? "ON" : "OFF";
                        }

                        // multibinary input present value
                        if ((clusterdId == 0x8005) && (attributId == 0x0000))
                        {
                            decoded.data.pin_state_1 = ((payload[index + 1] & 0x01) == 0x01);
                            decoded.data.pin_state_2 = ((payload[index + 1] & 0x02) == 0x02);
                            decoded.data.pin_state_3 = ((payload[index + 1] & 0x04) == 0x04);
                            decoded.data.pin_state_4 = ((payload[index + 1] & 0x08) == 0x08);
                            decoded.data.pin_state_5 = ((payload[index + 1] & 0x10) == 0x10);
                            decoded.data.pin_state_6 = ((payload[index + 1] & 0x20) == 0x20);
                            decoded.data.pin_state_7 = ((payload[index + 1] & 0x40) == 0x40);
                            decoded.data.pin_state_8 = ((payload[index + 1] & 0x80) == 0x80);
                            decoded.data.pin_state_9 = ((payload[index] & 0x01) == 0x01);
                            decoded.data.pin_state_10 = ((payload[index] & 0x02) == 0x02);
                        }

                        //analog input
                        if ((clusterdId == 0x000c) && (attributId == 0x0055))
                        {
                            decoded.data.analog = Convert.ToDouble(payload[index] * 256 * 256 * 256 + payload[index + 1] * 256 * 256 + payload[index + 2] * 256 + payload[index + 3]);
                        }

                        //simple metering
                        if ((clusterdId == 0x0052) && (attributId == 0x0000))
                        {
                            decoded.data.active_energy_Wh = Convert.ToInt32(payload[index + 1] * 256 * 256 + payload[index + 2] * 256 + payload[index + 3]);
                            decoded.data.reactive_energy_Varh = Convert.ToInt32(payload[index + 4] * 256 * 256 + payload[index + 5] * 256 + payload[index + 6]);
                            decoded.data.nb_samples = (payload[index + 7] * 256 + payload[index + 8]);
                            decoded.data.active_power_W = Convert.ToInt32(payload[index + 9] * 256 + payload[index + 10]);
                            decoded.data.reactive_power_VAR = Convert.ToInt32(payload[index + 11] * 256 + payload[index + 12]);
                        }
                        // configuration node power desc
                        if ((clusterdId == 0x0050) && (attributId == 0x0006))
                        {
                            var index2 = index + 3;

                            if ((payload[index + 2] & 0x01) == 0x01)
                            {
                                decoded.data.main_or_external_voltage = Convert.ToDouble(payload[index2] * 256 + payload[index2 + 1]) / 100;
                                index2 += 2;
                            }

                            if ((payload[index + 2] & 0x02) == 0x02)
                            {
                                decoded.data.rechargeable_battery_voltage = Convert.ToDouble(payload[index2] * 256 + payload[index2 + 1]) / 100;
                                index2 += 2;
                            }

                            if ((payload[index + 2] & 0x04) == 0x04)
                            {
                                decoded.data.disposable_battery_voltage = Convert.ToDouble(payload[index2] * 256 + payload[index2 + 1]) / 100;
                                index2 += 2;
                            }

                            if ((payload[index + 2] & 0x08) == 0x08)
                            {
                                decoded.data.solar_harvesting_voltage = Convert.ToDouble(payload[index2] * 256 + payload[index2 + 1]) / 100;
                                index2 += 2;
                            }
                            if ((payload[index + 2] & 0x10) == 0x10)
                            {
                                decoded.data.tic_harvesting_voltage = Convert.ToDouble(payload[index2] * 256 + payload[index2 + 1]) / 100;
                                index2 += 2;
                            }
                        }
                    }

                    //decode configuration response
                    if (cmdId == 0x07)
                    {
                        //AttributID
                        var attributID = payload[6] * 256 + payload[7];
                        decoded.zclheader.attributID = "0x" + attributID.ToString("X4");

                        //status
                        decoded.zclheader.status = payload[4];

                        //batch
                        decoded.zclheader.batch = payload[5];
                    }


                    //decode read configuration response
                    if (cmdId == 0x09)
                    {
                        //AttributID
                        var attributID = Convert.ToInt32(payload[6] * 256 + payload[7]);
                        decoded.zclheader.attributID = "0x" + attributID.ToString("X4");

                        //status
                        decoded.zclheader.status = payload[4];

                        //batch
                        decoded.zclheader.batch = payload[5];

                        //AttributType
                        decoded.zclheader.attribut_type = payload[8];

                        //min
                        decoded.zclheader.min = new JObject();

                        if ((payload[9] & 0x80) == 0x80)
                        {
                            decoded.zclheader.min.value = Convert.ToInt32((payload[9] - 0x80) * 256 + payload[10]);
                            decoded.zclheader.min.unity = "minutes";
                        }
                        else
                        {
                            decoded.zclheader.min.value = Convert.ToInt32(payload[9] * 256 + payload[10]);
                            decoded.zclheader.min.unity = "seconds";
                        }

                        //max
                        decoded.zclheader.max = new JObject();
                        if ((payload[9] & 0x80) == 0x80)
                        {
                            decoded.zclheader.max.value = Convert.ToInt32((payload[9] - 0x80) * 256 + payload[10]);
                            decoded.zclheader.max.unity = "minutes";
                        }
                        else
                        {
                            decoded.zclheader.max.value = Convert.ToInt32(payload[9] * 256 + payload[10]);
                            decoded.zclheader.max.unity = "seconds";
                        }

                        decoded.lora.payload = "";
                    }
                }
                else
                {
                    decoded.batch = new JObject();
                    decoded.batch.report = "batch";
                }
            }

            return JsonConvert.SerializeObject(decoded);
        }
    }
}
