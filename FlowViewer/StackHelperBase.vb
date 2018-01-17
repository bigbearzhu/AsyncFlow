Imports EnvDTE
Imports EnvDTE80
Imports EnvDTE90a
Imports FlowPreservation
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading.Tasks

Namespace FlowViewer
    ''' <summary>
    ''' Base class for classes that extract stored causality information 
    ''' </summary>
    Friend MustInherit Class StackHelperBase
        Private Shared ReadOnly unresolvedFrameRegex As New Regex("^[0-9a-f]{8}(?:[0-9a-f]{8})?$")

        Protected ReadOnly dte As DTE2

        Public Sub New(ByVal dte As DTE2)
            Me.dte = dte
        End Sub

        Protected Function IsNull(ByVal expression As Expression) As Boolean
            Return expression Is Nothing OrElse expression.Value = "null"
        End Function

        Protected Function StringParse(ByVal expression As Expression) As String
            If IsNull(expression) Then
                Return Nothing
            End If
           
            Return expression.Value.Trim(" ".ToCharArray).Replace("\\", "\")
        End Function

        ''' <summary>
        ''' Gets current stack segment for current thread by querying Call Stack window data
        ''' and selects topmost user frame in the debugger, so that debugger expression
        ''' evaluation has the right context
        ''' </summary>
        ''' <paramref name="dte">DTE</paramref>
        ''' <paramref name="justMyCode">Whether non-user code frames should be skipped</paramref>
        ''' <returns>Most recent stack segment, which is currently on stack</returns>
        Public Shared Function GetTopStackSegmentAndSelectUserFrame(ByVal dte As DTE2, ByVal justMyCode As Boolean) As String
            Dim topFrames = New List(Of String)
            Dim topUserFrameSelected As Boolean = False
            Dim managedToNativeTransitionEncountered As Boolean = False
            For Each frame In dte.Debugger.CurrentThread.StackFrames.Cast(Of StackFrame2)()
                Dim frameText As String = frame.FunctionName
                If frameText = "[Managed to Native Transition]" Then
                    If Not managedToNativeTransitionEncountered Then
                        topUserFrameSelected = False
                        managedToNativeTransitionEncountered = True
                    End If
                    Continue For
                End If

                If ExternalCodeHelper.IsEventInfrastructure(frameText) OrElse justMyCode AndAlso (ExternalCodeHelper.IsExternalCode(frameText) OrElse frameText = "Native to Managed Transition" OrElse frameText.StartsWith("Frames below may be incorrect and/or missing") OrElse unresolvedFrameRegex.IsMatch(frameText)) Then
                    Continue For
                End If

                If frameText = "[Resuming Async Method]" Then
                    frameText = StackFrameSlim.AsyncBoundary.ToString
                Else
                    If (Not frameText.StartsWith("[")) OrElse (Not frameText.EndsWith("]")) Then
                        frameText &= "(" & String.Join(", ", frame.Arguments.Cast(Of Expression)().Select(Function(a) a.Type & " " & a.Name)) & ")"
                        Dim fileName = frame.FileName
                        If Not String.IsNullOrEmpty(fileName) Then
                            frameText &= String.Format(" {0} {1}:line {2}", StackFrameSlim.FirstArrow, fileName, frame.LineNumber)
                        End If
                    End If
                End If
                topFrames.Add(frameText)
                If Not topUserFrameSelected Then
                    topUserFrameSelected = True
                    dte.Debugger.CurrentStackFrame = frame
                End If
            Next frame
            Do While topFrames.LastOrDefault = StackFrameSlim.AsyncBoundary.ToString
                topFrames.RemoveAt(topFrames.Count - 1)
            Loop

            Dim builder = New StringBuilder
            For Each frame In topFrames
                builder.AppendLine(frame)
            Next frame
            Return builder.ToString
        End Function

        Public MustOverride Function GetCausalityChain() As String
    End Class
End Namespace
