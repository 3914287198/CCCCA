using RGiesecke.DllExport;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProcessPathDLL
{
    public class ProcessPathHelper
    {
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllExport("GetProcessPathByHwnd", CallingConvention = CallingConvention.StdCall)]
        public static void GetProcessPathByHwnd(uint hwnd, IntPtr outPath, int outPathSize)
        {
            try
            {
                byte[] buffer = new byte[outPathSize];
                Marshal.Copy(buffer, 0, outPath, outPathSize);

                if (GetWindowThreadProcessId((IntPtr)hwnd, out uint processId) == 0 || processId == 0)
                {
                    WriteResult(outPath, outPathSize, "获取进程ID失败");
                    return;
                }

                using (Process process = Process.GetProcessById((int)processId))
                {
                    string path = process.MainModule.FileName;
                    WriteResult(outPath, outPathSize, path);
                }
            }
            catch (Exception ex)
            {
                WriteResult(outPath, outPathSize, $"错误：{ex.Message}");
            }
        }

        private static void WriteResult(IntPtr outPath, int outPathSize, string content)
        {
            byte[] bytes = System.Text.Encoding.Default.GetBytes(content);
            int copyLength = Math.Min(bytes.Length, outPathSize - 1);
            Marshal.Copy(bytes, 0, outPath, copyLength);
        }
    }
}
