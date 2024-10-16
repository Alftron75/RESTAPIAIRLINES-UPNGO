Imports System.Data.SqlClient

Public Class DataAccess
    Public ConnStr As String
    Public Cnx As SqlConnection
    Private Cmnd As SqlCommand
    Private SqlAdapter As SqlDataAdapter
    Private DtSt As DataSet
    Private DtTbl As DataTable

    Public Sub New(CnxStr As String)
        ConnStr = CnxStr
    End Sub

    Public Function ReturnDataSet(ByVal StoredProcedure As String) As DataSet
        DtSt = New DataSet()
        Cnx = New SqlConnection(ConnStr)
        Cmnd = New SqlCommand(StoredProcedure, Cnx)
        Cmnd.CommandTimeout = 0
        Cmnd.CommandType = CommandType.StoredProcedure
        SqlAdapter = New SqlDataAdapter(Cmnd)
        SqlAdapter.Fill(DtSt)
        Cnx.Dispose()
        Cmnd.Dispose()
        SqlAdapter.Dispose()

        Return DtSt
    End Function

    Public Function ReturnDataSet(ByVal StoredProcedure As String, ByVal HashT As Hashtable) As DataSet
        DtSt = New DataSet()
        Cnx = New SqlConnection(ConnStr)
        Cmnd = New SqlCommand(StoredProcedure, Cnx)
        Cmnd.CommandType = CommandType.StoredProcedure
        Cmnd.CommandTimeout = 0

        For Each ItemRow As DictionaryEntry In HashT
            Cmnd.Parameters.AddWithValue(ItemRow.Key.ToString(), ItemRow.Value)
        Next

        SqlAdapter = New SqlDataAdapter(Cmnd)
        SqlAdapter.Fill(DtSt)
        Cnx.Dispose()
        Cmnd.Parameters.Clear()
        Cmnd.Dispose()
        SqlAdapter.Dispose()

        Return DtSt
    End Function

    Public Sub SaveUpdate(ByVal StoredProcedure As String, ByVal HashT As Hashtable)
        Cnx = New SqlConnection(ConnStr)
        Cmnd = New SqlCommand(StoredProcedure, Cnx)
        Cmnd.CommandType = CommandType.StoredProcedure
        Cmnd.CommandTimeout = 0

        For Each ItemRow As DictionaryEntry In HashT
            Cmnd.Parameters.AddWithValue(ItemRow.Key.ToString(), ItemRow.Value)
        Next

        Cnx.Open()
        Cmnd.ExecuteScalar()
        Cnx.Close()
        Cnx.Dispose()
        Cmnd.Parameters.Clear()
        Cmnd.Dispose()
    End Sub

    Public Function ReturnString(ByVal SqlQuery As String) As String
        Dim StrResult As String = String.Empty
        Cnx = New SqlConnection(ConnStr)
        Cmnd = New SqlCommand(SqlQuery, Cnx)
        Dim SqlReader As SqlDataReader
        Cmnd.CommandType = CommandType.Text
        Cmnd.CommandTimeout = 0
        Cnx.Open()
        SqlReader = Cmnd.ExecuteReader()

        While SqlReader.Read()
            StrResult = SqlReader(0).ToString()
        End While

        SqlReader.Close()
        Cnx.Close()
        Cnx.Dispose()
        Cmnd.Dispose()

        Return StrResult
    End Function

    Public Function ReturnString(ByVal StoredProcedure As String, ByVal HashT As Hashtable) As String
        Dim StrResult As String = String.Empty
        Cnx = New SqlConnection(ConnStr)
        Cmnd = New SqlCommand(StoredProcedure, Cnx)
        Dim SqlReader As SqlDataReader
        Cmnd.CommandType = CommandType.StoredProcedure
        Cmnd.CommandTimeout = 0

        For Each ItemRow As DictionaryEntry In HashT
            Cmnd.Parameters.AddWithValue(ItemRow.Key.ToString(), ItemRow.Value)
        Next

        Cnx.Open()
        SqlReader = Cmnd.ExecuteReader()

        While SqlReader.Read()
            StrResult = SqlReader(0).ToString()
        End While

        SqlReader.Close()
        Cnx.Close()
        Cnx.Dispose()
        Cmnd.Dispose()

        Return StrResult
    End Function

    Public Function ReturnInteger(ByVal SqlQuery As String) As Int32
        Dim IntResult As Int32 = 0
        Dim SqlReader As SqlDataReader
        Cnx = New SqlConnection(ConnStr)
        Cmnd = New SqlCommand(SqlQuery, Cnx)
        Cmnd.CommandType = CommandType.Text
        Cmnd.CommandTimeout = 0
        Cnx.Open()
        SqlReader = Cmnd.ExecuteReader()

        While SqlReader.Read()
            IntResult = Convert.ToInt32(SqlReader(0))
        End While

        SqlReader.Close()
        Cnx.Close()
        Cnx.Dispose()
        Cmnd.Dispose()

        Return IntResult
    End Function

    Public Function ReturnInteger(ByVal StoredProcedure As String, ByVal HashT As Hashtable) As Int32
        Dim IntResult As Int32 = 0
        Dim SqlReader As SqlDataReader
        Cnx = New SqlConnection(ConnStr)
        Cmnd = New SqlCommand(StoredProcedure, Cnx)
        Cmnd.CommandType = CommandType.StoredProcedure

        If HashT IsNot Nothing Then
            For Each ItemRow As DictionaryEntry In HashT
                Cmnd.Parameters.AddWithValue(ItemRow.Key.ToString(), ItemRow.Value)
            Next
        End If

        Cnx.Open()
        SqlReader = Cmnd.ExecuteReader()

        While SqlReader.Read()
            IntResult = Convert.ToInt32(SqlReader(0))
        End While

        SqlReader.Close()
        Cnx.Close()
        Cnx.Dispose()
        Cmnd.Dispose()

        Return IntResult
    End Function

    Public Function ReturnDataTable(ByVal StoredProcedure As String, ByVal HashT As Hashtable) As DataTable
        DtTbl = New DataTable()
        Cnx = New SqlConnection(ConnStr)
        Cmnd = New SqlCommand(StoredProcedure, Cnx)
        Cmnd.CommandType = CommandType.StoredProcedure
        Cmnd.CommandTimeout = 0

        For Each ItemRow As DictionaryEntry In HashT
            Cmnd.Parameters.AddWithValue(ItemRow.Key.ToString(), ItemRow.Value)
        Next

        SqlAdapter = New SqlDataAdapter(Cmnd)
        SqlAdapter.Fill(DtTbl)
        Cnx.Dispose()
        Cmnd.Parameters.Clear()
        Cmnd.Dispose()
        SqlAdapter.Dispose()

        Return DtTbl
    End Function

    Public Function ReturnDataTable(ByVal StoredProcedure As String) As DataTable
        DtTbl = New DataTable()
        Cnx = New SqlConnection(ConnStr)
        Cmnd = New SqlCommand(StoredProcedure, Cnx)
        Cmnd.CommandType = CommandType.StoredProcedure
        Cmnd.CommandTimeout = 0
        SqlAdapter = New SqlDataAdapter(Cmnd)
        SqlAdapter.Fill(DtTbl)
        Cnx.Dispose()
        Cmnd.Parameters.Clear()
        Cmnd.Dispose()
        SqlAdapter.Dispose()

        Return DtTbl
    End Function

    Public Function ReturnFile(ByVal StoredProcedure As String) As Byte()
        Dim file As Byte()
        Dim SqlReader As SqlDataReader
        Cnx = New SqlConnection(ConnStr)
        Cmnd = New SqlCommand(StoredProcedure, Cnx)
        Cmnd.CommandType = CommandType.StoredProcedure
        Cmnd.CommandTimeout = 0
        Cnx.Open()
        SqlReader = Cmnd.ExecuteReader()

        While SqlReader.Read()
            file = SqlReader(0)
        End While

        SqlReader.Close()
        Cnx.Close()
        Cnx.Dispose()
        Cmnd.Dispose()

        Return file
    End Function

    Public Function ReturnFile(ByVal StoredProcedure As String, ByVal HashT As Hashtable) As Byte()
        Dim file As Byte()
        Dim SqlReader As SqlDataReader
        Cnx = New SqlConnection(ConnStr)
        Cmnd = New SqlCommand(StoredProcedure, Cnx)
        Cmnd.CommandType = CommandType.StoredProcedure
        Cmnd.CommandTimeout = 0

        If HashT IsNot Nothing Then
            For Each ItemRow As DictionaryEntry In HashT
                Cmnd.Parameters.AddWithValue(ItemRow.Key.ToString(), ItemRow.Value)
            Next
        End If

        Cnx.Open()
        SqlReader = Cmnd.ExecuteReader()

        While SqlReader.Read()
            file = SqlReader(0)
        End While

        SqlReader.Close()
        Cnx.Close()
        Cnx.Dispose()
        Cmnd.Dispose()

        Return file
    End Function

    Public Function ReturnBoolean(ByVal StoredProcedure As String) As Boolean
        Dim BolResult As Boolean
        Dim SqlReader As SqlDataReader
        Cnx = New SqlConnection(ConnStr)
        Cmnd = New SqlCommand(StoredProcedure, Cnx)
        Cmnd.CommandType = CommandType.Text
        Cmnd.CommandTimeout = 0
        Cnx.Open()
        SqlReader = Cmnd.ExecuteReader()

        While SqlReader.Read()
            BolResult = Convert.ToBoolean(SqlReader(0))
        End While

        SqlReader.Close()
        Cnx.Close()
        Cnx.Dispose()
        Cmnd.Dispose()

        Return BolResult
    End Function

    Public Function ReturnBoolean(ByVal StoredProcedure As String, ByVal HashT As Hashtable) As Boolean
        Dim BolResult As Boolean
        Dim SqlReader As SqlDataReader
        Cnx = New SqlConnection(ConnStr)
        Cmnd = New SqlCommand(StoredProcedure, Cnx)
        Cmnd.CommandType = CommandType.StoredProcedure

        If HashT IsNot Nothing Then
            For Each ItemRow As DictionaryEntry In HashT
                Cmnd.Parameters.AddWithValue(ItemRow.Key.ToString(), ItemRow.Value)
            Next
        End If

        Cnx.Open()
        SqlReader = Cmnd.ExecuteReader()

        While SqlReader.Read()
            BolResult = Convert.ToBoolean(SqlReader(0))
        End While

        SqlReader.Close()
        Cnx.Close()
        Cnx.Dispose()
        Cmnd.Dispose()

        Return BolResult
    End Function

End Class
