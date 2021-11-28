using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Shard.Uni.Handlers
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock
        ) : base(options, logger, encoder, clock) { }
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string header = this.Context.Request.Headers["Authorization"];

            // Probably an admin
            if(header != null)
            {
                // Header cleaning, trying to parse this format "Basic BASE64_STRING"
                header = header.Trim();
                header = header.Split(' ').Last();

                // Decode Base64 string
                string auth = Encoding.UTF8.GetString(Convert.FromBase64String(header));
                if(auth != null)
                {
                    if (auth == "admin:password")
                    {
                        return Task.Run(() => AuthenticateResult.Success(
                            new AuthenticationTicket(
                                new ClaimsPrincipal(
                                    new List<ClaimsIdentity>
                                    {
                                        new ClaimsIdentity(
                                            new List<Claim>
                                            {
                                                new Claim(ClaimTypes.Role, Constants.Roles.Admin)
                                            }
                                        )
                                    }
                                ),
                                "Basic"
                            )
                        ));
                    }
                    else
                    {
                        string pattern = @"shard-(?<nomDuShard>.*):(?<clefPartagee>.*)";
                        Match match = Regex.Match(auth, pattern);
                        if (match.Success)
                        {
                            GroupCollection groups = match.Groups;
                            string shardName = groups["nomDuShard"].Value;
                            string clefPartagee = groups["clefPartagee"].Value;
                            return Task.Run(() => AuthenticateResult.Success(
                                new AuthenticationTicket(
                                    new ClaimsPrincipal(
                                        new List<ClaimsIdentity>
                                        {
                                            new ClaimsIdentity(
                                                new List<Claim>
                                                {
                                                    new Claim(ClaimTypes.Role, Constants.Roles.Shard),
                                                    new Claim(ClaimTypes.Name, shardName)
                                                }
                                            )
                                        }
                                    ),
                                    "Basic"
                                )
                            ));
                        }
                    }
                }
            }

            // Unauthenticated
            return Task.Run(() => AuthenticateResult.Success(
                new AuthenticationTicket(
                    new ClaimsPrincipal(
                        new List<ClaimsIdentity>
                        {
                            new ClaimsIdentity(
                                new List<Claim>
                                {
                                    new Claim(ClaimTypes.Role, Constants.Roles.User)
                                }
                            )
                        }
                    ),
                    "Basic"
                )
            ));

        }
    }
}
