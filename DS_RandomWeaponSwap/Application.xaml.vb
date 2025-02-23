Class Application

    'Is app launched in "Debug" mode?
    Public Shared ReadOnly Property IsDebug As Boolean
        Get
            Return Debugger.IsAttached
        End Get
    End Property

End Class
