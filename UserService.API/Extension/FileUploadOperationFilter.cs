using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace UserService.API.Extension
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileParams = context.MethodInfo.GetParameters()
                .Where(p => p.ParameterType == typeof(IFormFile) || p.ParameterType == typeof(IEnumerable<IFormFile>))
                .ToList();

            if (!fileParams.Any())
                return;

            // Clear existing parameters
            operation.Parameters?.Clear();

            // Create multipart/form-data request body
            var properties = new Dictionary<string, OpenApiSchema>();
            foreach (var param in fileParams)
            {
                properties[param.Name ?? "file"] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                };
            }

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = properties,
                            Required = new HashSet<string>(fileParams.Select(p => p.Name ?? "file"))
                        }
                    }
                }
            };
        }
    }
}