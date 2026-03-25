using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace patches.Services;

public sealed class ProcessMemoryService : IDisposable
{
    private IntPtr _processHandle;

    public Process? AttachedProcess { get; private set; }

    public string? ExecutablePath { get; private set; }

    public nint MainModuleBase { get; private set; }

    public int MainModuleSize { get; private set; }

    public bool Attach(string processName)
    {
        Detach();

        var process = Process.GetProcessesByName(processName).FirstOrDefault();
        if (process is null)
        {
            return false;
        }

        var module = EnumerateModules(process.Id)
            .FirstOrDefault(m => string.Equals(m.ModuleName, $"{processName}.exe", StringComparison.OrdinalIgnoreCase));

        if (module is null || module.BaseAddress == IntPtr.Zero || module.Size <= 0)
        {
            return false;
        }

        _processHandle = OpenProcess(ProcessAccessFlags.QueryInformation | ProcessAccessFlags.VMRead, false, process.Id);
        if (_processHandle == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to open Minecraft process.");
        }

        AttachedProcess = process;
        MainModuleBase = module.BaseAddress;
        MainModuleSize = module.Size;
        ExecutablePath = module.ExecutablePath;
        return true;
    }

    public IReadOnlyList<MemoryRegion> ReadLikelyPatchRegions()
    {
        EnsureAttached();

        var headers = ReadBytes(MainModuleBase, 0x1000);
        var peOffset = BitConverter.ToInt32(headers, 0x3C);
        var numberOfSections = BitConverter.ToUInt16(headers, peOffset + 6);
        var sizeOfOptionalHeader = BitConverter.ToUInt16(headers, peOffset + 20);
        var sectionTable = peOffset + 24 + sizeOfOptionalHeader;

        var result = new List<MemoryRegion>();
        for (var i = 0; i < numberOfSections; i++)
        {
            var offset = sectionTable + (40 * i);
            var nameBytes = headers.Skip(offset).Take(8).TakeWhile(b => b != 0).ToArray();
            var name = System.Text.Encoding.ASCII.GetString(nameBytes);

            if (name is not ".text" and not ".rdata" and not ".data")
            {
                continue;
            }

            var virtualSize = BitConverter.ToInt32(headers, offset + 8);
            var virtualAddress = BitConverter.ToInt32(headers, offset + 12);
            if (virtualSize <= 0)
            {
                continue;
            }

            var baseAddress = MainModuleBase + virtualAddress;
            result.Add(new MemoryRegion(name, baseAddress, ReadBytes(baseAddress, virtualSize)));
        }

        if (result.Count == 0)
        {
            result.Add(new MemoryRegion("full", MainModuleBase, ReadBytes(MainModuleBase, MainModuleSize)));
        }

        return result;
    }

    private byte[] ReadBytes(nint address, int count)
    {
        EnsureAttached();
        var buffer = new byte[count];
        if (!ReadProcessMemory(_processHandle, address, buffer, count, out var read) || read <= 0)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to read Minecraft memory.");
        }

        if (read == count)
        {
            return buffer;
        }

        return buffer.Take(read).ToArray();
    }

    public void Detach()
    {
        if (_processHandle != IntPtr.Zero)
        {
            CloseHandle(_processHandle);
            _processHandle = IntPtr.Zero;
        }

        AttachedProcess = null;
        ExecutablePath = null;
        MainModuleBase = 0;
        MainModuleSize = 0;
    }

    public void Dispose()
    {
        Detach();
    }

    private void EnsureAttached()
    {
        if (_processHandle == IntPtr.Zero || AttachedProcess is null || AttachedProcess.HasExited)
        {
            throw new InvalidOperationException("Minecraft Bedrock is not attached.");
        }
    }

    private static IReadOnlyList<ModuleInfo> EnumerateModules(int processId)
    {
        var result = new List<ModuleInfo>();
        var snapshot = CreateToolhelp32Snapshot(SnapshotFlags.Module | SnapshotFlags.Module32, processId);
        if (snapshot == IntPtr.Zero || snapshot == (IntPtr)(-1))
        {
            return result;
        }

        try
        {
            var entry = MODULEENTRY32W.Create();
            if (!Module32FirstW(snapshot, ref entry))
            {
                return result;
            }

            do
            {
                result.Add(new ModuleInfo(entry.szModule, entry.szExePath, entry.modBaseAddr, (int)entry.modBaseSize));
                entry = MODULEENTRY32W.Create();
            } while (Module32NextW(snapshot, ref entry));
        }
        finally
        {
            CloseHandle(snapshot);
        }

        return result;
    }

    private sealed record ModuleInfo(string ModuleName, string ExecutablePath, IntPtr BaseAddress, int Size);

    [Flags]
    private enum ProcessAccessFlags : uint
    {
        VMRead = 0x0010,
        QueryInformation = 0x0400
    }

    [Flags]
    private enum SnapshotFlags : uint
    {
        Module = 0x00000008,
        Module32 = 0x00000010
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct MODULEENTRY32W
    {
        public uint dwSize;
        public uint th32ModuleID;
        public uint th32ProcessID;
        public uint GlblcntUsage;
        public uint ProccntUsage;
        public IntPtr modBaseAddr;
        public uint modBaseSize;
        public IntPtr hModule;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szModule;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szExePath;

        public static MODULEENTRY32W Create()
        {
            return new MODULEENTRY32W
            {
                dwSize = (uint)Marshal.SizeOf<MODULEENTRY32W>(),
                szModule = string.Empty,
                szExePath = string.Empty
            };
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(ProcessAccessFlags desiredAccess, bool inheritHandle, int processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ReadProcessMemory(IntPtr hProcess, nint lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateToolhelp32Snapshot(SnapshotFlags dwFlags, int th32ProcessID);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool Module32FirstW(IntPtr hSnapshot, ref MODULEENTRY32W lpme);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool Module32NextW(IntPtr hSnapshot, ref MODULEENTRY32W lpme);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);
}

public sealed record MemoryRegion(string Name, nint BaseAddress, byte[] Bytes);
