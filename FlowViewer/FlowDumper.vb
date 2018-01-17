Imports EnvDTE
Imports EnvDTE80
Imports FlowPreservation
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading.Tasks
Imports System.Reflection

Namespace FlowViewer
	''' <summary>
	''' Add-in that outputs causality chain for current thread into output window. 
	''' </summary>
	Friend Class FlowDumper
		Inherits PluginBase

		Public Sub New(ByVal dte As DTE2, ByVal addIn As AddIn)
			MyBase.New(dte, addIn, True)
		End Sub

		''' <summary>
		''' Displays stored causality chain for current thread in the Output window
		''' </summary>
		Protected Overrides Sub Run()
			Dim window = dte.ToolWindows.OutputWindow
			window.Parent.Activate()

			Const asyncFlowPaneName As String = "AsyncFlow"
			Dim pane = If(window.OutputWindowPanes.Cast(Of OutputWindowPane)().Any(Function(p) p.Name = asyncFlowPaneName), window.OutputWindowPanes.Item(asyncFlowPaneName), window.OutputWindowPanes.Add(asyncFlowPaneName))
			pane.Activate()

			Try
				Dim chain = GetFullCausalityChain(dte.Debugger.CurrentThread)
				pane.OutputString("Causality chain for thread " & dte.Debugger.CurrentThread.ID & ":" & eol & eol)
				pane.OutputString(chain & eol & eol)
			Catch ex As Exception
                pane.OutputString(ex.ToString & eol & eol)
			End Try
		End Sub
	End Class
End Namespace
