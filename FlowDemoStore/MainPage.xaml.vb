Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Threading
Imports System.Threading.Tasks
Imports FlowPreservation
Imports Windows.Foundation
Imports Windows.Foundation.Collections
Imports Windows.UI.Xaml
Imports Windows.UI.Xaml.Controls
Imports Windows.UI.Xaml.Controls.Primitives
Imports Windows.UI.Xaml.Data
Imports Windows.UI.Xaml.Input
Imports Windows.UI.Xaml.Media
Imports Windows.UI.Xaml.Navigation
Imports System.Linq.Expressions

Namespace FlowDemoStore
	''' <summary>
	''' Simple test for causality chain preservation in Windows Store applications. 
	''' To see how causality is preserved, set breakpoints after awaits in asynchronous methods of this class.
	''' </summary>
	Public NotInheritable Partial Class MainPage
		Inherits Page

		Public Sub New()
			Me.InitializeComponent()
		End Sub

		''' <summary>
		''' This event triggers a series of asynchronous operations over which causality is preserved.
		''' </summary>
		async Protected Overrides Sub OnNavigatedTo(ByVal e As NavigationEventArgs)
            Await ContextHelper.DropContext
            Await OuterAsync.WithCausality
		End Sub

		async Private Function OuterAsync() As Task
			await Inner1Async.WithCausality
			await Inner2Async.WithCausality
			await Inner3Async.WithCausality
		End Function

		async Private Function Inner1Async() As Task
			await Task.Delay(100).WithCausality
		End Function

		async Private Function Inner2Async() As Task
			await Task.Yield.WithCausality
		End Function

		async Private Function Inner3Async() As Task
			await Inner3InnerAsync.WithCausality
			await Task.Delay(100).WithCausality
		End Function

		async Private Function Inner3InnerAsync() As Task
			await Task.Delay(100).WithCausality
			await Task.Yield.WithCausality

            Dim semaphore As SemaphoreSlim = New SemaphoreSlim(0)
			semaphore.Wait()
        End Function
    End Class
End Namespace
