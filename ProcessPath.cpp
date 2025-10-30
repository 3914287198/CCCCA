#include <windows.h>
#include <tlhelp32.h>
#include <tchar.h>
#include <string>

// 导出函数声明（供易语言调用）
extern "C" __declspec(dllexport) void __stdcall GetProcessPathByHwnd(DWORD hwnd, char* outPath, int outPathSize) {
    // 初始化输出缓冲区
    memset(outPath, 0, outPathSize);

    // 1. 通过窗口句柄获取进程ID
    DWORD processId;
    GetWindowThreadProcessId((HWND)hwnd, &processId);
    if (processId == 0) {
        strncpy_s(outPath, outPathSize, "获取进程ID失败", _TRUNCATE);
        return;
    }

    // 2. 通过进程ID打开进程（需要查询信息权限）
    HANDLE hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, processId);
    if (hProcess == NULL) {
        strncpy_s(outPath, outPathSize, "打开进程失败", _TRUNCATE);
        return;
    }

    // 3. 获取进程可执行文件路径
    TCHAR exePath[MAX_PATH] = {0};
    DWORD pathLength = GetModuleFileNameEx(hProcess, NULL, exePath, MAX_PATH);
    CloseHandle(hProcess); // 关闭进程句柄

    if (pathLength == 0) {
        strncpy_s(outPath, outPathSize, "获取路径失败", _TRUNCATE);
        return;
    }

    // 4. 转换为ANSI编码（易语言通常使用ANSI）
    WideCharToMultiByte(CP_ACP, 0, exePath, -1, outPath, outPathSize, NULL, NULL);
}
