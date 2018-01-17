Imports FlowPreservation
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

Namespace FlowPreservation
	''' <summary>
	''' Graph node
	''' </summary>
	Friend Class StackGraphNode
		Private Shared idCounter As Integer = 0

		Public Sub New(ByVal frame As String, Optional ByVal id As Integer = 0)
			Me.Frame = frame
			Me.Id = If(id = 0, Interlocked.Increment(idCounter), id)
			Me.Links = New HashSet(Of StackGraphNode)
		End Sub

		''' <summary>
		''' Frame text
		''' </summary>
		Public Property Frame As String

		''' <summary>
		''' Frame id (only needed to store it in dgml)
		''' </summary>
        Private privateId As Integer
		Public Property Id As Integer
			Get
				Return privateId
			End Get
			Private Set(ByVal value As Integer)
				privateId = value
			End Set
		End Property

		''' <summary>
		''' Links
		''' </summary>
		Public Property Links As ICollection(Of StackGraphNode)
	End Class
End Namespace
