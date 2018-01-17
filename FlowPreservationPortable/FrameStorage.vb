Imports System.Linq
Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Threading.Tasks
Imports System.Collections.Generic
Imports System

Namespace FlowPreservation
    ''' <summary>
    ''' Main class that is used to preserve causality information in Windows Store applications
    ''' </summary>
    Public Module FrameStorage
        Private ReadOnly Frames As New AsyncLocal(Of LinkedList(Of String))(Function() New LinkedList(Of String)(Enumerable.Repeat(String.Empty, 1)), True)

        Private ReadOnly PreviousSegmentHead As New AsyncLocal(Of StrongBox(Of LinkedListNode(Of String)))(Function() New StrongBox(Of LinkedListNode(Of String)), True)

#Region "WithCausality"
        ''' <summary>
        ''' Adds currently executing method to frames list and registers an action to remove it 
        ''' as soon as asynchronous work completes
        ''' </summary>
        ''' <param name="task">Void-returning task</param>
        <System.Runtime.CompilerServices.Extension>
        Public Function WithCausality(ByVal task As Task, <CallerMemberName> Optional ByVal member As String = Nothing, <CallerFilePath> Optional ByVal file As String = Nothing, <CallerLineNumber> Optional ByVal line As Integer = 0) As Task
            Dim removeAction = AddAndCreateRemoveAction(member, file, line)
            Return task.ContinueWith(Sub(t)
                                         removeAction()
                                     End Sub)
        End Function

        ''' <summary>
        ''' Adds currently executing method to frames list and registers an action to remove it 
        ''' as soon as asynchronous work completes
        ''' </summary>
        ''' <param name="task">Result-returning task</param>
        <System.Runtime.CompilerServices.Extension>
        Public Function WithCausality(Of T)(ByVal task As Task(Of T), <CallerMemberName> Optional ByVal member As String = Nothing, <CallerFilePath> Optional ByVal file As String = Nothing, <CallerLineNumber> Optional ByVal line As Integer = 0) As Task(Of T)
            Dim removeAction = AddAndCreateRemoveAction(member, file, line)
            Return task.ContinueWith(Function(temp)
                                         removeAction()
                                         Return temp.Result
                                     End Function)
        End Function

        ''' <summary>
        ''' Adds currently executing method to frames list and registers an action to remove it 
        ''' as soon as asynchronous work completes
        ''' </summary>
        ''' <param name="awaitable">An awaitable returned by Task.Yield</param>
        <System.Runtime.CompilerServices.Extension>
        Public Function WithCausality(ByVal awaitable As YieldAwaitable, <CallerMemberName> Optional ByVal member As String = Nothing, <CallerFilePath> Optional ByVal file As String = Nothing, <CallerLineNumber> Optional ByVal line As Integer = 0) As CriticalAppendableAwaitable(Of YieldAwaitable.YieldAwaiter)
            Dim removeAction = AddAndCreateRemoveAction(member, file, line)
            Return New CriticalAppendableAwaitable(Of YieldAwaitable.YieldAwaiter)(Function() awaitable.GetAwaiter, removeAction, Function(awaiter) awaiter.IsCompleted, Sub(awaiter) awaiter.GetResult)
        End Function

        ''' <summary>
        ''' Adds currently executing method to frames list and registers an action to remove it 
        ''' as soon as asynchronous work completes
        ''' </summary>
        ''' <param name="awaitable">An awaitable returned by Task.ConfigureAwait on a void-returning task</param>
        <System.Runtime.CompilerServices.Extension>
        Public Function WithCausality(ByVal awaitable As ConfiguredTaskAwaitable, <CallerMemberName> Optional ByVal member As String = Nothing, <CallerFilePath> Optional ByVal file As String = Nothing, <CallerLineNumber> Optional ByVal line As Integer = 0) As CriticalAppendableAwaitable(Of ConfiguredTaskAwaitable.ConfiguredTaskAwaiter)
            Dim removeAction = AddAndCreateRemoveAction(member, file, line)
            Return New CriticalAppendableAwaitable(Of ConfiguredTaskAwaitable.ConfiguredTaskAwaiter)(Function() awaitable.GetAwaiter, removeAction, Function(awaiter) awaiter.IsCompleted, Sub(awaiter) awaiter.GetResult)
        End Function

        ''' <summary>
        ''' Adds currently executing method to frames list and registers an action to remove it 
        ''' as soon as asynchronous work completes
        ''' </summary>
        ''' <param name="awaitable">An awaitable returned by Task.ConfigureAwait on a result-returning task</param>
        <System.Runtime.CompilerServices.Extension>
        Public Function WithCausality(Of T)(ByVal awaitable As ConfiguredTaskAwaitable(Of T), <CallerMemberName> Optional ByVal member As String = Nothing, <CallerFilePath> Optional ByVal file As String = Nothing, <CallerLineNumber> Optional ByVal line As Integer = 0) As CriticalAppendableAwaitable(Of ConfiguredTaskAwaitable(Of T).ConfiguredTaskAwaiter, T)
            Dim removeAction = AddAndCreateRemoveAction(member, file, line)
            Return New CriticalAppendableAwaitable(Of ConfiguredTaskAwaitable(Of T).ConfiguredTaskAwaiter, T)(Function() awaitable.GetAwaiter, removeAction, Function(awaiter) awaiter.IsCompleted, Function(awaiter) awaiter.GetResult)
        End Function
#End Region

        ''' <summary>
        ''' Returns stored causality chain for current asynchronous operation (thread) except for currently executing frame
        ''' </summary>
        Public ReadOnly Property CausalityChain As String
            Get
                Return String.Join(Environment.NewLine, Frames.Value).Trim
            End Get
        End Property

        Private Function FormatMemberInfo(ByVal member As String, ByVal file As String, ByVal line As Integer) As String
            Return member & " in " & file & " at line " & line
        End Function

        ''' <summary>
        ''' Adds frame to the causality linked list and returns an action to remove it
        ''' </summary>
        ''' <returns>An action that removes the frame just added from the causality list</returns>
        Private Function AddAndCreateRemoveAction(ByVal member As String, ByVal file As String, ByVal line As Integer) As Action
            Dim framesValueCopy = Frames.Value
            Dim previousSegmentHeadValueCopy = PreviousSegmentHead.Value
            If previousSegmentHeadValueCopy.Value Is Nothing Then
                previousSegmentHeadValueCopy.Value = framesValueCopy.First
            End If
            framesValueCopy.AddBefore(previousSegmentHeadValueCopy.Value, FormatMemberInfo(member, file, line))
            Dim addedNode = previousSegmentHeadValueCopy.Value.Previous
            Return Sub()
                       PreviousSegmentHead.Value.Value = Nothing
                       previousSegmentHeadValueCopy.Value = Nothing
                       framesValueCopy.Remove(addedNode)
                       Frames.Value = framesValueCopy
                   End Sub
        End Function
    End Module
End Namespace
