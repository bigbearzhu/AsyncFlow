Imports EnvDTE
Imports EnvDTE80
Imports EnvDTE90a
Imports FlowPreservation
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Threading.Tasks

Namespace FlowViewer
	''' <summary>
	''' Base class for plugins
	''' </summary>
	Friend MustInherit Class PluginBase
		Protected Shared ReadOnly eol As String = Environment.NewLine
		Protected ReadOnly dte As DTE2
		Protected ReadOnly addIn As AddIn
		Protected ReadOnly classicStackHelper As ClassicStackHelper
		Protected ReadOnly modernStackHelper As ModernStackHelper

		Private ReadOnly runOnlyWhenDebugging As Boolean
		Private inProgress As Integer = 0

		Public Sub New(ByVal dte As DTE2, ByVal addIn As AddIn, ByVal runOnlyWhenDebugging As Boolean)
			Me.dte = dte
			Me.addIn = addIn
			Me.runOnlyWhenDebugging = runOnlyWhenDebugging
			Me.classicStackHelper = New ClassicStackHelper(dte)
			Me.modernStackHelper = New ModernStackHelper(dte)
		End Sub

		''' <summary>
		''' Main entry point
		''' </summary>
		async Public Sub RunAsync()
			await Task.Run(New Action(AddressOf RunSync))
		End Sub

		''' <summary>
		''' Runs the plugin, checking for existing running instance and debugging mode
		''' </summary>
		Public Sub RunSync()
			If runOnlyWhenDebugging AndAlso (dte.Debugger.DebuggedProcesses Is Nothing OrElse dte.Debugger.DebuggedProcesses.Count = 0 OrElse dte.Debugger.CurrentMode = dbgDebugMode.dbgRunMode) Then
				dte.StatusBar.Text = "This action is only available in break mode during debugging"
				dte.StatusBar.Highlight(True)
				Exit Sub
			End If

			If Interlocked.CompareExchange(inProgress, 1, 0) <> 0 Then
				dte.StatusBar.Text = "Action is already in progress"
				dte.StatusBar.Highlight(True)
				Exit Sub
			End If

			Try
				Run()
			Finally
				Interlocked.Exchange(inProgress, 0)
			End Try
		End Sub

		Protected MustOverride Sub Run()

		''' <summary>
		''' Gets stored causality chain for both classic and Windows Store applications, 
		''' including the topmost stack segment, which is extracted directly from Call Stack window.
		''' </summary>
		''' <param name="thread">Thread to look up causality information for</param>
		''' <returns>Causality chain formatted as string</returns>
		Protected Function GetFullCausalityChain(ByVal thread As EnvDTE.Thread) As String
			Dim originalThread = dte.Debugger.CurrentThread
			Dim originalFrame = dte.Debugger.CurrentStackFrame
			dte.Debugger.CurrentThread = thread
			Try
				Dim topSegment = StackHelperBase.GetTopStackSegmentAndSelectUserFrame(dte, True)

				Dim reservoir = dte.Debugger.GetExpression(GetType(FlowReservoir).FullName)
				Dim storedStack = If((Not reservoir.Value.Contains("does not exist in the")), classicStackHelper.GetCausalityChain, modernStackHelper.GetCausalityChain)

                Return topSegment.ToString & storedStack.ToString
			Catch ex As COMException
				If CUInt(ex.ErrorCode) = CUInt(&H89711006L) Then ' No valid context in which to evaluate the expression.
					Return Nothing
				Else
					Throw
				End If
			Catch e1 As NullReferenceException
				Return Nothing
			Finally
				dte.Debugger.CurrentThread = originalThread
				dte.Debugger.CurrentStackFrame = originalFrame
			End Try
		End Function
	End Class
End Namespace
