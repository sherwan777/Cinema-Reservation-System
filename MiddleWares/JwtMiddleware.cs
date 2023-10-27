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
        var token = context.Request.Cookies["token"];

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
                var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "userId");
                if (userIdClaim != null)
                {
                    // Set the user ID in the HttpContext.Items to use it later in the request
                    context.Items["UserId"] = userIdClaim.Value;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing JWT token in middleware.");
            // You can also set an error response here if needed
        }
    }
}
