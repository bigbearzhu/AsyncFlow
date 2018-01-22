Imports System.Linq.Expressions
Imports System.Reflection

Namespace FlowPreservation
	''' <summary>
	''' More lightweight analog to StackTrace
	''' </summary>
	Friend Class StackTraceSlim
		Public Shared FilterPrex As String = String.Empty
		Private Shared ReadOnly getStackFramesInternal As Action(Of Boolean, Object)

		''' <summary>
		''' Creates necessary delegate
		''' </summary>
		Shared Sub New()
            Dim getStackFramesInternalMethod = GetType(StackTrace).GetMethod("GetStackFramesInternal", BindingFlags.NonPublic Or BindingFlags.Static)
			Dim stackFrameHelperParam = Expression.Parameter(GetType(Object))
			Dim needFileLineColInfoParam = Expression.Parameter(GetType(Boolean))
			getStackFramesInternal = Expression.Lambda(Of Action(Of Boolean, Object))(
                Expression.Call(getStackFramesInternalMethod, 
                                Expression.Convert(stackFrameHelperParam, StackFrameHelperProxy.UnderlyingType), 
                                Expression.Constant(0), 
                                needFileLineColInfoParam,
                                Expression.Constant(Nothing, GetType(Exception))), 
                MethodNameHelper.GetLambdaMethodName, 
                { needFileLineColInfoParam, stackFrameHelperParam }).Compile
		End Sub

		Friend Sub New(ByVal frames() As StackFrameSlim)
			Me.Frames = frames
		End Sub

		''' <summary>
		''' Creates StackTraceSlim and fills its frames
		''' </summary>
		''' <param name="needFileLineColInfo">Whether source code information should be extracted</param>
		Public Sub New(ByVal needFileLineColInfo As Boolean)
			Dim sfh = New StackFrameHelperProxy()
			getStackFramesInternal(needFileLineColInfo, sfh.UnderlyingInstance)

            Frames = (From I in Enumerable.Range(0, sfh.GetNumberOfFrames)
                     Let frame = sfh.CreateFrame(i)
                     Where String.IsNullOrEmpty(FilterPrex) Or frame.Method.StringValue.StartsWith(FilterPrex)
                     Select frame).ToArray()
		End Sub

		Public Frames() As StackFrameSlim


	End Class
End Namespace
