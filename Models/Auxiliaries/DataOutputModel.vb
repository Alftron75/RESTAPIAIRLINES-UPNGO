Public Class DataOutputModel(Of T)
    Public Property ResponseCode As Integer
    Public Property ResponseMessage As String
    Public Property ResponseData As List(Of T)

End Class
