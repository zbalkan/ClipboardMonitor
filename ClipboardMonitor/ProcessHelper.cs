using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace ClipboardMonitor
{
    public static class ProcessHelper
    {
        // ReSharper disable once InconsistentNaming
        private const int DACL_SECURITY_INFORMATION = 0x00000004;

        private static bool _isProtected;

        public static bool IsDuplicate()
        {
            var processName = Process.GetCurrentProcess().ProcessName;
            var count = Process.GetProcesses().Count(p => p.ProcessName == processName);

            return count > 1;
        }

        public static ProcessInformation CaptureProcessInfo()
        {
            try
            {
                var activeWindow = NativeMethods.GetForegroundWindow();
                var length = NativeMethods.GetWindowTextLength(activeWindow);
                var title = new StringBuilder(length + 1);
                _ = NativeMethods.GetWindowText(activeWindow, title, title.Capacity);

                _ = NativeMethods.GetWindowThreadProcessId(activeWindow, out var processId);
                var process = FindProcess(processId);
                if (process == null)
                {
                    return default;
                }
                else
                {
                    var executablePath = string.Empty;
                    if (process.MainModule?.FileName != null)
                    {
                        executablePath = process.MainModule.FileName;
                    }

                    return new ProcessInformation
                    {
                        ProcessName = process.ProcessName,
                        ExecutablePath = executablePath,
                        WindowTitle = title.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return default;
            }
        }

        public static void SetCriticalProcess()
        {
            var isCritical = 1;  // we want this to be a Critical Process, thus 1
            const int BreakOnTermination = 0x1D;  // value for BreakOnTermination (flag)

            Process.EnterDebugMode();  //acquire Debug Privileges

            // setting the BreakOnTermination = 1 for the current process
            _ = NativeMethods.NtSetInformationProcess(Process.GetCurrentProcess().Handle, BreakOnTermination, ref isCritical, sizeof(int));
        }

        public static void UnsetCriticalProcess()
        {
            var isCritical = 0;  // we want this to be a Normal Process, thus 0
            const int BreakOnTermination = 0x1D;  // value for BreakOnTermination (flag)

            Process.EnterDebugMode();  //acquire Debug Privileges

            // setting the BreakOnTermination = 0 for the current process
            _ = NativeMethods.NtSetInformationProcess(Process.GetCurrentProcess().Handle, BreakOnTermination, ref isCritical, sizeof(int));
        }

        private static Process? FindProcess(int processId)
        {
            Process process;

            try
            {
                process = Process.GetProcessById(processId);
            }
            catch
            {
                return null;
            }
            return process;
        }

        /// <summary>
        /// inspired from here: http://csharptest.net/1043/how-to-prevent-users-from-killing-your-service-process/index.html
        /// </summary>
        public static void Cover()
        {
            // Get the current process handle
            var hProcess = NativeMethods.GetCurrentProcess();
            // Read the DACL
            var dacl = GetProcessSecurityDescriptor(hProcess);

            // Modify Users ACE
            var denyUsers = new CommonAce(AceFlags.None, AceQualifier.AccessDenied,
                (int)ProcessAccessRights.PROCESS_ALL_ACCESS, new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null),
                false, null);
            dacl.DiscretionaryAcl?.InsertAce(0, denyUsers);

            // Save the DACL
            SetProcessSecurityDescriptor(hProcess, dacl);
            _isProtected = true;
        }

        public static void Uncover()
        {
            if (!_isProtected)
            {
                return;
            }

            // Get the current process handle
            var hProcess = NativeMethods.GetCurrentProcess();
            // Read the DACL
            var dacl = GetProcessSecurityDescriptor(hProcess);

            // Modify Users ACE
            var denyUsers = new CommonAce(AceFlags.None, AceQualifier.AccessAllowed,
                (int)ProcessAccessRights.PROCESS_ALL_ACCESS, new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null),
                false, null);
            dacl.DiscretionaryAcl?.InsertAce(0, denyUsers);

            // Save the DACL
            SetProcessSecurityDescriptor(hProcess, dacl);
            _isProtected = false;
        }

        private static RawSecurityDescriptor GetProcessSecurityDescriptor(IntPtr processHandle)
        {
            if (processHandle == IntPtr.Zero)
            {
                throw new ArgumentException("The process handle is invalid.", nameof(processHandle));
            }

            var psd = Array.Empty<byte>();
            // Call with 0 size to obtain the actual size needed in bufSizeNeeded
            _ = NativeMethods.GetKernelObjectSecurity(processHandle, DACL_SECURITY_INFORMATION, psd, 0, out var bufSizeNeeded);
            if (bufSizeNeeded > short.MaxValue)
            {
                throw new Win32Exception();
            }
            // Allocate the required bytes and obtain the DACL
            if (!NativeMethods.GetKernelObjectSecurity(processHandle, DACL_SECURITY_INFORMATION, psd = new byte[bufSizeNeeded], bufSizeNeeded, out _))
            {
                throw new Win32Exception();
            }
            // Use the RawSecurityDescriptor class from System.Security.AccessControl to parse the bytes:
            return new RawSecurityDescriptor(psd, 0);
        }

        private static void SetProcessSecurityDescriptor(IntPtr processHandle, RawSecurityDescriptor dacl)
        {
            if (processHandle == IntPtr.Zero)
            {
                throw new ArgumentException("The process handle is invalid.", nameof(processHandle));
            }

            var pSecurityDescriptor = new byte[dacl.BinaryLength];
            dacl.GetBinaryForm(pSecurityDescriptor, 0);
            if (!NativeMethods.SetKernelObjectSecurity(processHandle, DACL_SECURITY_INFORMATION, pSecurityDescriptor))
            {
                throw new Win32Exception();
            }
        }

        [Flags]
        private enum ProcessAccessRights
        {
            None = 0,

            /// <summary>
            /// Required to terminate a process using TerminateProcess.
            /// </summary>
            PROCESS_TERMINATE = 0x0001,

            /// <summary>
            /// Required to create a thread.
            /// </summary>
            PROCESS_CREATE_THREAD = 1 << 1,

            /// <summary>
            /// Required to perform an operation on the address space of a process (see VirtualProtectEx and WriteProcessMemory).
            /// </summary>
            PROCESS_VM_OPERATION = 1 << 3,

            /// <summary>
            /// Required to read memory in a process using ReadProcessMemory.
            /// </summary>
            PROCESS_VM_READ = 1 << 4,

            /// <summary>
            /// Required to write to memory in a process using WriteProcessMemory.
            /// </summary>
            PROCESS_VM_WRITE = 1 << 5,

            /// <summary>
            /// Required to duplicate a handle using DuplicateHandle.
            /// </summary>
            PROCESS_DUP_HANDLE = 1 << 6,

            /// <summary>
            /// Required to create a process.
            /// </summary>
            PROCESS_CREATE_PROCESS = 1 << 7,

            /// <summary>
            /// Required to set memory limits using SetProcessWorkingSetSize.
            /// </summary>
            PROCESS_SET_QUOTA = 1 << 8,

            /// <summary>
            /// Required to set certain information about a process, such as its priority class (see SetPriorityClass).
            /// </summary>
            PROCESS_SET_INFORMATION = 1 << 9,

            /// <summary>
            /// Required to retrieve certain information about a process, such as its token, exit code, and priority class (see OpenProcessToken, GetExitCodeProcess, GetPriorityClass, and IsProcessInJob).
            /// </summary>
            PROCESS_QUERY_INFORMATION = 1 << 10,

            /// <summary>
            /// Required to suspend or resume a process.
            /// </summary>
            PROCESS_SUSPEND_RESUME = 1 << 11,

            /// <summary>
            /// Required to retrieve certain information about a process (see QueryFullProcessImageName). A handle that has the PROCESS_QUERY_INFORMATION access right is automatically granted PROCESS_QUERY_LIMITED_INFORMATION. Windows Server 2003 and Windows XP/2000:  This access right is not supported.
            /// </summary>
            PROCESS_QUERY_LIMITED_INFORMATION = 1 << 12,

            /// <summary>
            /// Required to delete the object.
            /// </summary>
            DELETE = 1 << 16,

            /// <summary>
            /// Required to read information in the security descriptor for the object, not including the information in the SACL. To read or write the SACL, you must request the ACCESS_SYSTEM_SECURITY access right. For more information, see SACL Access Right.
            /// </summary>
            READ_CONTROL = 1 << 17,

            /// <summary>
            /// Required to modify the DACL in the security descriptor for the object.
            /// </summary>
            WRITE_DAC = 1 << 18,

            /// <summary>
            /// Required to change the owner in the security descriptor for the object.
            /// </summary>
            WRITE_OWNER = 1 << 19,

            STANDARD_RIGHTS_REQUIRED = DELETE | READ_CONTROL | WRITE_DAC | WRITE_OWNER,

            /// <summary>
            /// The right to use the object for synchronization. This enables a thread to wait until the object is in the signaled state.
            /// </summary>
            SYNCHRONIZE = 1 << 20,

            /// <summary>
            /// All possible access rights for a process object.
            /// </summary>
            PROCESS_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFF,
        }
    }
}
