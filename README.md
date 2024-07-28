# MultiBlox
 Utility to remove Roblox singleton event so you can run multiple instances of Roblox.

## Download
Get the latest release here: [Releases](https://github.com/rasp8erries/multiblox-cs/releases/tag/release)

## Usage
- SEE REQUIREMENTS BELOW FIRST
- Launch an instance of Roblox
- Run the "MultiBlox.exe" 
- Click 'Yes' for the UAC prompt (Admin is required!) 
- Console output will indicate if successful

![MultiBlox Success](/images/multiblox-success.png)

## How it works
This uses an utility app provided by [SysInternals - Handle](https://learn.microsoft.com/en-us/sysinternals/downloads/handle). 

So all this MultiBlox app really does is call the "handle" utility to find the roblox singleton event handle. Then it calls the utility again to close that handle, if found. 

## Why Admin Required
Admin privileges are required in order to query for the handles of another process (Roblox). 

This is why when you run MultiBlox.exe it will first issue the UAC prompt in order to elevate permissions.

## Requirements
- Windows Only
  - sorry, not sure if can offer support on other os
- .Net Runtime v8 
  - You will know this is your problem if no console window comes up after it asks for UAC. 
  - Get it here: [.Net 8.0]("https://dotnet.microsoft.com/en-us/download/dotnet/8.0")
    - Scroll to find "Runtime" heading and download the correct one for your pc (probably x64). 
  - You can also go into Event Viewer and you will see an application error that tells you to download the above.