using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using Newtonsoft.Json.Linq;
using Windows.ServicesFramework;

namespace ThirdPartryComms.Customer
{
    public sealed class RestPostExample : CodeActivity
    {

        public RestPostExample()
        {

            DisplayName = "Customer Send Reparation for inspection";
            
        }

        /// <summary>
        /// The Customer Inspection Details Url. Ex: "https://dev.Customerhosting.ca:443/DV158/DEVCustomerApi/api/v1.1/reparations"
        /// </summary>
        [Category("Input")]
        public InArgument<string> RepairOrderUrlEndPoint { get; set; }

        [Category("Input")]
        public InArgument<VehicleInspectionActor> InspectionReport { get; set; }
        
        [Category("Input")]
        public InArgument<string> CustomerToken { get; set; }

        [Category("Input")]
        public InArgument<string> DefaultInspectionCompany { get; set; }

        [Category("Input")]
        public InArgument<ServiceReporter> Reporter { get; set; }

        [Category("Output")]
        public OutArgument<string> ErrorMessage { get; set; }

        [Category("Output")]
        public OutArgument<bool> success { get; set; }


        StringBuilder errorMessage = new StringBuilder();

        protected override void Execute(CodeActivityContext context)
        {

            string RepairOrderUrlEndPoint = this.RepairOrderUrlEndPoint.Get(context);
            VehicleInspectionReport inspectionReport = this.InspectionReport.Get(context);
            string CustomerToken = this.CustomerToken.Get(context);
            string defaultInspectionCompany = this.DefaultInspectionCompany.Get(context);

            ServiceReporter reporter = this.Reporter.Get(context);
            bool success = false;

            reporter.Debug(string.Format("Sending reparation report for repair order {0}", inspectionReport.PartID));

            success = SendRepairOrderToCustomer(RepairOrderUrlEndPoint, CustomerToken, inspectionReport, defaultInspectionCompany, ref reporter);
           
            context.SetValue(this.success, success);
                     
        }



        private bool SendRepairOrderToCustomer(string RepairOrderUrlEndPoint, string token, VehicleInspectionReport inspectionReport, string defaultInspectionCompany, ref ServiceReporter r)
        {
            bool success = false;

            System.Net.HttpWebRequest repairOrderPost = (HttpWebRequest)HttpWebRequest.Create(string.Format("{0}?", RepairOrderUrlEndPoint));


            repairOrderPost.Method = "POST";
            repairOrderPost.ContentType = "application/json";
            repairOrderPost.Headers.Add(string.Format("Authorization: Bearer {0}", token));

            //build body object aka repair order
            JObject formattedTransaction = new JObject();

            JObject repairedInspectionIds = new JObject();

            formattedTransaction.Add("reparationNo", inspectionReport.RepairOrderNumber);
            formattedTransaction.Add("mechanicName", (!string.IsNullOrWhiteSpace(inspectionReport.MechanicName)) ? inspectionReport.MechanicName : defaultInspectionCompany);
            formattedTransaction.Add("mechanicCompany", (!string.IsNullOrWhiteSpace(inspectionReport.ShopName)) ? inspectionReport.ShopName : defaultInspectionCompany);
            formattedTransaction.Add("mechanicDateUtc", DateTime.UtcNow);

            //build mechanic address object and add to repair order
            JObject mechanicAddress = new JObject();
            mechanicAddress.Add("name", "");
            mechanicAddress.Add("street", inspectionReport.ShopAddress1 + ((!string.IsNullOrWhiteSpace(inspectionReport.ShopAddress2)) ? $", {inspectionReport.ShopAddress2}" : ""));
            mechanicAddress.Add("city", inspectionReport.ShopCity);
            mechanicAddress.Add("district", inspectionReport.ShopState);
            mechanicAddress.Add("country", inspectionReport.ShopCountry);
            mechanicAddress.Add("postalCode", inspectionReport.ShopZip);

            formattedTransaction.Add("mechanicAddress", mechanicAddress);
            formattedTransaction.Add("mechanicComment", inspectionReport.Comment);
            formattedTransaction.Add("fleetRepName", inspectionReport.Driver);
            formattedTransaction.Add("fleetRepDateUtc", inspectionReport.TimeStamp);

            //build fleetRep address object and add to repair order
            JObject fleetRepAddress = new JObject();
            fleetRepAddress.Add("name", "");
            fleetRepAddress.Add("street", "");
            fleetRepAddress.Add("city", "");
            fleetRepAddress.Add("district", "");
            fleetRepAddress.Add("country", "");
            fleetRepAddress.Add("postalCode", "");

            formattedTransaction.Add("fleetRepAddress", fleetRepAddress);

            JArray idList = new JArray();
            idList.Add(inspectionReport.InspectionItemId);

            formattedTransaction.Add("repairedInspectionItemIdList", idList);

            //log current repair order
            Console.WriteLine(string.Format("Posting Reparation Record: {0}", formattedTransaction.ToString().Replace("\r\n", string.Empty)));

            //post the repair order to the Customer API
            using (System.IO.StreamWriter requestStreamWriter = new StreamWriter(repairOrderPost.GetRequestStream(), Encoding.UTF8))
            {
                requestStreamWriter.Write(formattedTransaction.ToString());
                requestStreamWriter.Close();
            }

            // get request response
            System.Net.WebResponse reparationReponse = repairOrderPost.GetResponse();
            try
            {

                // convert stream into a string for processing
                using (StreamReader responseReader = new StreamReader(reparationReponse.GetResponseStream()))
                {

                    string responseMessage = responseReader.ReadToEnd();

                    JObject transactionResponse = JObject.Parse(responseMessage);

                    if (transactionResponse.Values().Contains("message"))
                    {
                        errorMessage.AppendLine(string.Format("An error was registered in the Customer API when trying to send repair order {0}. Ex: {1}", inspectionReport.InspectionId, transactionResponse.GetValue("message").ToString()));
                        success = false;
                    }
                    else
                    {
                        success = true;
                    }

                }

                reparationReponse.Close();

            }
            catch (Exception ex)
            {
                errorMessage.AppendLine(string.Format("An error occurred while getting the reponse from posting the Repair Order to Customer. Ex: {0};{1}", ex.Message, ex.StackTrace));
                success = false;
            }
            finally
            {
                reparationReponse.Close();
            }


            return success;

        }

    }
}
