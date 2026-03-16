# Customizing Example Values for SearchParameters

## Overview

Here are the implemented approaches to customize example values for your `SearchParameters` model in the Kartverket Metadata Catalog API:

## ? Implemented Solutions

### 1. **Enhanced XML Documentation Examples**

I've updated the `SearchParameters` model with realistic example values:

```csharp
/// <summary>
/// The text to search for in the metadata catalogue
/// </summary>
/// <example>Norge kartdata</example>
public string text { get; set; }

/// <summary>
/// Maximum number of results to return per page. Range: 1-1000, Default is 10.
/// </summary>
/// <example>20</example>
public int limit { get; set; } = 10;

/// <summary>
/// Field to order results by. Valid values: score, title, title_desc, organization, organization_desc, newest, updated, popularMetadata. Default is 'score'.
/// </summary>
/// <example>title</example>
public string orderby { get; set; } = "score";

/// <summary>
/// Facet filters to apply. Use array format: facets[0]name=type&facets[0]value=dataset
/// Available facet names: type, theme, organization, nationalinitiative, DistributionProtocols, area, dataaccess, spatialscope
/// </summary>
/// <example>[{"name": "type", "value": "dataset"}, {"name": "organization", "value": "Kartverket"}]</example>
public List<FacetInput> facets { get; set; }
```

### 2. **Enhanced Help Endpoint with Interactive Examples**

Created `/api/search/help` endpoint with comprehensive documentation including:

- **Interactive Examples**: Clickable URLs with realistic Norwegian metadata scenarios
- **Parameter Documentation**: Clear descriptions with valid values
- **Common Use Cases**: Pre-built examples for different search scenarios
- **Facet Information**: Available facets with common values

**Example interactive scenarios:**
- ?? Basic Search: `Norge kartdata`
- ?? Organization Search: Filter by Kartverket
- ?? Dataset Search: Marine datasets with sorting
- ?? Recent Updates: Latest metadata
- ?? Date Range: Specific time periods
- ?? Complex Search: Multiple filters combined

### 3. **Improved Parameter Documentation**

Enhanced all parameter descriptions with:
- Clear examples using Norwegian terminology
- Realistic values (e.g., "Norge kartdata" instead of just "Norge")
- Multiple facet examples showing organization filtering
- Better date range examples

### 4. **FacetInput Model Enhancement**

Updated `FacetInput` with proper validation and examples:

```csharp
/// <summary>
/// The name of the facet. Valid values: type, theme, organization, nationalinitiative, DistributionProtocols, area, dataaccess, spatialscope
/// </summary>
/// <example>type</example>
[Required(ErrorMessage = "Facet name is required")]
public string name { get; set; }

/// <summary>
/// The value to filter by for this facet
/// </summary>
/// <example>dataset</example>
[Required(ErrorMessage = "Facet value is required")]
public string value { get; set; }
```

## ?? How Users See the Examples

### In Swagger UI Documentation:
- Properties show realistic examples in the schema
- Parameter descriptions include context-specific examples
- XML documentation comments are displayed

### Via Help Endpoint:
```
GET /api/search/help
```

Returns comprehensive JSON with:
- Interactive example URLs
- Parameter descriptions and valid values
- Common Norwegian metadata search scenarios
- Facet information with common values

### Example Requests Generated:
```
/api/search?text=Norge+kartdata&limit=20&orderby=title
/api/search?facets[0]name=organization&facets[0]value=Kartverket&limit=15
/api/search?text=marine&facets[0]name=type&facets[0]value=dataset&orderby=title
/api/search?datefrom=2023-01-01&dateto=2024-12-31&orderby=newest&limit=25
```

## ?? Technical Improvements

### Enhanced Model Binding:
- Better validation in `SearchParameterModelBuilder`
- Input sanitization and bounds checking
- Facet name validation against allowed values
- Date range validation and auto-correction

### Better Error Handling:
- Structured validation error responses
- Proper HTTP status codes
- Descriptive error messages

### Comprehensive Validation:
```csharp
public List<string> ValidateParameters()
{
    var errors = new List<string>();
    // Validates offset, limit, date ranges, and orderby values
    return errors;
}
```

## ?? Usage Patterns

### For Developers:
1. **Check the Help Endpoint**: `GET /api/search/help` for comprehensive documentation
2. **Use Swagger UI**: Enhanced with XML comments and realistic examples
3. **Follow Examples**: Copy-paste ready URLs for common scenarios

### For API Consumers:
1. **Basic Search**: Start with text and limit parameters
2. **Faceted Search**: Add organization or type filters
3. **Advanced Search**: Combine multiple facets with date ranges
4. **Pagination**: Use offset and limit for large result sets

## ?? Example Values Summary

| Parameter | Example Value | Description |
|-----------|---------------|-------------|
| `text` | `Norge kartdata` | Norwegian-specific search term |
| `limit` | `20` | Reasonable page size |
| `offset` | `1` | Standard pagination start |
| `orderby` | `title` | Human-readable sorting |
| `facets[0]name` | `type` | Common facet |
| `facets[0]value` | `dataset` | Common value |
| `facets[1]name` | `organization` | Norwegian context |
| `facets[1]value` | `Kartverket` | Real organization |
| `datefrom` | `2023-01-01` | Recent date |
| `dateto` | `2024-12-31` | Current relevance |

This implementation provides a much better developer experience while maintaining backward compatibility and following .NET 10 best practices for Razor Pages projects.