Imports System.Threading

Class MainWindow

    'System
    Private ReadOnly isDebug As Boolean = Debugger.IsAttached
    Private isRunning As Boolean = False

    'User settings
    Private settings As UserSettings
    Private Const defInterval As Integer = 5 'Default value
    Private Const minInterval As Integer = 3
    Private Const maxInterval As Integer = 60
    Private intervalInSeconds As Integer
    Private meleeWeaponsOnly As Boolean



    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        settings = LoadSettings()

        If settings.Interval IsNot Nothing AndAlso settings.Interval >= minInterval AndAlso settings.Interval <= maxInterval Then
            intervalInSeconds = settings.Interval
        Else
            intervalInSeconds = defInterval
        End If

        meleeWeaponsOnly = If(settings.MeleeWeaponsOnly, False)
    End Sub

    Private Sub Tbx_Interval_Loaded(sender As TextBox, e As RoutedEventArgs)
        sender.Text = intervalInSeconds
    End Sub

    Private Sub Tbx_Interval_GotFocus(sender As Object, e As RoutedEventArgs)
        intervalInSeconds = Tbx_Interval.Text 'Backup value
    End Sub

    Private Sub Tbx_Interval_LostFocus(sender As Object, e As RoutedEventArgs)
        If Not ValidateIntervalInput() Then Tbx_Interval.Text = intervalInSeconds 'Restore backup
    End Sub

    Private Sub Btn_IntervalMore_Click(sender As Object, e As RoutedEventArgs)
        AddToInterval(1)
    End Sub

    Private Sub Btn_IntervalLess_Click(sender As Object, e As RoutedEventArgs)
        AddToInterval(-1)
    End Sub

    Private Sub Cbx_MeleeWpnsOnly_Loaded(sender As CheckBox, e As RoutedEventArgs)
        sender.IsChecked = meleeWeaponsOnly
    End Sub

    Private Sub Btn_Run_Click(sender As Object, e As RoutedEventArgs)
        Run()
    End Sub

    Private Sub Btn_Stop_Click(sender As Object, e As RoutedEventArgs)
        Lbl_Status.Content = "Stopping..."
        Lbl_Status.Foreground = Brushes.Red
        isRunning = False
    End Sub



    Private Sub AddToInterval(valueToAdd As Integer)
        Dim tbxInterval As TextBox = Tbx_Interval

        Dim currentInterval As Integer
        If Integer.TryParse(tbxInterval.Text, currentInterval) AndAlso
            (valueToAdd > 0 AndAlso currentInterval + valueToAdd <= maxInterval) OrElse
            (valueToAdd < 0 AndAlso currentInterval + valueToAdd >= minInterval) Then

            currentInterval += valueToAdd
            tbxInterval.Text = currentInterval
            intervalInSeconds = currentInterval
        End If
    End Sub

    Private Async Sub Run()
        If Not ValidateIntervalInput() Then Return
        meleeWeaponsOnly = Cbx_MeleeWpnsOnly.IsChecked 'TODO? Use Checked/Unchecked events instead

        'Persist user settings
        settings.Interval = intervalInSeconds
        settings.MeleeWeaponsOnly = meleeWeaponsOnly
        SaveSettings(settings)

        'Now do your job, program!
        Dim swapper As New WeaponSwapper() With {.MeleeWeaponsOnly = meleeWeaponsOnly}
        If isDebug Then Debug.Print($"Id of current R1 weapon : {swapper.ReadFromMemory()}")

        isRunning = True
        UpdateControls(isRunning)

        Dim timer As New PeriodicTimer(TimeSpan.FromSeconds(intervalInSeconds))
        While Await timer.WaitForNextTickAsync()
            If Not isRunning Then Exit While

            Dim weapon As Weapon = swapper.GetRandomWeapon()
            If isDebug Then Debug.Print($"(Swap every {intervalInSeconds}s) Random weapon: Id={weapon.Id}, Name=""{weapon.Name}"", Category=""{weapon.Category}""")
            swapper.WriteIntoMemory(weapon.Id)
        End While

        UpdateControls(isRunning)
    End Sub

    Private Sub UpdateControls(isRunning As Boolean)
        If isRunning Then
            Lbl_Status.Content = "Running"
            Btn_Run.IsEnabled = False
            Btn_Stop.IsEnabled = True
            Btn_IntervalMore.IsEnabled = False
            Btn_IntervalLess.IsEnabled = False
            Tbx_Interval.IsEnabled = False
            Cbx_MeleeWpnsOnly.IsEnabled = False
            Lbl_Status.FontWeight = FontWeights.DemiBold
            Lbl_Status.Foreground = Brushes.Green
        Else
            Lbl_Status.Content = "Idle"
            Btn_Run.IsEnabled = True
            Btn_Stop.IsEnabled = False
            Btn_IntervalMore.IsEnabled = True
            Btn_IntervalLess.IsEnabled = True
            Tbx_Interval.IsEnabled = True
            Cbx_MeleeWpnsOnly.IsEnabled = True
            Lbl_Status.ClearValue(Label.FontWeightProperty)
            Lbl_Status.ClearValue(Label.ForegroundProperty)
        End If
    End Sub

    Private Function ValidateIntervalInput() As Boolean
        Dim input As Integer

        If Not Integer.TryParse(Tbx_Interval.Text, input) Then
            MessageBox.Show("Only numeric values are allowed", "Input error", MessageBoxButton.OK, MessageBoxImage.Information)
            Return False
        End If

        If input < minInterval OrElse input > maxInterval Then
            MessageBox.Show($"Interval must be between {minInterval} and {maxInterval} seconds", "Input error", MessageBoxButton.OK, MessageBoxImage.Information)
            Return False
        End If

        Return True
    End Function

End Class
