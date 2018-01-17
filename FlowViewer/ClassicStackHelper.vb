Imports EnvDTE
Imports EnvDTE80
Imports FlowPreservation
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading.Tasks

Namespace FlowViewer
	''' <summary>
	''' Extracts stored causality information for classic applications.
	''' </summary>
    Friend Class ClassicStackHelper
        Inherits StackHelperBase

        Public Sub New(ByVal dte As DTE2)
            MyBase.New(dte)
        End Sub

        ''' <summary>
        ''' Extracts causality information for the current thread by evaluating field expressions in the debugger, 
        ''' reconstructing instances of classes representing stored causality using these values,
        ''' and then calling their formatting methods directly.
        ''' </summary>
        ''' <returns>Stored causality chain except for the most recent stack segment, which is still on the stack</returns>
        Public Overrides Function GetCausalityChain() As String
            Dim reservoir = dte.Debugger.GetExpression(GetType(FlowReservoir).FullName)
            Dim stackStorage = reservoir.DataMember(GetType(FlowReservoir), GetType(StackStorage))
            Dim preservedStacks = stackStorage.WithStaticMembers.DataMember(GetType(StackStorage), GetType(StackTraceNode))
            If Not IsNull(preservedStacks) Then
                Dim stackTraceNode = CreateStackTraceNode(preservedStacks)
                Return stackTraceNode.ToStringEx
            Else
                Return Nothing
            End If
        End Function

        Private Function CreateStackTraceNode(ByVal expression As Expression) As StackTraceNode
            Dim [next] = expression.DataMember(Function(n As StackTraceNode) n.Next)
            Return New StackTraceNode(CreateStackTraceSegment(expression.DataMember(Function(n As StackTraceNode) n.Value)), If(IsNull([next]), Nothing, CreateStackTraceNode([next])))
        End Function

        Private Function CreateStackTraceSegment(ByVal expression As Expression) As StackTraceSegment
            Return New StackTraceSegment(CreateArray(expression.DataMember(Function(s As StackTraceSegment) s.Frames), AddressOf CreateStackFrameSlim), CreateArray(expression.DataMember(Function(s As StackTraceSegment) s.Hashes), Function(e) Long.Parse(e.Value)), CreateArray(expression.DataMember(Function(s As StackTraceSegment) s.Loops), AddressOf CreateStackTraceLoop), Integer.Parse(expression.DataMember(Function(s As StackTraceSegment) s.TotalCount).Value), Integer.Parse(expression.DataMember(Function(s As StackTraceSegment) s.TotalCountWithLoops).Value))
        End Function

        Private Function CreateArray(Of T)(ByVal expression As Expression, ByVal createElement As Func(Of Expression, T)) As T()
            Dim array = New T(expression.DataMembers.Count - 1) {}
            Dim indicesAndElements = Enumerable.Range(0, array.Length).Zip(expression.DataMembers.Cast(Of Expression)(), Function(i, e) New With {Key i, Key e})
            For Each indexAndElement In indicesAndElements
                array(indexAndElement.i) = createElement(indexAndElement.e)
            Next indexAndElement
            Return array
        End Function

        Private Function CreateStackFrameSlim(ByVal expression As Expression) As StackFrameSlim
            Dim stackFrameSlim = New StackFrameSlim(New IntPtr(Integer.Parse(expression.DataMember(Function(f As StackFrameSlim) f.MethodHandle).Value)), Integer.Parse(expression.DataMember(Function(f As StackFrameSlim) f.Offset).Value), Integer.Parse(expression.DataMember(Function(f As StackFrameSlim) f.ILOffset).Value), StringParse(expression.DataMember(Function(f As StackFrameSlim) f.FileName)), Integer.Parse(expression.DataMember(Function(f As StackFrameSlim) f.LineNumber).Value), Integer.Parse(expression.DataMember(Function(f As StackFrameSlim) f.ColumnNumber).Value), Boolean.Parse(expression.DataMember(Function(f As StackFrameSlim) f.IsLastFrameFromForeignExceptionStackTrace).Value))
            stackFrameSlim.Method = CreateMethodBaseSlim(expression.DataMember(Function(f As StackFrameSlim) f.Method))
            Return stackFrameSlim
        End Function

        Private Function CreateMethodBaseSlim(ByVal expression As Expression) As MethodBaseSlim
            If IsNull(expression) Then
                Return Nothing
            End If

            Return New MethodBaseSlim(StringParse(expression.DataMember(Function(m As MethodBaseSlim) m.StringValue)), Boolean.Parse(expression.DataMember(Function(m As MethodBaseSlim) m.IsEventInfrastructure).Value), Boolean.Parse(expression.DataMember(Function(m As MethodBaseSlim) m.IsExternalCode).Value))
        End Function

        Private Function CreateStackTraceLoop(ByVal expression As Expression) As StackTraceLoop
            Return New StackTraceLoop(Integer.Parse(expression.DataMember(Function(l As StackTraceLoop) l.HighIndex).Value), Integer.Parse(expression.DataMember(Function(l As StackTraceLoop) l.LowIndex).Value), Integer.Parse(expression.DataMember(Function(l As StackTraceLoop) l.Count).Value))
        End Function
    End Class
End Namespace
