Imports System.Data.SqlClient
Imports System.Net
Imports System.Web.Http
Imports Newtonsoft.Json

Namespace Controllers.Ngos
    Public Class NgoController
        Inherits ApiController

        Private RespCode As Integer
        Private RespMsg As String
        Private IdApi As Integer

        <Route("SelectAllNgos")>
        <HttpGet>
        Public Function GetNgos() As DataOutputModel(Of NgoModel)
            Dim listModel As New List(Of NgoModel)
            Dim DataModel As New NgoModel()
            Dim OutputMOdel As New NgoModel()
            Dim BusLog As New BusinessLogic()
            Dim DataOut As New DataOutputModel(Of NgoModel)
            Dim StrSession As String = "API" + Now.Year.ToString + Now.Month.ToString.PadLeft(2, "0") + Now.Day.ToString.PadLeft(2, "0") + Now.Month.ToString.PadLeft(2, "0") + Now.Hour.ToString.PadLeft(2, "0") + Now.Minute.ToString.PadLeft(2, "0") + Now.Month.ToString.PadLeft(2, "0") + Now.Second.ToString.PadLeft(2, "0")

            Try

                IdApi = BusLog.InsLogApi("SelectAllNgos", "", ConfigurationManager.AppSettings.Get("UserId").ToString, StrSession)

                Dim ResAutentication As Tuple(Of Integer, String) = Authentication("SelectAllNgos")

                RespCode = ResAutentication.Item1
                RespMsg = ResAutentication.Item2


                If RespCode = 200 Then

                    Dim DataAcc As DataAccess = New DataAccess(ConfigurationManager.ConnectionStrings("ConnStr").ConnectionString.ToString())
                    Dim DataTbl As DataTable = DataAcc.ReturnDataTable("SEL_NGOS_ROBOT")

                    If DataTbl.Rows.Count > 0 Then

                        For Each ItemRow In DataTbl.Rows

                            DataModel = New NgoModel()

                            DataModel.UserID = ItemRow.ItemArray(0).ToString()
                            DataModel.NgoId = ItemRow.ItemArray(1).ToString()
                            DataModel.Airline = ItemRow.ItemArray(2).ToString()
                            DataModel.AirlineId = ItemRow.ItemArray(3).ToString()
                            DataModel.AirportCode = ItemRow.ItemArray(4).ToString()
                            DataModel.DestinationCode = ItemRow.ItemArray(5)
                            DataModel.DepartureDate = ItemRow.ItemArray(6).ToString()
                            DataModel.ReturnDate = ItemRow.ItemArray(7).ToString()
                            DataModel.TypeTrip = ItemRow.ItemArray(8)
                            DataModel.NumCompanion = ItemRow.ItemArray(9).ToString()

                            listModel.Add(DataModel)

                        Next

                        RespCode = 200
                        RespMsg = "Success"

                    Else

                        RespCode = 405
                        RespCode = "No data"

                    End If
                End If


            Catch SqlEx As SqlException

                RespCode = 401
                RespMsg = "Database error"

                DataOut.ResponseCode = RespCode
                DataOut.ResponseMessage = RespMsg
                DataOut.ResponseData = listModel
                BusLog.UpdLogApi(IdApi, JsonConvert.SerializeObject(DataOut).ToString())

            Catch ex As Exception

                RespCode = 400
                RespMsg = "API error"

                DataOut.ResponseCode = RespCode
                DataOut.ResponseMessage = RespMsg
                DataOut.ResponseData = listModel
                BusLog.UpdLogApi(IdApi, JsonConvert.SerializeObject(DataOut).ToString())

            End Try

            DataOut.ResponseCode = RespCode
            DataOut.ResponseMessage = RespMsg
            DataOut.ResponseData = listModel
            BusLog.UpdLogApi(IdApi, JsonConvert.SerializeObject(DataOut).ToString())

            Return DataOut

        End Function

        <Route("InsertFlightOffers")>
        <HttpPost>
        Public Function PostInsertOffert(ByVal NgoModel As InserOffertInputModel) As DataOutputModel(Of InserOffertModel)
            Dim listModel As New List(Of InserOffertModel)
            Dim DataModel As New InserOffertModel()
            Dim BusLog As New BusinessLogic()
            Dim DataOut As New DataOutputModel(Of InserOffertModel)
            Dim StrSession As String = "API" + Now.Year.ToString + Now.Month.ToString.PadLeft(2, "0") + Now.Day.ToString.PadLeft(2, "0") + Now.Month.ToString.PadLeft(2, "0") + Now.Hour.ToString.PadLeft(2, "0") + Now.Minute.ToString.PadLeft(2, "0") + Now.Month.ToString.PadLeft(2, "0") + Now.Second.ToString.PadLeft(2, "0")

            Try

                IdApi = BusLog.InsLogApi("InsertFlightOffers", JsonConvert.SerializeObject(NgoModel).ToString(), ConfigurationManager.AppSettings.Get("UserId").ToString, StrSession)

                Dim ResAutentication As Tuple(Of Integer, String) = Authentication("InsertFlightOffers")

                RespCode = ResAutentication.Item1
                RespMsg = ResAutentication.Item2


                If RespCode = 200 Then

                    Dim DtNgoDetail As New DataTable()
                    Dim DtNgoOffert As New DataTable()

                    ' Agregar columnas al DataTable para las propiedades de OfferModel
                    DtNgoDetail.Columns.Add("UserID", GetType(Integer))
                    DtNgoDetail.Columns.Add("NgoId", GetType(Integer))
                    DtNgoDetail.Columns.Add("Airline", GetType(String))
                    DtNgoDetail.Columns.Add("AirlineId", GetType(String))
                    DtNgoDetail.Columns.Add("AirportCode", GetType(String))
                    DtNgoDetail.Columns.Add("DestinationCode", GetType(String))
                    DtNgoDetail.Columns.Add("DepartureDate", GetType(String))
                    DtNgoDetail.Columns.Add("ReturnDate", GetType(String))
                    DtNgoDetail.Columns.Add("TypeTrip", GetType(String))
                    DtNgoDetail.Columns.Add("NumCompanion", GetType(String))

                    Dim row As DataRow = DtNgoDetail.NewRow()

                    ' Llenar las columnas de NgoModel
                    row("UserID") = NgoModel.NgoDetail.UserID
                    row("NgoId") = NgoModel.NgoDetail.NgoId
                    row("Airline") = NgoModel.NgoDetail.Airline
                    row("AirlineId") = NgoModel.NgoDetail.AirlineId
                    row("AirportCode") = NgoModel.NgoDetail.AirportCode
                    row("DestinationCode") = NgoModel.NgoDetail.DestinationCode
                    row("DepartureDate") = NgoModel.NgoDetail.DepartureDate
                    row("ReturnDate") = NgoModel.NgoDetail.ReturnDate
                    row("TypeTrip") = NgoModel.NgoDetail.TypeTrip
                    row("NumCompanion") = NgoModel.NgoDetail.NumCompanion

                    DtNgoDetail.Rows.Add(row)

                    'Segunda

                    DtNgoOffert.Columns.Add("NgoId", GetType(String))
                    DtNgoOffert.Columns.Add("ResponseId", GetType(String))
                    DtNgoOffert.Columns.Add("OfferId", GetType(String))
                    DtNgoOffert.Columns.Add("OfferItemId", GetType(String))
                    DtNgoOffert.Columns.Add("Expiration", GetType(String))
                    DtNgoOffert.Columns.Add("OriginPrice", GetType(String))
                    DtNgoOffert.Columns.Add("Currency", GetType(String))
                    DtNgoOffert.Columns.Add("Scales", GetType(String))
                    DtNgoOffert.Columns.Add("CabinType", GetType(String))

                    For Each offer As OfferModel In NgoModel.NgoOffert
                        Dim row2 As DataRow = DtNgoOffert.NewRow()

                        ' Llenar las columnas de NgoModel
                        row2("NgoId") = offer.NgoId
                        row2("ResponseId") = offer.ResponseId
                        row2("OfferId") = offer.OfferId
                        row2("OfferItemId") = offer.OfferItemId
                        row2("Expiration") = offer.Expiration
                        row2("OriginPrice") = offer.OriginPrice
                        row2("Currency") = offer.Currency
                        row2("Scales") = offer.Scales
                        row2("CabinType") = offer.CabinType

                        ' Agregar la fila al DataTable
                        DtNgoOffert.Rows.Add(row2)
                    Next

                    Dim HshNgos As Hashtable = New Hashtable()
                    HshNgos.Add("@NGO", DtNgoDetail)
                    HshNgos.Add("@OFFERT", DtNgoOffert)

                    Dim DataAcc As DataAccess = New DataAccess(ConfigurationManager.ConnectionStrings("ConnStr").ConnectionString.ToString())
                    DataModel.Response = DataAcc.ReturnString("INS_NGO_ROBOT", HshNgos)
                    listModel.Add(DataModel)

                    RespCode = 200
                    RespMsg = "Success"

                End If


            Catch SqlEx As SqlException

                RespCode = 401
                RespMsg = "Database error"

                DataOut.ResponseCode = RespCode
                DataOut.ResponseMessage = RespMsg
                DataOut.ResponseData = listModel
                BusLog.UpdLogApi(IdApi, JsonConvert.SerializeObject(DataOut).ToString())

            Catch ex As Exception

                RespCode = 400
                RespMsg = "API error"

                DataOut.ResponseCode = RespCode
                DataOut.ResponseMessage = RespMsg
                DataOut.ResponseData = listModel
                BusLog.UpdLogApi(IdApi, JsonConvert.SerializeObject(DataOut).ToString())

            End Try

            DataOut.ResponseCode = RespCode
            DataOut.ResponseMessage = RespMsg
            DataOut.ResponseData = listModel
            BusLog.UpdLogApi(IdApi, JsonConvert.SerializeObject(DataOut).ToString())

            Return DataOut

        End Function

        Private Function Authentication(ByVal ApiName As String) As Tuple(Of Integer, String)

            Dim IntCode As Integer = 200
            Dim UserName As String = String.Empty
            Dim StrMsg As String = String.Empty

            Try

                Dim HtpHeaders As System.Net.Http.Headers.HttpRequestHeaders = Me.Request.Headers
                Dim StrToken As String = String.Empty

                If HtpHeaders.Contains("UserName") And HtpHeaders.Contains("Token") Then

                    StrToken = HtpHeaders.GetValues("token").First()
                    UserName = HtpHeaders.GetValues("UserName").First()

                    Dim TokenMan As New TokenManager()
                    Dim StrSecret As String = TokenMan.ValidateToken(StrToken, UserName)

                    If UserName = StrSecret Then

                        Dim DataAcc As New DataAccess(ConfigurationManager.ConnectionStrings("ConnStr").ConnectionString.ToString())
                        Dim HshGrant As Hashtable = New Hashtable()

                        HshGrant.Add("@USER_NAME", UserName)
                        HshGrant.Add("@API_NAME", ApiName)

                        Dim BolGrant As Boolean = DataAcc.ReturnBoolean("API_SEL_CHECK_PERMISSIONS", HshGrant)

                        If BolGrant = False Then

                            IntCode = 404
                            StrMsg = "You Don't have the permission to consume this Api"

                        End If

                    Else

                        IntCode = 403
                        StrMsg = "Invalid Token"

                    End If

                Else

                    IntCode = 402
                    StrMsg = "Hesders not found"

                End If

            Catch SqlEx As SqlException

                IntCode = 401
                StrMsg = "Database error"

            Catch ex As Exception

                IntCode = 400
                StrMsg = "Api error"

            End Try

            Return Tuple.Create(IntCode, StrMsg)
        End Function

    End Class
End Namespace