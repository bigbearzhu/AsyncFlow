Imports System.Collections.Concurrent
Imports System.Diagnostics.Tracing
Imports System.Linq
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports System
Imports System.Collections.Generic

Namespace FlowPreservation
    ''' <summary>
    ''' TplEtwProvider and FrameworkEventSource listener supporting the implementation of AsyncLocal{T}
    ''' </summary>
    Friend Class AsyncLocalListener
        Inherits TplFrameworkListenerBase

        Private Shared listener As AsyncLocalListener

        Public Overloads Shared Sub EnsureInitialized()
            EnsureInitialized(Function() New AsyncLocalListener, listener)
        End Sub

        Private ReadOnly temporaryStorage As New ConcurrentDictionary(Of Integer, Dictionary(Of Guid, Object))

        Protected Overrides Sub TaskWaitBegin(ByVal taskId As Integer, ByVal behavior As EventConstants.Tpl.TaskWaitBehavior)
            If Not temporaryStorage.TryAdd(taskId, AsyncLocal.PersistentThreadStorage) Then
                Throw New AsyncFlowDiagnosticsException("Data already exists in temporary storage for task " & taskId)
            End If
        End Sub

        ''' <summary>
        ''' Only transferring values to the new thread for PersistentThreadStorage, 
        ''' i.e. the one used by AsyncLocal{T} inititialized with discardOnAwaitBoundary = false
        ''' </summary>
        Protected Overrides Sub TaskWaitEnd(ByVal taskId As Integer)
            Dim threadStorage As Dictionary(Of Guid, Object) = Nothing
            If temporaryStorage.TryRemove(taskId, threadStorage) Then
                AsyncLocal.PersistentThreadStorage = threadStorage
            Else
                Throw New AsyncFlowDiagnosticsException("Data not present in temporary storage for task " & taskId)
            End If
        End Sub

        ''' <summary>
        ''' New unrelated work item starts executing on current thread - need to clean up thread-local storage
        ''' </summary>
        Protected Overrides Sub ResetLocal()
            AsyncLocal.PersistentThreadStorage = Nothing
            AsyncLocal.DiscardableThreadStorage = Nothing
        End Sub

        Private Overloads Shared Sub EnsureInitialized(p1 As Object, listener As AsyncLocalListener)
            Throw New NotImplementedException
        End Sub
    End Class
End Namespace
