Imports EnvDTE
Imports System.Text
Imports System.Threading.Tasks

Namespace FlowViewer
	''' <summary>
	''' Extensions class for crawling debugger expressions
	''' </summary>
	Friend Module DteExtensions
		Private Const retries As Integer = 5

		''' <summary>
		''' Given a debugger expression, returns a derived debugger expression
		''' </summary>
		''' <param name="expression">Original debugger expression</param>
		''' <param name="name">Name of member</param>
		''' <returns>New expression = original.name</returns>
		<System.Runtime.CompilerServices.Extension>
		Public Function DataMember(ByVal expression As Expression, ByVal name As String) As Expression
			If expression Is Nothing Then
				Return Nothing
			End If

			Dim result As Expression
			For i  = 0 To retries - 1
				result = expression.DataMembers.Cast(Of Expression)().SingleOrDefault(Function(e) e.Name = name)
				If result IsNot Nothing AndAlso (result.IsValidValue OrElse result.Value.Contains("thread is stopped at a point where")) Then
					Return result
				End If
			Next i
			Return Nothing
		End Function

		''' <summary>
		''' Given a debugger expression, returns a derived debugger expression, 
		''' extracting member name from linq expression passed in
		''' </summary>
		''' <typeparam name="T">Declaring type</typeparam>
		''' <typeparam name="M">Member type</typeparam>
		''' <param name="expression">Original debugger expression</param>
		''' <param name="getter">Getter linq expression, from which to extract member name</param>
		''' <returns>New expression = original.name_of_the_member_accessed_by_the_getter</returns>
		<System.Runtime.CompilerServices.Extension>
		Public Function DataMember(Of T, M)(ByVal expression As Expression, ByVal getter As System.Linq.Expressions.Expression(Of Func(Of T, M))) As Expression
			Return expression.DataMember(ReflectionHelper.MemberName(getter))
		End Function

		''' <summary>
		''' Given a debugger expression, returns a derived debugger expression,
		''' finding member and its name by field type.
		''' </summary>
		''' <param name="expression">Original debugger expression</param>
		''' <param name="declaringType">Type of original debugger expression</param>
		''' <param name="fieldType">Type of the field defined on the declaring type</param>
		''' <returns>New expression = original.name_of_the_single_member_of_fieldType</returns>
		<System.Runtime.CompilerServices.Extension>
		Public Function DataMember(ByVal expression As Expression, ByVal declaringType As Type, ByVal fieldType As Type) As Expression
			Return expression.DataMember(ReflectionHelper.FieldName(declaringType, fieldType))
		End Function

		''' <summary>
		''' Given a debugger expression, returns a derived debugger expression,
		''' finding member by predicate.
		''' </summary>
		''' <param name="expression">Original debugger expression</param>
		''' <param name="predicate">Delegate that checks for a condition on debugger expression</param>
		''' <returns>Single derived expression that matches predicate</returns>
		<System.Runtime.CompilerServices.Extension>
		Public Function DataMember(ByVal expression As Expression, ByVal predicate As Func(Of Expression, Boolean)) As Expression
			Return If(expression Is Nothing, Nothing, expression.DataMembers.Cast(Of Expression)().Single(predicate))
		End Function

		''' <summary>
		''' Given a debugger expression, returns an expression for its "Static members" node if it exists
		''' or original expression otherwise
		''' </summary>
		''' <param name="expression">Original debugger expression</param>
		''' <returns>Expression for "Static members" or the original one</returns>
		<System.Runtime.CompilerServices.Extension>
		Public Function WithStaticMembers(ByVal expression As Expression) As Expression
			Return If(expression.DataMember("Static members"), expression)
		End Function

		''' <summary>
		''' Given a debugger expression, returns an expression for its "Non-Public members" node 
		''' and optionally "Raw View" if they exist or original expression otherwise
		''' </summary>
		''' <param name="expression">Original debugger expression</param>
		''' <returns>Expression for "Non-Public members" and possibly "Raw View" or the original one</returns>
		<System.Runtime.CompilerServices.Extension>
		Public Function WithNonPublicMembers(ByVal expression As Expression) As Expression
			expression = If(expression.DataMember("Raw View"), expression)
			Return If(expression.DataMember("Non-Public members"), expression)
		End Function
	End Module
End Namespace
