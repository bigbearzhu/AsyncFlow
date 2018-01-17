Imports System.Diagnostics.Tracing
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Collections.Generic
Imports System

Namespace FlowPreservation
    ''' <summary>
    ''' TplEtwProvider and FrameworkEventSource listener base class
    ''' </summary>
    Public MustInherit Class TplFrameworkListenerBase
        Inherits EventListener

        Protected Shared Sub EnsureInitialized(Of T As TplFrameworkListenerBase)(ByVal constructor As Func(Of T), ByRef storage As T)
            ' Sometimes EventListener constructor may throw a "Collection was modified" exception,
            ' when event sources are being created simultaneously with listener instantiation.
            ' To work around that subscriptions are delayed till after constuctor successfully executes.
            LazyInitializer.EnsureInitialized(storage, Function()
                                                           Dim listener As T
                                                           Do
                                                               Try
                                                                   listener = constructor()
                                                                   Exit Do
                                                               Catch e1 As InvalidOperationException
                                                               End Try
                                                           Loop
                                                           listener.EnableSubscriptions()
                                                           listener.Initialized.Wait()
                                                           Return listener
                                                       End Function)
        End Sub
        Private _delayedSubscriptions As List(Of EventSource)
        Private _initializedTcs As TaskCompletionSource(Of Object)

        Private ReadOnly Property DelayedSubscriptions As List(Of EventSource)
            Get
                LazyInitializer.EnsureInitialized(_delayedSubscriptions)
                Return _delayedSubscriptions
            End Get
        End Property

        Private ReadOnly Property InitializedTcs As TaskCompletionSource(Of Object)
            Get
                LazyInitializer.EnsureInitialized(_initializedTcs)
                Return _initializedTcs
            End Get
        End Property

        ''' <summary>
        ''' Completes when both event sources are subscribed to
        ''' </summary>
        Public ReadOnly Property Initialized As Task
            Get
                Return initializedTcs.Task
            End Get
        End Property

        ''' <summary>
        ''' Adds event source to the list of subscriptions 
        ''' and subscribes to them if event listener initialization has already completed (subscriptionsEnabled == true)
        ''' </summary>
        ''' <param name="eventSource">Event source just created or existing one 
        ''' passed in to a newly created event listener by eventing infrastructure</param>
        Protected NotOverridable Overrides Sub OnEventSourceCreated(ByVal eventSource As EventSource)
            SyncLock delayedSubscriptions
                If eventSource.Guid = EventConstants.Tpl.GUID OrElse eventSource.Guid = EventConstants.Framework.GUID Then
                    delayedSubscriptions.Add(eventSource)
                End If
            End SyncLock
            If subscriptionsEnabled Then
                ProcessDelayedSubscriptions()
            End If
        End Sub

        Private subscriptionsEnabled As Boolean = False

        ''' <summary>
        ''' Indicate that event listener constructor has completed successfully and event sources stored 
        ''' in delayed subscription list can now be subscribed to
        ''' </summary>
        Public Sub EnableSubscriptions()
            subscriptionsEnabled = True
            ProcessDelayedSubscriptions()
        End Sub

        ''' <summary>
        ''' Actually subscribes event listener to sources 
        ''' </summary>
        Private Sub ProcessDelayedSubscriptions()
            Dim toSubscribe = New List(Of EventSource)
            SyncLock delayedSubscriptions
                toSubscribe.AddRange(delayedSubscriptions)
                delayedSubscriptions.Clear()
            End SyncLock
            For Each eventSource In toSubscribe
                If eventSource.Guid = EventConstants.Tpl.GUID Then
                    EnableEvents(eventSource, EventLevel.LogAlways)
                    TryCompleteInitialization()
                ElseIf eventSource.Guid = EventConstants.Framework.GUID Then
                    EnableEvents(eventSource, EventLevel.LogAlways, EventConstants.Framework.Keywords.ThreadPool)
                    TryCompleteInitialization()
                End If
            Next eventSource
        End Sub

        ''' <summary>
        ''' Set Initialized task as complete if subscribed to both sources
        ''' </summary>
        Private initializationCount As Integer

        Private Sub TryCompleteInitialization()
            If Interlocked.Increment(initializationCount) = 2 Then
                InitializedTcs.SetResult(Nothing)
            End If
        End Sub


        ''' <summary>
        ''' Handling events
        ''' </summary>
        ''' <param name="eventData">Event data</param>
        Protected NotOverridable Overrides Sub OnEventWritten(ByVal eventData As EventWrittenEventArgs)
            If eventData.EventSource.Guid = EventConstants.Tpl.GUID Then
                Select Case eventData.EventId
                    Case EventConstants.Tpl.TASKSCHEDULED_ID
                        TaskScheduled(GetTaskId(eventData))
                    Case EventConstants.Tpl.TASKWAITBEGIN_ID
                        TaskWaitBegin(GetTaskId(eventData), GetWaitBehavior(eventData))
                    Case EventConstants.Tpl.TASKWAITEND_ID
                        TaskWaitEnd(GetTaskId(eventData))
                End Select
            ElseIf eventData.EventSource.Guid = EventConstants.Framework.GUID Then
                Select Case eventData.EventId
                    Case EventConstants.Framework.THREADPOOLDEQUEUEWORK_ID
                        ResetLocal()
                End Select
            End If
        End Sub

        Private Function GetTaskId(ByVal eventData As EventWrittenEventArgs) As Integer
            Return CInt((eventData.Payload(2)))
        End Function

        Private Function GetWaitBehavior(ByVal eventData As EventWrittenEventArgs) As EventConstants.Tpl.TaskWaitBehavior
            Return CType(CInt((eventData.Payload(3))), EventConstants.Tpl.TaskWaitBehavior)
        End Function

        Protected Overridable Sub TaskScheduled(ByVal taskId As Integer)
        End Sub

        Protected Overridable Sub TaskWaitBegin(ByVal taskId As Integer, ByVal behavior As EventConstants.Tpl.TaskWaitBehavior)
        End Sub

        Protected Overridable Sub TaskWaitEnd(ByVal taskId As Integer)
        End Sub

        ''' <summary>
        ''' Indicates that new unrelated work item starts executing on current thread
        ''' </summary>
        Protected Overridable Sub ResetLocal()
        End Sub
    End Class
End Namespace
