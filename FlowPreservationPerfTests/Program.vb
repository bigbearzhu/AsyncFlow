Imports System.IO
Imports System.Text
Imports System.Threading.Tasks

Namespace FlowPreservationPerfTests
	''' <summary>
	''' Runs a series of performance tests
	''' </summary>
	Friend Class Program
		Shared Sub Main(ByVal args() As String)
			Const logFile As String = "perf.txt"
			File.Delete(logFile)
			Trace.Listeners.Clear()
			Trace.Listeners.Add(New TextWriterTraceListener(logFile))
			Dim perfTests = New PerfTests
			perfTests.TestStackTracePerf()
			perfTests.TestStackTracePerfAwaitSync.Wait()
			perfTests.TestStackTracePerfYield.Wait()
			perfTests.TestStoragePerf.Wait()
			perfTests.TestAsyncLocalPerf.Wait()
			Console.WriteLine("Done!")
			If Debugger.IsAttached Then
				Console.ReadLine()
			End If
		End Sub
	End Class
End Namespace
