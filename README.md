# clientApp-call-TodoListServiceAPI-Protected-by-AAD-and-call-landingPage-heartbeat-clientCredentialsF

- How to call AAD protected Api by a client app using client credenials flow and using ADAL to aquire access token
#### this app shows how to build API and protects it by Azure Active Directory to accept bearer token.

``` Ruby
// This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //add authentication to DI container defining bearer token technique rather than cookies to authenticate
            services.AddAuthentication(AzureADDefaults.BearerAuthenticationScheme)
                //add the handler to validate the bearer token.
                .AddAzureADBearer(options => Configuration.Bind("AzureAd", options));
            //Add Api Versioning
            services.AddApiVersioning();
            //Add MVC
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }
end
```

- Consume protected Api by client app
##### this client app uses ADAL to aquire access token

``` Ruby
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
end
```
