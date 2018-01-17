Imports FlowPreservation
Imports System.Reflection
Imports System.Reflection.Emit
Imports System.Runtime.Remoting.Messaging
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

Namespace FlowPreservationPerfTests
    ''' <summary>
    ''' Simple set of performance tests
    ''' </summary>
    Public Class PerfTests
        Public Async Function TestAsyncLocalPerf() As Task
            Const iterations As Integer = 10000

            Await MeasurePerfAsync(Async Function()
                                       Await Task.Yield
                                   End Function, "await Task.Yield", iterations)

            Await MeasurePerfAsync(Async Function()
                                       Await Task.Yield
                                   End Function, "await Task.Yield", iterations)
            CallContext.LogicalSetData("Temp", "Hello!")
            Await MeasurePerfAsync(Async Function()
                                       Await Task.Yield
                                   End Function, "await Task.Yield with CallContext data", iterations)
            Console.WriteLine("Data: " & CallContext.LogicalGetData("Temp").ToString)
        End Function

        Public Sub TestStackTracePerf()
            Const iterations As Integer = 1000

            MeasurePerf(Function() New StackTraceSlim(False), "new StackTraceSlim(false)", iterations)
            MeasurePerf(Function() New StackTraceSlim(True), "new StackTraceSlim(true)", iterations)
            MeasurePerf(Function() New StackTrace(False), "new StackTrace(false)", iterations)
            MeasurePerf(Function() New StackTrace(True), "new StackTrace(true)", iterations)
        End Sub

        Public Async Function TestStackTracePerfYield() As Task
            Const iterations As Integer = 1000

            Await MeasurePerfAsync(Async Function()
                                       Await Task.Yield
                                   End Function, "await Task.Yield", iterations)
            Await MeasurePerfAsync(Async Function()
                                       Await Task.Yield
                                       Dim TempStackTraceSlim As StackTraceSlim = New StackTraceSlim(False)
                                   End Function, "await Task.Yield; new StackTraceSlim(false)", iterations)
            Await MeasurePerfAsync(Async Function()
                                       Await Task.Yield
                                       Dim TempStackTraceSlim As StackTraceSlim = New StackTraceSlim(True)
                                   End Function, "await Task.Yield; new StackTraceSlim(true)", iterations)
            Await MeasurePerfAsync(Async Function()
                                       Await Task.Yield
                                       Dim TempStackTrace As StackTrace = New StackTrace(False)
                                   End Function, "await Task.Yield; new StackTrace(false)", iterations)
            Await MeasurePerfAsync(Async Function()
                                       Await Task.Yield
                                       Dim TempStackTrace As StackTrace = New StackTrace(True)
                                   End Function, "await Task.Yield; new StackTrace(true)", iterations)
        End Function

        Public Async Function TestStackTracePerfAwaitSync() As Task
            Const iterations As Integer = 1000

            Await MeasurePerfAsync(Async Function() Await Task.FromResult(0), "await Task.FromResult(0)", iterations)
            Await MeasurePerfAsync(Async Function()
                                       Await Task.FromResult(0)
                                       Dim TempStackTraceSlim As StackTraceSlim = New StackTraceSlim(False)
                                   End Function, "await Task.FromResult(0); new StackTraceSlim(false)", iterations)
            Await MeasurePerfAsync(Async Function()
                                       Await Task.FromResult(0)
                                       Dim TempStackTraceSlim As StackTraceSlim = New StackTraceSlim(True)
                                   End Function, "await Task.FromResult(0); new StackTraceSlim(true)", iterations)
            Await MeasurePerfAsync(Async Function()
                                       Await Task.FromResult(0)
                                       Dim TempStackTrace As StackTrace = New StackTrace(False)
                                   End Function, "await Task.FromResult(0); new StackTrace(false)", iterations)
            Await MeasurePerfAsync(Async Function()
                                       Await Task.FromResult(0)
                                       Dim TempStackTrace As StackTrace = New StackTrace(True)
                                   End Function, "await Task.FromResult(0); new StackTrace(true)", iterations)
        End Function

        Public Async Function TestStoragePerf() As Task
            Const iterations As Integer = 1000
            Dim methods = New Func(Of Task)() {AddressOf InnerNoAssert, AddressOf MiddleNoAssert, AddressOf OuterNoAssert}

            FlowReservoir.Enroll()

            For Each task In methods
                Await MeasurePerfAsync(task, task.Method.Name & " with logging", iterations)
            Next task
            For Each task In methods
                Await MeasurePerfAsync(task, task.Method.Name & " with logging", iterations)
            Next task
            For Each task In methods
                Await MeasurePerfAsync(task, task.Method.Name & " with logging", iterations)
            Next task
        End Function

        Private Sub MeasurePerf(ByVal action As Action, ByVal name As String, ByVal iterations As Integer)
            Dim completedTask As Task = Task.FromResult(CObj(Nothing))
            MeasurePerfAsync(Function()
                                          action()
                                          Return completedTask
                                      End Function, name, iterations).Wait
        End Sub

        Private Async Function MeasurePerfAsync(ByVal action As Func(Of Task), ByVal name As String, ByVal iterations As Integer) As Task
            Await action()
            Dim stopwatch1 = Stopwatch.StartNew
            For i  = 0 To iterations - 1
                Await action()
            Next i
            stopwatch1.Stop()
            Dim toWrite As String = name & ": " & stopwatch1.Elapsed.ToString
            Console.WriteLine(toWrite)
            Trace.WriteLine(toWrite)
        End Function

        Private Shared Async Function OuterNoAssert() As Task
            Await MiddleNoAssert()
        End Function

        Private Shared Async Function MiddleNoAssert() As Task
            Await InnerNoAssert()
        End Function

        Private Shared Async Function InnerNoAssert() As Task
            Await Task.Yield
        End Function
    End Class
End Namespace
