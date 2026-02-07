Imports System.Data
Imports System.Runtime.InteropServices

''' <summary>
''' This class interacts with module MemoryHander in order to actually swap weapons
''' </summary>

Public Class WeaponSwapper

#Region "Members"

    Private Const PROCESS_NAME As String = "DarkSoulsRemastered"
    Private Const MAIN_MODULE_NAME As String = "DarkSoulsRemastered.exe"
    Private Const GAME_BASE_OFFSET As Integer = &H1C823A0
    Private ReadOnly R1WeaponIdOffsets As New List(Of Integer) From {&H8, &H0, &H7E8, &H28, &H328}
    Private ReadOnly R1WeaponDuraOffsets As New List(Of Integer) From {&H8, &H0, &H280, &H88, &H578, &HDB0}
    Public MeleeWeaponsOnly As Boolean

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
                    Dim errMsg As String = $"Wrong number of processes for ""{PROCESS_NAME}"": expected 1, provided {processes.Length}. Make sure the game is running."
                    WriteLog(errMsg)
                    MessageBox.Show(errMsg, "Error while retrieving game process", MessageBoxButton.OK, MessageBoxImage.Error)
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


    Private _R1WeaponIdAddress As IntPtr
    Private ReadOnly Property R1WeaponIdAddress() As IntPtr
        Get
            If _R1WeaponIdAddress = IntPtr.Zero Then

                'Start at base offset. Move from one pointer to the next (except the last one)
                Dim targetAddress = GetValueAtAddress(GameBaseAddress() + GAME_BASE_OFFSET, 8)
                Dim maxIndex As Integer = R1WeaponIdOffsets.Count - 1
                For i As Integer = 0 To maxIndex
                    targetAddress += R1WeaponIdOffsets.Item(i)
                    If i < maxIndex Then
                        targetAddress = GetValueAtAddress(targetAddress, 8)
                    End If
                Next
                _R1WeaponIdAddress = targetAddress

            End If
            Return _R1WeaponIdAddress
        End Get
    End Property


    Private _R1WeaponDuraAddress As IntPtr
    Private ReadOnly Property R1WeaponDuraAddress() As IntPtr
        Get
            If _R1WeaponDuraAddress = IntPtr.Zero Then

                'Start at base offset. Move from one pointer to the next (except the last one)
                Dim targetAddress = GetValueAtAddress(GameBaseAddress() + GAME_BASE_OFFSET, 8)
                Dim maxIndex As Integer = R1WeaponDuraOffsets.Count - 1
                For i As Integer = 0 To maxIndex
                    targetAddress += R1WeaponDuraOffsets.Item(i)
                    If i < maxIndex Then
                        targetAddress = GetValueAtAddress(targetAddress, 8)
                    End If
                Next
                _R1WeaponDuraAddress = targetAddress

            End If
            Return _R1WeaponDuraAddress
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
            If _Weapons Is Nothing Then

                Dim loadedWeapons As List(Of Weapon) = LoadWeaponData()
                If MeleeWeaponsOnly Then
                    _Weapons = loadedWeapons.Where(Function(w As Weapon) w.Melee).ToList
                Else
                    _Weapons = loadedWeapons
                End If
                If Application.IsDebug Then Debug.Print($"Loaded {_Weapons.Count} weapons")

            End If
            Return _Weapons
        End Get
    End Property

#End Region

#Region "Constructor"

    Public Sub New()
        'Perform at least this basic check before continuing
        If GameProcess() Is Nothing Then Environment.Exit(0)

        If Application.IsDebug Then Debug.Print("New WeaponSwapper object instantiated")
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
            Dim success As Boolean = ReadProcessMemory(GameProcessPtr(), R1WeaponIdAddress(), buffer, totalBytesToRead, bytesRead)
            If Not success Then
                Throw New Exception($"Failed to read value from memory. Error code: {Marshal.GetLastWin32Error()}")
            End If
            weaponId = BitConverter.ToInt32(buffer, 0)

        Catch ex As Exception
            Dim errMsg As String = $"An unexpected error occurred. Details: {ex}"
            If Application.IsDebug Then Debug.Print(errMsg)
            WriteLog(errMsg)

        Finally
            CloseHandle(GameProcessPtr())

        End Try

        'Reset for next access
        _GameProcess = Nothing
        _GameProcessPtr = IntPtr.Zero
        _GameBaseAddress = IntPtr.Zero
        _R1WeaponIdAddress = IntPtr.Zero
        _R1WeaponDuraAddress = IntPtr.Zero

        Return weaponId
    End Function


    Public Sub WriteIntoMemory(weaponId As Integer, Optional weaponDura As Integer = 999)
        '02/2026 - v1.1:
        'Added a fix for the durability issue (i.e. force weapon dura to 999)
        'This is not 100% reliable but should be much better than 1st release
        'Advice, never change your weapons using the menus, let the mod do so

        Dim bytesWritten As IntPtr
        Dim bufferWeaponId As Byte() = BitConverter.GetBytes(weaponId)
        Dim bufferWeapDura As Byte() = BitConverter.GetBytes(weaponDura)

        Try
            Dim successWeaponId As Boolean = WriteProcessMemory(GameProcessPtr(), R1WeaponIdAddress(), bufferWeaponId, bufferWeaponId.Length, bytesWritten)
            If Not successWeaponId Then Throw New Exception($"Failed to write Weapon Id {weaponId} into memory. Error code: {Marshal.GetLastWin32Error()}")

            Dim successWeapDura As Boolean = WriteProcessMemory(GameProcessPtr(), R1WeaponDuraAddress(), bufferWeapDura, bufferWeapDura.Length, bytesWritten)
            If Not successWeapDura Then Throw New Exception($"Failed to write Weapon Durability {weaponDura} into memory. Error code: {Marshal.GetLastWin32Error()}")

        Catch ex As Exception
            Dim errMsg As String = $"An unexpected error occurred. Details: {ex}"
            If Application.IsDebug Then Debug.Print(errMsg)
            WriteLog(errMsg)

        Finally
            CloseHandle(GameProcessPtr())

        End Try

        'Reset for next access
        _GameProcess = Nothing
        _GameProcessPtr = IntPtr.Zero
        _GameBaseAddress = IntPtr.Zero
        _R1WeaponIdAddress = IntPtr.Zero
        _R1WeaponDuraAddress = IntPtr.Zero
    End Sub

#End Region

End Class
