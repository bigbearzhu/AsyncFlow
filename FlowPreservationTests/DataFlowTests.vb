Imports FlowPreservation
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Text
Imports System.Threading.Tasks
Imports System.Threading.Tasks.Dataflow

Namespace FlowPreservationTests
	<TestClass>
	Public Class DataFlowTests
		<TestMethod>
		async Public Function TestChainedBlocks() As Task
			Dim action = New ActionBlock(Of Integer)(New Func(Of Integer, Task)(AddressOf Methods.SecondBlockAction))
			Dim transform = New TransformBlock(Of Integer, Integer)(New Func(Of Integer, Task(Of Integer))(AddressOf Methods.FirstBlockAction))
			transform.LinkTo(action)
			await transform.SendAsync(0)
			AssertEx.StackHasRoot()
			transform.Complete()
			await transform.Completion
			AssertEx.StackHasRoot()
			action.Complete()
			await action.Completion
			AssertEx.StackHasRoot()
		End Function
	End Class
End Namespace
