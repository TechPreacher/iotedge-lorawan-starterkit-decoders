# Azure IoT Edge LoRaWAN Starter Kit

## DecoderSample with Standard Decoders

This repository contains sample implementations a set of decoders for LoRaWAN end devices available on the market and is meant to supplement the [Azure IoT Edge LoRaWAN Starter Kit](https://github.com/Azure/iotedge-lorawan-starterkit).

The [original sample](https://github.com/Azure/iotedge-lorawan-starterkit/tree/master/Samples/DecoderSample) that this repository bases off allows you to create and run your own LoRa message decoder in an independent container running on your LoRa gateway without having to edit the main LoRa Engine.

This modified version can be run directly witout modification if the decoder you need is included. Unnecessary decoders can also be simply deleted.

This description shows you how to get started.

### Included Decoders

The following decoders have been included in this collection:

|Sensor|Image|Source Code Location|Decoder|More Info|
|---|---|---|---|---|
|AAEON LoRa EndNode||[Aaeon_LoRa_EndNode_Decoder.cs](/DecoderCollection/Decoders/Aaeon_LoRa_EndNode_Decoder.cs)|Aaeon_LoRa_EndNode_Sensor_Decoder||   |
|[ASCOEL CO868LR](http://www.ascoel.it/index.php/products/all-products/93-ambiental-control/18-co868lr)|![ASCOEL CO868LR](/Docs/Pictures/ascoel_co868lr.png)|[Ascoel_Decoder.cs](/DecoderCollection/Decoders/Ascoel_Decoder.cs)|Ascoel_CO868LR_Sensor_Decoder|Based on the [lora-device-payloader](https://github.com/tkiraly/lora-device-payloader/blob/master/src/ascoel.js) repo.|
|[DecentLab DL-PR26](https://www.catsensors.com/media/Decentlab/DL-PR26-datasheet.pdf)|![DecentLab DL-PR26](/Docs/Pictures/decentlab_dl-pr26.png)|[DecentLab_Decoder.cs](/DecoderCollection/Decoders/DecentLab_Decoder.cs)|DecentLab_DLPR26_Sensor_Decoder|Based on DecentLab [decentlab-decoders](https://github.com/decentlab/decentlab-decoders/blob/master/LoRaWAN%20Pressure%20(Depth)%20and%20Temperature%20Sensor/DL-PR26D%20(Pmin%3D0.0%2C%20Pmax%3D1.0).cs) repo.|
|[Netvox R311W](http://www.netvox.com.tw/products.asp?pro=r311w)|![Netvox R311W](/Docs/Pictures/netvox_r311w.png)|[Netvox_Decoder.cs](/DecoderCollection/Decoders/DecentLab_Decoder.cs)|Netvox_R311W_Sensor_Decoder|
|[Nke Sensors using ZCL Payload](http://www.nke-watteco.com/gamme/lora-range/)|![Nke sensors](/Docs/Pictures/nke_sensors.png)|[Nke_Decoder.cs](/DecoderCollection/Decoders/Nke_Decoder.cs)|Nke_Sensor_Decoder|Based on NKE's [ZCL payload decoder](http://support.nke-watteco.com/wp-content/uploads/2019/01/decodeZCL_svn4683.js).|








### Preparing and Testing the Docker Image

Create a docker image from your finished solution based on the target architecture and host it in an [Azure Container Registry](https://azure.microsoft.com/en-us/services/container-registry/), on DockerHub or in any other container registry of your choice.

We are using the [Azure IoT Edge for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=vsciot-vscode.azure-iot-edge) extension to build and push the Docker image.

Make sure you are logged in to the Azure Container Registry you are using. Run `docker login <mycontainerregistry>.azurecr.io` on your development machine.

Edit the file [module.json](./module.json) to contain your container registry address, image name and version number:

![Decoder Sample - module.json file](/Docs/Pictures/decodersample-module-json.png)

We provide the Dockerfiles for the following architectures:

- [Dockerfile.amd64](/Samples/DecoderSample/Dockerfile.amd64)
- [Dockerfile.arm32v7](/Samples/DecoderSample/Dockerfile.arm32v7)

To build the Docker image, right-click on the [module.json](./module.json) file and select "Build IoT Edge Module Image" or "Build and Push IoT Edge Module Image". Select the architecture you want to build for (ARM32v7 or AMD64) from the drop-down menu.

To **temporarily test** the container running you decoder using a webbrowser or Postman, you can manually start it in Docker and bind the container's port 80 to a free port on your host machine, like for example 8881.

```bash
docker run --rm -it -p 8881:80 --name decodersample <container registry>/<image>:<tag>
````

You can then use a browser to navigate to:

```
http://localhost:8881/api/DecoderValueSensor?fport=1&payload=QUJDREUxMjM0NQ%3D%3D
```

Replace the name of the basic decoder `DecoderValueSensor` with the name of the decoder you want to use from the list above, i.e. `Nke_Sensor_Decoder`

### Deploying to IoT Edge

If required, add credentials to access your container registry to the IoT Edge device by adding them to IoT Hub &rarr; IoT Edge &rarr; Your Device &rarr; Set Modules &rarr; Container Registry settings.

![Decoder Sample - Edge Module Container Registry Permission](/Docs/Pictures/decodersample-edgepermission.png)

Configure your IoT Edge gateway device to include the custom container. IoT Hub &rarr; IoT Edge &rarr; Your Device &rarr; Set Modules &rarr; Deployment Modules &rarr; Add &rarr; IoT Edge Module. Set the module Name and Image URI, pointing to your image created above.

**Make sure to choose all lowercase letters for the Module Name as the container will be unreachable otherwise!**

![Decoder Sample - Edge Module](/Docs/Pictures/decodersample-edgemodule.png)

To activate the decoder for a LoRa device, navigate to your IoT Hub &rarr; IoT Devices &rarr; Device Details &rarr; Device Twin and set the ```SensorDecoder``` value in the desired properties to: 

```
http://<decoder module name>/api/<DecoderName>
```

**Again make sure to chosse all lowercase letters for the module name to make sure it is reachable.**

![Decoder Sample - LoRa Device Twin](/Docs/Pictures/decodersample-devicetwin.png)

In case the custom decoder is unreachable, throws an error or return invalid JSON, the error message will be shown in your device's messages in IoT Hub.

### Customizing

To add a new decoder, simply copy or reuse  the sample ```DecoderValueSensor``` method from the ```LoraDecoders``` class in [LoraDecoder.cs](/Samples/DecoderSample/Classes/LoraDecoder.cs). You can name the method whatever you like and can create as many decoders as you need by adding new, individual methods to the ```LoraDecoders``` class.

The payload sent to the decoder is passed as byte[] ```payload``` and uint ```fport```.

After writing the code that decodes your message, your method should return a **string containing valid JSON** containing the response to be sent upstream.

```cs
internal static class LoraDecoders
{
    private static string DecoderValueSensor(byte[] payload, uint fport)
    {
        var result = Encoding.ASCII.GetString(payload);
        return JsonConvert.SerializeObject(new { value = result });
    }
}
```

You can test the decoder on your machine by debugging the SensorDecoderModule project in Visual Studio.

When creating a debugging configuration in Visual Studio Code or a ```launchSettings.json``` file in Visual Studio, the default address that the webserver will try to use is ```http://localhost:5000``` or ```https://localhost:5001```. You can override this with any port of your choice.

On launching the debugger you will see a webbrowser with a ```404 Not Found``` error as there is no default document to be served in this Web API app.

LoRa devices usually send byte arrays of data to be decoded.

You will also manually need to [base64-encode](https://www.base64encode.org/) and [URL-encode](https://www.urlencoder.org/) the payload before adding it to the URL parameters.

For example, to test a payload of `ABCDE12345`, you:
- Convert it to a base64 encoded string: `QUJDREUxMjM0NQ==`
- Convert the result to a valid URL parameter: `QUJDREUxMjM0NQ%3D%3D`
- Add this to your test URL.

For the built-in sample decoder ```DecoderValueSensor``` with Visual Studio (Code)'s default settings this would be:

```
http://localhost:5000/api/DecoderValueSensor?fport=1&payload=QUJDREUxMjM0NQ%3D%3D
`````

You can call your decoder at:

```
http://localhost:yourPort/api/<decodername>?fport=<1>&payload=<QUJDREUxMjM0NQ%3D%3D>
```

You should see the result as JSON string.

![Decoder Sample - Debugging on localhost](/Docs/Pictures/decodersample-debugging.png)

When running the solution in a container, the [Kestrel webserver](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-2.1) from .NET Core uses the HTTP default port 80 of the container and does not need to bind it to a port on the host machine as Docker allows for container-to-container communication. IoT Edge automatically creates the required [Docker Network Bridge](https://docs.docker.com/network/bridge/).

