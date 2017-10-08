// ==========================================================================
//  GoogleIdentityServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Squidex.Config.Identity
{
    public static class GoogleIdentityServices
    {
        public static AuthenticationBuilder AddMyGoogleAuthentication(this AuthenticationBuilder authenticationBuilder, MyIdentityOptions options)
        {
            if (options.IsGoogleAuthConfigured())
            {
                authenticationBuilder.AddGoogle(google =>
                {
                    google.ClientId = options.GoogleClient;
                    google.ClientSecret = options.GoogleSecret;
                    google.Events = new GoogleHandler();
                });
            }

            return authenticationBuilder;
        }
    }
}
