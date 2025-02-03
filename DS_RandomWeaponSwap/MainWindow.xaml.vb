Imports System.Threading

Class MainWindow

    Private isDebug As Boolean = Debugger.IsAttached 'Best way?

    Private settings As UserSettings
    Private isRunning As Boolean = False

    Private Const defInterval As Integer = 5 'Default value
    Private Const minInterval As Integer = 3
    Private Const maxInterval As Integer = 60
    Private intervalInSeconds As Integer


    Private Async Sub Run()
        'Persist user settings
        settings.Interval = intervalInSeconds
        SaveSettings(settings)

        'Now do your job, program!
        isRunning = True
        Lbl_Status.Content = "Running!"
        Lbl_Status.FontWeight = FontWeights.DemiBold
        Lbl_Status.Foreground = Brushes.Green

        Dim swapper As New WeaponSwapper()
        If isDebug Then Debug.Print($"Id of current R1 weapon : {swapper.ReadFromMemory()}")

        Dim timer As New PeriodicTimer(TimeSpan.FromSeconds(intervalInSeconds))
        While Await timer.WaitForNextTickAsync()
            If Not isRunning Then Exit While

            Dim weapon As Weapon = swapper.GetRandomWeapon()
            If isDebug Then Debug.Print($"(Swap every {intervalInSeconds}s) Random weapon: {weapon.Name} ({weapon.Id})")
            swapper.WriteIntoMemory(weapon.Id)
        End While

        Lbl_Status.Content = "Idle"
        Lbl_Status.ClearValue(Label.FontWeightProperty)
        Lbl_Status.ClearValue(Label.ForegroundProperty)
    End Sub


    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        settings = LoadSettings()

        If settings.Interval IsNot Nothing AndAlso
           settings.Interval >= minInterval AndAlso
           settings.Interval <= maxInterval Then
            intervalInSeconds = settings.Interval
        Else
            intervalInSeconds = defInterval
        End If
    End Sub


    Private Sub Tbx_Interval_Loaded(sender As TextBox, e As RoutedEventArgs)
        sender.Text = intervalInSeconds
    End Sub


    'This is bad. TODO: Replace it with a Focus-oriented logic
    Private Sub Tbx_Interval_PreviewTextInput(sender As TextBox, e As TextCompositionEventArgs)
        'If Not Integer.TryParse(e.Text, Nothing) Then
        '    MessageBox.Show("Input error : Only numeric values are allowed")
        '    e.Handled = True
        '    Return
        'End If

        'Dim interval As Integer = Convert.ToInt32(e.Text)
        'If interval < minInterval OrElse interval > maxInterval Then
        '    MessageBox.Show($"Input error : Interval must be between {minInterval} and {maxInterval} seconds")
        '    e.Handled = True
        'End If

        'intervalInSeconds = interval
    End Sub


    Private Sub Btn_Run_Click(sender As Object, e As RoutedEventArgs)
        Run()
    End Sub

    Private Sub Btn_Stop_Click(sender As Object, e As RoutedEventArgs)
        Lbl_Status.Content = "Stopping..."
        Lbl_Status.Foreground = Brushes.Red
        isRunning = False
    End Sub


    Private Sub Btn_IntervalMore_Click(sender As Object, e As RoutedEventArgs)
        UpdateInterval(1)
    End Sub

    Private Sub Btn_IntervalLess_Click(sender As Object, e As RoutedEventArgs)
        UpdateInterval(-1)
    End Sub

    Private Sub UpdateInterval(valueToAdd As Integer)
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

End Class
