Imports System.Threading

Class MainWindow
    Private isRunning As Boolean = False
    Private intervalInSeconds As Integer = 5

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        'GetProcessModule()
        'FetchResourceData()

        'Run()
    End Sub

    Private Async Sub Run()
        isRunning = True
        Lbl_Status.Content = "Running!"
        Lbl_Status.FontWeight = FontWeights.DemiBold
        Lbl_Status.Foreground = Brushes.Green

        Dim swapper As New WeaponSwapper()

        'TEST
        'Dim weaponId As Integer = swapper.ReadFromMemory()
        'Debug.Print($"Id of current R1 weapon : {weaponId}")

        Dim timer As New PeriodicTimer(TimeSpan.FromSeconds(intervalInSeconds))
        While isRunning AndAlso Await timer.WaitForNextTickAsync()
            Dim weapon As Weapon = swapper.GetRandomWeapon()

            'TEST
            'Debug.Print($"(Swap every {intervalInSeconds}s) Random weapon: {weapon.Name} ({weapon.Id})")

            swapper.WriteIntoMemory(weapon.Id)
        End While

        Lbl_Status.Content = "Idle"
        Lbl_Status.ClearValue(Label.ForegroundProperty)
    End Sub

    Private Sub Tbx_Interval_Loaded(sender As TextBox, e As RoutedEventArgs)
        'intervalInSeconds = sender.Text
        sender.Text = intervalInSeconds
    End Sub

    Private Sub Tbx_Interval_TextChanged(sender As TextBox, e As TextChangedEventArgs)

        ''MessageBox.Show(e.Source.GetType().ToString)
        'MessageBox.Show(e.Handled)

        'If Not Integer.TryParse(sender.Text, Nothing) Then
        '    MessageBox.Show("Only numeric values are allowed")
        '    sender.Text = intervalInSeconds
        '    Return
        'End If

        'Dim interval As Integer = Convert.ToInt32(sender.Text)
        'If interval < 3 OrElse interval > 60 Then
        '    MessageBox.Show("Interval must be between 3 and 60 seconds")
        '    sender.Text = intervalInSeconds
        '    Return
        'End If

        'intervalInSeconds = interval
    End Sub

    Private Sub Btn_Run_Click(sender As Object, e As RoutedEventArgs)
        Run()
    End Sub

    Private Sub Btn_Stop_Click(sender As Object, e As RoutedEventArgs)
        isRunning = False
    End Sub

    Private Sub Btn_IntervalMore_Click(sender As Object, e As RoutedEventArgs)
        'Dim tbxInterval As TextBox = Tbx_Interval
        'tbxInterval.Text = Convert.ToInt32(tbxInterval.Text) + 1
    End Sub

    Private Sub Btn_IntervalLess_Click(sender As Object, e As RoutedEventArgs)
        'Dim tbxInterval As TextBox = Tbx_Interval
        'tbxInterval.Text = Convert.ToInt32(tbxInterval.Text) - 1
    End Sub

End Class
