namespace DealershipChatBot;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using DealerWebPageBlazorWebAppShared.Resources;

public class TokenHelper
{
  private readonly AppSettings _appSettings;
  public string signingkey { get; set; } = string.Empty;
  public string validationKey { get; set; } = string.Empty;

  public TokenHelper(AppSettings appSettings)
  {
    _appSettings = appSettings;
  }

  /// <summary>
  /// Generates a signed JWT token.
  /// </summary>
  public string GenerateJWTToken(IEnumerable<Claim> claims)
  {
    if (claims.Any() == false)
      throw new Exception("Claims for the Token must be specified");

    var signingCredentials = GetTokenSigningCredentials();

    JwtSecurityTokenHandler jwtSecurityTokenHandler = new();

    var tokenTypeAsString = claims.FirstOrDefault(c => c.Type == "tokenType")?.Value ?? TokenTypeValues.Unknown.ToString();
    var tokenType = Enum.Parse<TokenTypeValues>(tokenTypeAsString);

    var expirationDateTime = DateTime.Now.AddMinutes(_appSettings.DealershipChatBotConfiguration.TokenExpirationTimeSpan.TotalMinutes);
    if (tokenType == TokenTypeValues.DealershipToken)
    {
      expirationDateTime = DateTime.MaxValue;
    }

    var jwtToken = new JwtSecurityToken(
        issuer: _appSettings.DealershipChatBotConfiguration.HostURL,
        audience: _appSettings.DealershipChatBotConfiguration.AudienceURL,
        claims: claims,
        expires: expirationDateTime,
        signingCredentials: signingCredentials);

    var jwtTokenString = jwtSecurityTokenHandler.WriteToken(jwtToken);

    //an assertion that after creation it can be read - may be unnecessary
    var canReadJwtTokenString = jwtSecurityTokenHandler.CanReadToken(jwtTokenString);
    if (canReadJwtTokenString == false)
    {
      throw new Exception("Token cannot be read");
    }

    //todo: more research on this property
    var validToken = jwtSecurityTokenHandler.CanValidateToken;
    if (validToken == false)
    {
      throw new Exception("Token cannot be validated");
    }

    //an assertion that the token will validate against the Validation tests
    var jwtTokenValidationParameters = GetTokenValidationParameters();

    var principal = jwtSecurityTokenHandler.ValidateToken(jwtTokenString, jwtTokenValidationParameters, out SecurityToken validatedToken);

    return jwtSecurityTokenHandler.WriteToken(jwtToken);
  }

  internal SigningCredentials GetTokenSigningCredentials()
  {
    var jwtSecretGeneratedKey = GenerateKey(_appSettings.DealershipChatBotConfiguration.JwtSecret);
    var signingCredentials = new SigningCredentials(jwtSecretGeneratedKey, SecurityAlgorithms.HmacSha256);

    return signingCredentials;
  }

  internal EncryptingCredentials GetEncryptingCredentials()
  {
    var encryptionKey = GenerateKey(_appSettings.DealershipChatBotConfiguration.EncryptionKey);
    var encryptingCredentials = new EncryptingCredentials(
        encryptionKey,
        SecurityAlgorithms.Aes256KW,
        SecurityAlgorithms.Aes256CbcHmacSha512);

    return encryptingCredentials;
  }

  /// <summary>
  /// Encrypts a signed JWT token.
  /// </summary>
  public string EncryptJwtToken(string jwt)
  {
    var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
    if (!jwtSecurityTokenHandler.CanReadToken(jwt))
    {
      throw new ArgumentException("Invalid JWT format", nameof(jwt));
    }

    var signingCredentials = GetTokenSigningCredentials();

    var encryptingCredentials = GetEncryptingCredentials();

    var jwtSecurityToken = jwtSecurityTokenHandler.ReadJwtToken(jwt);

    var securityTokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(jwtSecurityToken.Claims),
      Issuer = jwtSecurityToken.Issuer,
      Audience = jwtSecurityToken.Audiences.FirstOrDefault(),
      Expires = jwtSecurityToken.ValidTo,
      EncryptingCredentials = encryptingCredentials,
      SigningCredentials = signingCredentials,
      TokenType = jwtSecurityToken.Header.Typ
    };

    var encryptedToken = jwtSecurityTokenHandler.CreateJwtSecurityToken(securityTokenDescriptor);
    return jwtSecurityTokenHandler.WriteToken(encryptedToken);
  }

  /// <summary>
  /// Decrypts and validates an encrypted JWT token.
  /// </summary>
  public ClaimsPrincipal? DecryptJWTTokenForClaimsPrincipal(string encryptedAsciiToken)
  {
    var jwtSigningCredentials = GetTokenSigningCredentials();
    var encryptingCredentials = GetEncryptingCredentials();

    var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

    if (jwtSecurityTokenHandler.CanReadToken(encryptedAsciiToken) == false)
    {
      throw new ArgumentException("Invalid encrypted JWT format", nameof(encryptedAsciiToken));
    }

    var validationParameters = GetTokenValidationParameters();


    try
    {
      var claimPrinciple = jwtSecurityTokenHandler.ValidateToken(encryptedAsciiToken, validationParameters, out SecurityToken validatedToken);

      if (validatedToken is not JwtSecurityToken jwtToken ||
          !jwtToken.Header.Alg.Equals(SecurityAlgorithms.Aes256KW, StringComparison.OrdinalIgnoreCase) ||
          !jwtToken.Header.ContainsKey("enc") ||
          jwtToken.Header["enc"]?.ToString() != SecurityAlgorithms.Aes256CbcHmacSha512)
      {
        throw new SecurityTokenException("Invalid token algorithms in header");
      }

      return claimPrinciple;
    }
    catch (SecurityTokenException ex)
    {
      Console.WriteLine($"Token validation failed: {ex.Message}");
      return null;
    }
  }

  /// <summary>
  /// Generates a symmetric security key from a base string.
  /// </summary>
  public static SymmetricSecurityKey GenerateKey(string keyValue)
  {
    var keyLength = keyValue.Length;
    if (keyLength < 32 || keyLength > 64)
    {
      throw new ArgumentOutOfRangeException(nameof(keyValue), "Length must be between 32 and 64 bytes.");
    }

    var keyBytes = Encoding.ASCII.GetBytes(keyValue);

    if (keyBytes.Length != keyLength)
    {
      throw new ArgumentException($"The key must be {keyLength} bytes long.");
    }

    return new SymmetricSecurityKey(keyBytes);
  }

  internal TokenValidationParameters GetTokenValidationParameters()
  {
    return new TokenValidationParameters
    {
      ValidateIssuer = true,
      ValidIssuer = _appSettings.DealershipChatBotConfiguration.HostURL,

      ValidateAudience = true,
      //todo: decrease the scope of the audience to the dealerId specified in the token, rather than the web host that provides the token
      ValidAudience = _appSettings.DealershipChatBotConfiguration.AudienceURL,

      ValidateLifetime = true,

      ValidateIssuerSigningKey = true,
      IssuerSigningKey = GetTokenSigningCredentials().Key,

      TokenDecryptionKey = GetEncryptingCredentials().Key,

      ClockSkew = _appSettings.DealershipChatBotConfiguration.TokenExpirationTimeSpan
    };
  }

  public static string Base64Decode(string base64EncodedData)
  {
    var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
    return Encoding.UTF8.GetString(base64EncodedBytes);
  }

  public static string Base64Encode(string plainText)
  {
    var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
    return Convert.ToBase64String(plainTextBytes);
  }

  public IEnumerable<Claim> GenerateClaims(TokenTypeValues tokenTypeValue, string dealershipId, string dealerName, string clientIPAddress)
  {
    return
      [
        new Claim(JWTTokenClaimNameValues.DealershipId.ToString(), dealershipId),
        new Claim(JWTTokenClaimNameValues.DealerName.ToString(), dealerName),
        new Claim(JWTTokenClaimNameValues.TokenType.ToString(), tokenTypeValue.ToString()),
        new Claim(JWTTokenClaimNameValues.ClientIPAddress.ToString(), clientIPAddress)
      ];
  }

  public string? GetClaimValue(ClaimsPrincipal user, string claimName)
  {
    return GetClaimValueHelper(user, claimName);
  } 

  public static string? GetClaimValueHelper(ClaimsPrincipal user, string claimName)
  {
    // Find the first claim with the specified name
    var claim = user?.FindFirst(claimName);

    // Return the claim's value or null if not found
    return claim?.Value;
  }
}
