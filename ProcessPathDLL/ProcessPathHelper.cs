using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RGiesecke.DllExport; // 必须保留此命名空间

namespace ProcessPathDLL
{
    public static class ProcessPathHelper // 修改为静态类（确保函数可被正确导出）
    {
        [DllImport("user32.dll", SetLastError = true)] // 增加错误处理
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        // 显式指定导出名称和调用约定，确保无名称修饰
        [DllExport("GetProcessPathByHwnd", CallingConvention = CallingConvention.StdCall, ExportName = "GetProcessPathByHwnd")]
        public static void GetProcessPathByHwnd(uint hwnd, IntPtr outPath, int outPathSize)
        {
            try
            {
                // 初始化输出缓冲区为0
                byte[] buffer = new byte[outPathSize];
                Marshal.Copy(buffer, 0, outPath, outPathSize);

                // 获取进程ID
                uint threadId = GetWindowThreadProcessId((IntPtr)hwnd, out uint processId);
                if (threadId == 0 || processId == 0)
                {
                    WriteResult(outPath, outPathSize, "获取进程ID失败（错误码：" + Marshal.GetLastWin32Error() + "）");
                    return;
                }

                // 获取进程路径
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
            int copyLength = Math.Min(bytes.Length, outPathSize - 1); // 预留结束符位置
            Marshal.Copy(bytes, 0, outPath, copyLength);
        }
    }
}
