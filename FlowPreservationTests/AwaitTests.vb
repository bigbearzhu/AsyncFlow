Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Threading.Tasks
Imports FlowPreservation
Imports System.Threading
Imports System.Reflection

Namespace FlowPreservationTests
	<TestClass>
	Public Class AwaitTests
		<TestMethod>
		async Public Function TestNested() As Task
			await Methods.NestedOuter
			AssertEx.StackHasRoot()
		End Function

		<TestMethod>
		async Public Function TestLoopTwoDelays() As Task
			await Methods.Loop
			AssertEx.StackHasRoot()
		End Function

		<TestMethod>
		async Public Function TestFlakiness() As Task
			await Methods.Flaky(10000)
			AssertEx.StackHasRoot()
		End Function

		<TestMethod>
		Public Sub TestFork()
			Dim t = Task.Run(New Func(Of Task)(AddressOf Methods.FirstAsync))
			SpinWait.SpinUntil(Function() t.IsCompleted)
			AssertEx.StackHasRoot()
		End Sub
	End Class
End Namespace
