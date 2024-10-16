Imports System.IdentityModel.Tokens.Jwt
Imports System.Security.Claims
Imports Microsoft.IdentityModel.Tokens

Public Class TokenManager
    Public Function GenerateToken(ByVal username As String, ByVal uSecret As String) As TokenOutputModel

        Dim minutesTimeout As Integer = CInt(ConfigurationManager.AppSettings.[Get]("TokenExpiration"))
        Dim key As Byte() = Convert.FromBase64String(uSecret)
        Dim securityKey As SymmetricSecurityKey = New SymmetricSecurityKey(key)

        Dim descriptor As SecurityTokenDescriptor = New SecurityTokenDescriptor With {
            .Subject = New ClaimsIdentity({New Claim(ClaimTypes.Name, username)}),
            .Expires = DateTime.UtcNow.AddMinutes(minutesTimeout),
            .SigningCredentials = New SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
        }

        Dim handler As JwtSecurityTokenHandler = New JwtSecurityTokenHandler()
        Dim token As JwtSecurityToken = handler.CreateJwtSecurityToken(descriptor)

        Dim dt As TokenOutputModel = New TokenOutputModel()
        dt.Token = handler.WriteToken(token)
        dt.Expiration = descriptor.Expires.ToString()
        dt.UserName = descriptor.Subject.Name

        Return dt
    End Function

    Public Function GetPrincipal(ByVal token As String, ByVal Secret As String) As ClaimsPrincipal
        Try
            Dim tokenHandler As JwtSecurityTokenHandler = New JwtSecurityTokenHandler()
            Dim jwtToken As JwtSecurityToken = CType(tokenHandler.ReadToken(token), JwtSecurityToken)

            If jwtToken Is Nothing Then
                Return Nothing
            End If

            Dim key As Byte() = Convert.FromBase64String(Secret)
            Dim parameters As TokenValidationParameters = New TokenValidationParameters() With {
                .RequireExpirationTime = True,
                .ValidateIssuer = False,
                .ValidateAudience = False,
                .IssuerSigningKey = New SymmetricSecurityKey(key)
            }

            Dim securityToken As SecurityToken
            Dim principal As ClaimsPrincipal = tokenHandler.ValidateToken(token, parameters, securityToken)

            Return principal
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function ValidateToken(ByVal token As String, ByVal User As String) As String
        Dim DataAcc As New DataAccess(ConfigurationManager.ConnectionStrings("ConnStr").ConnectionString)
        Dim username As String = Nothing
        Dim Hsh As Hashtable = New Hashtable()
        Hsh.Add("@USER_NAME", User)
        Hsh.Add("@USER_SECRET", DBNull.Value)
        Dim uSecret As String = DataAcc.ReturnString("API_SEL_SECRET", Hsh)

        Dim principal As ClaimsPrincipal = GetPrincipal(token, uSecret)

        If principal Is Nothing Then
            Return Nothing
        End If

        Dim identity As ClaimsIdentity = Nothing

        Try
            identity = CType(principal.Identity, ClaimsIdentity)
        Catch __unusedNullReferenceException1__ As NullReferenceException
            Return Nothing
        End Try

        Dim usernameClaim As Claim = identity.FindFirst(ClaimTypes.Name)
        username = usernameClaim.Value
        Return username
    End Function
End Class
