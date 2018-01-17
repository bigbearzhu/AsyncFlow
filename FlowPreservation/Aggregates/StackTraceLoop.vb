Imports System.Text
Imports System.Threading.Tasks

Namespace FlowPreservation
	''' <summary>
	''' Represents a loop in a causality chain
	''' </summary>
	Friend Structure StackTraceLoop
		''' <summary>
		''' Loop constructor
		''' </summary>
		''' <param name="highIndex">Index of the most recent loop element</param>
		''' <param name="lowIndex">Index of the oldest loop element</param>
		''' <param name="count">Number of iterations of the loop</param>
		Public Sub New(ByVal highIndex As Integer, ByVal lowIndex As Integer, ByVal count As Integer)
			If highIndex < lowIndex Then
				Throw New AsyncFlowDiagnosticsException("highIndex is expected to be strictly higher than lowIndex: " & highIndex & ", " & lowIndex)
			End If

			Me.HighIndex = highIndex
			Me.LowIndex = lowIndex
			Me.Count = count
		End Sub

		''' <summary>
		''' Index of the most recent loop element
		''' </summary>
		Public ReadOnly HighIndex As Integer

		''' <summary>
		''' Index of the oldest loop element
		''' </summary>
		Public ReadOnly LowIndex As Integer

		''' <summary>
		''' Number of iterations of the loop
		''' </summary>
		Public ReadOnly Count As Integer
	End Structure
End Namespace
