using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static AkashaScanner.Core.ProcessControl.IProcessControl;

namespace AkashaScanner.Core.ProcessControl
{
    public class GenshinProcessControl : IProcessControl
    {
        private static readonly List<string> Executables = new() { "GenshinImpact", "YuanShen" };

        public event OnStatusChangeEvent? OnStatusChange;

        private volatile ProcessStatus CurrentStatus = ProcessStatus.Unknown;
        private volatile IntPtr ActiveHwnd = IntPtr.Zero;

        public ProcessStatus Status { get => CurrentStatus; }

        private readonly ILogger Logger;
        public GenshinProcessControl(ILogger<GenshinProcessControl> logger)
        {
            Logger = logger;
            CheckStatus();
        }

        private Process? GetProcess()
        {
            foreach (var executable in Executables)
            {
                Process? proc = Process.GetProcessesByName(executable).FirstOrDefault();
                if (proc != null)
                {
                    ActiveHwnd = proc.MainWindowHandle;
                    return proc;
                }
            }
            ActiveHwnd = IntPtr.Zero;
            return null;
        }

        private ProcessStatus LoadStatus()
        {
            using var proc = GetProcess();
            if (proc == null) return ProcessStatus.Stopped;
            var activeHandle = GetForegroundWindow();
            if (activeHandle == proc.MainWindowHandle) return ProcessStatus.Active;
            return ProcessStatus.Inactive;
        }

        public Rectangle GetWindowDimension()
        {
            using var proc = GetProcess();
            if (proc == null)
                throw new IOException("Genshin Impact is not running.");

            var rect = new RECT();

            if (!GetClientRect(proc.MainWindowHandle, ref rect))
                throw new IOException("Fail to get client rect of Genshin Impact");

            if (!ClientToScreen(proc.MainWindowHandle, ref rect))
                throw new IOException("Fail to get client rect of Genshin Impact");

            return new Rectangle(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        public void SetActive()
        {
            using var proc = GetProcess();
            if (proc == null)
                throw new IOException("Fail to get the Genshin Impact process");

            var handle = proc.MainWindowHandle;

            var windowPlacement = new WindowPlacement { length = Marshal.SizeOf(typeof(WindowPlacement)) };
            if (!GetWindowPlacement(handle, ref windowPlacement))
                throw new IOException("Fail to get window placement of Genshin Impact");

            if (windowPlacement.showCmd == ShowWindowEnum.ShowMinimized)
            {
                windowPlacement.showCmd = ShowWindowEnum.ShowNormal;
                if (!SetWindowPlacement(handle, ref windowPlacement))
                    throw new IOException("Fail to set window placement of Genshin Impact");
            }
            if (!SetForegroundWindow(handle))
                throw new IOException("Fail to set Genshin Impact as foreground window");

            SetStatus(ProcessStatus.Active);
        }

        public bool IsActive()
        {
            var activeHandle = GetForegroundWindow();
            return activeHandle == ActiveHwnd;
        }

        private void SetStatus(ProcessStatus status)
        {
            if (CurrentStatus != status)
            {
                CurrentStatus = status;
                OnStatusChange?.Invoke(status);
            }
        }

        private void CheckStatus()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(_ =>
            {
                while (true)
                {
                    Thread.Sleep(500);
                    SetStatus(LoadStatus());
                }
            }));
        }


        [StructLayout(LayoutKind.Sequential)]
        private record struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private record struct POINT
        {
            public long x;
            public long y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowPlacement
        {
            public int length;
            public int flags;
            public ShowWindowEnum showCmd;
            public Point ptMinPosition;
            public Point ptMaxPosition;
            public Rectangle rcNormalPosition;
        }
        private enum ShowWindowEnum
        {
            Hide = 0,
            ShowNormal = 1, ShowMinimized = 2,
            Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
            Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
            Restore = 9, ShowDefault = 10, ForceMinimized = 11
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private record struct NativeMonitorInfo
        {
            public int Size = Marshal.SizeOf(typeof(NativeMonitorInfo));
            public RECT Monitor = default;
            public RECT Work = default;
            public int Flags = default;

            public NativeMonitorInfo() { }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetClientRect(IntPtr hWnd, ref RECT Rect);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ClientToScreen(IntPtr hWnd, ref RECT Rect);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WindowPlacement lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPlacement(IntPtr hWnd, ref WindowPlacement lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetForegroundWindow();
    }
}
