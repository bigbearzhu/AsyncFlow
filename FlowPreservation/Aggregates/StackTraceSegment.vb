Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Threading.Tasks

Namespace FlowPreservation
	''' <summary>
	''' Stack trace segment
	''' </summary>
	Friend Class StackTraceSegment
		' Pertaining to segments

		''' <summary>
		''' Frames belonging to this segment
		''' </summary>
		Public ReadOnly Frames() As StackFrameSlim
		''' <summary>
		''' Hashes belonging to this segment
		''' </summary>
		Public ReadOnly Hashes() As Long

		' Pertaining to the whole sublist starting at this element

		''' <summary>
		''' Loops belonging to the whole list
		''' </summary>
		Public ReadOnly Loops() As StackTraceLoop
		''' <summary>
		''' Total element count with loops collapsed in the entire list
		''' </summary>
		Public ReadOnly TotalCount As Integer
		''' <summary>
		''' Total element count in the entire list with loops expanded
		''' </summary>
		Public ReadOnly TotalCountWithLoops As Integer

		''' <summary>
		''' Constructor of a stack trace segment
		''' </summary>
		''' <param name="frames">Frames for the segment</param>
		''' <param name="hashesUpTo">Hashes for the segment</param>
		''' <param name="loops">Loops for the entire list</param>
		''' <param name="totalCount">Count with loops collapsed for the entire list</param>
		''' <param name="countWithLoops">Count with loops expanded for the entire list</param>
		Public Sub New(ByVal frames() As StackFrameSlim, ByVal hashesUpTo() As Long, ByVal loops() As StackTraceLoop, ByVal totalCount As Integer, ByVal countWithLoops As Integer)
			Me.Frames = frames
			Me.Hashes = hashesUpTo
			Me.Loops = loops
			Me.TotalCount = totalCount
			Me.TotalCountWithLoops = countWithLoops
		End Sub

		''' <summary>
		''' Frames count in this segment itself
		''' </summary>
		Public ReadOnly Property Count As Integer
			Get
				Return Frames.Length
			End Get
		End Property
	End Class
End Namespace
