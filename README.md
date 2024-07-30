# MultiBlox
 Utility to remove Roblox singleton event so you can run multiple instances of Roblox.  

 I made this for my own use because bloxstrap stopped supporting the multi instance feature.  

 And running older version of bloxstrap was starting to become problematic.. 

## Download
Get the latest release here: [Releases](https://github.com/rasp8erries/multiblox-cs/releases/latest)

## Usage
- SEE [REQUIREMENTS](#reqs) BELOW FIRST
- Run the "MultiBlox.exe" 
- Click 'Yes' for UAC elevation, if prompted (Admin is required!) 
- That's it! Will find existing AND future Roblox instances. 
- *(So leave it running while you're alting)*
- If you have problems running it see [Issues](#issues) below. 

![MultiBlox Success](/images/multiblox-success-v1.0.4.png)

## How it works
This uses an utility app provided by [SysInternals - Handle](https://learn.microsoft.com/en-us/sysinternals/downloads/handle). 

So all this MultiBlox app really does is call the "handle" utility to find the roblox singleton event handle. Then it calls the utility again to close that handle, if found. 

Here it is in action with 4 roblox alts on same computer. 

![example-usage-1](/images/example-usage-1.png)

## Why Admin Required
Admin privileges are required in order to query for the handles of another process (Roblox). 

This is why when you run MultiBlox.exe it will first issue the UAC prompt in order to elevate permissions. 

*Unless you have UAC turned off in Windows settings ofc.* 

Otherwise, this will popup when you run MultiBlox. 

![uac-prompt](/images/uac-prompt.png) 

So just click Yes to this to continue. 

## <a name="reqs"></a>Requirements
- Windows Only
  - sorry, not sure if can offer support on other operating systems 
- .Net Runtime v8 
  - You will know this is your problem if no console window comes up after it asks for UAC. 
  - Get it here: [.Net 8.0](https://aka.ms/dotnet-core-applaunch?framework=Microsoft.NETCore.App&framework_version=8.0.0&arch=x64&rid=win10-x64)
  - You can also go into Event Viewer and you will see an application error that tells you to download the above.

## <a name="issues"></a>Issues
### Nothing Opens
You probably need to download/install [.Net Runtime 8.0](https://aka.ms/dotnet-core-applaunch?framework=Microsoft.NETCore.App&framework_version=8.0.0&arch=x64&rid=win10-x64).

### Security Warnings
If you get a Windows Security warning similar to the below, don't be alarmed. Its only happening because I am not a "known publisher" and this app is access system management stuff in order to watch for new Roblox processes. 

![ms-sec-1](/images/ms-security-1.png)![ms-sec-2](/images/ms-security-2.png) 

And it is happening probably because you did not "Unblock" the zip file before extracting.  

**BUT BY ALL MEANS GO AHEAD AND VIRUS SCAN THE ZIP FILE**  

*(I would too, not offended, lol)*  

*You can always clone the repository and build the code yourself if you want to be safe! ;)*

Go to properties on the release zip file, check "Unblock", and click OK. 

![unblock-zip](/images/multiblox-zip-props.png)