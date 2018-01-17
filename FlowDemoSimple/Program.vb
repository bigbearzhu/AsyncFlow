Imports FlowPreservation
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

Namespace FlowDemoSimple
	''' <summary>
	''' Simple class to test preservation of causality chains over different scenarios in classic applications.
	''' Try setting breakpoints after await boundaries to see how causality is stored.
	''' </summary>
	Friend Class Program
		Private Shared ReadOnly semaphore As New SemaphoreSlim(0)

		Shared Sub Main(ByVal args() As String)
			FlowReservoir.Enroll()
            'OuterAsync.Wait
            'FireAndForget()
            Task.WaitAll(Simple, Loop1)
		End Sub

		Private async Shared Function Simple() As Task
			await Task.Delay(1)
			await Common()
		End Function

        Private Shared Async Function Loop1() As Task
            For i = 0 To 9
                Await Task.Delay(1)
            Next i
            Await Common()
        End Function

		Private async Shared Function Common() As Task
			await Task.Delay(1)
			semaphore.Wait()
            'Do
            'Loop
		End Function

		Private async Shared Function InnerAsync() As Task
			await Task.Delay(1)
		End Function

		Private async Shared Function OuterAsync() As Task
			await InnerAsync()
		End Function

		Private async Shared Sub FireAndForget()
			await Task.Delay(1)
		End Sub
	End Class
End Namespace
