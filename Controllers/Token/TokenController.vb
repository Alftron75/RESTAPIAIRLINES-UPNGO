Imports System.Data.SqlClient
Imports System.Net
Imports System.Web.Http

Namespace Controllers
    Public Class TokenController
        Inherits ApiController

        'Global variables
        Private RespCode As Integer
        Private RespMsg As String

        <Route("Token")>
        <HttpPost>
        Public Function PostToken(ByVal ModelInput As UserInfoModel) As DataOutputModel(Of TokenOutputModel)
            Dim ListToken As List(Of TokenOutputModel) = New List(Of TokenOutputModel)
            Dim DataToken As TokenOutputModel = New TokenOutputModel()

            Try
                If CheckUser(ModelInput.UserName, ModelInput.UserSecret) Then
                    Dim Secret As String = ReturnSecret(ModelInput.UserName, ModelInput.UserSecret)
                    Dim TokenMan As New TokenManager()
                    DataToken = TokenMan.GenerateToken(ModelInput.UserName, Secret)
                    ListToken.Add(DataToken)

                    RespCode = 200
                    RespMsg = "Success"
                Else
                    RespCode = 402
                    RespMsg = "Invalid User or Secret"
                End If
            Catch SqlEx As SqlException
                RespCode = 401
                RespMsg = "DataBase Error: " + SqlEx.Message.ToString()
            Catch ex As Exception
                RespCode = 400
                RespMsg = "Api Error: " + ex.Message.ToString()
            End Try

            Dim DataOut As DataOutputModel(Of TokenOutputModel) = New DataOutputModel(Of TokenOutputModel)() With
            {
                .ResponseCode = RespCode,
                .ResponseMessage = RespMsg,
                .ResponseData = ListToken
            }

            Return DataOut
        End Function

        Public Function CheckUser(ByVal UserName As String, ByVal UserSecret As String) As Boolean
            Dim DataAcc As New DataAccess(ConfigurationManager.ConnectionStrings("ConnStr").ConnectionString)
            Dim CheckedUser As Boolean
            Dim HshCheck As Hashtable = New Hashtable()
            HshCheck.Add("@USER_NAME", UserName.Trim())
            HshCheck.Add("@USER_SECRET", UserSecret.Trim())
            CheckedUser = DataAcc.ReturnInteger("API_SEL_USRSECRET", HshCheck)

            Return CheckedUser
        End Function

        Public Function ReturnSecret(ByVal UserName As String, ByVal UserSecret As String) As String
            Dim DataAcc As New DataAccess(ConfigurationManager.ConnectionStrings("ConnStr").ConnectionString)
            Dim HshSecret As Hashtable = New Hashtable()
            HshSecret.Add("@USER_NAME", UserName.Trim())
            HshSecret.Add("@USER_SECRET", UserSecret.Trim())
            Dim Secret As String = DataAcc.ReturnString("API_SEL_SECRET", HshSecret)

            Return Secret
        End Function

    End Class
End Namespace