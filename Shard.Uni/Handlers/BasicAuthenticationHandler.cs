using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
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
            if(header != null)
            {
                string auth = Encoding.UTF8.GetString(Convert.FromBase64String(header));
                if(auth != null)
                {
                    if(auth == "admin:password")
                    {
                        IEnumerable<ClaimsIdentity> claims = new List<ClaimsIdentity>() {
                            new ClaimsIdentity()
                        };
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
                }
            }
            return Task.Run(() => AuthenticateResult.Success(
                new AuthenticationTicket(
                    new ClaimsPrincipal(), "Basic"   
                )    
            ));
            
        }
    }
}
