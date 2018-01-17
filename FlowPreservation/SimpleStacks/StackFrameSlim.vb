Imports System.Collections.Concurrent
Imports System.Reflection
Imports System.Text
Imports System.Threading.Tasks

Namespace FlowPreservation
	''' <summary>
	''' Lighter analog to StackFrame
	''' </summary>
	<DebuggerDisplay("{LongHash} : {ToString()}")>
	Friend Structure StackFrameSlim
		Implements IEquatable(Of StackFrameSlim)

		Friend Const FirstArrow As Char = ChrW(&H21e6)
		Friend Const SecondArrow As Char = ChrW(&H2190)

		Public Shared ReadOnly AsyncBoundary As New StackFrameSlim(New IntPtr(-1L), -1, -1, Nothing, -1, -1, False)
		Private Shared ReadOnly asyncBoundaryString As String = ChrW(&H231B).ToString & " " & String.Join(" ", Enumerable.Range(0, 10).Select(Function(i) ChrW(&HB7)))

		Public Sub New(ByVal rgMethodHandle As IntPtr, ByVal rgiOffset As Integer, ByVal rgiILOffset As Integer, ByVal rgFilename As String, ByVal rgiLineNumber As Integer, ByVal rgiColumnNumber As Integer, ByVal rgiLastFrameFromForeignExceptionStackTrace As Boolean)
			Me.Method = Nothing

			Me.MethodHandle = rgMethodHandle
			Me.Offset = rgiOffset
			Me.ILOffset = rgiILOffset
			Me.FileName = rgFilename
			Me.LineNumber = rgiLineNumber
			Me.ColumnNumber = rgiColumnNumber
			Me.IsLastFrameFromForeignExceptionStackTrace = rgiLastFrameFromForeignExceptionStackTrace
		End Sub

		Public Method As MethodBaseSlim

		Public ReadOnly MethodHandle As IntPtr
		Public ReadOnly Offset As Integer
		Public ReadOnly ILOffset As Integer
		Public ReadOnly FileName As String
		Public ReadOnly LineNumber As Integer
		Public ReadOnly ColumnNumber As Integer
		Public ReadOnly IsLastFrameFromForeignExceptionStackTrace As Boolean

		Public Overrides Function ToString() As String
			Dim builder = New StringBuilder
			If Equals(AsyncBoundary) Then
				builder.Append(asyncBoundaryString)
			Else
				builder.Append(If((If(Equals(Method, Nothing), Nothing, Method.StringValue)), "<UnknownFrame>"))
				If (Not String.IsNullOrEmpty(FileName)) OrElse LineNumber <> 0 OrElse ColumnNumber <> 0 Then
					builder.AppendFormat(" {3} {0}:line {1}:column {2}", FileName, LineNumber, ColumnNumber, FirstArrow)
				End If
                'builder.AppendFormat(" {2} 0x{0:X} bytes (0x{1:X} IL)", Offset, ILOffset, SecondArrow)
			End If
			Return builder.ToString
		End Function

		''' <summary>
		''' Hashes are long to reduce collisions when relying on hash comparisons between stack trace segments
		''' </summary>
        Public ReadOnly Property LongHash As Long
            Get
                Return MethodHandle.ToInt64 _
                    Xor (CUInt(ILOffset) * 76319L) _
                    Xor (CUInt(Offset) * 1743920413L) _
                    Xor (If(FileName Is Nothing, 0, CUInt(FileName.GetHashCode) * 9432173L)) _
                    Xor (LineNumber * 18472183L) _
                    Xor (ColumnNumber * 29428193823821L) _
                    Xor (If(IsLastFrameFromForeignExceptionStackTrace, &H400000, 0))
            End Get
        End Property


		Public Overrides Function GetHashCode() As Integer
			Dim hash As Long = LongHash
			Return CInt(Fix(hash Xor (hash >> 32)))
		End Function

		Public Overrides Overloads Function Equals(ByVal obj As Object) As Boolean
			If obj Is Nothing OrElse obj.GetType IsNot GetType(StackFrameSlim) Then
				Return False
			End If

			Return Equals(CType(obj, StackFrameSlim))
		End Function

		Public Overloads Function Equals(ByVal other As StackFrameSlim) As Boolean Implements IEquatable(Of StackFrameSlim).Equals
			Return MethodHandle = other.MethodHandle AndAlso Offset = other.Offset AndAlso ILOffset = other.ILOffset AndAlso FileName = other.FileName AndAlso LineNumber = other.LineNumber AndAlso ColumnNumber = other.ColumnNumber AndAlso IsLastFrameFromForeignExceptionStackTrace = other.IsLastFrameFromForeignExceptionStackTrace
		End Function

		''' <summary>
		''' Checks if the frame should be visible against justMyCode filter
		''' </summary>
		''' <param name="justMyCode">Hide framework boilerplate</param>
		''' <returns>Whether frame needs to be hidden</returns>
		Public Function IsHidden(ByVal justMyCode As Boolean) As Boolean
			Return (Not Equals(AsyncBoundary)) AndAlso (Method.IsEventInfrastructure OrElse justMyCode AndAlso Method.IsExternalCode)
		End Function
	End Structure
End Namespace
