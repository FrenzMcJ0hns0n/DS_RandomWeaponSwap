Imports System.Runtime.InteropServices

''' <summary>
''' This class interacts with module MemoryHander in order to actually swap weapons
''' </summary>
Public Class WeaponSwapper

#Region "Members"

    Private Const PROCESS_NAME As String = "DarkSoulsRemastered"
    Private Const MAIN_MODULE_NAME As String = "DarkSoulsRemastered.exe"
    Private Const R1_WEAPON_BASE_ADDRESS As Integer = &H1C823A0
    Private ReadOnly R1WeaponOffsets As New List(Of Integer) From {&H8, &H0, &H7E8, &H28, &H328}

#End Region

#Region "Properties"

    Private _GameProcess As Process
    Private ReadOnly Property GameProcess() As Process
        Get
            If _GameProcess Is Nothing Then

                Dim processes As Process() = Process.GetProcessesByName(PROCESS_NAME)
                If processes.Length = 1 Then
                    _GameProcess = processes(0)
                Else
                    Dim errMsg As String = $"Error while retrieving process {PROCESS_NAME} : wrong number of processes. Expected 1. Provided {processes.Length}"
                    WriteLog(errMsg)
                    Debug.Print(errMsg)
                    MessageBox.Show(errMsg, "Error while retrieving process", MessageBoxButton.OK, MessageBoxImage.Error)
                End If

            End If
            Return _GameProcess
        End Get
    End Property


    Private _GameProcessPtr As IntPtr
    Private ReadOnly Property GameProcessPtr() As IntPtr
        Get
            If _GameProcessPtr = IntPtr.Zero Then

                _GameProcessPtr = OpenProcess(
                    ProcessAccessFlags.VMOperation Or ProcessAccessFlags.VMRead Or ProcessAccessFlags.VMWrite,
                    False,
                    GameProcess().Id
                )

            End If
            Return _GameProcessPtr
        End Get
    End Property


    Private _GameBaseAddress As IntPtr
    Private ReadOnly Property GameBaseAddress() As IntPtr
        Get
            If _GameBaseAddress = IntPtr.Zero Then

                For Each m As ProcessModule In GameProcess().Modules
                    If m.ModuleName = MAIN_MODULE_NAME Then
                        _GameBaseAddress = m.BaseAddress
                        Exit For
                    End If
                Next

            End If
            Return _GameBaseAddress
        End Get
    End Property


    Private _R1WeaponAddress As IntPtr
    Private ReadOnly Property R1WeaponAddress() As IntPtr
        Get
            If _R1WeaponAddress = IntPtr.Zero Then

                'Start from base address and move from one pointer to the next (except the last one)
                Dim targetAddress = GetValueAtAddress(GameBaseAddress() + R1_WEAPON_BASE_ADDRESS, 8)
                Dim maxIndex As Integer = R1WeaponOffsets.Count - 1
                For i As Integer = 0 To maxIndex
                    targetAddress += R1WeaponOffsets.Item(i)
                    If i < maxIndex Then
                        targetAddress = GetValueAtAddress(targetAddress, 8)
                    End If
                Next
                _R1WeaponAddress = targetAddress

            End If
            Return _R1WeaponAddress
        End Get
    End Property


    Private _Randomizer As Random
    Private ReadOnly Property Randomizer() As Random
        Get
            If _Randomizer Is Nothing Then _Randomizer = New Random()
            Return _Randomizer
        End Get
    End Property


    Private _Weapons As List(Of Weapon)
    Private ReadOnly Property Weapons() As List(Of Weapon)
        Get
            If _Weapons Is Nothing Then _Weapons = LoadWeaponData()
            Return _Weapons
        End Get
    End Property

#End Region

#Region "Constructor"

    Public Sub New()
        'Perform at least this basic check before continuing
        If GameProcess() Is Nothing Then Environment.Exit(0)

        Debug.Print("New WeaponSwapper object instantiated")
    End Sub

#End Region

#Region "Private methods"

    Private Function GetValueAtAddress(address As IntPtr, dataSize As Integer) As IntPtr
        Dim bytesRead As IntPtr
        Dim buffer(dataSize) As Byte

        ReadProcessMemory(GameProcessPtr(), address, buffer, buffer.Length, bytesRead)

        If dataSize = 4 Then
            Return BitConverter.ToInt32(buffer, 0)
        Else
            Return BitConverter.ToInt64(buffer, 0)
        End If
    End Function

#End Region

#Region "Public methods"

    Public Function GetRandomWeapon() As Weapon
        Return Weapons(Randomizer().Next(0, Weapons().Count))
    End Function


    Public Function ReadFromMemory() As Integer
        Dim weaponId As Integer

        Dim bytesRead As IntPtr
        Dim totalBytesToRead = 4
        Dim buffer(totalBytesToRead) As Byte

        Try
            Dim success As Boolean = ReadProcessMemory(GameProcessPtr(), R1WeaponAddress(), buffer, totalBytesToRead, bytesRead)
            If Not success Then
                Throw New Exception($"Failed to read value from memory. Error code: {Marshal.GetLastWin32Error()}")
            End If
            weaponId = BitConverter.ToInt32(buffer, 0)

        Catch ex As Exception
            Dim errMsg As String = $"An unexpected error occurred. Details: {ex}"
            WriteLog(errMsg)
            Debug.Print(errMsg)

        Finally
            CloseHandle(GameProcessPtr())

        End Try

        'Reset for next access
        _GameProcess = Nothing
        _GameProcessPtr = IntPtr.Zero
        _GameBaseAddress = IntPtr.Zero
        _R1WeaponAddress = IntPtr.Zero

        Return weaponId
    End Function


    Public Sub WriteIntoMemory(weaponId As Integer)
        Dim bytesWritten As IntPtr
        Dim buffer As Byte() = BitConverter.GetBytes(weaponId)

        Try
            Dim success As Boolean = WriteProcessMemory(GameProcessPtr(), R1WeaponAddress(), buffer, buffer.Length, bytesWritten)
            If Not success Then
                Throw New Exception($"Failed to write value ({weaponId}) into memory. Error code: {Marshal.GetLastWin32Error()}")
            End If

        Catch ex As Exception
            Dim errMsg As String = $"An unexpected error occurred. Details: {ex}"
            WriteLog(errMsg)

        Finally
            CloseHandle(GameProcessPtr())

        End Try

        'Reset for next access
        _GameProcess = Nothing
        _GameProcessPtr = IntPtr.Zero
        _GameBaseAddress = IntPtr.Zero
        _R1WeaponAddress = IntPtr.Zero
    End Sub

#End Region

End Class
