using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MelonLoaderDearImGui
{
    internal class Injector
    {
        // API Imports
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out uint lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibraryA(string lpLibFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out uint lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        // Constants
        const uint PROCESS_CREATE_THREAD = 0x0002;
        const uint PROCESS_QUERY_INFORMATION = 0x0400;
        const uint PROCESS_VM_OPERATION = 0x0008;
        const uint PROCESS_VM_WRITE = 0x0020;
        const uint PROCESS_VM_READ = 0x0010;

        // Function to inject the DLL
        public static bool InjectDll(int processId, string dllPath)
        {
            IntPtr hProcess = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, (uint)processId);

            if (hProcess == IntPtr.Zero)
            {
                Log.Error("Failed to open process.");
                return false;
            }

            // Allocate memory in the remote process for the DLL path
            IntPtr lpAddress = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)((dllPath.Length + 1) * Marshal.SizeOf(typeof(char))), 0x1000 | 0x2000, 0x40);

            if (lpAddress == IntPtr.Zero)
            {
                Log.Error("Failed to allocate memory.");
                return false;
            }

            // Write the DLL path to the allocated memory
            byte[] dllPathBytes = Encoding.ASCII.GetBytes(dllPath);
            WriteProcessMemory(hProcess, lpAddress, dllPathBytes, (uint)dllPathBytes.Length, out uint bytesWritten);

            // Get the address of LoadLibraryA
            IntPtr loadLibraryAddr = GetProcAddress(LoadLibraryA("kernel32.dll"), "LoadLibraryA");

            if (loadLibraryAddr == IntPtr.Zero)
            {
                Log.Error("Failed to get LoadLibraryA address.");
                return false;
            }

            // Create a remote thread to execute LoadLibraryA and load the DLL
            IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, loadLibraryAddr, lpAddress, 0, out uint threadId);

            if (hThread == IntPtr.Zero)
            {
                Log.Error("Failed to create remote thread.");
                return false;
            }

            // Wait for the thread to finish
            CloseHandle(hThread);
            CloseHandle(hProcess);

            Log.Error("DLL injected successfully!");
            return true;
        }
    }
}
