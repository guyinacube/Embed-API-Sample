using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerBI.Api.V2;
using Microsoft.PowerBI.Api.V2.Models;
using Microsoft.Rest;
using System;
using System.Threading.Tasks;

namespace EmbedAPISample
{
    class Program
    {
        private static string authorityUrl = "https://login.windows.net/common/";
        private static string resourceUrl = "https://analysis.windows.net/powerbi/api";
        private static string apiUrl = "https://api.powerbi.com/";
        private static string embedUrlBase = "https://app.powerbi.com/";

        private static string tenantId = "<TENANT ID>";
        private static string groupId = "<GROUP ID>";
        
        private static string reportId = "<REPORT ID>";
        private static string datasetId = "<DATASET ID>";

        // Update the Client ID and Secret within Secrets.cs

        private static ClientCredential credential = null;
        private static AuthenticationResult authenticationResult = null;
        private static TokenCredentials tokenCredentials = null;

        static void Main(string[] args)
        {

            try
            {
                // Create a user password cradentials.
                credential = new ClientCredential(Secrets.ClientID, Secrets.ClientSecret);

                // Authenticate using created credentials
                Authorize().Wait();

                using (var client = new PowerBIClient(new Uri(apiUrl), tokenCredentials))
                {

                    EmbedToken embedToken = client.Reports.GenerateTokenInGroup(groupId, reportId, new GenerateTokenRequest(accessLevel: "View", datasetId: datasetId));

                    Report report = client.Reports.GetReportInGroup(groupId, reportId);

                    #region Output Embed Token
                    Console.WriteLine("\r*** EMBED TOKEN ***\r");

                    Console.Write("Report Id: ");

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(reportId);
                    Console.ResetColor();

                    Console.Write("Report Embed Url: ");

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(report.EmbedUrl);
                    Console.ResetColor();

                    Console.WriteLine("Embed Token Expiration: ");

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(embedToken.Expiration.Value.ToString());
                    Console.ResetColor();


                    Console.WriteLine("Report Embed Token: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(embedToken.Token);
                    Console.ResetColor();
                    #endregion

                    #region Output Datasets
                    Console.WriteLine("\r*** DATASETS ***\r");

                    // List of Datasets
                    // This method calls for items in a Group/App Workspace. To get a list of items within your "My Workspace"
                    // call GetDatasets()
                    ODataResponseListDataset datasetList = client.Datasets.GetDatasetsInGroup(groupId);

                    foreach (Dataset ds in datasetList.Value)
                    {
                        Console.WriteLine(ds.Id + " | " + ds.Name);
                    }
                    #endregion

                    #region Output Reports
                    Console.WriteLine("\r*** REPORTS ***\r");

                    // List of reports
                    // This method calls for items in a Group/App Workspace. To get a list of items within your "My Workspace"
                    // call GetReports()
                    ODataResponseListReport reportList = client.Reports.GetReportsInGroup(groupId);

                    foreach (Report rpt in reportList.Value)
                    {
                        Console.WriteLine(rpt.Id + " | " + rpt.Name + " | DatasetID = " + rpt.DatasetId);
                    }
                    #endregion

                    #region Output Dashboards
                    Console.WriteLine("\r*** DASHBOARDS ***\r");

                    // List of reports
                    // This method calls for items in a Group/App Workspace. To get a list of items within your "My Workspace"
                    // call GetReports()
                    ODataResponseListDashboard dashboards = client.Dashboards.GetDashboardsInGroup(groupId);

                    foreach (Dashboard db in dashboards.Value)
                    {
                        Console.WriteLine(db.Id + " | " + db.DisplayName);
                    }
                    #endregion

                    #region Output Gateways
                    Console.WriteLine("\r*** Gateways ***\r");

                    ODataResponseListGateway gateways = client.Gateways.GetGateways();

                    Console.WriteLine(gateways.Value[0].Name);

                    //foreach (Gateway g in gateways)
                    //{
                    //    Console.WriteLine(g.Name + " | " + g.GatewayStatus);
                    //}
                    #endregion
                }

            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
                Console.ResetColor();
            }

        }

        private static Task Authorize()
        {
            return Task.Run(async () => {
                authenticationResult = null;
                tokenCredentials = null;

                var tenantSpecificURL = authorityUrl.Replace("common", tenantId);
                var authenticationContext = new AuthenticationContext(authorityUrl);

                authenticationResult = await authenticationContext.AcquireTokenAsync(resourceUrl, credential);

                if (authenticationResult != null)
                {
                    tokenCredentials = new TokenCredentials(authenticationResult.AccessToken, "Bearer");
                }
            });
        }






    }
}
