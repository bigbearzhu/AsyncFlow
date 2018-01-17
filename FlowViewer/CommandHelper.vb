Imports EnvDTE
Imports EnvDTE80
Imports FlowPreservation
Imports System.Runtime.ExceptionServices
Imports System.Text
Imports System.Threading.Tasks

Namespace FlowViewer
	''' <summary>
	''' Helps run statements in Command window
	''' </summary>
	Friend Class CommandHelper
		Private ReadOnly dte As DTE2

		Public Sub New(ByVal dte As DTE2)
			Me.dte = dte
		End Sub

		Public ReadOnly Property CommandWindow As CommandWindow
			Get
				Return dte.ToolWindows.CommandWindow
			End Get
		End Property

		''' <summary>
		''' Execute statement and get result
		''' </summary>
		''' <param name="statement">Statement</param>
		''' <returns>Result</returns>
		Public Function ExecuteStatement(ByVal statement As String) As String
			CommandWindow.Clear()
			Dim offset As Integer = CommandWindow.TextDocument.EndPoint.AbsoluteCharOffset
			dte.Debugger.ExecuteStatement(statement)
			Dim point = CommandWindow.TextDocument.StartPoint.CreateEditPoint
			point.MoveToAbsoluteOffset(offset - 1)
			Dim text = point.GetText(CommandWindow.TextDocument.EndPoint)
			text = text.TrimEnd(">"c).TrimEnd
			Return text
		End Function
	End Class
End Namespace
