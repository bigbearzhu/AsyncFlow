Imports System.Linq.Expressions
Imports System.Text
Imports System.Threading.Tasks
Imports System.Reflection

Namespace FlowViewer
	''' <summary>
	''' Helps extract member names from expressions
	''' </summary>
	Friend NotInheritable Class ReflectionHelper
		''' <summary>
		''' Finds the name of the field or property
        ''' </summary>      
        ''' <seealso>Declaring type</seealso>
        ''' <paramref name="M"></paramref>       
        ''' <paramref name="getter">Linq expression representing member access</paramref>
        ''' <seealso>Field or property name</seealso>

		Public Shared Function MemberName(Of T, M)(ByVal getter As Expression(Of Func(Of T, M))) As String
			Dim memberExpression = TryCast(getter.Body, MemberExpression)
			If memberExpression Is Nothing Then
				Throw New ArgumentException("expression is not MemberExpression", "expression")
			End If

			Return memberExpression.Member.Name
		End Function

		''' <summary>
		''' Finds field name by field type
		''' </summary>
		''' <param name="declaringType">Declaring type</param>
		''' <param name="fieldType">Field type (there must be exactly one field of this type defined on declaring type)</param>
		''' <returns>Field name</returns>
		Public Shared Function FieldName(ByVal declaringType As Type, ByVal fieldType As Type) As String
			Return declaringType.GetTypeInfo.DeclaredFields.Single(Function(f) f.FieldType = fieldType).Name
		End Function
	End Class
End Namespace
