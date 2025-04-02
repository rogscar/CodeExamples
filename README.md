This repository contains a C# class, GetBikeOrder, designed to retrieve bike order data from a SQL Server database and convert it into a list of strongly-typed objects. It demonstrates modern .NET practices, secure database access, and robust error handling.

Heres is how to use this class.

using CodeExamples;
using DataObjects; // Assumes BikeOrder class is defined here

var bikeOrderService = new GetBikeOrder();
var statuses = new List<string> { "OPEN", "COMPLETE" };
string connectionString = "your-connection-string-here";

// Fetch data asynchronously
DataTable dataTable = await bikeOrderService.GetBikeOrderDataAsync(statuses, connectionString);

// Convert to list of BikeOrder objects
List<BikeOrder> bikeOrders = bikeOrderService.ConvertDataTableToBikeOrders(dataTable);

// Use the data
foreach (var order in bikeOrders)
{
    Console.WriteLine($"Order #{order.OrderNumber}: {order.OrderStatus}");
}

Used async/await for I/O-bound database calls to improve scalability

Used nolock so i could get all realated data, even if bike doesnt exist.
