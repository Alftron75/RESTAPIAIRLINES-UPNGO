Imports System.Data.SqlClient
Imports System.Net
Imports System.Web.Http
Imports System.Xml
Imports Newtonsoft.Json
Imports RestSharp

Namespace Controllers.Airline
    Public Class IBController
        Inherits ApiController

        Private RespCode As Integer
        Private RespMsg As String
        Private IdApi As Integer

        <Route("IBAirShopping")>
        <HttpPost>
        Public Function PostIBAirShopping(ByVal NgoModel As NgoModel) As DataOutputModel(Of OfferModel)
            Dim ListModel As New List(Of OfferModel)
            Dim BusLog As BusinessLogic = New BusinessLogic()
            Dim DataOut As DataOutputModel(Of OfferModel) = New DataOutputModel(Of OfferModel)
            Dim StrSession As String = "API" + Now.Year.ToString + Now.Month.ToString.PadLeft(2, "0") + Now.Day.ToString.PadLeft(2, "0") + Now.Month.ToString.PadLeft(2, "0") + Now.Hour.ToString.PadLeft(2, "0") + Now.Minute.ToString.PadLeft(2, "0") + Now.Month.ToString.PadLeft(2, "0") + Now.Second.ToString.PadLeft(2, "0")

            Try

                IdApi = BusLog.InsLogApi("IBAirShopping", JsonConvert.SerializeObject(NgoModel).ToString(), ConfigurationManager.AppSettings.Get("UserId").ToString, StrSession)

                Dim ResAutentication As Tuple(Of Integer, String) = Authentication("IBAirShopping")

                RespCode = ResAutentication.Item1
                RespMsg = ResAutentication.Item2


                If RespCode = 200 Then

                    Dim HshXmlInfo As Hashtable = New Hashtable()
                    HshXmlInfo.Add("@AIRLINE_NAME", NgoModel.Airline)
                    HshXmlInfo.Add("@AIRLINE_FILE", "AirShopping")

                    Dim DataAcc As DataAccess = New DataAccess(ConfigurationManager.ConnectionStrings("ConnStr").ConnectionString.ToString())
                    Dim XmlString As String = DataAcc.ReturnString("SEL_AIRLINE_FILE", HshXmlInfo)

                    Dim Xml As String = ReplaceTagsAirShoppingIB(XmlString, NgoModel)

                    'Consumo de Iberia
                    Dim Client As New RestSharp.RestClient(ConfigurationManager.AppSettings.Get("IBAirShopping").ToString)
                    Dim Request As New RestRequest(Method.POST)
                    Request.AddHeader("Content-Type", "application/xml")
                    Request.AddHeader("api_key", ConfigurationManager.AppSettings.Get("api_key").ToString)

                    Request.AddParameter("application/xml", Xml, ParameterType.RequestBody)
                    Dim response As New RestResponse
                    response = Client.Execute(Request)

                    Dim XmlFile As String = response.Content

                    Dim DocXML As New XmlDocument()
                    DocXML.LoadXml(XmlFile)

                    Dim SaveFile As Boolean = CBool(ConfigurationManager.AppSettings.Get("SavexML").ToString())

                    If SaveFile = True Then
                        DocXML.Save(ConfigurationManager.AppSettings.Get("UrlSave").ToString() + "/" + "Prueba" + ".xml")
                    End If

                    ListModel = ProcessOffers(XmlFile, NgoModel.NgoId)

                    RespCode = 200
                    RespMsg = "Success"

                End If


            Catch SqlEx As SqlException

                RespCode = 401
                RespMsg = "Database error"

                DataOut.ResponseCode = RespCode
                DataOut.ResponseMessage = RespMsg
                DataOut.ResponseData = ListModel
                BusLog.UpdLogApi(IdApi, JsonConvert.SerializeObject(DataOut).ToString())

            Catch ex As Exception

                RespCode = 400
                RespMsg = "API error"

                DataOut.ResponseCode = RespCode
                DataOut.ResponseMessage = RespMsg
                DataOut.ResponseData = ListModel
                BusLog.UpdLogApi(IdApi, JsonConvert.SerializeObject(DataOut).ToString())

            End Try

            DataOut.ResponseCode = RespCode
            DataOut.ResponseMessage = RespMsg
            DataOut.ResponseData = ListModel
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

        Public Function ReplaceTagsAirShoppingIB(XMLString As String, Model As NgoModel) As String
            ' Cargar el XML en un XmlDocument
            Dim Doc As New XmlDocument()
            Doc.LoadXml(XMLString)

            ' Crear un XmlNamespaceManager para manejar los espacios de nombres
            Dim Nsmgr As New XmlNamespaceManager(Doc.NameTable)
            Nsmgr.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/")
            Nsmgr.AddNamespace("ns", "http://www.iata.org/IATA/EDIST/2017.2")

            Dim OriginDestinationsListNode As XmlNode = Doc.SelectSingleNode("//ns:AirShoppingRQ/ns:CoreQuery/ns:OriginDestinations", Nsmgr)

            If OriginDestinationsListNode IsNot Nothing Then
                ' Limpiar el contenido actual del nodo OriginDestinations
                OriginDestinationsListNode.RemoveAll()

                Dim OriginDestinationNode As XmlElement = Doc.CreateElement("OriginDestination", OriginDestinationsListNode.NamespaceURI)

                ' Crear el nodo Departure
                Dim departureNode As XmlElement = Doc.CreateElement("Departure", OriginDestinationNode.NamespaceURI)

                ' Crear el nodo AirportCode para Departure
                Dim departureAirportCodeNode As XmlElement = Doc.CreateElement("AirportCode", departureNode.NamespaceURI)
                departureAirportCodeNode.InnerText = Model.AirportCode

                ' Crear el nodo Date para Departure
                Dim departureDateNode As XmlElement = Doc.CreateElement("Date", departureNode.NamespaceURI)
                departureDateNode.InnerText = Model.DepartureDate

                ' Agregar AirportCode y Date al nodo Departure
                departureNode.AppendChild(departureAirportCodeNode)
                departureNode.AppendChild(departureDateNode)

                ' Crear el nodo Arrival
                Dim arrivalNode As XmlElement = Doc.CreateElement("Arrival", OriginDestinationNode.NamespaceURI)

                ' Crear el nodo AirportCode para Arrival
                Dim arrivalAirportCodeNode As XmlElement = Doc.CreateElement("AirportCode", arrivalNode.NamespaceURI)
                arrivalAirportCodeNode.InnerText = Model.DestinationCode

                ' Agregar AirportCode y Date al nodo Arrival
                arrivalNode.AppendChild(arrivalAirportCodeNode)

                ' Agregar Departure y Arrival al nodo OriginDestination
                OriginDestinationNode.AppendChild(departureNode)
                OriginDestinationNode.AppendChild(arrivalNode)

                ' Agregar el nodo OriginDestination al nodo OriginDestinations
                OriginDestinationsListNode.AppendChild(OriginDestinationNode)

                If Model.TypeTrip = "R" Then

                    Dim OriginDestinationNodeR As XmlElement = Doc.CreateElement("OriginDestination", OriginDestinationsListNode.NamespaceURI)

                    ' Crear el nodo Departure
                    Dim departureNodeR As XmlElement = Doc.CreateElement("Departure", OriginDestinationNodeR.NamespaceURI)

                    ' Crear el nodo AirportCode para Departure
                    Dim departureAirportCodeNodeR As XmlElement = Doc.CreateElement("AirportCode", departureNodeR.NamespaceURI)
                    departureAirportCodeNodeR.InnerText = Model.DestinationCode

                    ' Crear el nodo Date para Departure
                    Dim departureDateNodeR As XmlElement = Doc.CreateElement("Date", departureNodeR.NamespaceURI)
                    departureDateNodeR.InnerText = Model.ReturnDate

                    ' Agregar AirportCode y Date al nodo Departure
                    departureNodeR.AppendChild(departureAirportCodeNodeR)
                    departureNodeR.AppendChild(departureDateNodeR)

                    ' Crear el nodo Arrival
                    Dim arrivalNodeR As XmlElement = Doc.CreateElement("Arrival", OriginDestinationNodeR.NamespaceURI)

                    ' Crear el nodo AirportCode para Arrival
                    Dim arrivalAirportCodeNodeR As XmlElement = Doc.CreateElement("AirportCode", arrivalNodeR.NamespaceURI)
                    arrivalAirportCodeNodeR.InnerText = Model.AirportCode

                    ' Agregar AirportCode y Date al nodo Arrival
                    arrivalNodeR.AppendChild(arrivalAirportCodeNodeR)

                    ' Agregar Departure y Arrival al nodo OriginDestination
                    OriginDestinationNodeR.AppendChild(departureNodeR)
                    OriginDestinationNodeR.AppendChild(arrivalNodeR)

                    ' Agregar el nodo OriginDestination al nodo OriginDestinations
                    OriginDestinationsListNode.AppendChild(OriginDestinationNodeR)
                End If

            Else
                Throw New Exception("El nodo OriginDestinations no fue encontrado en el XML.")
            End If

            ' Encontrar el nodo PassengerList usando el espacio de nombres
            Dim PassengerListNode As XmlNode = Doc.SelectSingleNode("//ns:AirShoppingRQ/ns:DataLists/ns:PassengerList", Nsmgr)

            If PassengerListNode IsNot Nothing Then
                ' Limpiar el contenido actual del nodo PassengerList
                PassengerListNode.RemoveAll()

                ' Crear una lista de pasajeros nueva
                For i As Integer = 1 To Model.NumCompanion
                    ' Crear el nodo Passenger
                    Dim PassengerNode As XmlElement = Doc.CreateElement("Passenger", PassengerListNode.NamespaceURI)
                    PassengerNode.SetAttribute("PassengerID", "ADULT_" & i.ToString("00"))

                    ' Crear el nodo PTC (tipo de pasajero, ADT es adulto)
                    Dim PtcNode As XmlElement = Doc.CreateElement("PTC", PassengerNode.NamespaceURI)
                    PtcNode.InnerText = "ADT" ' Adult Passenger

                    ' Agregar el nodo PTC al pasajero
                    PassengerNode.AppendChild(PtcNode)

                    ' Agregar el pasajero a la lista de pasajeros
                    PassengerListNode.AppendChild(PassengerNode)
                Next
            Else
                Throw New Exception("El nodo PassengerList no fue encontrado en el XML.")
            End If

            ' Devolver el XML modificado como cadena
            Return doc.OuterXml
        End Function

        Public Function ProcessOffers(xmlString As String, NgoId As Integer) As List(Of OfferModel)

            Dim ListModel As New List(Of OfferModel)
            Dim OutPutModel As New OfferModel()

            ' Cargar el XML en un XmlDocument
            Dim doc As New XmlDocument()
            doc.LoadXml(xmlString)

            ''Variable del modelo
            Dim ResponseID As String = String.Empty
            Dim offerItemID As String = String.Empty
            Dim expirationNodeS As String = String.Empty
            Dim PriceS As String = String.Empty
            Dim CurrencyS As String = String.Empty
            Dim cabinTypeNodeS As String = String.Empty
            Dim segmentCount As Integer = 0
            Dim DepartureDate As String = String.Empty
            Dim ReturnDate As String = String.Empty

            ' Crear un XmlNamespaceManager para manejar los espacios de nombres
            Dim nsmgr As New XmlNamespaceManager(doc.NameTable)
            nsmgr.AddNamespace("ns", "http://www.iata.org/IATA/EDIST/2017.2")

            ' Recuperar el ResponseID (asumiendo que está en un nodo superior)
            Dim responseIDNode As XmlNode = doc.SelectSingleNode("//ns:ResponseID", nsmgr)
            If responseIDNode IsNot Nothing Then
                ResponseID = responseIDNode.InnerText
            Else
                Console.WriteLine("ResponseID no encontrado.")
            End If

            ' Seleccionar todos los nodos Offer
            Dim offerNodes As XmlNodeList = doc.SelectNodes("//ns:OffersGroup/ns:AirlineOffers/ns:Offer", nsmgr)
            Dim flightsegmnet As XmlNodeList = doc.SelectNodes("//ns:DataLists/ns:FlightSegmentList/ns:FlightSegment", nsmgr)

            ' Recorrer cada Offer
            For Each offerNode As XmlNode In offerNodes
                ' Extraer el OfferID
                Dim offerID As String = offerNode.Attributes("OfferID").Value
                Console.WriteLine("OfferID: " & offerID)

                ' Extraer OfferExpirationDateTime
                Dim expirationNode As XmlNode = offerNode.SelectSingleNode("ns:OfferExpirationDateTime", nsmgr)
                If expirationNode IsNot Nothing Then
                    expirationNodeS = expirationNode.InnerText
                End If

                ' Extraer SimpleCurrencyPrice
                Dim priceNode As XmlNode = offerNode.SelectSingleNode("ns:TotalPrice/ns:SimpleCurrencyPrice", nsmgr)
                If priceNode IsNot Nothing Then
                    PriceS = priceNode.InnerText
                    CurrencyS = priceNode.Attributes("Code").Value
                End If

                ' Extraer cada OfferItem y sus detalles
                Dim offerItemNodes As XmlNodeList = offerNode.SelectNodes("ns:OfferItem", nsmgr)

                If offerItemNodes.Count = 0 Then
                    Console.WriteLine("No se encontraron OfferItem nodes.")
                Else
                    For Each offerItemNode As XmlNode In offerItemNodes
                        ' Extraer OfferItemID
                        offerItemID = offerItemNode.Attributes("OfferItemID").Value
                        Console.WriteLine("OfferItemID: " & offerItemID)

                        ' Extraer CabinTypeName dentro de FareComponent
                        Dim cabinTypeNode As XmlNode = offerItemNode.SelectSingleNode("ns:FareDetail/ns:FareComponent/ns:FareBasis/ns:CabinType/ns:CabinTypeName", nsmgr)
                        If cabinTypeNode IsNot Nothing Then
                            cabinTypeNodeS = cabinTypeNode.InnerText
                        Else
                            Console.WriteLine("CabinTypeName no encontrado.")
                        End If

                        ' Contar los elementos dentro de SegmentRefs
                        Dim segmentRefsNodes As XmlNodeList = offerItemNode.SelectNodes("ns:FareDetail/ns:FareComponent/ns:SegmentRefs", nsmgr)
                        Dim contador As Integer = 1

                        For Each segmentRefNode As XmlNode In segmentRefsNodes
                            Dim segments As String = segmentRefNode.InnerText
                            segmentCount += segments.Split(" "c).Length

                            Dim elementos As String() = segments.Split(" "c) ' Separa el string por espacios
                            Dim valor As String = elementos.ElementAt(0)

                            Dim dateTimeValue As String = String.Empty

                            ' Iterar sobre cada FlightSegment
                            For Each segment As XmlNode In flightsegmnet
                                ' Verificar si el SegmentKey coincide con el valor IB031220241112MEXMAD
                                If segment.Attributes("SegmentKey").Value = valor Then
                                    ' Buscar los nodos de Departure -> Date y Time
                                    Dim departureNode As XmlNode = segment.SelectSingleNode("ns:Departure", GetNamespaceManager(doc))

                                    ' Obtener el valor de la fecha y la hora
                                    Dim flightDate As String = departureNode.SelectSingleNode("ns:Date", GetNamespaceManager(doc)).InnerText
                                    Dim flightTime As String = departureNode.SelectSingleNode("ns:Time", GetNamespaceManager(doc)).InnerText

                                    ' Combinar fecha y hora en una sola variable
                                    dateTimeValue = flightDate & " " & flightTime
                                    If contador = 1 Then
                                        DepartureDate = dateTimeValue
                                    Else
                                        ReturnDate = dateTimeValue
                                    End If
                                    Exit For ' Salir del bucle una vez encontrado el segmento
                                End If
                            Next
                            contador += 1
                        Next
                    Next
                End If

                OutPutModel = New OfferModel()

                OutPutModel.NgoId = NgoId
                OutPutModel.ResponseId = ResponseID
                OutPutModel.OfferId = offerID
                OutPutModel.OfferItemId = offerItemID
                OutPutModel.Expiration = expirationNodeS
                OutPutModel.OriginPrice = PriceS
                OutPutModel.Currency = CurrencyS
                OutPutModel.Scales = CStr(segmentCount)
                OutPutModel.CabinType = cabinTypeNodeS
                OutPutModel.DepartureDate = DepartureDate
                OutPutModel.ReturnDate = ReturnDate

                ListModel.Add(OutPutModel)

                segmentCount = 0
            Next

            Return ListModel

        End Function

        Private Function GetNamespaceManager(xmlDoc As XmlDocument) As XmlNamespaceManager
            Dim nsManager As New XmlNamespaceManager(xmlDoc.NameTable)
            nsManager.AddNamespace("ns", "http://www.iata.org/IATA/EDIST/2017.2")
            Return nsManager
        End Function
    End Class
End Namespace