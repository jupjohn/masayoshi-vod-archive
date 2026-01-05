using Intellenum;

namespace Masayoshi.Archive.Authentication.Twitch;

public static class TwitchAuthConstants
{
    public const string AuthenticationScheme = "TwitchUser";

    public const string BackChannelHttpClientKey = "twitch-oauth-backchannel";

    public static class Claims
    {
        public const string Id = "twitch:user_id";
        public const string Login = "twitch:login";
        public const string DisplayName = "twitch:display_name";
    }
}

[Intellenum<string>]
[Member("Administrator", "admin")]
public partial class TwitchUserRole;
