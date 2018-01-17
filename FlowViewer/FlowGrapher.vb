Imports EnvDTE
Imports EnvDTE80
Imports FlowPreservation
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Threading.Tasks

Namespace FlowViewer
	''' <summary>
	''' DGML diagram generator, which presents causality data for all threads in a Parallel Stacks-like view
	''' </summary>
	Friend Class FlowGrapher
		Inherits PluginBase

		Public Sub New(ByVal dte As DTE2, ByVal addIn As AddIn)
			MyBase.New(dte, addIn, True)
		End Sub

		''' <summary>
		''' Generates DGML diagram with all user-code frames of all threads interconnected by causality matters
		''' and opens it in the IDE.
		''' </summary>
		Protected Overrides Sub Run()
			Try
				Dim graph = New StackGraph
				For Each thread As Thread In dte.Debugger.CurrentProgram.Threads
					Dim chain = GetFullCausalityChain(thread)
					If Not String.IsNullOrWhiteSpace(chain) Then
						graph.AddSegment(chain)
					End If
				Next thread
				graph.Collapse()

				Dim dgml = graph.GetDgml
				Dim fileName = Path.GetTempFileName & ".dgml"
				dgml.Save(fileName)
				dte.ItemOperations.OpenFile(fileName)
			Catch ex As Exception
				MessageBox.Show(ex.ToString)
			End Try
		End Sub
	End Class
End Namespace
