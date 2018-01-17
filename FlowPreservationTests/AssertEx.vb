Imports FlowPreservation
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Threading.Tasks

Namespace FlowPreservationTests
	''' <summary>
	''' Assertions regarding frame presence on stack
	''' </summary>
	Friend NotInheritable Class AssertEx
		Private Shared ReadOnly topMethods() As String = { "Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestExecuter.RunTestMethod", "Microsoft.VisualStudio.TestPlatform.MSTestFramework.UnitTestRunner.RunSingleTest" }
		Private Shared ReadOnly newLineChars() As Char = { ControlChars.Cr, ControlChars.Lf }
		Private Shared Function FormatTimes(ByVal verb As String, ByVal count As Integer) As String
			Return String.Format("was{0}{1}{2}",If(count = 0, " not", ""), " " & verb,If(count = 1, " once", If(count > 1, " " & count & " times", "")))
		End Function

		Private Shared Function FormatPresence(ByVal str As String, ByVal expectedCount As Integer, ByVal actualCount As Integer) As String
			Return String.Format("""{0}"" {1}, but {2}", str, FormatTimes("expected", expectedCount), FormatTimes("present", actualCount))
		End Function

		Private Shared Function GetOnStackCount(ByVal str As String) As Integer
			Dim stack As String = FlowReservoir.ExtendedStack
			Dim lines = stack.Split(newLineChars, StringSplitOptions.RemoveEmptyEntries)
			Dim actualCount As Integer = lines.Count(Function(l) l.Contains(str))
			Return actualCount
		End Function

		''' <summary>
		''' String is on stack for exactly expected number of times
		''' </summary>
		''' <param name="str">String</param>
		''' <param name="expectedCount">How many times</param>
		Public Shared Sub OnStackCount(ByVal str As String, ByVal expectedCount As Integer)
			Dim actualCount As Integer = GetOnStackCount(str)
			Assert.IsTrue(expectedCount = actualCount, FormatPresence(str, expectedCount, actualCount) & " on stack:" & Environment.NewLine & FlowReservoir.ExtendedStack)
		End Sub

		''' <summary>
		''' Method is not on stack (as a state machine)
		''' </summary>
		''' <param name="method">Method</param>
		Public Shared Sub NotOnStack(ByVal method As Func(Of Task))
			OnStackCount("<" & method.Method.Name & ">", 0)
		End Sub

		''' <summary>
		''' A unit test runner method is present on the bottom of the stack (that means that the chain did not tear down)
		''' </summary>
		Public Shared Sub StackHasRoot()
			Dim actualCounts = New Integer(topMethods.Length - 1){}
			For i  = 0 To topMethods.Length - 1
				actualCounts(i) = GetOnStackCount(topMethods(i))
				If actualCounts(i) = 1 Then
					Exit Sub
				End If
			Next i
            Assert.Fail(String.Join("," & Environment.NewLine, topMethods.Zip(actualCounts, Function(t, a) FormatPresence(t, 1, a))) & Environment.NewLine & "on stack:" & Environment.NewLine & FlowReservoir.ExtendedStack)
		End Sub
	End Class
End Namespace
