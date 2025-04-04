using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using DataObjects; // Assuming this is where BikeOrder lives

namespace CodeExamples
{
    public sealed class GetBikeOrder
    {
        public async Task<DataTable> GetBikeOrderDataAsync(List<string> statuses, string databaseConnectionString)
        {
            var bikeDataTable = new DataTable();
            var defaultStatuses = new[] { "OPEN", "DELIVERED", "STARTED", "COMPLETE" };
            var validStatuses = statuses?.Where(s => !string.IsNullOrWhiteSpace(s)).ToList() ?? defaultStatuses.ToList();

            try
            {
                using (var sqlCon = new SqlConnection(databaseConnectionString + ";Connection Timeout=600"))
                {
                    await sqlCon.OpenAsync();
                    using (var sqlCmd = new SqlCommand(BuildQuery(validStatuses.Count), sqlCon))
                    {
                        for (int i = 0; i < validStatuses.Count; i++)
                        {
                            sqlCmd.Parameters.AddWithValue($"@Status{i}", validStatuses[i]);
                        }
                        using (var sqlDa = new SqlDataAdapter(sqlCmd))
                        {
                            await Task.Run(() => sqlDa.Fill(bikeDataTable));
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("Failed to retrieve bike order data from the database.", ex);
            }

            return bikeDataTable;
        }

        public List<BikeOrder> ConvertDataTableToBikeOrders(DataTable dataTable)
        {
            if (dataTable == null) throw new ArgumentNullException(nameof(dataTable), "DataTable cannot be null.");

            var bikeOrders = new List<BikeOrder>();

            foreach (DataRow row in dataTable.Rows)
            {
                try
                {
                    var bikeOrder = new BikeOrder();
                    var properties = typeof(BikeOrder).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    foreach (var prop in properties)
                    {
                        try
                        {
                            if (!row.Table.Columns.Contains(prop.Name)) continue;

                            object value = row[prop.Name];
                            if (value == DBNull.Value || string.IsNullOrEmpty(value?.ToString()))
                            {
                                prop.SetValue(bikeOrder, GetDefaultValue(prop.PropertyType));
                            }
                            else
                            {
                                prop.SetValue(bikeOrder, Convert.ChangeType(value, prop.PropertyType));
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to set property {prop.Name} for OrderNumber {row["OrderNumber"]}: {ex.Message}");
                            continue;
                        }
                    }

                    bikeOrders.Add(bikeOrder);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to parse row with OrderNumber {row["OrderNumber"]}: {ex.Message}");
                    continue;
                }
            }

            return bikeOrders;
        }

        private static object GetDefaultValue(Type type)
        {
            return type switch
            {
                Type t when t == typeof(string) => string.Empty,
                Type t when t == typeof(decimal) => 0m,
                Type t when t == typeof(DateTime) => DateTime.Now,
                Type t when t == typeof(int) => 0,
                _ => null
            };
        }

        private static string BuildQuery(int statusCount)
        {
            var query = new StringBuilder();
            query.AppendLine(@"SELECT DISTINCT
                ISNULL(bo.ordernumber, 0) AS [OrderNumber],
                ISNULL(bo.createdDate, GETDATE()) AS [CreatedDate],
                ISNULL(bo.earliestDeliveryDate, CAST('1900-01-01' AS DateTime)) AS [EarliestDeliverDate],
                ISNULL(bo.latestDeliveryDate, CAST('1900-01-01' AS DateTime)) AS [LatestDeliveryDate],
                ISNULL(bo.employeeTakingOrder, '') AS [EnteredBy],
                bo.status AS [OrderStatus],
                ISNULL(bo.pieces, 0) AS [Quantity],
                ISNULL(bi.weight, 0) AS [Weight],
                ISNULL(bi.biketype, '') AS [BikeType],
                ISNULL(bi.comment, '') AS [Comment]
            FROM bike_orders AS bo WITH (NOLOCK)
            INNER JOIN bikes AS bi WITH (NOLOCK) ON bo.ordernumber = bi.ordernumber
            WHERE bo.status IN (");

            var statusParams = Enumerable.Range(0, statusCount).Select(i => $"@Status{i}");
            query.Append(string.Join(",", statusParams));
            query.AppendLine(")");
            query.AppendLine("ORDER BY bo.ordernumber DESC");

            return query.ToString();
        }
    }

    // Usage example integrated into a Program class
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize the service
            var bikeOrderService = new GetBikeOrder();

            // Define inputs
            var statuses = new List<string> { "OPEN", "COMPLETE" };
            string connectionString = "Server=your-server;Database=your-db;User Id=your-user;Password=your-password;TrustServerCertificate=True;";

            try
            {
                // Fetch data asynchronously
                DataTable dataTable = await bikeOrderService.GetBikeOrderDataAsync(statuses, connectionString);

                // Convert to list of BikeOrder objects
                List<BikeOrder> bikeOrders = bikeOrderService.ConvertDataTableToBikeOrders(dataTable);

                // Use the data
                Console.WriteLine("Bike Orders Retrieved:");
                foreach (var order in bikeOrders)
                {
                    Console.WriteLine($"Order #{order.OrderNumber}: {order.OrderStatus} - {order.BikeType} (Qty: {order.Quantity})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving bike orders: {ex.Message}");
            }
        }
    }
}