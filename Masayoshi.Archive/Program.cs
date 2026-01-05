var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "Hello world!");

app.Run();

// TODO:
//   - fast endpoints
//   - static site files + htmx
//   - efcore sqlite
//   - twitch auth
//   - admin page
//   - disk metadata & src loading
//   - VOD listing page
//   - video serving (byte range + auth etc.)
//   - VOD player page (videojs.org)
//   - nice to haves:
//      - remembering playback
