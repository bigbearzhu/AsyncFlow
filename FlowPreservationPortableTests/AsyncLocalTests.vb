Imports FlowPreservation
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Linq
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

Namespace FlowPreservationTests
	<TestClass>
	Public Class AsyncLocalTests
		<TestMethod>
		async Public Function AsyncLocalBasic() As Task
			await VerifyValueFlow(AddressOf InnerAsync)
		End Function

		<TestMethod>
		async Public Function AsyncLocalNested() As Task
			await VerifyValueFlow(AddressOf OuterAsync)
		End Function

		async Private Function VerifyValueFlow(ByVal action As Func(Of Action, Task)) As Task
			await ContextHelper.DropContext
			Const expectedValue As String = "Hello!"
			Dim asyncLocal = New AsyncLocal(Of String)
			asyncLocal.Value = expectedValue
			Dim initialThreadId As Integer = Thread.CurrentThread.ManagedThreadId
			Do
                Await action(Sub() Assert.AreEqual(expectedValue, asyncLocal.Value))
			Loop While Thread.CurrentThread.ManagedThreadId = initialThreadId
			VerifyNoLeftovers(initialThreadId, asyncLocal)
		End Function

		Private Sub VerifyNoLeftovers(Of T)(ByVal initialThreadId As Integer, ByVal asyncLocal As AsyncLocal(Of T))
			Dim countdown = New CountdownEvent(10)
			Dim releaser = New ManualResetEventSlim(False)
			Dim verifiedAny As Boolean = False
			Dim tasks = New Task(countdown.InitialCount - 1){}
			For i  = 0 To tasks.Length - 1
				tasks(i) = Task.Run(Sub()
					If Thread.CurrentThread.ManagedThreadId = initialThreadId Then
						Assert.AreEqual(Nothing, asyncLocal.Value, "Leftover value stored on old thread")
						Volatile.Write(verifiedAny, True)
					End If
					countdown.Signal()
					releaser.Wait()
				End Sub)
			Next i
			countdown.Wait()
			releaser.Set()
			Assert.IsTrue(verifiedAny, "Could not verify absence of leftover value: no task was scheduled to initial thread")
		End Sub

		async Private Function InnerAsync(ByVal verify As Action) As Task
			await Task.Delay(1)
			verify()
		End Function

		async Private Function OuterAsync(ByVal verify As Action) As Task
			await InnerAsync(verify)
			verify()
		End Function
	End Class
End Namespace
