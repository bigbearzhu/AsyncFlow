Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Threading.Tasks

Namespace FlowPreservation
    ''' <summary>
    ''' Wraps a void-returning ICriticalNotifyCompletion-based awaiter to execute an action 
    ''' after asynchronous part is completed, but before it schedules the continuation
    ''' </summary>
    ''' <typeparam name="TAwaiter">Awaiter type</typeparam>
    Public Structure CriticalAppendableAwaiter(Of TAwaiter As ICriticalNotifyCompletion)
        Implements ICriticalNotifyCompletion

        Private ReadOnly awaiter As TAwaiter
        Private ReadOnly preCompleted As Action
        Private ReadOnly isCompletedVal As Func(Of TAwaiter, Boolean)
        Private ReadOnly getResultVal As Action(Of TAwaiter)
        Private _getAwaiterVal As Object
        Private _preCompleted As Action
        Private _isCompleted As Object
        Private _getResult As Object

        Public Sub New(ByVal awaiter As TAwaiter, ByVal preCompleted As Action, ByVal isCompleted As Func(Of TAwaiter, Boolean), ByVal getResult As Action(Of TAwaiter))
            Me.awaiter = awaiter
            Me.preCompleted = preCompleted
            Me.isCompletedVal = isCompleted
            Me.getResultVal = getResult
        End Sub

        Sub New(getAwaiterVal As Object, preCompleted As Action, isCompleted As Object, getResult As Object)
            ' TODO: Complete member initialization 
            _getAwaiterVal = getAwaiterVal
            _preCompleted = preCompleted
            _isCompleted = isCompleted
            _getResult = getResult
        End Sub

        Public ReadOnly Property IsCompleted As Boolean
            Get
                Return isCompletedVal(awaiter)
            End Get
        End Property

        Public Sub UnsafeOnCompleted(ByVal continuation As Action) Implements ICriticalNotifyCompletion.UnsafeOnCompleted
            Dim thisCopy = Me
            awaiter.OnCompleted(thisCopy.preCompleted.Append(continuation))
        End Sub

        Public Sub OnCompleted(ByVal continuation As Action) Implements System.Runtime.CompilerServices.INotifyCompletion.OnCompleted
            Dim thisCopy = Me
            awaiter.OnCompleted(thisCopy.preCompleted.Append(continuation))
        End Sub

        Public Sub GetResult()
            getResultVal(awaiter)
        End Sub
    End Structure

    ''' <summary>
    ''' Wraps a result-returning ICriticalNotifyCompletion-based awaiter to execute an action 
    ''' after asynchronous part is completed, but before it schedules the continuation
    ''' </summary>
    ''' <typeparam name="TAwaiter">Awaiter type</typeparam>
    ''' <typeparam name="TResult">Result type</typeparam>
    Public Structure CriticalAppendableAwaiter(Of TAwaiter As ICriticalNotifyCompletion, TResult)
        Implements ICriticalNotifyCompletion

        Private ReadOnly awaiter As TAwaiter
        Private ReadOnly preCompleted As Action
        Private ReadOnly isCompletedVal As Func(Of TAwaiter, Boolean)
        Private ReadOnly getResultVal As Func(Of TAwaiter, TResult)
        Private _getAwaiterVal As Object
        Private _preCompleted As Action
        Private _isCompleted As Object
        Private _getResult As Object

        Public Sub New(ByVal awaiter As TAwaiter, ByVal preCompleted As Action, ByVal isCompleted As Func(Of TAwaiter, Boolean), ByVal getResult As Func(Of TAwaiter, TResult))
            Me.awaiter = awaiter
            Me.preCompleted = preCompleted
            Me.isCompletedVal = isCompleted
            Me.getResultVal = getResult
        End Sub

        Sub New(getAwaiterVal As Object, preCompleted As Action, isCompleted As Object, getResult As Object)
            ' TODO: Complete member initialization 
            _getAwaiterVal = getAwaiterVal
            _preCompleted = preCompleted
            _isCompleted = isCompleted
            _getResult = getResult
        End Sub

        Public ReadOnly Property IsCompleted As Boolean
            Get
                Return isCompletedVal(awaiter)
            End Get
        End Property

        Public Sub UnsafeOnCompleted(ByVal continuation As Action) Implements ICriticalNotifyCompletion.UnsafeOnCompleted
            Dim thisCopy = Me
            awaiter.OnCompleted(thisCopy.preCompleted.Append(continuation))
        End Sub

        Public Sub OnCompleted(ByVal continuation As Action) Implements System.Runtime.CompilerServices.INotifyCompletion.OnCompleted
            Dim thisCopy = Me
            awaiter.OnCompleted(thisCopy.preCompleted.Append(continuation))
        End Sub

        Public Function GetResult() As TResult
            Return getResultVal(awaiter)
        End Function
    End Structure
End Namespace
