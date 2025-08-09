# USB Device Locking/Unlocking System

This C# program is designed to monitor the USB devices connected to a system and lock or unlock the system based on the presence of a specific USB device.

## Features

1. Monitors USB device insertion and removal events using the Windows Management Instrumentation (WMI) API.
2. Checks for the presence of a specific USB device based on its device ID.
3. Locks the system by blocking all input (keyboard and mouse) when the required USB device is not connected.
4. Unlocks the system by allowing input when the required USB device is connected.
5. Provides visual feedback on the system state (locked or unlocked) using the console.

## Requirements

- .NET Framework or .NET Core
- Windows operating system

## Usage

1. Modify the `requiredDeviceId` variable in the `Main()` method to match the device ID of the USB device you want to use as the unlock key.
2. Compile and run the program.
3. If the required USB device is not connected, the system will be locked, and the program will wait for the device to be connected within 5 seconds.
4. If the required USB device is connected, the system will be unlocked, and the program will start listening for device insertion and removal events.
5. Press any key to stop the event listener and exit the program.

## Note

- This program is designed for educational and demonstration purposes only. It should not be used in a production environment.
- The `shutdown.exe` command in the code is currently commented out. If you want to automatically shut down the system when the required USB device is not connected, you can uncomment this line or modify to your needs..

## Acknowledgments
This program was developed using the .NET Framework and the Windows Management Instrumentation (WMI) API.
