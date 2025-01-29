Imports System.Runtime.InteropServices

''' <summary>
''' This module (static code) gives access to Windows API functions to interact with the virtual memory 
''' </summary>

Module MemoryHandler

    <Flags>
    Enum ProcessAccessFlags As UInteger
        VMOperation = &H8
        VMRead = &H10
        VMWrite = &H20
    End Enum

    <DllImport("kernel32.dll", SetLastError:=True)>
    Function OpenProcess(dwDesiredAccess As ProcessAccessFlags, bInheritHandle As Boolean, dwProcessId As Integer) As IntPtr
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Function ReadProcessMemory(hProcess As IntPtr, lpBaseAddress As IntPtr, lpBuffer As Byte(), nSize As Integer, ByRef lpNumberOfBytesRead As IntPtr) As Boolean
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Function WriteProcessMemory(hProcess As IntPtr, lpBaseAddress As IntPtr, lpBuffer As Byte(), nSize As Integer, ByRef lpNumberOfBytesWritten As IntPtr) As Boolean
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Function CloseHandle(hObject As IntPtr) As Boolean
    End Function

End Module
