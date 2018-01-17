Imports System.Linq
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports System

Namespace FlowPreservation
    ''' <summary>
    ''' Immediately schedules continuation onto default synchronization context
    ''' </summary>
    Public NotInheritable Class ContextHelper
        Public Shared Function DropContext() As DropContextAwaitable
            Return Nothing
        End Function
    End Class

    ''' <summary>
    ''' Analog to YieldAwaitable that does not preserve context
    ''' </summary>
    Public Structure DropContextAwaitable
        Public Function GetAwaiter() As DropContextAwaiter
            Return Nothing
        End Function
    End Structure

    ''' <summary>
    ''' Immediately schedules continuation onto default synchronization context
    ''' </summary>
    Public Structure DropContextAwaiter
        Implements INotifyCompletion

        Private Shared ReadOnly DefaultContext As New SynchronizationContext

        Public ReadOnly Property IsCompleted As Boolean
            Get
                Return False
            End Get
        End Property
        Public Sub GetResult()
        End Sub
        Public Sub OnCompleted(ByVal continuation As Action) Implements INotifyCompletion.OnCompleted
            DefaultContext.Post(Sub(t) continuation(), Nothing)
		End Sub
    End Structure
End Namespace
