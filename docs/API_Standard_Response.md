# Standardized API Response Structure

This document outlines the standardized API response structure implemented across all API endpoints in the Order Management System.

## Response Format

All API responses follow a consistent structure:

```json
{
  "success": true,
  "statusCode": 200,
  "message": "Operation completed successfully",
  "data": {
    // Actual response data goes here (specific to each endpoint)
  },
  "validationErrors": null,
  "metadata": {
    // Optional metadata about the response
  },
  "timestamp": "2024-05-10T14:30:45.1234567Z"
}
```

### Response Properties

| Property | Type | Description |
|----------|------|-------------|
| `success` | boolean | Indicates if the request was successful. `true` for success, `false` for failure. |
| `statusCode` | integer | HTTP status code of the response (e.g., 200, 400, 404, 500) |
| `message` | string | A human-readable message describing the result. Typically empty or generic for success, specific error message for failures. |
| `data` | object/array/null | The actual payload of the response. This will be the requested resource(s) on success, or `null` on error. |
| `validationErrors` | array/null | Array of validation errors when request data fails validation, otherwise `null`. |
| `metadata` | object/null | Additional information about the response that might be useful for clients (e.g., pagination info). |
| `timestamp` | string | ISO 8601 formatted date and time when the response was generated. |

## Successful Response Example

### GET /api/orders

```json
{
  "success": true,
  "statusCode": 200,
  "message": "Operation completed successfully",
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "productName": "Laptop",
      "price": 1299.99,
      "quantity": 1,
      "totalPrice": 1299.99,
      "status": "Delivered",
      "createdAt": "2024-05-05T10:00:00Z",
      "updatedAt": "2024-05-07T15:30:00Z",
      "customerName": "John Doe",
      "customerEmail": "john@example.com",
      "shippingAddress": "123 Main St, City"
    },
    {
      "id": "5fa85f64-5717-4562-b3fc-2c963f66afb7",
      "productName": "Smartphone",
      "price": 799.99,
      "quantity": 2,
      "totalPrice": 1599.98,
      "status": "Processing",
      "createdAt": "2024-05-09T09:15:00Z",
      "updatedAt": null,
      "customerName": "Jane Smith",
      "customerEmail": "jane@example.com",
      "shippingAddress": "456 Oak St, Town"
    }
  ],
  "validationErrors": null,
  "metadata": {
    "totalCount": 2,
    "pageSize": 2
  },
  "timestamp": "2024-05-10T14:30:45.1234567Z"
}
```

### GET /api/orders/{id}

```json
{
  "success": true,
  "statusCode": 200,
  "message": "Operation completed successfully",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "productName": "Laptop",
    "price": 1299.99,
    "quantity": 1,
    "totalPrice": 1299.99,
    "status": "Delivered",
    "createdAt": "2024-05-05T10:00:00Z",
    "updatedAt": "2024-05-07T15:30:00Z",
    "customerName": "John Doe",
    "customerEmail": "john@example.com",
    "shippingAddress": "123 Main St, City"
  },
  "validationErrors": null,
  "metadata": null,
  "timestamp": "2024-05-10T14:30:45.1234567Z"
}
```

### POST /api/orders (Create)

```json
{
  "success": true,
  "statusCode": 201,
  "message": "Order created successfully",
  "data": {
    "id": "7fa85f64-5717-4562-b3fc-2c963f66afc8",
    "productName": "Headphones",
    "price": 149.99,
    "quantity": 1,
    "totalPrice": 149.99,
    "status": "Pending",
    "createdAt": "2024-05-10T14:30:45Z",
    "updatedAt": null,
    "customerName": "Alice Johnson",
    "customerEmail": "alice@example.com",
    "shippingAddress": "789 Pine St, Village"
  },
  "validationErrors": null,
  "metadata": null,
  "timestamp": "2024-05-10T14:30:45.1234567Z"
}
```

## Error Response Examples

### 404 Not Found

```json
{
  "success": false,
  "statusCode": 404,
  "message": "Order with ID 3fa85f64-5717-4562-b3fc-2c963f66afa7 not found",
  "data": null,
  "validationErrors": null,
  "metadata": null,
  "timestamp": "2024-05-10T14:30:45.1234567Z"
}
```

### 400 Bad Request (Validation Error)

```json
{
  "success": false,
  "statusCode": 400,
  "message": "Validation failed",
  "data": null,
  "validationErrors": [
    {
      "propertyName": "CustomerEmail",
      "errorMessage": "Email must be a valid email address",
      "errorCode": "EmailValidator"
    },
    {
      "propertyName": "Quantity",
      "errorMessage": "Quantity must be greater than 0",
      "errorCode": "GreaterThanValidator"
    }
  ],
  "metadata": null,
  "timestamp": "2024-05-10T14:30:45.1234567Z"
}
```

### 500 Internal Server Error

```json
{
  "success": false,
  "statusCode": 500,
  "message": "Internal server error",
  "data": null,
  "validationErrors": null,
  "metadata": null,
  "timestamp": "2024-05-10T14:30:45.1234567Z"
}
```

## Client-Side Handling

When handling responses on the client-side, always check the `success` property first to determine if the request was successful. Then you can access the `data` property for successful responses or check the `message` and/or `validationErrors` for failure details.

### TypeScript Example

```typescript
interface ApiResponse<T> {
  success: boolean;
  statusCode: number;
  message: string;
  data: T | null;
  validationErrors?: ValidationError[] | null;
  metadata?: Record<string, any> | null;
  timestamp: string;
}

interface ValidationError {
  propertyName: string;
  errorMessage: string;
  errorCode?: string;
}

// Example fetch with response handling
async function getOrders(): Promise<Order[]> {
  try {
    const response = await fetch('/api/orders');
    const result: ApiResponse<Order[]> = await response.json();
    
    if (result.success) {
      return result.data || [];
    } else {
      // Handle error based on status code
      if (result.statusCode === 404) {
        // Handle not found
      } else if (result.statusCode === 400) {
        // Handle validation errors
        if (result.validationErrors) {
          // Process validation errors
        }
      }
      
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Error fetching orders:', error);
    throw error;
  }
}
```

## Benefits of Standardized Responses

1. **Consistency**: All API endpoints return data in the same format, making client-side development more predictable.
2. **Better Error Handling**: Detailed error information helps developers quickly identify and fix issues.
3. **Metadata Support**: Additional context can be provided without changing the core response structure.
4. **Clear Status Indicators**: The `success` flag makes it immediately clear if a request succeeded or failed.
5. **Self-Documenting**: Responses include timestamps and descriptive messages for better debugging.

This standardized structure allows for easier client development and improves the overall developer experience when working with our APIs.
