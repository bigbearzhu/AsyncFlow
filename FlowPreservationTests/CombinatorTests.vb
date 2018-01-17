Imports FlowPreservation
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Text
Imports System.Threading.Tasks

Namespace FlowPreservationTests
    <TestClass>
    Public Class CombinatorTests
        <TestMethod>
        Public Async Function TestWhenAny() As Task
            Await Task.WhenAny(Methods.InfiniteAsync(), Methods.FirstAsync())
            AssertEx.StackHasRoot()
            AssertEx.NotOnStack(AddressOf Methods.InfiniteAsync)
        End Function

        <TestMethod>
        Public Async Function TestWhenAll() As Task
            Await Task.WhenAll(Methods.FirstAsync(), Methods.SecondAsync())
            AssertEx.StackHasRoot()
        End Function

        <TestMethod>
              Public Async Function TestContinueWith() As Task
            Await Await Methods.FirstAsync().ContinueWith(Async Function(t)
                                                              Await Methods.SecondAsync()
                                                          End Function)
            AssertEx.StackHasRoot()
        End Function
        <TestMethod>
        Public Async Function TestContinueWhenAll() As Task
            Await Task.Factory.ContinueWhenAll({Methods.FirstSimple(), Methods.SecondSimple()}, Sub(tt)
                                                                                                         End Sub)
            AssertEx.StackHasRoot()
        End Function
    End Class
End Namespace
