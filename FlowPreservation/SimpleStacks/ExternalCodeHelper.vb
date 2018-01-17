Imports System.Reflection
Imports System.Text
Imports System.Threading.Tasks

Namespace FlowPreservation
	''' <summary>
	''' Filters out boilerplate eventing and framework code
	''' </summary>
	Friend NotInheritable Class ExternalCodeHelper
		Private Shared ReadOnly myNamespace As String = GetType(StackFrameSlim).Namespace

		Private Shared ReadOnly eventInfrastructureMethods As New Dictionary(Of String, String) From {{ GetType(Tracing.EventSource).FullName, Nothing }, { GetType(System.Runtime.CompilerServices.TaskAwaiter).FullName, "OutputWaitEtwEvents" }, { GetType(StackTrace).FullName, "GetStackFramesInternal" }, { "System.Threading.Tasks.TplEtwProvider", Nothing }, { "System.Diagnostics.Tracing.FrameworkEventSource", Nothing }}

		''' <summary>
		''' Whether method belongs to eventing infrastructure
		''' </summary>
		''' <param name="method">Method</param>
		''' <returns>Whether method belongs to eventing infrastructure</returns>
		Public Shared Function IsEventInfrastructure(ByVal method As MethodBase) As Boolean
			If method Is Nothing Then
				Return False
			End If
			If method.DeclaringType Is Nothing Then
				Return MethodNameHelper.IsOurLambdaMethod(method.Name)
			End If
			If method.DeclaringType.Namespace = myNamespace Then
				Return True
			End If
            Dim expectedName As String = Nothing
			If Not eventInfrastructureMethods.TryGetValue(method.DeclaringType.FullName, expectedName) Then
				Return False
			End If
			Return expectedName Is Nothing OrElse expectedName = method.Name
		End Function

		''' <summary>
		''' Whether method belongs to eventing infrastructure
		''' </summary>
		''' <param name="method">Method</param>
		''' <returns>Whether method belongs to eventing infrastructure</returns>
		Public Shared Function IsEventInfrastructure(ByVal method As String) As Boolean
			If method Is Nothing Then
				Return False
			End If
			If method.StartsWith(myNamespace & ".") Then
				Return True
			End If
			For Each pair In eventInfrastructureMethods
				If method.StartsWith(pair.Key & "." & pair.Value) Then
					Return True
				End If
			Next pair
			Return False
		End Function

		Private Shared ReadOnly externalClassesAndNamespaces As New HashSet(Of String) From {GetType(System.Runtime.CompilerServices.AsyncTaskMethodBuilder).Namespace, GetType(System.Threading.ThreadPool).Namespace, GetType(System.Threading.Tasks.Task).Namespace, GetType(System.Threading.Tasks.Task).Namespace & ".Dataflow", GetType(System.Threading.Tasks.Task).Namespace & ".Dataflow.Internal", "Microsoft.VisualStudio.HostingProcess.HostProc", "Microsoft.Win32.SystemEvents", "System.AppDomain", "BaseThreadInitThunk", "_RtlUserThreadStart", "DestroyThread", "_NtWaitForSingleObject@", "_WaitForSingleObjectEx@", "_NtWaitForMultipleObjects@", "_WaitForMultipleObjectsEx@"}

		''' <summary>
		''' Whether method is boilerplate framework code
		''' </summary>
		''' <param name="method">Method</param>
		''' <returns>Whether method is boilerplate framework code</returns>
		Public Shared Function IsExternalCode(ByVal method As MethodBase) As Boolean
			If method Is Nothing OrElse method.DeclaringType Is Nothing Then
				Return True
			End If
			Return externalClassesAndNamespaces.Contains(method.DeclaringType.FullName) OrElse externalClassesAndNamespaces.Contains(method.DeclaringType.Namespace)
		End Function

		''' <summary>
		''' Whether method is boilerplate framework code
		''' </summary>
		''' <param name="method">Method</param>
		''' <returns>Whether method is boilerplate framework code</returns>
		Public Shared Function IsExternalCode(ByVal method As String) As Boolean
			If method Is Nothing Then
				Return True
			End If
			If method.StartsWith("@") OrElse method.StartsWith("__") OrElse method.Contains("::") Then
				Return True
			End If
			For Each item In externalClassesAndNamespaces
				If method.StartsWith(If(item.Contains("."c), item & ".", item)) Then
					Return True
				End If
			Next item
			Return False
		End Function
	End Class
End Namespace
