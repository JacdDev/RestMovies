namespace Movies.Api.Auth;

public static class IdentityExtensions
{
    public static Guid? GetUserGuid(this HttpContext context)
    {
        var userId = context.User.Claims.SingleOrDefault(x => x.Type == "userid");

        if (Guid.TryParse(userId?.Value, out var userGuid))
        {
            return userGuid;
        }
        
        return null;
    }
}