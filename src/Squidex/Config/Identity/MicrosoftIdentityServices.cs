// ==========================================================================
//  MicrosoftIdentityServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Squidex.Config.Identity
{
    public static class MicrosoftIdentityServices
    {
        public static AuthenticationBuilder AddMyMicrosoftAuthentication(this AuthenticationBuilder authenticationBuilder, MyIdentityOptions options)
        {
            if (options.IsMicrosoftAuthConfigured())
            {
                authenticationBuilder.AddMicrosoftAccount(microsoft =>
                {
                    microsoft.ClientId = options.MicrosoftClient;
                    microsoft.ClientSecret = options.MicrosoftSecret;
                    microsoft.Events = new MicrosoftHandler();
                });
            }

            return authenticationBuilder;
        }
    }
}
