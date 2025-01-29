Imports System.IO
Imports System.Text.Json

Module IOHelper

    Private Const WEAPONS_FILE As String = "DS weapons.json"


    Function GetRootDirPath() As String
        Return AppDomain.CurrentDomain.BaseDirectory
    End Function


    Sub WriteLog(message As String)
        Using logWriter As New StreamWriter(Path.Combine(GetRootDirPath(), "log.txt"), True)
            logWriter.WriteLine($"{Now} - {message}")
        End Using
    End Sub


    Function LoadWeaponData() As List(Of Weapon)
        Dim weapons As New List(Of Weapon)

        Dim weaponsFilePath As String = Path.Combine(GetRootDirPath(), "data", WEAPONS_FILE)
        If Not File.Exists(weaponsFilePath) Then
            Dim errMsg As String = $"File ""{WEAPONS_FILE}"" not found. Please verify your installation"
            WriteLog(errMsg)
            Debug.Print(errMsg)
            'MessageBox.Show(errMsg, "Error: missing data", MessageBoxButton.OK, MessageBoxImage.Error)
        End If

        Try
            Dim weaponsJsonData As String = File.ReadAllText(weaponsFilePath)
            weapons = JsonSerializer.Deserialize(Of List(Of Weapon))(weaponsJsonData)
        Catch ex As Exception
            Dim errMsg As String = $"Failed to load weapons data from resource file : {ex}"
            WriteLog(errMsg)
            Debug.Print(errMsg)
            'MessageBox.Show(errMsg, "Error: cannot load data", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try

        Return weapons
    End Function

End Module
