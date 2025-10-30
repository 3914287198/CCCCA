using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProcessPathDLL
{
    public class ProcessPathHelper
    {
        // 导入Windows API：通过窗口句柄获取进程ID
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        // 导出函数（供易语言调用，需标记为静态方法并使用DllExport）
        [DllExport("GetProcessPathByHwnd", CallingConvention = CallingConvention.StdCall)]
        public static void GetProcessPathByHwnd(uint hwnd, IntPtr outPath, int outPathSize)
        {
            try
            {
                // 1. 初始化输出缓冲区（填充空字符）
                byte[] buffer = new byte[outPathSize];
                Marshal.Copy(buffer, 0, outPath, outPathSize);

                // 2. 通过窗口句柄获取进程ID
                if (!GetWindowThreadProcessId((IntPtr)hwnd, out uint processId) || processId == 0)
                {
                    WriteResult(outPath, outPathSize, "获取进程ID失败");
                    return;
                }

                // 3. 通过进程ID获取进程路径
                Process process = Process.GetProcessById((int)processId);
                string path = process.MainModule.FileName;

                // 4. 转换为ANSI编码并写入输出缓冲区
                WriteResult(outPath, outPathSize, path);
            }
            catch (Exception ex)
            {
                // 捕获异常（如权限不足、进程已退出等）
                WriteResult(outPath, outPathSize, $"错误：{ex.Message}");
            }
        }

        // 辅助方法：将字符串写入输出缓冲区（转换为ANSI编码）
        private static void WriteResult(IntPtr outPath, int outPathSize, string content)
        {
            byte[] bytes = System.Text.Encoding.Default.GetBytes(content);
            int copyLength = Math.Min(bytes.Length, outPathSize - 1); // 留一个字节存结束符
            Marshal.Copy(bytes, 0, outPath, copyLength);
        }
    }
}
