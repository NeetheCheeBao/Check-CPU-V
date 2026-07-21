using System;
using System.Management;
using System.Runtime.InteropServices;

namespace CheckCpuV.Services;

public sealed class CpuInfo
{
    public string ProcessorName { get; init; } = "Unknown";
    public string Manufacturer { get; init; } = "Unknown";
    public bool IsAmd { get; init; }
    public bool IsIntel { get; init; }
    public string ProcessorArch { get; init; } = "Unknown";
    public string OsArch { get; init; } = "Unknown";
    public bool VirtSupported { get; init; }
    public bool VirtEnabled { get; init; }
    public bool DepEnabled { get; init; }
    public bool SlatSupported { get; init; }
    public bool VmMonitorMode { get; init; }
    public bool HypervisorPresent { get; init; }
    public string VirtBrandName { get; init; } = "Virtualization";
    public string StatusMessage { get; init; } = string.Empty;
    public bool StatusOk { get; init; }
}

public static class CpuDetector
{
    [DllImport("kernel32.dll")]
    private static extern int GetSystemDEPPolicy();

    public static CpuInfo Detect()
    {
        string name = "Unknown Processor";
        string manufacturer = "Unknown";
        bool virtFirmware = false;
        bool slat = false;
        bool vmm = false;
        ushort dataWidth = 0;
        ushort architecture = 0;

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                name = obj["Name"]?.ToString()?.Trim() ?? name;
                manufacturer = obj["Manufacturer"]?.ToString()?.Trim() ?? manufacturer;
                virtFirmware = ToBool(obj["VirtualizationFirmwareEnabled"]);
                slat = ToBool(obj["SecondLevelAddressTranslationExtensions"]);
                vmm = ToBool(obj["VMMonitorModeExtensions"]);
                dataWidth = ToUShort(obj["DataWidth"]);
                architecture = ToUShort(obj["Architecture"]);
                break;
            }
        }
        catch
        {
        }

        bool hypervisor = false;
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT HypervisorPresent FROM Win32_ComputerSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                hypervisor = ToBool(obj["HypervisorPresent"]);
                break;
            }
        }
        catch
        {
        }

        bool isAmd = ContainsIgnoreCase(manufacturer, "AMD")
                     || ContainsIgnoreCase(name, "AMD")
                     || ContainsIgnoreCase(name, "Ryzen")
                     || ContainsIgnoreCase(name, "Athlon")
                     || ContainsIgnoreCase(name, "EPYC");

        bool isIntel = ContainsIgnoreCase(manufacturer, "Intel")
                       || ContainsIgnoreCase(name, "Intel");

        string virtBrand = isAmd ? "AMD-v" : isIntel ? "Intel VT-x" : "Virtualization";

        string procArch = dataWidth switch
        {
            64 => "64bit",
            32 => "32bit",
            _ => architecture switch
            {
                0 => "32bit",
                9 => "64bit",
                12 => "ARM64",
                _ => Environment.Is64BitProcess ? "64bit" : "32bit"
            }
        };

        string osArch = Environment.Is64BitOperatingSystem ? "X64" : "X86";

        bool virtSupported = slat || vmm || virtFirmware || hypervisor
                             || (dataWidth >= 64 && (isAmd || isIntel));

        bool virtEnabled = virtFirmware || hypervisor;

        bool dep = false;
        try
        {
            dep = GetSystemDEPPolicy() != 0;
        }
        catch
        {
            dep = true;
        }

        bool statusOk = virtSupported && virtEnabled;
        string message;
        if (virtSupported && virtEnabled)
            message = "此处逻辑支持虚拟化技术，BIOS中已启用。";
        else if (virtSupported && !virtEnabled)
            message = "此处逻辑支持虚拟化技术，但BIOS中未启用。";
        else
            message = "此处逻辑不支持虚拟化技术。";

        return new CpuInfo
        {
            ProcessorName = name,
            Manufacturer = manufacturer,
            IsAmd = isAmd,
            IsIntel = isIntel,
            ProcessorArch = procArch,
            OsArch = osArch,
            VirtSupported = virtSupported,
            VirtEnabled = virtEnabled,
            DepEnabled = dep,
            SlatSupported = slat,
            VmMonitorMode = vmm,
            HypervisorPresent = hypervisor,
            VirtBrandName = virtBrand,
            StatusMessage = message,
            StatusOk = statusOk
        };
    }

    private static bool ContainsIgnoreCase(string text, string value)
        => text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;

    private static bool ToBool(object? value)
    {
        if (value is null) return false;
        if (value is bool b) return b;
        if (value is int i) return i != 0;
        if (value is uint u) return u != 0;
        if (bool.TryParse(value.ToString(), out var parsed)) return parsed;
        return false;
    }

    private static ushort ToUShort(object? value)
    {
        if (value is null) return 0;
        try { return Convert.ToUInt16(value); }
        catch { return 0; }
    }
}
