Imports Extensibility
Imports EnvDTE
Imports EnvDTE80
Imports Microsoft.VisualStudio.CommandBars
Imports System.Resources
Imports System.Reflection
Imports System.Globalization

Namespace FlowViewer
	''' <summary>The object for implementing an Add-in.</summary>
	''' <seealso class='IDTExtensibility2' />
	Public Class Connect
		Implements IDTExtensibility2, IDTCommandTarget
		''' <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
        ''' <paramref term='application'>Root object of the host application.</paramref>
        ''' <paramref term='connectMode'>Describes how the Add-in is being loaded.</paramref>
        ''' <paramref term='addInInst'>Object representing this Add-in.</paramref>
		''' <seealso class='IDTExtensibility2' />
		Public Sub OnConnection(ByVal application As Object, ByVal connectMode As ext_ConnectMode, ByVal addInInst As Object, ByRef custom As Array) Implements IDTExtensibility2.OnConnection
			_applicationObject = CType(application, DTE2)
			_addInInstance = CType(addInInst, AddIn)
			flowDumper = New FlowDumper(_applicationObject, _addInInstance)
			flowGrapher = New FlowGrapher(_applicationObject, _addInInstance)
			If connectMode = ext_ConnectMode.ext_cm_UISetup Then
				Dim contextGUIDS() As Object = { }
				Dim commands As Commands2 = CType(_applicationObject.Commands, Commands2)
				Dim toolsMenuName As String = "Tools"
				Dim menuBarCommandBar As CommandBar = (CType(_applicationObject.CommandBars, CommandBars))("MenuBar")
				Dim toolsControl As CommandBarControl = menuBarCommandBar.Controls(toolsMenuName)
				Dim toolsPopup As CommandBarPopup = CType(toolsControl, CommandBarPopup)

				'This try/catch block can be duplicated if you wish to add multiple commands to be handled by your Add-in,
				'  just make sure you also update the QueryStatus/Exec method to include the new command names.
				Try
					'Add a command to the Commands collection:
					Dim dumperCommand As Command = commands.AddNamedCommand2(_addInInstance, "FlowDumper", "Async Flow for Current Thread", "Displays preserved execution flow for current thread", False, Nothing, contextGUIDS, CInt(vsCommandStatus.vsCommandStatusSupported) + CInt(vsCommandStatus.vsCommandStatusEnabled), CInt(vsCommandStyle.vsCommandStyleText), vsCommandControlType.vsCommandControlTypeButton)
					Dim grapherCommand As Command = commands.AddNamedCommand2(_addInInstance, "FlowGrapher", "Async Flow Graph for All Threads", "Displays preserved execution flow graph for all threads", False, Nothing, contextGUIDS, CInt(vsCommandStatus.vsCommandStatusSupported) + CInt(vsCommandStatus.vsCommandStatusEnabled), CInt(vsCommandStyle.vsCommandStyleText), vsCommandControlType.vsCommandControlTypeButton)

					'Add a control for the command to the tools menu:
					If toolsPopup IsNot Nothing Then
						If dumperCommand IsNot Nothing Then
							dumperCommand.AddControl(toolsPopup.CommandBar, 1)
						End If
						If grapherCommand IsNot Nothing Then
							grapherCommand.AddControl(toolsPopup.CommandBar, 2)
						End If
					End If
				Catch e1 As ArgumentException
					'If we are here, then the exception is probably because a command with that name
					'  already exists. If so there is no need to recreate the command and we can 
					'  safely ignore the exception.
				End Try
			End If
		End Sub

		''' <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        ''' <paramref term='disconnectMode'>Describes how the Add-in is being unloaded.</paramref>
        ''' <paramref term='custom'>Array of parameters that are host application specific.</paramref>
		''' <seealso class='IDTExtensibility2' />
		Public Sub OnDisconnection(ByVal disconnectMode As ext_DisconnectMode, ByRef custom As Array) Implements IDTExtensibility2.OnDisconnection
		End Sub

		''' <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
        ''' <paramref term='custom'>Array of parameters that are host application specific.</paramref>
		''' <seealso class='IDTExtensibility2' />		
		Public Sub OnAddInsUpdate(ByRef custom As Array) Implements IDTExtensibility2.OnAddInsUpdate
		End Sub

		''' <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        ''' <paramref term='custom'>Array of parameters that are host application specific.</paramref>
		''' <seealso class='IDTExtensibility2' />
		Public Sub OnStartupComplete(ByRef custom As Array) Implements IDTExtensibility2.OnStartupComplete
		End Sub

		''' <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        ''' <paramref term='custom'>Array of parameters that are host application specific.</paramref>
		''' <seealso class='IDTExtensibility2' />
		Public Sub OnBeginShutdown(ByRef custom As Array) Implements IDTExtensibility2.OnBeginShutdown
		End Sub

		''' <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
        ''' <paramref term='commandName'>The name of the command to determine state for.</paramref>
        ''' <paramref term='neededText'>Text that is needed for the command.</paramref>
        ''' <paramref term='status'>The state of the command in the user interface.</paramref>
        ''' <paramref term='commandText'>Text requested by the neededText parameter.</paramref>
		''' <seealso class='Exec' />
		Public Sub QueryStatus(ByVal commandName As String, ByVal neededText As vsCommandStatusTextWanted, ByRef status As vsCommandStatus, ByRef commandText As Object) Implements IDTCommandTarget.QueryStatus
			If neededText = vsCommandStatusTextWanted.vsCommandStatusTextWantedNone Then
				If commandName = "FlowViewer.Connect.FlowDumper" OrElse commandName = "FlowViewer.Connect.FlowGrapher" Then
                    status = CType(vsCommandStatus.vsCommandStatusSupported, vsCommandStatus) Or vsCommandStatus.vsCommandStatusEnabled
					Exit Sub
				End If
			End If
		End Sub
		''' <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
        ''' <paramref term='commandName'>The name of the command to execute.</paramref>
        ''' <paramref term='executeOption'>Describes how the command should be run.</paramref>
        ''' <paramref term='varIn'>Parameters passed from the caller to the command handler.</paramref>
        ''' <paramref term='varOut'>Parameters passed from the command handler to the caller.</paramref>
        ''' <paramref term='handled'>Informs the caller if the command was handled or not.</paramref>
		''' <seealso class='Exec' />
		Public Sub Exec(ByVal commandName As String, ByVal executeOption As vsCommandExecOption, ByRef varIn As Object, ByRef varOut As Object, ByRef handled As Boolean) Implements IDTCommandTarget.Exec
			handled = False
			If executeOption = vsCommandExecOption.vsCommandExecOptionDoDefault Then
				Select Case commandName
					Case "FlowViewer.Connect.FlowDumper"
						flowDumper.RunAsync()
						handled = True
						Exit Sub
					Case "FlowViewer.Connect.FlowGrapher"
						flowGrapher.RunAsync()
						handled = True
						Exit Sub
				End Select
			End If
		End Sub

		Private _applicationObject As DTE2
		Private _addInInstance As AddIn
		Private flowDumper As FlowDumper
		Private flowGrapher As FlowGrapher
	End Class
End Namespace
