using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;

namespace Kartverket.Metadatakatalog.Swagger
{
    /// <summary>
    /// Custom schema filter to handle complex model binding scenarios that cause Swagger generation issues
    /// </summary>
    public class CustomModelBinderSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            try
            {
                // Handle SearchParameters and other complex types that use custom model binders
                if (context.Type.Name.Contains("SearchParameters") || 
                    context.Type.Namespace?.Contains("ModelBinders") == true)
                {
                    // Simplify the schema for custom model-bound types
                    schema.Type = "object";
                    schema.Properties?.Clear();
                    schema.AdditionalPropertiesAllowed = true;
                    schema.Description = $"Complex parameter object (Type: {context.Type.Name})";
                }
                
                // Handle IFormCollection and other problematic types
                if (context.Type.Name == "IFormCollection" || 
                    context.Type == typeof(Microsoft.AspNetCore.Http.IFormCollection))
                {
                    schema.Type = "object";
                    schema.Properties?.Clear();
                    schema.AdditionalPropertiesAllowed = true;
                    schema.Description = "Form data collection";
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the schema generation
                System.Diagnostics.Debug.WriteLine($"? CustomModelBinderSchemaFilter error: {ex.Message}");
                
                // Fallback to a simple object schema
                schema.Type = "object";
                schema.Properties?.Clear();
                schema.AdditionalPropertiesAllowed = true;
                schema.Description = "Complex object";
            }
        }
    }
}