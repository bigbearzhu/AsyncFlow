Imports System.Collections.Concurrent
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

Namespace FlowPreservation
	''' <summary>
	''' Stores stack trace segment lists per thread 
	''' </summary>
	Friend Class StackStorage
		Private ReadOnly stacksByTask As New ConcurrentDictionary(Of Long, StackTraceNode)
		Private ReadOnly storeFileLineColumnInfo As Boolean
		Private ReadOnly justMyCode As Boolean

		<ThreadStatic>
		Private Shared preservedStacks As StackTraceNode

		''' <summary>
		''' Constructor for stack trace segment lists storage
		''' </summary>
		''' <param name="storeFileLineColumnInfo">Whether to extract source code information</param>
		''' <param name="justMyCode">Avoid framework boilerplate</param>
		Public Sub New(ByVal storeFileLineColumnInfo As Boolean, ByVal justMyCode As Boolean)
			Me.storeFileLineColumnInfo = storeFileLineColumnInfo
			Me.justMyCode = justMyCode
		End Sub

		''' <summary>
		''' Prepend current (synchronous) stack trace segment a stack to currently stored list for current thread and store the result in the dictionary by taskId (not clearing up thread-local storage)
		''' </summary>
		''' <param name="taskId">Correlation id to store stack trace list under temporarily, while asynchronous operation is being scheduled onto a possibly different thread</param>
		''' <param name="synchronous">TaskWaitBehavior.Synchronous, if a task is being executed synchronously, or has completed already</param>
		Public Sub StoreStack(ByVal taskId As Long, ByVal synchronous As Boolean)
			Dim toStore = If(synchronous, Nothing, preservedStacks.Prepend(GetCurrentSegment))
			stacksByTask.AddOrUpdate(taskId, toStore, Function(tid, existing) existing.SelectLongest(toStore))
		End Sub

		''' <summary>
		''' Remove temporarily stored stack trace list from the dictionary and store it back to thread-local storage
		''' </summary>
		''' <param name="taskId">Correlation id</param>
		Public Sub RestoreStack(ByVal taskId As Long)
            Dim stored As StackTraceNode = Nothing
			If stacksByTask.TryRemove(taskId, stored) Then
				preservedStacks = preservedStacks.SelectLongest(stored)
			Else
				Trace.WriteLine("Stack information not stored in transitional repository for task " & taskId & " and thread " & Thread.CurrentThread.ManagedThreadId)
			End If
		End Sub

		''' <summary>
		''' Clear thread-local storage, because a new unrelated work item has started to execute on current thread
		''' </summary>
		Public Sub ResetLocal()
			preservedStacks = Nothing
		End Sub

		''' <summary>
		''' Get formatted causality chain for current thread
		''' </summary>
		''' <returns>Formatted causality chain</returns>
		Public Function GetAggregateStackString() As String
			Dim builder = New StringBuilder
			For Each frame In GetCurrentSegment()
				builder.AppendLine(StackTraceNodeExtensions.PrettifyFrame(frame.ToString))
			Next frame
			builder.Append(preservedStacks.ToStringEx)
			Return builder.ToString
		End Function

		''' <summary>
		''' Captures current stack trace
		''' </summary>
		''' <returns>Current stack frames</returns>
		Private Function GetCurrentSegment() As IEnumerable(Of StackFrameSlim)
			Return New StackTraceSlim(storeFileLineColumnInfo).Frames.Where(Function(f) (Not f.IsHidden(justMyCode)))
		End Function
	End Class
End Namespace
