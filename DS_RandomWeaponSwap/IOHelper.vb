Imports System.IO
Imports System.Text.Json

Module IOHelper

    Private Const SETTINGS_FILE As String = "Settings.json"
    Private Const WEAPONS_FILE As String = "DS weapons.json"

#Region "Utils"

    Function GetRootDirPath() As String
        Return AppDomain.CurrentDomain.BaseDirectory
    End Function


    Sub WriteLog(message As String)
        Using logWriter As New StreamWriter(Path.Combine(GetRootDirPath(), "log.txt"), True)
            logWriter.WriteLine($"{Now} - {message}")
        End Using
    End Sub

#End Region


#Region "Data"

    Public Function LoadSettings() As UserSettings
        Dim settings As New UserSettings

        Try
            Dim settingsFilePath As String = Path.Combine(GetRootDirPath(), SETTINGS_FILE)
            If File.Exists(Path.Combine(settingsFilePath)) Then
                Dim userSettingsJsonData As String = File.ReadAllText(settingsFilePath)
                settings = JsonSerializer.Deserialize(Of UserSettings)(userSettingsJsonData)
            End If

        Catch ex As Exception
            Dim errMsg As String = $"Error while loading user settings from file ""{SETTINGS_FILE}"" : {ex}"
            WriteLog(errMsg)
            Debug.Print(errMsg)
            MessageBox.Show(errMsg, "Data error", MessageBoxButton.OK, MessageBoxImage.Error)

        End Try

        Return settings
    End Function

    Public Sub SaveSettings(settings As UserSettings)

        Try
            Dim settingsFilePath As String = Path.Combine(GetRootDirPath(), SETTINGS_FILE)
            Dim serializeOptions As New JsonSerializerOptions() With {.WriteIndented = True}
            Dim userSettingsJsonData As String = JsonSerializer.Serialize(settings, serializeOptions)

            File.WriteAllText(settingsFilePath, userSettingsJsonData)

        Catch ex As Exception
            Dim errMsg As String = $"Error while saving user settings to file ""{SETTINGS_FILE}"" : {ex}"
            WriteLog(errMsg)
            Debug.Print(errMsg)
            MessageBox.Show(errMsg, "Data error", MessageBoxButton.OK, MessageBoxImage.Error)

        End Try

    End Sub

    Function LoadWeaponData() As List(Of Weapon)
        Dim weapons As New List(Of Weapon)

        Dim weaponsFilePath As String = Path.Combine(GetRootDirPath(), "data", WEAPONS_FILE)
        If Not File.Exists(weaponsFilePath) Then
            Dim errMsg As String = $"File ""{WEAPONS_FILE}"" not found. Please verify your installation"
            WriteLog(errMsg)
            MessageBox.Show(errMsg, "Resource error", MessageBoxButton.OK, MessageBoxImage.Error)
        End If

        Try
            Dim weaponsJsonData As String = File.ReadAllText(weaponsFilePath)
            weapons = JsonSerializer.Deserialize(Of List(Of Weapon))(weaponsJsonData)

        Catch ex As Exception
            Dim errMsg As String = $"Failed to load weapons data from resource file : {ex}"
            WriteLog(errMsg)
            MessageBox.Show(errMsg, "Data error", MessageBoxButton.OK, MessageBoxImage.Error)

        End Try

        Return weapons
    End Function

#End Region

End Module
