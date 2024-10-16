Public Class BusinessLogic

    Public Function InsLogApi(ByVal StrApi As String, ByVal JsonRequest As String, ByVal UserId As String, ByVal StrSession As String) As Integer
        Dim DataAcc As New DataAccess(ConfigurationManager.ConnectionStrings("ConnStr").ConnectionString)
        Dim Hsh As New Hashtable
        Hsh.Add("@API", StrApi)
        Hsh.Add("@JSON_REQUEST", JsonRequest)
        Hsh.Add("@USER_ID", UserId)
        Hsh.Add("@SESSION", StrSession)
        Dim intLog As Integer = DataAcc.ReturnInteger("INS_LOGAPI", Hsh)

        Return intLog
    End Function

    Public Sub UpdLogApi(ByVal IdLogApi As String, ByVal JsonRequest As String)
        Dim DataAcc As New DataAccess(ConfigurationManager.ConnectionStrings("ConnStr").ConnectionString)
        Dim Hsh As New Hashtable
        Hsh.Add("@ID", IdLogApi)
        Hsh.Add("@JSON_RESPONSE", JsonRequest)
        DataAcc.SaveUpdate("UPD_LOGAPI", Hsh)
    End Sub

End Class
