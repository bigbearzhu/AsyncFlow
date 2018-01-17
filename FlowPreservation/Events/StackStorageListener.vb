Imports System.Diagnostics.Tracing
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

Namespace FlowPreservation
	''' <summary>
	''' Stores and restores stack information on TPL and framework events
	''' </summary>
	Friend Class StackStorageListener
		Inherits TplFrameworkListenerBase

		Private ReadOnly stackStorage As StackStorage

		Public Sub New(ByVal stackStorage As StackStorage)
			Me.stackStorage = stackStorage
		End Sub

		Protected Overrides Sub TaskScheduled(ByVal taskId As Integer)
			stackStorage.StoreStack(taskId, False)
		End Sub

		Protected Overrides Sub TaskWaitBegin(ByVal taskId As Integer, ByVal behavior As EventConstants.Tpl.TaskWaitBehavior)
			stackStorage.StoreStack(taskId, behavior = EventConstants.Tpl.TaskWaitBehavior.Synchronous)
		End Sub

		Protected Overrides Sub TaskWaitEnd(ByVal taskId As Integer)
			stackStorage.RestoreStack(taskId)
		End Sub

		Protected Overrides Sub ResetLocal()
			stackStorage.ResetLocal()
		End Sub
	End Class
End Namespace
