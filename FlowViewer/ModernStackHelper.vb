Imports EnvDTE
Imports EnvDTE80
Imports FlowPreservation
Imports System.Text
Imports System.Threading.Tasks

Namespace FlowViewer
	''' <summary>
	''' Extracts stored causality information for Windows Store applications.
	''' </summary>
    Friend Class ModernStackHelper
        Inherits StackHelperBase

        Public Sub New(ByVal dte As DTE2)
            MyBase.New(dte)
        End Sub

        ''' <summary>
        ''' Extracts causality chain for the current thread by evaluating field expressions in the debugger
        ''' </summary>
        ''' <returns>Stored causality chain except for the most recent stack segment, which is still on the stack</returns>
        Public Overrides Function GetCausalityChain() As String
            Dim builder = New StringBuilder
            Dim frameStorage = dte.Debugger.GetExpression(GetType(FrameStorage).FullName)
            Dim frames = frameStorage.DataMember(GetType(FrameStorage), GetType(AsyncLocal(Of LinkedList(Of String))))
            Dim framesGuid = CreateGuid(frames.DataMember(GetType(AsyncLocal(Of LinkedList(Of String))), GetType(Guid)))
            Dim discardableStorage = frames.DataMember(String.Format("base {{{0}}}", GetType(AsyncLocal).FullName)).WithStaticMembers.DataMember("DiscardableThreadStorage")
            Dim entries = discardableStorage.WithNonPublicMembers.DataMember("entries")
            Dim entry = entries.DataMember(Function(e) CreateGuid(e.DataMember("key")) = framesGuid)
            Dim entryValue = entry.DataMember("value")
            Dim count As Integer = Integer.Parse(entryValue.WithNonPublicMembers.DataMember("count").Value)
            Dim currentNode = entryValue.WithNonPublicMembers.DataMember("head")
            For i  = 0 To count - 1
                Dim frame = StringParse(currentNode.WithNonPublicMembers.DataMember("item"))
                If i <> count - 1 OrElse (Not String.IsNullOrWhiteSpace(frame)) Then
                    builder.AppendLine(frame)
                End If
                currentNode = currentNode.WithNonPublicMembers.DataMember("next")
            Next i
            Return builder.ToString
        End Function

        Private Function CreateGuid(ByVal expression As Expression) As Guid
            Dim nonPublic = expression.WithNonPublicMembers
            Return New Guid(Integer.Parse(nonPublic.DataMember("_a").Value), Short.Parse(nonPublic.DataMember("_b").Value), Short.Parse(nonPublic.DataMember("_c").Value), Byte.Parse(nonPublic.DataMember("_d").Value), Byte.Parse(nonPublic.DataMember("_e").Value), Byte.Parse(nonPublic.DataMember("_f").Value), Byte.Parse(nonPublic.DataMember("_g").Value), Byte.Parse(nonPublic.DataMember("_h").Value), Byte.Parse(nonPublic.DataMember("_i").Value), Byte.Parse(nonPublic.DataMember("_j").Value), Byte.Parse(nonPublic.DataMember("_k").Value))
        End Function
    End Class
End Namespace
