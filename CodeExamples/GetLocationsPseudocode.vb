Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Newtonsoft.Json
Imports Windows.ServicesFramework

Module LocationModule

    ' Input arguments (equivalent to XAML properties)
    Public API_TOKEN As String
    Public API_ID As Integer
    Public Destination As String = "Generic"
    Public pastHours As Double = 0.125
    Public divideDesc As Boolean = False
    Public debug As Boolean = False
    Public DataBaseConnectionString As String = "Server=YourServerName;Database=YourDatabaseName;User Id=YourUsername;Password=YourPassword;"

    Sub Main()
        ' Initialize variables
        Dim connection As New CommunicationConnection With {
            .commType = commType.Generic,
            .Username = API_ID.ToString(),
            .AuthCode = API_TOKEN
        }
        Dim Locations As New List(Of LocationObject)
        Dim DBEntry2 As DBEntry
        Dim Reporter As New ServiceReporter

        ' Step 1: Get Locations
        Locations = GetLocations(connection)

        ' Step 2: Process each location
        For Each location As LocationObject In Locations
            Dim locList As List(Of String)

            ' Step 3: Split location Description if enabled
            If divideDesc Then
                Dim locCityList As List(Of String)

                If debug Then
                    Reporter.Debug(String.Format("LocDesc is : {0}", location.LocDesc))
                End If

                ' Split LocDesc by comma
                locList = location.LocDesc.Split(","c).ToList()

                If debug Then
                    Reporter.Debug(String.Format("LocDescList count is : {0}", locList.Count()))
                End If

                ' Handle different locList lengths
                Select Case locList.Count
                    Case 3
                        location.LocDescCity = locList(1).Trim()
                        location.LocDescState = locList(2).Trim()

                    Case 4
                        location.LocDescCity = locList(1).Trim()
                        location.LocDescState = locList(2).Trim()
                        location.LocDescZip = locList(3).Trim()

                    Case 5
                        location.LocDescCity = locList(1).Trim()
                        location.LocDescState = locList(3).Trim()
                        location.LocDescZip = locList(4).Trim()

                        If String.IsNullOrEmpty(location.LocDescCity) Then
                            Dim count As Integer = 0
                            locCityList = location.LocDescCity.Split(" "c).ToList()

                            If debug Then
                                Reporter.Debug(String.Format("locCityList count is : {0}", locCityList.Count()))
                            End If

                            If locCityList.Count > 3 Then
                                location.LocDescDistance = Double.Parse(locCityList(0))
                                location.LocDescDirection = locCityList(2)

                                count = 0
                                If locCityList.Count - 3 > 1 Then
                                    Dim tempCity As String = String.Empty
                                    While count < locCityList.Count - 3
                                        tempCity &= " " & locCityList(count + 3)
                                        count += 1
                                    End While
                                    location.LocDescCity = tempCity.Trim()
                                Else
                                    location.LocDescCity = locCityList(3)
                                End If
                            End If
                        End If
                End Select

                If debug Then
                    Reporter.Debug(String.Format("LocDescCity is : {0}", location.LocDescCity))
                End If
            End If

            ' Step 4: Filter by time and create DBEntry
            If location.locationDateTime.ConvertToUnixMilliSeconds() > DateTime.Now.AddHours(-pastHours).ConvertToUnixMilliSeconds() Then
                Dim jsonData As String = JsonConvert.SerializeObject(location)
                DBEntry2 = CreateDBEntry(
                    ConnectionString:=DataBaseConnectionString,
                    ContentType:="MobileComlocationReport",
                    DataFile:=jsonData,
                    Destination:=Destination,
                    Direction:="Incoming",
                    Source:="Generic",
                    Status:="New",
                    DBEntrySource:="Workflow"
                )
            End If
        Next
    End Sub

    ' Placeholder for retrieving location reports (replace with actual Generic API call)
    Private Function GetLocations(connection As CommunicationConnection) As List(Of LocationObject)
        ' Simulate API call to Generic
        ' Example: Call Generic API with connection.Username and connection.AuthCode
        Dim Locations As New List(Of LocationObject)
        ' Add logic to fetch location reports using HTTP client or Generic SDK
        Return Locations
    End Function

    ' Placeholder for creating a DBEntry (replace with actual database operation)
    Private Function CreateDBEntry(ConnectionString As String, ContentType As String, DataFile As String, Destination As String, Direction As String, Source As String, Status As String, DBEntrySource As String) As DBEntry
        ' Simulate DBEntry creation
        ' Example: Insert into database using ConnectionString
        Dim DBEntry As New DBEntry
        ' Add logic to create DBEntry in Tranzactor system
        Return DBEntry
    End Function

    ' Extension method to convert DateTime to Unix milliseconds (assumed to exist in the original workflow)
    <Runtime.CompilerServices.Extension>
    Public Function ConvertToUnixMilliSeconds(dateTime As DateTime) As Long
        Return CLng((dateTime.ToUniversalTime() - New DateTime(1970, 1, 1)).TotalMilliseconds)
    End Function

End Module

' Placeholder classes (replace with actual definitions from Tranzactor.Data and other libraries)
Public Class CommunicationConnection
    Public Property commType As commType
    Public Property Username As String
    Public Property AuthCode As String
End Class

Public Enum commType
    Generic
End Enum

Public Class LocationObject
    Public Property LocDesc As String
    Public Property LocDescCity As String
    Public Property LocDescState As String
    Public Property LocDescZip As String
    Public Property LocDescDistance As Double
    Public Property LocDescDirection As String
    Public Property locationDateTime As DateTime
End Class

Public Class DBEntry
    ' Define properties as needed
End Class

Public Class ServiceReporter
    Public Sub Debug(message As String)
        Console.WriteLine(message) ' Replace with actual logging mechanism
    End Sub
End Class
