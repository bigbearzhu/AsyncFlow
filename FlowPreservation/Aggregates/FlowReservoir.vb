Imports System.Reflection
Imports System.Text
Imports System.Threading.Tasks

Namespace FlowPreservation
	''' <summary>
	''' Facade class for causality chain preservation engine
	''' </summary>
	Public NotInheritable Class FlowReservoir
		Private Const storageUninitialized As String = "Execution flow storage is not initialized. Please call FlowReservoir.Enroll first."

		Private Shared ReadOnly initLocker As New Object
		Private Shared stackStorage As StackStorage
		Private Shared listener As StackStorageListener

		''' <summary>
		''' Start tracking causality in current application domain
		''' </summary>
		''' <param name="storeFileLineColumnInfo">Whether source code information has to be extracted (slower)</param>
		''' <param name="justMyCode">Omit boilerplate framework code</param>
		Public Shared Sub Enroll(Optional ByVal storeFileLineColumnInfo As Boolean = True, Optional ByVal justMyCode As Boolean = True)
			If stackStorage Is Nothing Then
				SyncLock initLocker
					If stackStorage Is Nothing Then
						stackStorage = New StackStorage(storeFileLineColumnInfo, justMyCode)
						listener = New StackStorageListener(stackStorage)
						listener.EnableSubscriptions()
						listener.Initialized.Wait()
					End If
				End SyncLock
			End If
		End Sub

		''' <summary>
		''' Provides causality chain for current thread in live debugging 
		''' (for post-mortem scenario or when native frame is on top of the stack refer to FlowViewer add-in)
		''' </summary>
		Public Shared ReadOnly Property ExtendedStack As String
			Get
				Return If(stackStorage Is Nothing, storageUninitialized, stackStorage.GetAggregateStackString)
			End Get
		End Property
	End Class
End Namespace
