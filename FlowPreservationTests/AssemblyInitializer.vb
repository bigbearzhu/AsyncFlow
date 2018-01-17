Imports FlowPreservation
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Text
Imports System.Threading.Tasks

Namespace FlowPreservationTests
	<TestClass>
	Public Class AssemblyInitializer
		<AssemblyInitialize>
		Public Shared Sub AssemblyInitialize(ByVal context As TestContext)
			FlowReservoir.Enroll()
		End Sub
	End Class
End Namespace
