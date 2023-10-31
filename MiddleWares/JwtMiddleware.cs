using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtMiddleware> _logger;

    public JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation("JWT Middleware invoked.");

        var token = context.Request.Cookies["token"];
        _logger.LogInformation($"Token from cookie: {token}");

        if (token != null)
            AttachUserIdToContext(context, token);

        await _next(context);
    }

    private void AttachUserIdToContext(HttpContext context, string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken != null)
            {
                _logger.LogInformation($"JWT Token parsed successfully. Token: {jwtToken}");

                // Change here: Use the "unique_name" claim to extract the user's email
                var userEmailClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "unique_name");
                if (userEmailClaim != null)
                {
                    _logger.LogInformation($"User email extracted from token: {userEmailClaim.Value}");
                    context.Items["UserEmail"] = userEmailClaim.Value;
                }
                else
                {
                    _logger.LogWarning("User email claim not found in token.");
                }
            }
            else
            {
                _logger.LogWarning("Failed to parse JWT Token.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing JWT token in middleware.");
        }
    }

}
