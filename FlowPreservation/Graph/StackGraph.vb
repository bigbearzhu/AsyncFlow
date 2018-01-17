Imports FlowPreservation
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading.Tasks

Namespace FlowPreservation
	''' <summary>
	''' A graph representation for causality chains
	''' </summary>
	Friend Class StackGraph
		Private Const ns As String = "{http://schemas.microsoft.com/vs/2009/dgml}"

        Private Shared ReadOnly singleFrameRegex As New Regex("(" & StackFrameSlim.AsyncBoundary.ToString & "[" & vbCrLf & "]+)?.*[" & vbCrLf & "]+")
		Private Shared ReadOnly indentRegex As New Regex("^\s*")
		Private Shared ReadOnly multiLineRegex1 As New Regex("\s*(" & StackFrameSlim.FirstArrow & "|:(?=line))\s*")
		Private Shared ReadOnly multiLineRegex2 As New Regex("\s*" & StackFrameSlim.SecondArrow & "\s*")

        Private ReadOnly nodesVal As New Dictionary(Of String, StackGraphNode)

		''' <summary>
		''' Runs through all frames in the segment and, if equal frame exists already, reuses the node, otherwise creates a new one
		''' </summary>
		''' <param name="concatenated">Concatenated causality chain for one thread, each line containing a separate frame</param>
		Public Sub AddSegment(ByVal concatenated As String)
			Dim singleFrames = singleFrameRegex.Matches(concatenated)
			Dim previous As StackGraphNode = Nothing
			For Each singleFrame In singleFrames.Cast(Of Match)().Select(Function(m) m.Value)
				Dim replacement As String = Environment.NewLine & "    " & indentRegex.Match(singleFrame).Value
				Dim frame = multiLineRegex1.Replace(singleFrame, replacement)
				If frame Is singleFrame Then
					frame = multiLineRegex2.Replace(singleFrame, replacement)
				End If

                Dim current As StackGraphNode = Nothing
                If Not nodesVal.TryGetValue(frame, current) Then
                    current = New StackGraphNode(frame)
                    nodesVal.Add(frame, current)
                End If

				If previous IsNot Nothing Then
					current.Links.Add(previous)
				End If

				previous = current
			Next singleFrame
		End Sub

		''' <summary>
		''' Collapses node chains to single nodes
		''' </summary>
        Public Sub Collapse()
            Dim parents = (From n In nodesVal.Values
                Group Join l In (
                    From n In nodesVal.Values
                    From l In n.Links
                    Select New With {Key .From = n, Key .To = l}
                    )
                On n Equals l.To Into parentNodes = Group
            Select New With {Key .Node = n, Key .Parent = If(parentNodes.Count = 1, parentNodes.Single.From, Nothing)}
            ).ToDictionary(Function(t) t.Node, Function(t) t.Parent)

            For Each node In nodesVal.Values.ToArray
                Dim parent = parents(node)
                Do While parent IsNot Nothing AndAlso Not nodesVal.ContainsKey(parent.Frame)
                    parent = parents(parent)
                Loop
                If parent IsNot Nothing AndAlso parent.Links.Count = 1 Then
                    nodesVal.Remove(parent.Frame)
                    parent.Frame = node.Frame + parent.Frame
                    parent.Links = node.Links
                    nodesVal.Add(parent.Frame, parent)
                    nodesVal.Remove(node.Frame)
                End If
            Next node
        End Sub

		''' <summary>
		''' Graph vertices
		''' </summary>
		Public ReadOnly Property Nodes As IEnumerable(Of StackGraphNode)
			Get
                Return nodesVal.Values
			End Get
		End Property

		''' <summary>
		''' Creates DGML xml content
		''' </summary>
		''' <returns>XDocument containing DGML</returns>
		Public Function GetDgml() As XDocument
			Dim doc = New XDocument(New XElement(ns & "DirectedGraph", New XAttribute("GraphDirection", "BottomToTop"), New XAttribute("Layout", "Sugiyama"), New XElement(ns & "Nodes", Nodes.Select(Function(n) New XElement(ns & "Node", New XAttribute("Id", n.Id), New XAttribute("Label", n.Frame))).ToArray), New XElement(ns & "Links", Nodes.SelectMany(Function(n) n.Links.Select(Function(l) New XElement(ns & "Link", New XAttribute("Source", n.Id), New XAttribute("Target", l.Id)))).ToArray)))
			Return doc
		End Function
	End Class
End Namespace
