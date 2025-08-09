using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;




class Program
{
    // Block all devices hook
    [DllImport("user32.dll")]
    private static extern bool BlockInput(bool block);

    static void Main()
    {
        // Key deviceID
        string requiredDeviceId = "USB\\VID_FFFF&PID_5678\\9207135152767517146";




        // {WIN API Insert USB Event hook}
        var insertWatcher = new ManagementEventWatcher();
        var insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
        insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceNotification.DeviceInsertedEvent);
        insertWatcher.Query = insertQuery;
        insertWatcher.Start();


        // {WIN API Remove USB Event hook}
        var removeWatcher = new ManagementEventWatcher();
        var removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
        removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceNotification.DeviceRemovedEvent);
        removeWatcher.Query = removeQuery;
        removeWatcher.Start();


        // {Prefetch and check existance}
        var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0");
        var devices = new List<ManagementObject>();

        // Create a temp list to work with
        foreach (ManagementObject device in searcher.Get())
        {
            devices.Add(device);
        }

        


        // Check for existance after preflight
        if (IsUSBDeviceMatchingString(requiredDeviceId, devices))
        {
            BlockInput(false);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("[System State] : ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Unlocked!\n");

            // Enable Event Listener
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Starting Event Listener..");
            Console.ReadKey();
            insertWatcher.Stop();
            removeWatcher.Stop();


        }

        // Not existant
        if (!IsUSBDeviceMatchingString(requiredDeviceId, devices))
        {
            BlockInput(true);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("[System State] : ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Locked!\n");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Please connect key device withhin 5 seconds! \n");
            


            var searcher2 = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0");
            var devices2 = new List<ManagementObject>();
            DateTime startTime = DateTime.Now;
            TimeSpan fiveSeconds = TimeSpan.FromSeconds(5);
            bool isMatchFound = false;

            // 5 Second long condition to satisfy requirement.
            while (DateTime.Now - startTime <= fiveSeconds)
            {
                devices2.Clear();

                // Create a temp list to work with
                foreach (ManagementObject device in searcher2.Get())
                {
                    devices2.Add(device);
                }

                if (IsUSBDeviceMatchingString(requiredDeviceId, devices2))
                {
                    BlockInput(false);
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write("[System State] : ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Detected! \n");
                    devices2.Clear();
                    isMatchFound = true;

                    // Delegate other work th Event Listener
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Starting Event Listener..");
                    Console.ReadKey();
                    insertWatcher.Stop();
                    removeWatcher.Stop();

                    break;
                }
                else
                {
                    BlockInput(true);
                    Console.WriteLine("[!Not matched!]");
                }
            }

            if (!isMatchFound)
            {
                BlockInput(true);
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Condition not satisfied. Shutting down asap.");
                devices2.Clear();
               // System.Diagnostics.Process.Start("shutdown.exe", "/s /f /t 0");
            }
        }

        // Delete the temp list
        devices.Clear();

    }

 
    // Check persistant devices for key device
    private static bool IsUSBDeviceMatchingString(string deviceId, List<ManagementObject> devices)
    {
        foreach (var device in devices)
        {
            string deviceIdFromDevice = device["DeviceID"].ToString();

            if (deviceIdFromDevice == deviceId)
            {
                return true;
            }
        }

        return false;
    }

}


class DeviceNotification
{
    // Block all devices hook
    [DllImport("user32.dll")]
    private static extern bool BlockInput(bool block);

    // Event Listener
    public static void DeviceInsertedEvent(object sender, EventArrivedEventArgs e)
    {
        /// CHANGE THIS ASWELL!
        string requiredDeviceId2 = "USB\\VID_FFFF&PID_5678\\9207135152767517146";


        var instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
        var devID = instance["DeviceId"];
        // Console.WriteLine($"USB device {devID} connected:");
        


        if (devID.ToString() == requiredDeviceId2)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("[System State] : ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Unlocked! \n");

           // Console.WriteLine($"USB device {devID} matches the required device ID");
            BlockInput(false);
        }

        if (devID.ToString() != requiredDeviceId2)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("[System State] : ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Locked!\n");
            BlockInput(true);
        }
    }

    //Event Listener
        public static void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
    {
        BlockInput(true);
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("[System State] : ");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("Locked!\n");
    }
}






