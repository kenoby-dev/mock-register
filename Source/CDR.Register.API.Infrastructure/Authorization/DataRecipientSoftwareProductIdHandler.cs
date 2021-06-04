﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CDR.Register.API.Infrastructure.Authorization
{
    public class DataRecipientSoftwareProductIdHandler : AuthorizationHandler<DataRecipientSoftwareProductIdRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<DataRecipientSoftwareProductIdHandler> _logger;

        public DataRecipientSoftwareProductIdHandler(IHttpContextAccessor httpContextAccessor, ILogger<DataRecipientSoftwareProductIdHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DataRecipientSoftwareProductIdRequirement requirement)
        {
            // Check that authentication was successful before doing anything else
            if (!context.User.Identity.IsAuthenticated)
            {
                return Task.CompletedTask;
            }

            // If user does not have the scope claim, get out of here
            if (!context.User.HasClaim(c => c.Type == "client_id" && c.Issuer == requirement.Issuer))
            {
                _logger.LogError($"Unauthorized request. Access token is missing 'client_id' claim for issuer '{requirement.Issuer}'.");
                return Task.CompletedTask;
            }

            var accessTokenClientId = context.User.FindFirst("client_id")?.Value;
            if (string.IsNullOrWhiteSpace(accessTokenClientId))
            {
                _logger.LogError("Unauthorized request. Access token is missing 'client_id' claim.");
                return Task.CompletedTask;
            }

            string requestDataRecipientProductId = _httpContextAccessor.HttpContext.Request.RouteValues["softwareProductId"]?.ToString();

            // Token ClientId should match the ProductId.
            if (!accessTokenClientId.Equals(requestDataRecipientProductId, System.StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogError($"Unauthorized request. Access token client_id '{accessTokenClientId}' does not match request softwareProductId '{requestDataRecipientProductId}'");
                return Task.CompletedTask;
            }

            // If we get this far all good
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
