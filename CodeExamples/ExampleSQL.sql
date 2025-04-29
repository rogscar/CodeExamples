-- This query analyzes PLC data in a manufacturing context, calculating temperature/pressure metrics,
-- correlating with errors, and ranking devices by temperature variance.
-- It demonstrates advanced SQL concepts like CTEs, window functions, joins, and conditional logic.

-- CTE 1: DeviceStats - Calculate metrics for each device (temperature, pressure, variance)
WITH DeviceStats AS (
    SELECT 
        DeviceId,  -- Group by device to analyze each PLC device separately
        -- Calculate average, min, and max for temperature and pressure
        AVG(Temperature) AS AvgTemperature,
        MIN(Temperature) AS MinTemperature,
        MAX(Temperature) AS MaxTemperature,
        AVG(Pressure) AS AvgPressure,
        MIN(Pressure) AS MinPressure,
        MAX(Pressure) AS MaxPressure,
        -- Standard deviation of temperature to measure variability
        STDEV(Temperature) AS TempStdDev,
        -- Rank devices by temperature variance (window function)
        -- Higher variance means more fluctuation, which might indicate issues
        RANK() OVER (ORDER BY STDEV(Temperature) DESC) AS TempVarianceRank
    FROM PlcReadings  -- Source table with PLC data
    -- Filter data for April 2025 to focus on a specific time period
    WHERE Timestamp BETWEEN '2025-04-01' AND '2025-04-28'
    GROUP BY DeviceId  -- Aggregate metrics per device
),
-- CTE 2: ErrorCounts - Count errors per device and aggregate error messages
ErrorCounts AS (
    SELECT 
        DeviceId,  -- Group by device to match with DeviceStats
        COUNT(*) AS ErrorCount,  -- Count errors for each device
        -- Concatenate all error messages into a single string for reporting
        STRING_AGG(ErrorMessage, '; ') AS ErrorMessages
    FROM ErrorLog  -- Source table with error logs
    -- Same time period as DeviceStats for correlation
    WHERE Timestamp BETWEEN '2025-04-01' AND '2025-04-28'
    GROUP BY DeviceId  -- Aggregate errors per device
)
-- Main query: Combine device stats with error counts and add status logic
SELECT 
    ds.DeviceId,  -- Device identifier
    -- Temperature metrics
    ds.AvgTemperature,
    ds.MinTemperature,
    ds.MaxTemperature,
    -- Pressure metrics
    ds.AvgPressure,
    ds.MinPressure,
    ds.MaxPressure,
    -- Temperature variability
    ds.TempStdDev,
    ds.TempVarianceRank,  -- Ranking of devices by temperature fluctuation
    -- Error counts and messages, using COALESCE to handle devices with no errors
    COALESCE(ec.ErrorCount, 0) AS ErrorCount,  -- Show 0 if no errors
    COALESCE(ec.ErrorMessages, 'No Errors') AS ErrorMessages,  -- Show 'No Errors' if none
    -- Conditional logic to flag temperature status for potential issues
    CASE 
        WHEN ds.AvgTemperature > 100 THEN 'High Temperature Warning'  -- Flag high temps
        WHEN ds.AvgTemperature < 0 THEN 'Low Temperature Warning'    -- Flag low temps
        ELSE 'Normal'  -- Otherwise, normal operation
    END AS TemperatureStatus
FROM DeviceStats ds  -- Base data from DeviceStats CTE
-- LEFT JOIN to include devices even if they have no errors
LEFT JOIN ErrorCounts ec ON ds.DeviceId = ec.DeviceId
-- Order by temperature variance rank (most variable first) and device ID
ORDER BY ds.TempVarianceRank, ds.DeviceId;
