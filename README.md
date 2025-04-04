# C# Code Examples

This repository contains C# classes designed for data retrieval and processing in modern .NET applications.

## Classes

### GetBikeOrder
The `GetBikeOrder` class is designed to retrieve bike order data from a SQL Server database and convert it into a list of strongly-typed `BikeOrder` objects from the `DataObjects` namespace. It demonstrates modern .NET practices, including:

- Asynchronous database access using `Microsoft.Data.SqlClient`.
- Secure parameterized SQL queries to prevent SQL injection.
- Robust error handling with try-catch blocks and meaningful exceptions.
- Reflection-based mapping of `DataTable` rows to object properties.

**Usage**: An example of how to use `GetBikeOrder` is included in the class file itself within the `Program` class's `Main` method.

### RestClient
The `RestClient` class provides a simple, reusable way to make RESTful API calls, specifically a GET request to retrieve charge data. It showcases modern .NET practices for HTTP communication, including:

- Asynchronous HTTP requests using `HttpClient`.
- JSON parsing with `Newtonsoft.Json` to handle API responses.
- Bearer token authentication for secure API access.
- Error handling with tuple-based return values for success, data, and error messages.

**Usage**: An example of how to use `RestClient` is included in the class file itself within the `Program` class's `Main` method.

## Examples
Examples of usage for both classes are embedded in the class files themselves, within a `Program` class with a `Main` method. These examples demonstrate how to instantiate the classes, call their methods, and handle the results in a console application.

## Prerequisites
- **.NET Framework/Core**: Compatible with .NET versions supporting the used libraries.
- **NuGet Packages**:
  - `Microsoft.Data.SqlClient` (for `GetBikeOrder`).
  - `Newtonsoft.Json` (for `RestClient`).
- **Database**: A SQL Server instance with `bike_orders` and `bikes` tables (for `GetBikeOrder`).
- **API Access**: A REST API endpoint with Bearer token authentication (for `RestClient`).

## Getting Started
1. Clone the repository.
2. Open the solution in Visual Studio or your preferred IDE.
3. Install the required NuGet packages.
4. Update connection strings (for `GetBikeOrder`) or API details (for `RestClient`) in the usage examples.
5. Run the `Main` method in the desired class file to see it in action.
