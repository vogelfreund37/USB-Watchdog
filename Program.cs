using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;

class Program
{
    static void Main()
    {
        var requiredDeviceId = "USB\\VID_FFFF&PID_5678\\9207135152767517146";
        var usbKeyManager = new USBKeyManager(requiredDeviceId);
        usbKeyManager.StartMonitoring();
    }
}

class USBKeyManager
{
    private string _requiredDeviceId;
    private ManagementEventWatcher _insertWatcher;
    private ManagementEventWatcher _removeWatcher;

    public USBKeyManager(string requiredDeviceId)
    {
        _requiredDeviceId = requiredDeviceId;
        InitializeWatchers();
    }

    private void InitializeWatchers()
    {
        _insertWatcher = new ManagementEventWatcher(new WqlEventQuery(
            "SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'"));
        _insertWatcher.EventArrived += (sender, e) => DeviceNotification.DeviceInsertedEvent(sender, e, _requiredDeviceId);

        _removeWatcher = new ManagementEventWatcher(new WqlEventQuery(
            "SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'"));
        _removeWatcher.EventArrived += DeviceNotification.DeviceRemovedEvent;
    }

    public void StartMonitoring()
    {
        var devices = GetConnectedDevices();

        if (IsUSBDeviceMatchingString(_requiredDeviceId, devices))
        {
            UnlockSystem();
        }
        else
        {
            LockSystem();
            CheckForDeviceWithinTimeout(5);
        }

        _insertWatcher.Start();
        _removeWatcher.Start();
    }

    private List<ManagementObject> GetConnectedDevices()
    {
        var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0");
        var devices = new List<ManagementObject>();
        foreach (ManagementObject device in searcher.Get())
        {
            devices.Add(device);
        }

        return devices;
    }

    private bool IsUSBDeviceMatchingString(string deviceId, List<ManagementObject> devices)
    {
        foreach (var device in devices)
        {
            if (device["DeviceID"].ToString() == deviceId)
            {
                return true;
            }
        }
        return false;
    }

    private void CheckForDeviceWithinTimeout(int seconds)
    {
        DateTime startTime = DateTime.Now;
        TimeSpan timeout = TimeSpan.FromSeconds(seconds);
        bool isMatchFound = false;

        while (DateTime.Now - startTime <= timeout)
        {
            var devices = GetConnectedDevices();

            if (IsUSBDeviceMatchingString(_requiredDeviceId, devices))
            {
                UnlockSystem();
                isMatchFound = true;
                break;
            }
        }

        if (!isMatchFound)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Condition not satisfied. Shutting down asap.");
            // System.Diagnostics.Process.Start("shutdown.exe", "/s /f /t 0");
        }
    }

    private void UnlockSystem()
    {
        BlockInput(false);
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("[System State] : ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Unlocked!\n");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Starting Event Listener..");
        Console.ReadKey();
        _insertWatcher.Stop();
        _removeWatcher.Stop();
    }

    private void LockSystem()
    {
        BlockInput(true);
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("[System State] : ");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("Locked!\n");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("Please connect key device within 5 seconds! \n");
    }

    [DllImport("user32.dll")]
    private static extern bool BlockInput(bool block);
}

static class DeviceNotification
{
    [DllImport("user32.dll")]
    private static extern bool BlockInput(bool block);

    public static void DeviceInsertedEvent(object sender, EventArrivedEventArgs e, string requiredDeviceId)
    {
        var instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
        var devID = instance["DeviceId"].ToString();

        if (devID == requiredDeviceId)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("[System State] : ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Unlocked! \n");
            BlockInput(false);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("[System State] : ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Locked!\n");
            BlockInput(true);
        }
    }

    public static void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
    {
        BlockInput(true);
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("[System State] : ");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("Locked!\n");
    }
}