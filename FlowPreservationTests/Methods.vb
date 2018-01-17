Imports FlowPreservation
Imports System.Text
Imports System.Threading.Tasks

Namespace FlowPreservationTests
	''' <summary>
	''' Asynchronous methods called by tests
	''' </summary>
	Friend NotInheritable Class Methods
		async Public Shared Function FirstAsync() As Task
			await Task.Delay(1)
			AssertEx.StackHasRoot()
		End Function

		async Public Shared Function SecondAsync() As Task
			await Task.Delay(1)
			AssertEx.StackHasRoot()
		End Function

		async Public Shared Function CommonAsync() As Task
			await Task.Delay(1)
			AssertEx.StackHasRoot()
		End Function

		async Public Shared Function InfiniteAsync() As Task
			await Task.Delay(Integer.MaxValue)
			AssertEx.StackHasRoot()
		End Function

		async Public Shared Function NestedOuter() As Task
			await NestedInner()
			AssertEx.StackHasRoot()
		End Function

		async Public Shared Function NestedInner() As Task
			await Task.Delay(1)
			AssertEx.StackHasRoot()
		End Function

		async Public Shared Function [Loop]() As Task
			Const iterations As Integer = 6
			For i  = 0 To iterations - 1
				await FirstAsync()
				await SecondAsync()
			Next i
			AssertEx.StackHasRoot()
			AssertEx.OnStackCount((iterations - 1) & " times", 1)
		End Function

		async Public Shared Function Flaky(ByVal iterations As Integer) As Task
			For i  = 0 To iterations - 1
				await Task.Delay(TimeSpan.FromTicks(1))
			Next i
			AssertEx.StackHasRoot()
		End Function

		async Public Shared Function FirstBlockAction(ByVal arg As Integer) As Task(Of Integer)
			await Task.Delay(1)
			AssertEx.StackHasRoot()
			Return arg + 1
		End Function

		async Public Shared Function SecondBlockAction(ByVal arg As Integer) As Task(Of Integer)
			await Task.Delay(1)
			AssertEx.StackHasRoot()
			Return arg + 1
		End Function

		async Public Shared Function FirstSimple() As Task
			await Task.Delay(1)
		End Function

		async Public Shared Function SecondSimple() As Task
			await Task.Delay(1)
		End Function
	End Class
End Namespace
