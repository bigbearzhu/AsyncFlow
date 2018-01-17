Imports System.Reflection
Imports System.Text
Imports System.Threading.Tasks

Namespace FlowPreservation
	''' <summary>
	''' Analog to MethodBase that stores its string representation in a ready-to-consume form for easy extraction
	''' </summary>
	Friend Structure MethodBaseSlim
		Friend Sub New(ByVal stringValue As String, ByVal isEventInfrastructure As Boolean, ByVal isExternalCode As Boolean)
			Me.StringValue = stringValue
			Me.IsEventInfrastructure = isEventInfrastructure
			Me.IsExternalCode = isExternalCode
		End Sub

		Public Sub New(ByVal method As MethodBase)
			Me.StringValue = GetStringValue(method)
			Me.IsEventInfrastructure = ExternalCodeHelper.IsEventInfrastructure(method)
			Me.IsExternalCode = ExternalCodeHelper.IsExternalCode(method)
		End Sub

		Public ReadOnly StringValue As String

		Public ReadOnly IsEventInfrastructure As Boolean
		Public ReadOnly IsExternalCode As Boolean

		''' <summary>
		''' Fancy ToString
		''' </summary>
		''' <param name="method">Method</param>
		''' <returns>String value</returns>
		Private Shared Function GetStringValue(ByVal method As MethodBase) As String
			If method Is Nothing Then
				Return Nothing
			End If

			Dim builder = New StringBuilder
			If method.DeclaringType IsNot Nothing Then
				builder.Append(method.DeclaringType.FullName)
				builder.Append(".")
			End If
			builder.Append(method.Name)
			If method.IsGenericMethod Then
				builder.Append("[")
				Dim genericArguments = method.GetGenericArguments
				For i  = 0 To genericArguments.Length - 1
					If i > 0 Then
						builder.Append(",")
					End If
					builder.Append(genericArguments(i))
				Next i
				builder.Append("]")
			End If
			builder.Append("(")
			Dim parameters = method.GetParameters
			For i  = 0 To parameters.Length - 1
				If i > 0 Then
					builder.Append(", ")
				End If
                builder.Append((If(parameters(i) IsNot Nothing, parameters(i).ParameterType.Name, "<UnknownType>")) & (If(String.IsNullOrEmpty(parameters(i).Name), String.Empty, " " & parameters(i).Name)))
			Next i
			builder.Append(")")
			Return builder.ToString
		End Function
	End Structure
End Namespace
