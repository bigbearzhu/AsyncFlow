Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System

Namespace FlowPreservation
    ''' <summary>
    ''' Extensions class that allows for combining actions
    ''' </summary>
    Friend Module AppendableExtensions
        ''' <summary>
        ''' Combines two actions
        ''' </summary>
        ''' <param name="preAction">Action to execute first</param>
        ''' <param name="postAction">Action to execute second</param>
        ''' <returns>A composition of two actions</returns>
        <System.Runtime.CompilerServices.Extension>
        Public Function Append(ByVal preAction As Action, ByVal postAction As Action) As Action
            Return Sub()
                       preAction()
                       postAction()
                   End Sub
        End Function
    End Module
End Namespace
