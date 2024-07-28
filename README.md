# MultiBlox
 Utility to remove Roblox singleton event so you can run multiple instances of Roblox.

## Download
Get the latest release here: [Releases](https://github.com/rasp8erries/multiblox-cs/releases/tag/release)

## Usage
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