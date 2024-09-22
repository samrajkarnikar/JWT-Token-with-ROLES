using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Security.Cryptography.Xml;
using Microsoft.AspNetCore.Authorization;


namespace WebAPI.Filter
{
    //AuthResponseOperationFilter
    // to see if api requires athorization
    // requires - add JWT token 
    // adds 401 unauthorized response to indicate authentication is needed.
    public class AuthResponseOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {

            //This line retrieves all AuthorizeAttribute attributes applied to the method or the controller (class) containing the method.
            //AuthorizeAttribute is used to specify that an action or controller requires authorization.
            var authAttributes = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .OfType<AuthorizeAttribute>();

            if (authAttributes.Any())
            {
                var securityRequirement = new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"//BearerToken Authentication(Jwt)
                            }
                        },
                        new List<string>()
                    }
                };

                //specifies this operation requires Bearer Token Authentication
                operation.Security = new List<OpenApiSecurityRequirement>
                        {
                            securityRequirement
                        };
                operation.Responses.Add("401", new OpenApiResponse
                {
                    Description = "Unauthorized"
                });
            }
        }
    }
}

