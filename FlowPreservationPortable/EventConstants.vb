Imports System.Collections.ObjectModel
Imports System.Diagnostics.Tracing
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System

Namespace FlowPreservation
    ''' <summary>
    ''' A set of constants from TplEtwProvider and FrameworkEventSource
    ''' </summary>
    Public NotInheritable Class EventConstants
        ''' <summary>
        ''' An excerpt from TplEtwProvider
        ''' </summary>
        Public MustInherit Class Tpl
            Public Shared ReadOnly GUID As New Guid("2e5dba47-a3d2-4d16-8ee0-6671ffdcd7b5")

            Public Const PARALLELLOOPBEGIN_ID As Integer = 1
            Public Const PARALLELLOOPEND_ID As Integer = 2

            Public Const PARALLELINVOKEBEGIN_ID As Integer = 3
            Public Const PARALLELINVOKEEND_ID As Integer = 4

            Public Const PARALLELFORK_ID As Integer = 5
            Public Const PARALLELJOIN_ID As Integer = 6

            Public Const TASKSCHEDULED_ID As Integer = 7

            Public Const TASKSTARTED_ID As Integer = 8
            Public Const TASKCOMPLETED_ID As Integer = 9

            Public Const TASKWAITBEGIN_ID As Integer = 10
            Public Const TASKWAITEND_ID As Integer = 11

            ' Tasks.Loop == 1
            Public MustOverride Sub ParallelLoopBegin(ByVal OriginatingTaskSchedulerID As Integer, ByVal OriginatingTaskID As Integer, ByVal ForkJoinContextID As Integer, ByVal OperationType As ForkJoinOperationType, ByVal InclusiveFrom As Long, ByVal ExclusiveTo As Long)

            Public MustOverride Sub ParallelLoopEnd(ByVal OriginatingTaskSchedulerID As Integer, ByVal OriginatingTaskID As Integer, ByVal ForkJoinContextID As Integer, ByVal TotalIterations As Long)

            ' Tasks.Invoke == 2
            Public MustOverride Sub ParallelInvokeBegin(ByVal OriginatingTaskSchedulerID As Integer, ByVal OriginatingTaskID As Integer, ByVal ForkJoinContextID As Integer, ByVal OperationType As ForkJoinOperationType, ByVal ActionCount As Integer)

            Public MustOverride Sub ParallelInvokeEnd(ByVal OriginatingTaskSchedulerID As Integer, ByVal OriginatingTaskID As Integer, ByVal ForkJoinContextID As Integer)

            ' Tasks.ForkJoin == 5
            Public MustOverride Sub ParallelFork(ByVal OriginatingTaskSchedulerID As Integer, ByVal OriginatingTaskID As Integer, ByVal ForkJoinContextID As Integer)

            Public MustOverride Sub ParallelJoin(ByVal OriginatingTaskSchedulerID As Integer, ByVal OriginatingTaskID As Integer, ByVal ForkJoinContextID As Integer)

            ' No task
            Public MustOverride Sub TaskScheduled(ByVal OriginatingTaskSchedulerID As Integer, ByVal OriginatingTaskID As Integer, ByVal TaskID As Integer, ByVal CreatingTaskID As Integer, ByVal TaskCreationOptions As Integer)

            ' Tasks.TaskExecute == 3
            Public MustOverride Sub TaskStarted(ByVal OriginatingTaskSchedulerID As Integer, ByVal OriginatingTaskID As Integer, ByVal TaskID As Integer)

            Public MustOverride Sub TaskCompleted(ByVal OriginatingTaskSchedulerID As Integer, ByVal OriginatingTaskID As Integer, ByVal TaskID As Integer, ByVal IsExceptional As Boolean)

            ' Tasks.TaskWait == 4
            Public MustOverride Sub TaskWaitBegin(ByVal OriginatingTaskSchedulerID As Integer, ByVal OriginatingTaskID As Integer, ByVal TaskID As Integer, ByVal Behavior As TaskWaitBehavior)

            Public MustOverride Sub TaskWaitEnd(ByVal OriginatingTaskSchedulerID As Integer, ByVal OriginatingTaskID As Integer, ByVal TaskID As Integer)

            Public Enum ForkJoinOperationType
                ParallelInvoke = 1
                ParallelFor = 2
                ParallelForEach = 3
            End Enum

            Public Class Tasks
                Public Const Loop1 As EventTask = (CType(1, EventTask))
                Public Const Invoke As EventTask = (CType(2, EventTask))
                Public Const ForkJoin As EventTask = (CType(5, EventTask))
                Public Const TaskExecute As EventTask = (CType(3, EventTask))
                Public Const TaskWait As EventTask = (CType(4, EventTask))
            End Class

            Public Enum TaskWaitBehavior
                Synchronous = 1
                Asynchronous = 2
            End Enum
        End Class

        ''' <summary>
        ''' An excerpt frm FrameworkEventSource
        ''' </summary>
        Public MustInherit Class Framework
            Public Shared ReadOnly GUID As New Guid("8E9F5090-2D75-4d03-8A81-E5AFBF85DAF1")

            Public Const THREADPOOLDEQUEUEWORK_ID As Integer = 31
            Public Const THREADPOOLENQUEUEWORK_ID As Integer = 30

            ' Keywords.ThreadPool == 2L
            Public MustOverride Sub ThreadPoolDequeueWork(ByVal workID As Long)

            Public MustOverride Sub ThreadPoolEnqueueWork(ByVal workID As Long)

            Public NotInheritable Class Keywords
                Public Const Loader As EventKeywords = CType(1L, EventKeywords)
                Public Const ThreadPool As EventKeywords = CType(2L, EventKeywords)
            End Class
        End Class
    End Class
End Namespace
