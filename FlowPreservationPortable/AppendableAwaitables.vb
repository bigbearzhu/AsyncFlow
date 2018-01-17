Imports System.Collections.Concurrent
Imports System.Linq
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Threading.Tasks
Imports System

Namespace FlowPreservation
    ''' <summary>
    ''' Wraps a void-returning ICriticalNotifyCompletion-based awaitable to execute an action 
    ''' after asynchronous part is completed, but before it schedules the continuation
    ''' </summary>
    ''' <typeparam name="TAwaiter">Awaiter type</typeparam>
    Public Structure CriticalAppendableAwaitable(Of TAwaiter As ICriticalNotifyCompletion)
        Private ReadOnly getAwaiterVal As Func(Of TAwaiter)
        Private ReadOnly preCompleted As Action
        Private ReadOnly isCompleted As Func(Of TAwaiter, Boolean)
        Private ReadOnly getResult As Action(Of TAwaiter)

        Public Sub New(ByVal getAwaiter As Func(Of TAwaiter), ByVal preCompleted As Action, ByVal isCompleted As Func(Of TAwaiter, Boolean), ByVal getResult As Action(Of TAwaiter))
            Me.getAwaiterVal = getAwaiter
            Me.preCompleted = preCompleted
            Me.isCompleted = isCompleted
            Me.getResult = getResult
        End Sub

        Public Function GetAwaiter() As CriticalAppendableAwaiter(Of TAwaiter)
            Return New CriticalAppendableAwaiter(Of TAwaiter)(getAwaiterVal, preCompleted, isCompleted, getResult)
        End Function
    End Structure

    ''' <summary>
    ''' Wraps a result-returning ICriticalNotifyCompletion-based awaitable to execute an action 
    ''' after asynchronous part is completed, but before it schedules the continuation
    ''' </summary>
    ''' <typeparam name="TAwaiter">Awaiter type</typeparam>
    ''' <typeparam name="TResult">Result type</typeparam>
    Public Structure CriticalAppendableAwaitable(Of TAwaiter As ICriticalNotifyCompletion, TResult)
        Private ReadOnly getAwaiterVal As Func(Of TAwaiter)
        Private ReadOnly preCompleted As Action
        Private ReadOnly isCompleted As Func(Of TAwaiter, Boolean)
        Private ReadOnly getResult As Func(Of TAwaiter, TResult)

        Public Sub New(ByVal getAwaiter As Func(Of TAwaiter), ByVal preCompleted As Action, ByVal isCompleted As Func(Of TAwaiter, Boolean), ByVal getResult As Func(Of TAwaiter, TResult))
            Me.getAwaiterVal = getAwaiter
            Me.preCompleted = preCompleted
            Me.isCompleted = isCompleted
            Me.getResult = getResult
        End Sub

        Public Function GetAwaiter() As CriticalAppendableAwaiter(Of TAwaiter, TResult)
            Return New CriticalAppendableAwaiter(Of TAwaiter, TResult)(getAwaiterVal, preCompleted, isCompleted, getResult)
        End Function
    End Structure
End Namespace
