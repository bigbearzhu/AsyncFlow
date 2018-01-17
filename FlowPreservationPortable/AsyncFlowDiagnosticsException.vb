Imports System.Linq
Imports System.Runtime.Serialization
Imports System.Text
Imports System.Threading.Tasks
Imports System

Namespace FlowPreservation
    ''' <summary>
    ''' All-wrapping async diagnostics exception
    ''' </summary>
    Public Class AsyncFlowDiagnosticsException
        Inherits Exception

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByVal message As String)
            MyBase.New(message)
        End Sub

        Public Sub New(ByVal message As String, ByVal innerException As Exception)
            MyBase.New(message, innerException)
        End Sub
    End Class
End Namespace
