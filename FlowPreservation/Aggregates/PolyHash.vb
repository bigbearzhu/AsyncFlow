Imports System.Text
Imports System.Threading.Tasks

Namespace FlowPreservation
	''' <summary>
	''' Helper class that calculates polynomial hash, which allows for O(1) comparison of arbitrary sub-lists in a causality chain.
	''' </summary>
	Friend NotInheritable Class PolyHash
		Private Const factor As Long = 1542691

		''' <summary>
		''' Adds one element to the hash
		''' </summary>
		''' <param name="aggregate">Current hash value</param>
		''' <param name="next">New element value</param>
		''' <returns>New hash value</returns>
        Public Shared Function Append(ByVal aggregate As Long, ByVal nextValue As Long) As Long
            Return aggregate * factor + nextValue
        End Function


		''' <summary>
		''' Calculates the hash for a list interval, given the hashes of super-list and sub-list sharing an end, and difference between their cardinality.
		''' E.g. given a super-list of elements N downto 0 and a sub-list M downto 0, where N >= M, and cardinality difference (N - M), returns a hash for interval N downto M+1.
		''' </summary>
		''' <param name="higher">Hash of a super-list</param>
		''' <param name="lower">Hash of a sub-list</param>
		''' <param name="steps">Cardinality difference</param>
		''' <returns>Hash of the interval</returns>
        Public Shared Function Subtract(ByVal higher As Long, ByVal lower As Long, ByVal steps As Integer) As Long
            For i As Integer = 0 To steps - 1
                lower = lower * factor
            Next i
            Return higher - lower
        End Function
    End Class
End Namespace
