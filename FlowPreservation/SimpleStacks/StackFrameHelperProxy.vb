Imports System.Collections.Concurrent
Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

Namespace FlowPreservation
	''' <summary>
	''' Proxy to StackFrameHelper class (the one that fills StackTrace).
	''' Working with it directly allows for slight performance increase capturing stack traces in managed code.
	''' </summary>
	Friend Class StackFrameHelperProxy
		Private Shared privateUnderlyingType As Type
		Private Shared ReadOnly methods As New ConcurrentDictionary(Of IntPtr, MethodBaseSlim)
		Public Shared Property UnderlyingType As Type
			Get
				Return privateUnderlyingType
			End Get
			Private Set(ByVal value As Type)
				privateUnderlyingType = value
			End Set
		End Property

		Private Shared ReadOnly createUnderlyingInstance As Func(Of Object)
        Private Shared ReadOnly getNumberOfFramesVal As Func(Of Object, Integer)
        Private Shared ReadOnly createFrameVal As Func(Of Object, Integer, StackFrameSlim)
        Private Shared ReadOnly getMethodVal As Func(Of IntPtr, MethodBase)

		''' <summary>
		''' Creates necessary delegates that proxy calls to StackFrameHelper
		''' </summary>
		Shared Sub New()
			UnderlyingType = Type.GetType("System.Diagnostics.StackFrameHelper", True)
			Dim stackFrameHelperCtor = UnderlyingType.GetConstructor( { GetType(Thread) })
			Dim currentThreadProperty = GetType(Thread).GetProperty("CurrentThread")
            createUnderlyingInstance = Expression.Lambda(Of Func(Of Object))(
                Expression.[New](
                    stackFrameHelperCtor,
                    Expression.Property(Nothing, currentThreadProperty))).Compile()

			Dim stackFrameHelperParam = Expression.Parameter(GetType(Object))
			Dim stackFrameHelperAsItself = Expression.Convert(stackFrameHelperParam, UnderlyingType)
            getNumberOfFramesVal = Expression.Lambda(Of Func(Of Object, Integer))(Expression.Field(stackFrameHelperAsItself, UnderlyingType.GetField("iFrameCount", BindingFlags.Instance Or BindingFlags.NonPublic)), stackFrameHelperParam).Compile

			Dim stackFrameSlimCtor = GetType(StackFrameSlim).GetConstructors.Single
			Dim intParam = Expression.Parameter(GetType(Integer))
			Dim getters = From param In stackFrameSlimCtor.GetParameters
			              Let field = UnderlyingType.GetField(param.Name, BindingFlags.Instance Or BindingFlags.NonPublic)
			              Let fieldExpr = Expression.Field(stackFrameHelperAsItself, field)
			              Select Expression.Condition(Expression.Equal(fieldExpr, Expression.Constant(Nothing, field.FieldType)), Expression.Default(param.ParameterType), Expression.ArrayAccess(fieldExpr, intParam))
            createFrameVal = Expression.Lambda(Of Func(Of Object, Integer, StackFrameSlim))(Expression.[New](stackFrameSlimCtor, getters), stackFrameHelperParam, intParam).Compile

			Dim intPtrParam = Expression.Parameter(GetType(IntPtr))
			Dim iRuntimeMethodInfoParams = { Type.GetType("System.IRuntimeMethodInfo") }
            getMethodVal = Expression.Lambda(Of Func(Of IntPtr, MethodBase))(Expression.Condition(Expression.Call(intPtrParam, GetType(IntPtr).GetMethod("IsNull", BindingFlags.Instance Or BindingFlags.NonPublic)), Expression.Constant(Nothing, GetType(MethodBase)), Expression.Call(Type.GetType("System.RuntimeType").GetMethod("GetMethodBase", BindingFlags.Static Or BindingFlags.NonPublic, Nothing, iRuntimeMethodInfoParams, Nothing), Expression.Call(GetType(RuntimeMethodHandle).GetMethod("GetTypicalMethodDefinition", BindingFlags.Static Or BindingFlags.NonPublic, Nothing, iRuntimeMethodInfoParams, Nothing), Expression.[New](Type.GetType("System.RuntimeMethodInfoStub").GetConstructor({GetType(IntPtr), GetType(Object)}), intPtrParam, Expression.Constant(Nothing, GetType(Object)))))), intPtrParam).Compile
		End Sub

		''' <summary>
		''' Actual instance of StackFrameHelper
		''' </summary>
		Private privateUnderlyingInstance As Object
		Public Property UnderlyingInstance As Object
			Get
				Return privateUnderlyingInstance
			End Get
			Private Set(ByVal value As Object)
				privateUnderlyingInstance = value
			End Set
		End Property

		''' <summary>
		''' Constructor
		''' </summary>
		''' <param name="needFileLineColInfo">Whether source file information should be extracted</param>
		Public Sub New()
			Me.UnderlyingInstance = createUnderlyingInstance()
		End Sub

		''' <summary>
		''' How many frames does current stack trace have
		''' </summary>
		''' <returns>Number of frames</returns>
		Public Function GetNumberOfFrames() As Integer
            Return getNumberOfFramesVal(UnderlyingInstance)
		End Function

		''' <summary>
		''' Fills frame information into a StackFrameSlim
		''' </summary>
		''' <param name="index">Frame index in a stack trace</param>
		''' <returns>New StackFrameSlim</returns>
		Public Function CreateFrame(ByVal index As Integer) As StackFrameSlim
		    Dim frame  = createFrameVal(UnderlyingInstance, index)
            frame.Method = GetMethodForHandle(frame.MethodHandle)
		    Return frame
		End Function

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

		''' <summary>
		''' Resolve method from handle (this is expensive)
		''' </summary>
		''' <param name="handle">Handle</param>
		''' <returns>MethodBase</returns>
		Public Shared Function GetMethod(ByVal handle As IntPtr) As MethodBase
            Return getMethodVal(handle)
		End Function
    End Class
End Namespace
