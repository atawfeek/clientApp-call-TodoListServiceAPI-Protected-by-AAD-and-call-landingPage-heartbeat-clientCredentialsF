using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;

namespace TodoListDaemonWebClient
{
    public class Program
    {

        //
        // The Client ID is used by the application to uniquely identify itself to Azure AD.
        // The App Key is a credential used by the application to authenticate to Azure AD.
        // The Tenant is the name of the Azure AD tenant in which this application is registered.
        // The AAD Instance is the instance of Azure, for example public Azure or Azure China.
        // The Authority is the sign-in URL of the tenant.
        //
        private static string aadInstance;
        private static string tenant;
        private static string clientId;
        private static string appKey;
        static string authority;
        //
        // To authenticate to the To Do list service, the client needs to know the service's App ID URI.
        // To contact the To Do list service we need it's URL as well.
        //
        private static string todoListResourceId;
        private static string todoListBaseAddress;

        /// <summary>
        /// Heartbeat
        /// </summary>
        private static string aadInstance_heartbeat;
        private static string tenant_heartbeat;
        private static string clientId_heartbeat;
        private static string appKey_heartbeat;
        static string authority_heartbeat;
        private static string resourceId_heartbeat;
        private static string apiUrl_heartbeat;

        private static HttpClient httpClient = new HttpClient();
        private static AuthenticationContext authContext = null;
        private static ClientCredential clientCredential = null;

        //heartbeat
        private static AuthenticationContext authContext_heartbeat = null;
        private static ClientCredential clientCredential_heartbeat = null;

        const string apiVersionParam = "2.0";//"api-version=3.0";

        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

            Configurations settings_Web = new Configurations();
            config.GetSection("AzureAd").Bind(settings_Web);

            ConfigurationsHeartbeat settings_Web_heartbeat = new ConfigurationsHeartbeat();
            config.GetSection("heartbeat").Bind(settings_Web_heartbeat);

            aadInstance = settings_Web.Instance;
            tenant = settings_Web.Tenant;
            clientId = settings_Web.ClientId;
            appKey = settings_Web.AppKey;
            authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
            todoListResourceId = settings_Web.TodoListResourceId;
            todoListBaseAddress = settings_Web.TodoListBaseAddress;

            ///heartbeat
            aadInstance_heartbeat = settings_Web_heartbeat.Instance;
            tenant_heartbeat = settings_Web_heartbeat.Tenant;
            clientId_heartbeat = settings_Web_heartbeat.ClientId;
            appKey_heartbeat = settings_Web_heartbeat.AppKey;
            authority_heartbeat = String.Format(CultureInfo.InvariantCulture, aadInstance_heartbeat, tenant_heartbeat);
            resourceId_heartbeat = settings_Web_heartbeat.ResourceId;
            apiUrl_heartbeat = settings_Web_heartbeat.ApiUrl;

            //
            // Call the To Do service 10 times with short delay between calls.
            //

            authContext = new AuthenticationContext(authority);
            clientCredential = new ClientCredential(clientId, appKey);

            GetTodo().Wait();


            //heartbeat
            authContext_heartbeat = new AuthenticationContext(authority_heartbeat);
            clientCredential_heartbeat = new ClientCredential(clientId_heartbeat, appKey_heartbeat);

            HeartbeatApi().Wait();


            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        /// <summary>
        /// call landing page API using client credentials flow - OpenID Connect
        /// </summary>
        /// <returns></returns>
        static async Task HeartbeatApi()
        {
            //
            // Get an access token from Azure AD using client credentials.
            // If the attempt to get a token fails because the server is unavailable, retry twice after 3 seconds each.
            //
            AuthenticationResult result = null;
            int retryCount = 0;
            bool retry = false;

            do
            {
                retry = false;
                try
                {
                    // ADAL includes an in memory cache, so this call will only send a message to the server if the cached token is expired.
                    result = await authContext_heartbeat.AcquireTokenAsync(resourceId_heartbeat, clientCredential_heartbeat);
                }
                catch (AdalException ex)
                {
                    if (ex.ErrorCode == "temporarily_unavailable")
                    {
                        retry = true;
                        retryCount++;
                        Thread.Sleep(3000);
                    }

                    Console.WriteLine(
                        String.Format("An error occurred while acquiring a token\nTime: {0}\nError: {1}\nRetry: {2}\n",
                        DateTime.Now.ToString(),
                        ex.ToString(),
                        retry.ToString()));
                }

            } while ((retry == true) && (retryCount < 3));

            if (result == null)
            {
                Console.WriteLine("Canceling attempt to contact To Do list service.\n");
                return;
            }

            //
            // Read items from the To Do list service.
            //

            // Add the access token to the authorization header of the request.
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

            // Call the To Do list service.
            Console.WriteLine("Retrieving To Do list at {0}", DateTime.Now.ToString());
            HttpResponseMessage response = await httpClient.GetAsync(apiUrl_heartbeat);

            if (response.IsSuccessStatusCode)
            {
                // Read the response and output it to the console.
                string s = await response.Content.ReadAsStringAsync();
                Heartbeat heartbeat = JsonConvert.DeserializeObject<Heartbeat>(s);

                Console.WriteLine(heartbeat.value);
                Console.WriteLine(heartbeat.diffrence);
                Console.WriteLine(heartbeat.time);
            }
            else
            {
                Console.WriteLine("Failed to retrieve To Do list\nError:  {0}\n", response.ReasonPhrase);
            }
        }

        static async Task GetTodo()
        {
            //
            // Get an access token from Azure AD using client credentials.
            // If the attempt to get a token fails because the server is unavailable, retry twice after 3 seconds each.
            //
            AuthenticationResult result = null;
            int retryCount = 0;
            bool retry = false;

            do
            {
                retry = false;
                try
                {
                    // ADAL includes an in memory cache, so this call will only send a message to the server if the cached token is expired.
                    result = await authContext.AcquireTokenAsync(todoListResourceId, clientCredential);
                }
                catch (AdalException ex)
                {
                    if (ex.ErrorCode == "temporarily_unavailable")
                    {
                        retry = true;
                        retryCount++;
                        Thread.Sleep(3000);
                    }

                    Console.WriteLine(
                        String.Format("An error occurred while acquiring a token\nTime: {0}\nError: {1}\nRetry: {2}\n",
                        DateTime.Now.ToString(),
                        ex.ToString(),
                        retry.ToString()));
                }

            } while ((retry == true) && (retryCount < 3));

            if (result == null)
            {
                Console.WriteLine("Canceling attempt to contact To Do list service.\n");
                return;
            }

            //
            // Read items from the To Do list service.
            //

            // Add the access token to the authorization header of the request.
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);

            // Call the To Do list service.
            Console.WriteLine("Retrieving To Do list at {0}", DateTime.Now.ToString());
            HttpResponseMessage response = await httpClient.GetAsync($"{todoListBaseAddress}/api/v{apiVersionParam}/todolist");

            if (response.IsSuccessStatusCode)
            {
                // Read the response and output it to the console.
                string s = await response.Content.ReadAsStringAsync();
                List<TodoItem> toDoArray = JsonConvert.DeserializeObject<List<TodoItem>>(s);

                int count = 0;
                foreach (TodoItem item in toDoArray)
                {
                    Console.WriteLine(item.Title);
                    count++;
                }

                Console.WriteLine("Total item count:  {0}\n", count);
            }
            else
            {
                Console.WriteLine("Failed to retrieve To Do list\nError:  {0}\n", response.ReasonPhrase);
            }
        }
    }
}
