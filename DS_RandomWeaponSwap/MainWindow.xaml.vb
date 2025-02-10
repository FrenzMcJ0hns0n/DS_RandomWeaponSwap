Imports System.Threading

Class MainWindow

    Private ReadOnly isDebug As Boolean = Debugger.IsAttached

    Private settings As UserSettings
    Private isRunning As Boolean = False

    Private Const defInterval As Integer = 5 'Default value
    Private Const minInterval As Integer = 3
    Private Const maxInterval As Integer = 60
    Private intervalInSeconds As Integer



    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        settings = LoadSettings()

        If settings.Interval IsNot Nothing AndAlso settings.Interval >= minInterval AndAlso settings.Interval <= maxInterval Then
            intervalInSeconds = settings.Interval
        Else
            intervalInSeconds = defInterval
        End If
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

        'Persist user settings
        settings.Interval = intervalInSeconds
        SaveSettings(settings)

        'Now do your job, program!
        isRunning = True
        UpdateControls(isRunning)

        Dim swapper As New WeaponSwapper()
        If isDebug Then Debug.Print($"Id of current R1 weapon : {swapper.ReadFromMemory()}")

        Dim timer As New PeriodicTimer(TimeSpan.FromSeconds(intervalInSeconds))
        While Await timer.WaitForNextTickAsync()
            If Not isRunning Then Exit While

            Dim weapon As Weapon = swapper.GetRandomWeapon()
            If isDebug Then Debug.Print($"(Swap every {intervalInSeconds}s) Random weapon: {weapon.Name} ({weapon.Id})")
            swapper.WriteIntoMemory(weapon.Id)
        End While

        UpdateControls(isRunning)
    End Sub

    Private Sub UpdateControls(isRunning As Boolean)
        If isRunning Then
            Lbl_Status.Content = "Running!"
            Btn_Run.IsEnabled = False
            Btn_Stop.IsEnabled = True
            Btn_IntervalMore.IsEnabled = False
            Btn_IntervalLess.IsEnabled = False
            Tbx_Interval.IsEnabled = False
            Lbl_Status.FontWeight = FontWeights.DemiBold
            Lbl_Status.Foreground = Brushes.Green
        Else
            Lbl_Status.Content = "Idle"
            Btn_Run.IsEnabled = True
            Btn_Stop.IsEnabled = False
            Btn_IntervalMore.IsEnabled = True
            Btn_IntervalLess.IsEnabled = True
            Tbx_Interval.IsEnabled = True
            Lbl_Status.ClearValue(Label.FontWeightProperty)
            Lbl_Status.ClearValue(Label.ForegroundProperty)
        End If
    End Sub

    Private Function ValidateIntervalInput() As Boolean
        Dim input As Integer

        If Not Integer.TryParse(Tbx_Interval.Text, input) Then
            MessageBox.Show("Only numeric values are allowed", "Input error", MessageBoxButton.OK, MessageBoxImage.Error)
            Return False
        End If

        If input < minInterval OrElse input > maxInterval Then
            MessageBox.Show($"Interval must be between {minInterval} and {maxInterval} seconds", "Input error", MessageBoxButton.OK, MessageBoxImage.Error)
            Return False
        End If

        Return True
    End Function

End Class
