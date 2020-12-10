# IoTDataTranslator

> :warning: The Azure function uses .NET Core 3.1. Azure functions is not implemented for .NET 5.
> Note that the libraries/packages (declared in the .csproj) file must use versions compatible
> with .NET Core 3.1

> :information_source: I've started to work under the assumption that code should be
> self-explanatory. The code should not contain things that is so \_clever\* that an explanation is
> needed. You would probably not remember or understad what you have done after 1 year. Such code if
> used will be explained ;-)

## Explanation

This Azure function is part of the IoT message chain used by Sarpsborg kommune. The function
intercepts incomming IoT messages from Azure IoTHub and delivers the message to a Azure EventHub.
Internally, the message is stripped of unnecessary parts, data from the DeviceTwin is injected and
the binary data is decoded into the Json data format adhering to Json formatting rules.

Internally the Azure function uses the Microsoft.Test.Json library and not Newtonsofts Json library.
The incomming Json is deserialized into corresponding classes. The Azure function utilizes
MemoryCache to store DeviceTwin data.

## Installation

Eventually the function will be deployed from GitHub directly to Azure. You must also add the
following Azure environment variables: `IoTHubConnection IoTHubEndpoint EventHubConnection'
corresponding to the connection information for the Azure IoTHub data, devicetwin data and EventHub
respectively.

## Development

My current development environment uses Visual Studio Code with a Powershell commandline using
dotnet, func, nuget, git commandline utilities. The code should function with a pure Visual Studio Code
or Visual Studio environment also (not tested).
