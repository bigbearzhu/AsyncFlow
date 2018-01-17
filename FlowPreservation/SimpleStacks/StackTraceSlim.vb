Imports System.Collections.Concurrent
Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

Namespace FlowPreservation
	''' <summary>
	''' More lightweight analog to StackTrace
	''' </summary>
	Friend Class StackTraceSlim
		Private Shared ReadOnly getStackFramesInternal As Action(Of Object)
		Private Shared ReadOnly methods As New ConcurrentDictionary(Of IntPtr, MethodBaseSlim)

		''' <summary>
		''' Creates necessary delegate
		''' </summary>
		Shared Sub New()
            Dim getStackFramesInternalMethod = GetType(StackTrace).GetMethod("GetStackFramesInternal", BindingFlags.NonPublic Or BindingFlags.Static)
			Dim stackFrameHelperParam = Expression.Parameter(GetType(Object))
			getStackFramesInternal = Expression.Lambda(Of Action(Of Object))(Expression.Call(getStackFramesInternalMethod, Expression.Convert(stackFrameHelperParam, StackFrameHelperProxy.UnderlyingType), Expression.Constant(0), Expression.Constant(Nothing, GetType(Exception))), MethodNameHelper.GetLambdaMethodName, { stackFrameHelperParam }).Compile
		End Sub

		Friend Sub New(ByVal frames() As StackFrameSlim)
			Me.Frames = frames
		End Sub

		''' <summary>
		''' Creates StackTraceSlim and fills its frames
		''' </summary>
		''' <param name="needFileLineColInfo">Whether source code information should be extracted</param>
		Public Sub New(ByVal needFileLineColInfo As Boolean)
			Dim sfh = New StackFrameHelperProxy(needFileLineColInfo)
			getStackFramesInternal(sfh.UnderlyingInstance)
			Frames = New StackFrameSlim(sfh.GetNumberOfFrames - 1){}
			For i  = 0 To Frames.Length - 1
				Frames(i) = sfh.CreateFrame(i)
				Frames(i).Method = GetMethodForHandle(Frames(i).MethodHandle)
			Next i
		End Sub

		Public Frames() As StackFrameSlim

		''' <summary>
		''' Finds method by handle, first looking at cached methods table
		''' </summary>
		''' <param name="methodHandle">Handle</param>
		''' <returns>MethodBaseSlim</returns>
		Private Function GetMethodForHandle(ByVal methodHandle As IntPtr) As MethodBaseSlim
            Dim method As MethodBaseSlim = Nothing
			If Not methods.TryGetValue(methodHandle, method) Then
				method = New MethodBaseSlim(StackFrameHelperProxy.GetMethod(methodHandle))
				methods.AddOrUpdate(methodHandle, method, Function(k, v) v)
			End If
			Return method
		End Function
	End Class
End Namespace
