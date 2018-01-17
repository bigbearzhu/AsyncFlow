Imports System.Text
Imports System.Threading.Tasks

Namespace FlowPreservation
	''' <summary>
	''' Singly linked list node
	''' </summary>
	Friend Class StackTraceNode
		''' <summary>
		''' Node constructor
		''' </summary>
		''' <param name="value">Stack trace segment</param>
		''' <param name="next">Next list node</param>
        Public Sub New(ByVal value As StackTraceSegment, ByVal next1 As StackTraceNode)
            Me.Value = value
            Me.Next = next1
        End Sub

		''' <summary>
		''' Stack trace segment
		''' </summary>
		Public ReadOnly Value As StackTraceSegment

		''' <summary>
		''' Next list node
		''' </summary>
        Public ReadOnly [Next] As StackTraceNode
	End Class
End Namespace
