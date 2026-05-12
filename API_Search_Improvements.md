# API Search Endpoint Improvements

## Summary of Improvements

This document outlines the improvements made to the `/api/search` endpoint and `SearchParameters` model to enhance the UI/developer experience.

## ?? Key Improvements Made

### 1. **Enhanced API Documentation**
- Added comprehensive XML documentation with examples
- Created detailed parameter descriptions with valid values
- Added response type annotations for better API documentation
- Created `/api/search/help` endpoint with interactive documentation

### 2. **Input Validation & Error Handling**
- Added data annotations for parameter validation
- Implemented custom validation logic in `SearchParameters.ValidateParameters()`
- Enhanced model binding with input sanitization
- Added proper error responses with descriptive messages

### 3. **Better Parameter Handling**
- Added default values and ranges for all parameters
- Implemented parameter clamping (e.g., limit between 1-1000)
- Added validation for date ranges
- Enhanced facet validation with allowed values

### 4. **Improved Error Responses**
- Structured error responses using `ValidationProblemDetails`
- Proper HTTP status codes (400 for validation errors, 500 for server errors)
- Descriptive error messages for developers

### 5. **Developer Experience Enhancements**
- Created facet discovery endpoint (`/api/search/facets/{facetName}`)
- Added examples in documentation for common use cases
- Better parameter naming consistency
- Added safety guards against infinite loops and malformed input

## ?? API Usage Examples

### Basic Search
```
GET /api/search/?text=Norge&limit=10&offset=1
```

### With Facet Filters
```
GET /api/search/?facets[0]name=type&facets[0]value=dataset&facets[1]name=organization&facets[1]value=Kartverket
```

### Date Range Filter
```
GET /api/search/?datefrom=2023-01-01&dateto=2023-12-31&orderby=newest
```

### Get API Documentation
```
GET /api/search/help
```

### Get Available Facet Values
```
GET /api/search/facets/type
GET /api/search/facets/organization
```

## ?? Parameter Validation Rules

| Parameter | Type | Range/Rules | Default |
|-----------|------|-------------|---------|
| `text` | string | Any text | null |
| `limit` | integer | 1-1000 | 10 |
| `offset` | integer | ? 1 | 1 |
| `orderby` | string | score, title, title_desc, organization, organization_desc, newest, updated, popularMetadata | score |
| `listhidden` | boolean | true/false | false |
| `datefrom` | datetime | Valid date, ? dateto | null |
| `dateto` | datetime | Valid date, ? datefrom | null |
| `facets` | array | Valid facet names only | [] |

## ??? Valid Facet Names

- `type` - Resource type (dataset, service, etc.)
- `theme` - Thematic categories  
- `organization` - Publishing organization
- `nationalinitiative` - National initiatives
- `DistributionProtocols` - Distribution protocols
- `area` - Geographic area
- `dataaccess` - Data access levels
- `spatialscope` - Spatial scope

## ??? Error Handling

The API now returns structured error responses:

### Validation Error (400)
```json
{
  "title": "Invalid search parameters",
  "errors": {
    "SearchParameters": [
      "Limit must be between 1 and 1000",
      "DateFrom cannot be later than DateTo"
    ]
  }
}
```

### Server Error (500)
```json
{
  "error": "An error occurred while processing your search request"
}
```

## ?? Testing the Improvements

1. **Try invalid parameters:**
   - `GET /api/search/?limit=2000` (should return validation error)
   - `GET /api/search/?orderby=invalid` (should use default 'score')

2. **Test documentation endpoints:**
   - `GET /api/search/help` (comprehensive API documentation)
   - `GET /api/search/facets/type` (available facet values)

3. **Test edge cases:**
   - Empty searches, large offsets, date range validation

## ?? Next Steps (Optional)

For further improvements, consider:

1. **OpenAPI/Swagger Integration**: Configure full Swagger documentation
2. **Rate Limiting**: Add throttling for production use
3. **Caching**: Implement response caching for frequently accessed facets
4. **Analytics**: Add search analytics and logging
5. **API Versioning**: Consider versioning strategy for future changes

---

**Note**: These improvements maintain backward compatibility while significantly enhancing the developer experience and API robustness.