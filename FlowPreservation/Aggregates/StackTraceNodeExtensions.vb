Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading.Tasks

Namespace FlowPreservation
    ''' <summary>
    ''' Extensions for StackTraceNode. Implemented as extensions to allow executing on a null list head
    ''' </summary>
    Friend Module StackTraceNodeExtensions
        Private Const loopHeaderFormat1 As String = "\u21ba {0} times {{"
        Private Const loopFooter As String = "}"
        Private Const indentStop As String = "    "
        Private ReadOnly lambdaRegex As New Regex("^(?<container>.*?)\+<>c__DisplayClass\d+\.<(?<method>[^>]+)>b__[0-9A-Fa-f]+\(\)")
        Private ReadOnly stateMachineRegex As New Regex("^(?<container>.*?)\+(?:<>c__DisplayClass\d+\.)?<(?<method>[^>]+)>d__[0-9A-Fa-f]+\.MoveNext\(\)")
        Private ReadOnly methodNameRegex As New Regex("^(?<fullName>[^() ]{10,})\((?<args>[^)]*)\)")

        ''' <summary>
        ''' Returns the longest stack trace list
        ''' </summary>
        ''' <param name="a">First stack trace list</param>
        ''' <param name="b">Second stack trace list</param>
        ''' <returns>The longest stack trace list of the two</returns>
        <System.Runtime.CompilerServices.Extension> _
        Public Function SelectLongest(ByVal a As StackTraceNode, ByVal b As StackTraceNode) As StackTraceNode
            Return If(a.TotalCountWithLoops() > b.TotalCountWithLoops(), a, b)
        End Function

        ''' <summary>
        ''' Adds new element to the linked list and returns the new head
        ''' </summary>
        ''' <param name="node">Old list head</param>
        ''' <param name="topFrames">Stack frames to prepent. If empty, old list head will be returned</param>
        ''' <returns>Head of the list with prepended frames</returns>
        <System.Runtime.CompilerServices.Extension> _
        Public Function Prepend(ByVal node As StackTraceNode, ByVal topFrames As IEnumerable(Of StackFrameSlim)) As StackTraceNode
            If Not topFrames.Any() Then
                Return node
            End If

            Dim frames = topFrames.Reverse().Concat(Enumerable.Repeat(StackFrameSlim.AsyncBoundary, 1)).ToArray()

            Dim lastHash As Long = If(node Is Nothing OrElse node.Value.Count = 0, 0, node.Value.Hashes(node.Value.Count - 1))
            Dim hashes = New Long(frames.Length - 1) {}
            For i As Integer = 0 To frames.Length - 1
                hashes(i) = PolyHash.Append(lastHash, frames(i).LongHash)
                lastHash = hashes(i)
            Next i

            Dim loops = If(node Is Nothing, New StackTraceLoop() {}, node.Value.Loops.ToArray())

            Dim newAgg = New StackTraceSegment(frames, hashes, loops, node.TotalCount() + frames.Length, node.TotalCountWithLoops() + frames.Length)
            node = New StackTraceNode(newAgg, node)

            node = node.CollapseLoops()

            Return node
        End Function

        ''' <summary>
        ''' Walks the list and calculates sum of element values
        ''' </summary>
        ''' <param name="node">List head</param>
        ''' <param name="getElement">Value getter</param>
        ''' <returns>Sum of values</returns>
        <System.Runtime.CompilerServices.Extension> _
        Private Function Sum(ByVal node As StackTraceNode, ByVal getElement As Func(Of StackTraceSegment, Integer)) As Integer
            'INSTANT VB NOTE: The local variable sum was renamed since Visual Basic will not allow local variables with the same name as their enclosing function or property:
            Dim sum_Renamed As Integer = 0
            Do While node IsNot Nothing
                sum_Renamed += getElement(node.Value)
                node = node.Next
            Loop
            Return sum_Renamed
        End Function

        ''' <summary>
        ''' Count of elements in the list as if loops were expanded
        ''' </summary>
        ''' <param name="node">List head</param>
        ''' <returns>Total count of elements as if loops were expanded</returns>
        <System.Runtime.CompilerServices.Extension> _
        Public Function TotalCountWithLoops(ByVal node As StackTraceNode) As Integer
            Return If(node Is Nothing, 0, node.Value.TotalCountWithLoops)
        End Function

        ''' <summary>
        ''' Count of elements with loops collapsed (as if only one iteration of each counts)
        ''' </summary>
        ''' <param name="node">List head</param>
        ''' <returns>Count of elements with loops collapsed</returns>
        <System.Runtime.CompilerServices.Extension> _
        Public Function TotalCount(ByVal node As StackTraceNode) As Integer
            Return If(node Is Nothing, 0, node.Value.TotalCount)
        End Function

        ''' <summary>
        ''' Number of loops in the list
        ''' </summary>
        ''' <param name="node">List head</param>
        ''' <returns>Number of loops in the list</returns>
        <System.Runtime.CompilerServices.Extension> _
        Public Function LoopsCount(ByVal node As StackTraceNode) As Integer
            Return If(node Is Nothing, 0, node.Value.Loops.Length)
        End Function

        ''' <summary>
        ''' Get stack frame or its hash by index, using continuous numbering over the entire list
        ''' </summary>
        ''' <typeparam name="T">Stack frame or hash type</typeparam>
        ''' <param name="node">List head</param>
        ''' <param name="index">Index of the frame of interest, using continuous numbering over the entire list</param>
        ''' <param name="arrayGetter">Function returning stack frame or hash array from stack trace segment</param>
        ''' <returns>Stack frame or its hash</returns>
        <System.Runtime.CompilerServices.Extension> _
        Private Function GetFragment(Of T)(ByVal node As StackTraceNode, ByVal index As Integer, ByVal arrayGetter As Func(Of StackTraceSegment, T())) As T
            Dim curCount As Integer = node.TotalCount()
            If index >= curCount OrElse index < 0 Then
                Throw New ArgumentOutOfRangeException("index")
            End If

            Do While node IsNot Nothing
                If index < curCount AndAlso index >= curCount - node.Value.Count Then
                    Return arrayGetter(node.Value)(index - curCount + node.Value.Count)
                End If

                curCount -= node.Value.Count
                node = node.Next
            Loop
            Throw New InvalidOperationException("Could not get the frame by index in a linked list")
        End Function

        ''' <summary>
        ''' Get the stack frame by index, using continuous numbering over the entire list
        ''' </summary>
        ''' <param name="node">List head</param>
        ''' <param name="index">Index of the frame of interest, using continuous numbering over the entire list</param>
        ''' <returns>Stack frame</returns>
        <System.Runtime.CompilerServices.Extension> _
        Public Function GetFrame(ByVal node As StackTraceNode, ByVal index As Integer) As StackFrameSlim
            Return node.GetFragment(index, Function(t) t.Frames)
        End Function

        ''' <summary>
        ''' Gets hash of a frame by index (continuous numbering over the entire list)
        ''' </summary>
        ''' <param name="node">List head</param>
        ''' <param name="index">Index (continuous numbering over the entire list)</param>
        ''' <returns>Hash of a stack frame in question</returns>
        <System.Runtime.CompilerServices.Extension> _
        Public Function GetHash(ByVal node As StackTraceNode, ByVal index As Integer) As Long
            Return If(index < 0, 0, node.GetFragment(index, Function(t) t.Hashes))
        End Function

        ''' <summary>
        ''' Gets loop by index
        ''' </summary>
        ''' <param name="node">List head</param>
        ''' <param name="index">Index</param>
        ''' <returns>Loop</returns>
        <System.Runtime.CompilerServices.Extension> _
        Public Function GetLoop(ByVal node As StackTraceNode, ByVal index As Integer) As StackTraceLoop
            Return node.Value.Loops(index)
        End Function

        ''' <summary>
        ''' Makes 'c__DisplayClass' frame name more readable, by replacing it with the name of corresponding state machine (which is a method name)
        ''' </summary>
        ''' <param name="frameStr">A frame name string possibly with 'c__DisplayClass' content</param>
        ''' <param name="args">Method arguments (if null replaced by '...' symbol)</param>
        ''' <returns>A frame string with 'c__DisplayClass' replaced with actual state machine name</returns>
        Public Function PrettifyFrame(ByVal frameStr As String, Optional ByVal args As String = Nothing) As String
            'Dim replacement = "${container}.${method}(" & (If(args, ChrW(&H2026).ToString())) & ")"
            'frameStr = stateMachineRegex.Replace(frameStr, replacement)
            'frameStr = lambdaRegex.Replace(frameStr, replacement)
            Return frameStr
        End Function

        ''' <summary>
        ''' Formats the list nicely
        ''' </summary>
        ''' <param name="node">List head</param>
        ''' <returns>Nicely formatted concatenated causality chain</returns>
        <System.Runtime.CompilerServices.Extension> _
        Public Function ToStringEx(ByVal node As StackTraceNode) As String
            Dim indent = ""
            Return node.ToStringEx(indent)
        End Function

        ''' <summary>
        ''' Formats the list nicely
        ''' </summary>
        ''' <param name="node">List head</param>
        ''' <param name="indent">Indentation</param>
        ''' <returns>Nicely formatted contatenated causality chain</returns>
        <System.Runtime.CompilerServices.Extension> _
        Public Function ToStringEx(ByVal node As StackTraceNode, ByRef indent As String) As String
            If node Is Nothing Then
                Return ""
            End If

            Dim builder = New StringBuilder

            Dim count As Integer = node.TotalCount()
            Dim printNext As Boolean = True
            For i As Integer = count - 1 To 0 Step -1
                For j As Integer = 0 To node.LoopsCount() - 1
                    If node.GetLoop(j).HighIndex = i Then
                        builder.AppendFormat(indent & loopHeaderFormat1 & Environment.NewLine, node.GetLoop(j).Count)
                        indent &= indentStop
                    End If
                Next j

                If printNext Then
                    printNext = True
                    Dim nextMatch = If(i > 0, methodNameRegex.Match(node.GetFrame(i - 1).ToString()), Nothing)
                    Dim curStr = node.GetFrame(i).ToString()
                    'INSTANT VB TODO TASK: Assignments within expressions are not supported in VB
                    'ORIGINAL LINE: curStr = PrettifyFrame(curStr, nextMatch != Nothing && nextMatch.Success && (printNext = !curStr.StartsWith(nextMatch.Groups["fullName"].Value)) ? nextMatch.Groups["args"].Value : Nothing);
                    curStr = PrettifyFrame(curStr, If(nextMatch IsNot Nothing AndAlso nextMatch.Success AndAlso (printNext = (Not curStr.StartsWith(nextMatch.Groups("fullName").Value))), nextMatch.Groups("args").Value, Nothing))
                    printNext = If(nextMatch IsNot Nothing AndAlso nextMatch.Success, (Not curStr.StartsWith(nextMatch.Value)), True)
                    builder.AppendLine(indent & curStr)
                Else
                    printNext = True
                End If

                For j As Integer = 0 To node.LoopsCount() - 1
                    If node.GetLoop(j).LowIndex = i Then
                        indent = indent.Substring(0, indent.Length - indentStop.Length)
                        builder.AppendLine(indent & loopFooter)
                    End If
                Next j
            Next i

            Return builder.ToString()
        End Function

        ''' <summary>
        ''' Finds and collapses loops starting from the list head by finding a longest list prefix that is also a prefix of a sublist with this prefix removed.
        ''' </summary>
        ''' <param name="node">List head</param>
        ''' <returns>New list head if loops were collapsed, unchanged list head otherwise</returns>
        <System.Runtime.CompilerServices.Extension> _
        Private Function CollapseLoops(ByVal node As StackTraceNode) As StackTraceNode
            Do
                Dim slowLoopBraces As Integer = 0, fastLoopBraces As Integer = 0
                Dim count As Integer = node.TotalCount()
                Dim loopHigh As Integer = count, loopLow As Integer = count
                Dim slow As Integer = count - 1
                Dim fast As Integer = count - 2
                Do While slow >= 0 AndAlso fast >= 0
                    For i As Integer = 0 To node.LoopsCount() - 1
                        If node.GetLoop(i).HighIndex = slow Then
                            slowLoopBraces += 1
                        End If
                        If node.GetLoop(i).LowIndex = slow Then
                            slowLoopBraces -= 1
                        End If
                        If node.GetLoop(i).HighIndex = fast + 1 Then
                            fastLoopBraces += 1
                        End If
                        If node.GetLoop(i).LowIndex = fast + 1 Then
                            fastLoopBraces -= 1
                        End If
                        If node.GetLoop(i).HighIndex = fast Then
                            fastLoopBraces += 1
                        End If
                        If node.GetLoop(i).LowIndex = fast Then
                            fastLoopBraces -= 1
                        End If
                    Next i

                    If PolyHash.Subtract(node.GetHash(count - 1), node.GetHash(slow - 1), count - slow) = PolyHash.Subtract(node.GetHash(slow - 1), node.GetHash(fast - 1), slow - fast) Then
                        If slowLoopBraces = 0 AndAlso fastLoopBraces = 0 Then
                            loopHigh = slow - 1
                            loopLow = fast
                        End If
                    End If
                    slow -= 1
                    fast -= 2
                Loop

                If loopHigh >= count Then
                    Exit Do
                End If

                Dim loopLength As Integer = loopHigh - loopLow + 1
                Dim curNode As StackTraceNode
                curNode = node
                Do While curNode.Next IsNot Nothing AndAlso loopHigh < curNode.TotalCount() - curNode.Value.Count
                    curNode = curNode.Next
                Loop
                Dim boundary As Integer = curNode.TotalCount() - curNode.Value.Count

                Dim frames = curNode.Value.Frames.Take(loopHigh - boundary + 1).ToArray()
                Dim hashes = curNode.Value.Hashes.Take(loopHigh - boundary + 1).ToArray()
                Dim loopsList = New List(Of StackTraceLoop)(node.Value.Loops)

                Dim foundLoop As Boolean = False
                For i As Integer = 0 To loopsList.Count - 1
                    If loopsList(i).HighIndex = loopHigh AndAlso loopsList(i).LowIndex = loopLow Then
                        loopsList(i) = New StackTraceLoop(loopHigh, loopLow, loopsList(i).Count + 1)
                        foundLoop = True
                        Exit For
                    End If
                Next i

                If Not foundLoop Then
                    loopsList.Add(New StackTraceLoop(loopHigh, loopLow, 2))
                End If

                For i As Integer = 0 To loopsList.Count - 1
                    If i <= loopsList.Count - 1 Then
                        If loopsList(i).HighIndex > loopHigh OrElse loopsList(i).LowIndex > loopHigh Then
                            If loopsList(i).HighIndex <= loopHigh OrElse loopsList(i).LowIndex <= loopHigh Then
                                Throw New AsyncFlowDiagnosticsException("Loops cannot be broken apart by other loops")
                            End If
                            Dim newHigh As Integer = loopsList(i).HighIndex - loopLength
                            Dim newLow As Integer = loopsList(i).LowIndex - loopLength
                            Dim existingLoop = loopsList.SingleOrDefault(Function(l) l.HighIndex = newHigh AndAlso l.LowIndex = newLow)
                            loopsList(i) = New StackTraceLoop(newHigh, newLow, Math.Max(loopsList(i).Count, existingLoop.Count))
                            loopsList.Remove(existingLoop)
                        End If
                    End If
                Next i

                Dim newSegment = New StackTraceSegment(frames, hashes, loopsList.ToArray(), curNode.Next.TotalCount() + frames.Length, node.TotalCountWithLoops())
                node = New StackTraceNode(newSegment, curNode.Next)
            Loop
            Return node
        End Function
    End Module
End Namespace
