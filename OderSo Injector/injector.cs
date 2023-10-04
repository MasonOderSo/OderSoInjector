using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;

namespace OderSo_Injector
{
    public class Injector
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(IntPtr dwDesiredAccess, bool bInheritHandle, uint processId);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, char[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, ref IntPtr lpThreadId);

        [DllImport("kernel32.dll")]
        public static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);

        [DllImport("kernel32.dll")]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, IntPtr dwFreeType);

        public static void Inject(string path)
        {

            if (!File.Exists(path))
            {
                MessageBox.Show("DLL not found, your AntiVirus might have deleted it (fucking retarded shit)");
                return;
            }

            byte[] data = File.ReadAllBytes(path);

            if (data.Length < 10)
            {
                MessageBox.Show("DLL broken (Less than 10 bytes)");
                return;
            }

            if (data[0] != 'M' || data[1] != 'Z')
            {
                MessageBox.Show("Invalid PE file");
                return;
            }

            try
            {
                var fileInfo = new FileInfo(path);
                var accessControl = fileInfo.GetAccessControl();
                accessControl.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier("S-1-15-2-1"), FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                fileInfo.SetAccessControl(accessControl);
            }
            catch (Exception)
            {
                MessageBox.Show("Could not set permissions, try running the injector as admin");
                return;
            }

            var processes = Process.GetProcessesByName("Minecraft.Windows");

            if (processes.Length == 0)
            {
                MessageBox.Show("Failed to find Minecraft process");
                return;
            }

            var process = processes.First(p => p.Responding);

            for (int i = 0; i < process.Modules.Count; i++)
            {
                if (process.Modules[i].FileName == path)
                {
                    MessageBox.Show("Already injected!");
                    return;
                }
            }

            IntPtr processHandle = OpenProcess((IntPtr)2035711, false, (uint)process.Id);

            if (processHandle == IntPtr.Zero || !process.Responding)
            {
                MessageBox.Show("Failed to get process handle");
                return;
            }

            IntPtr dllMemory = VirtualAllocEx(processHandle, IntPtr.Zero, (uint)(path.Length + 1), 12288U, 64U);
            WriteProcessMemory(processHandle, dllMemory, path.ToCharArray(), path.Length, out IntPtr p2);

            IntPtr loadLibraryA = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            IntPtr remoteThread = CreateRemoteThread(processHandle, IntPtr.Zero, 0U, loadLibraryA, dllMemory, 0U, ref p2);

            if (remoteThread == IntPtr.Zero)
            {
                MessageBox.Show("Failed to create remote thread");
                return;
            }

            uint n = WaitForSingleObject(remoteThread, 5000);

            if (n == 128L || n == 258L)
                CloseHandle(remoteThread);
            else
            {
                VirtualFreeEx(processHandle, dllMemory, 0, (IntPtr)32768);

                if (remoteThread != IntPtr.Zero)
                    CloseHandle(remoteThread);

                if (processHandle != IntPtr.Zero)
                    CloseHandle(processHandle);
            }

        }
    }
}
