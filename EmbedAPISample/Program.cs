using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmbedAPISample
{
    class Program
    {
        private static bool useEmbedToken = true;
        private static bool useRLS = true;

        private static string authorityUrl = "https://login.microsoftonline.com/organizations/";
        private static string resourceUrl = "https://analysis.windows.net/powerbi/api";
        private static string apiUrl = "https://api.powerbi.com/";

        private static string tenantId = "<ENTER TENANT ID>";
        private static Guid groupId = Guid.Parse("<ENTER GROUP/WORKSPACE ID>");
        
        private static Guid reportId = Guid.Parse("<ENTER REPORT ID>");
        private static Guid datasetId = Guid.Parse("<ENTER DATASET ID>");

        // **** Update the Client ID and Secret within Secrets.cs ****

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

                    #region Embed Token
                    EmbedToken embedToken = null;


                    if (useEmbedToken && !useRLS)
                    {
                        // **** Without RLS ****
                        embedToken = client.Reports.GenerateTokenInGroup(groupId, reportId, 
                            new GenerateTokenRequest(accessLevel: "View", datasetId: datasetId.ToString()));
                    }
                    else if(useEmbedToken && useRLS)
                    {
                        // **** With RLS ****

                        // Documentation: https://docs.microsoft.com/power-bi/developer/embedded/embedded-row-level-security
                        // Example: 
                        //
                        // Define Embed Token request:
                        //var generateTokenRequestParameters = new GenerateTokenRequest("View", null, 
                        //    identities: new List<EffectiveIdentity> { new EffectiveIdentity(username: "username", 
                        //        roles: new List<string> { "roleA", "roleB" }, 
                        //        datasets: new List<string> { "datasetId" }) });
                        // 
                        // Generate Embed Token:
                        //var tokenResponse = await client.Reports.GenerateTokenInGroupAsync("groupId", "reportId", 
                        //    generateTokenRequestParameters);

                        var rls = new EffectiveIdentity(username: "<ENTER USERNAME>", new List<string> { datasetId.ToString() });

                        var rolesList = new List<string>();
                        rolesList.Add("<ENTER ROLE>");
                        rls.Roles = rolesList;

                        embedToken = client.Reports.GenerateTokenInGroup(groupId, reportId, 
                            new GenerateTokenRequest(accessLevel: "View", datasetId: datasetId.ToString(), rls));
                    }
                    #endregion

                    #region Output Embed Token

                    if (useEmbedToken)
                    {
                        // Get a single report used for embedding
                        Report report = client.Reports.GetReportInGroup(groupId, reportId);

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
                        Console.WriteLine(embedToken.Expiration.ToString());
                        Console.ResetColor();


                        Console.WriteLine("Report Embed Token: ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(embedToken.Token);
                        Console.ResetColor();
                    }
                    #endregion

                    #region Output Datasets
                    Console.WriteLine("\r*** DATASETS ***\r");

                    try
                    {
                        // List of Datasets
                        // This method calls for items in a Group/App Workspace. To get a list of items within your "My Workspace"
                        // call GetDatasets()
                        var datasetList = client.Datasets.GetDatasetsInGroup(groupId);

                        foreach (Dataset ds in datasetList.Value)
                        {
                            Console.WriteLine(ds.Id + " | " + ds.Name);
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error fetching datasets: " + ex.Message);
                        Console.ResetColor();
                    }
                    #endregion

                    #region Output Reports
                    Console.WriteLine("\r*** REPORTS ***\r");

                    try
                    {
                        // List of reports
                        // This method calls for items in a Group/App Workspace. To get a list of items within your "My Workspace"
                        // call GetReports()
                        var reportList = client.Reports.GetReportsInGroup(groupId);

                        foreach (Report rpt in reportList.Value)
                        {
                            Console.WriteLine(rpt.Id + " | " + rpt.Name + " | DatasetID = " + rpt.DatasetId);
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error fetching reports: " + ex.Message);
                        Console.ResetColor();
                    }
                    #endregion

                    #region Output Dashboards
                    Console.WriteLine("\r*** DASHBOARDS ***\r");

                    try
                    {
                        // List of reports
                        // This method calls for items in a Group/App Workspace. To get a list of items within your "My Workspace"
                        // call GetReports()
                        var dashboards = client.Dashboards.GetDashboardsInGroup(groupId);

                        foreach (Dashboard db in dashboards.Value)
                        {
                            Console.WriteLine(db.Id + " | " + db.DisplayName);
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error fetching dashboards: " + ex.Message);
                        Console.ResetColor();
                    }
                    #endregion

                    #region Output Gateways
                    Console.WriteLine("\r*** Gateways ***\r");

                    try
                    {
                        var gateways = client.Gateways.GetGateways();

                        Console.WriteLine(gateways.Value[0].Name);

                        //foreach (Gateway g in gateways)
                        //{
                        //    Console.WriteLine(g.Name + " | " + g.GatewayStatus);
                        //}
                    }
                    catch(Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error fetching gateways: " + ex.Message);
                        Console.ResetColor();
                    }
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

                // TENANT ID is required when using a Service Principal
                var tenantSpecificURL = authorityUrl.Replace("organizations", tenantId);

                var authenticationContext = new AuthenticationContext(tenantSpecificURL);

                authenticationResult = await authenticationContext.AcquireTokenAsync(resourceUrl, credential);

                if (authenticationResult != null)
                {
                    tokenCredentials = new TokenCredentials(authenticationResult.AccessToken, "Bearer");
                }
            });
        }






    }
}
