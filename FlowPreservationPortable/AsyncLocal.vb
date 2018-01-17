Imports System.Linq
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports System
Imports System.Collections.Generic

Namespace FlowPreservation
    ''' <summary>
    ''' Abstract base class for async-local storage, allowing for event listeners to access thread-local storages
    ''' </summary>
    Public MustInherit Class AsyncLocal
        <ThreadStatic>
        Friend Shared PersistentThreadStorage As Dictionary(Of Guid, Object)

        <ThreadStatic>
        Friend Shared DiscardableThreadStorage As Dictionary(Of Guid, Object)

        Protected Sub New()
            AsyncLocalListener.EnsureInitialized()
        End Sub
    End Class

    ''' <summary>
    ''' Generic concrete class for async-local storage 
    ''' </summary>
    ''' <typeparam name="T">Type of value stored in async-local storage</typeparam>
    Public Class AsyncLocal(Of T)
        Inherits AsyncLocal

        Private ReadOnly guid As Guid = guid.NewGuid
        Private ReadOnly valueFactory As Func(Of T)
        Private ReadOnly discardOnAwaitBoundary As Boolean

        Public Sub New()
            Me.New(Function() Nothing)
        End Sub

        ''' <summary>
        ''' Constructor, allowing to instantiate two different flavors of async-local storage,
        ''' where the value is either discarded on first await boundary or preserved along the execution flow
        ''' </summary>
        ''' <param name="valueFactory"></param>
        ''' <param name="discardOnAwaitBoundary"></param>
        Public Sub New(ByVal valueFactory As Func(Of T), Optional ByVal discardOnAwaitBoundary As Boolean = False)
            Me.valueFactory = valueFactory
            Me.discardOnAwaitBoundary = discardOnAwaitBoundary
        End Sub

        Public Property Value As T
            Get
                Dim value1 As Object = Nothing
                If ThreadStorage.TryGetValue(guid, value1) Then
                    Return CType(value1, T)
                Else
                    Dim newValue = valueFactory()
                    ThreadStorage.Add(guid, newValue)
                    Return newValue
                End If
            End Get
            Set(ByVal value As T)
                ThreadStorage(guid) = value
            End Set
        End Property

        Private ReadOnly Property ThreadStorage As Dictionary(Of Guid, Object)
            Get
                If discardOnAwaitBoundary Then
                    If DiscardableThreadStorage IsNot Nothing Then
                        Return DiscardableThreadStorage
                    Else
                        DiscardableThreadStorage = New Dictionary(Of Guid, Object)
                        Return DiscardableThreadStorage
                    End If
                Else
                    If PersistentThreadStorage IsNot Nothing Then
                        Return PersistentThreadStorage
                    Else
                        PersistentThreadStorage = New Dictionary(Of Guid, Object)
                        Return PersistentThreadStorage
                    End If
                End If
            End Get
        End Property
    End Class
End Namespace
