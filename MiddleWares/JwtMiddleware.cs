using System.IdentityModel.Tokens.Jwt;

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
                var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "nameid")?.Value;
                var userEmailClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
                var userNameClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "unique_name")?.Value;

                if (!string.IsNullOrWhiteSpace(userIdClaim))
                {
                    context.Items["UserId"] = userIdClaim;
                }
                else
                {
                    _logger.LogWarning("User ID claim not found in token.");
                }

                if (!string.IsNullOrWhiteSpace(userEmailClaim))
                {
                    context.Items["UserEmail"] = userEmailClaim;
                }
                else
                {
                    _logger.LogWarning("User email claim not found in token.");
                }
                if (!string.IsNullOrWhiteSpace(userNameClaim))
                {
                    context.Items["UserName"] = userNameClaim;
                }
                else
                {
                    _logger.LogWarning("User Name claim not found in token.");
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