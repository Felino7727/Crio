using System;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Terrasoft.Core.Configuration;
using Terrasoft.Core;
using Newtonsoft.Json;
using ExternalSystemsIntegration.Files.cs.Exchange.DTO;

namespace ExternalSystemsIntegration.Files.cs.Exchange.Auth
{
    /// <summary> Авторизация ApiUm </summary>
    class ApiUmAuth
    {
        /// <summary> Получение токена доступа </summary>
        /// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
        public async Task Authorization(UserConnection userConnection)
        {
            string uri = string.Empty;
            string token = string.Empty;
            string clientId = string.Empty;
            string username = string.Empty;
            string password = string.Empty;
            string setToken = "UsrTokenApiUmStarIs";
            string setTokenLifetime = "UsrTokenLifetimeApiUmStarIs";
            try
            {
                uri = Convert.ToString(SysSettings.GetValue(userConnection, "UsrUriApiUmAuth"));
                token = Convert.ToString(SysSettings.GetValue(userConnection, "UsrAuthorizationApiUmAuthStarIs"));
                clientId = Convert.ToString(SysSettings.GetValue(userConnection, "UsrClientIdApiUmAuthStarIs"));
                username = Convert.ToString(SysSettings.GetValue(userConnection, "UsrUsernameApiUmAuthStarIs"));
                password = Convert.ToString(SysSettings.GetValue(userConnection, "UsrPasswordApiUmAuthStarIs"));
            }
            catch (Exception ex) { Logger.WriteToOrderLog("ApiUmAuth.Authorization.SysSettings.Exception", ex.Message, userConnection); }

            if (!string.IsNullOrEmpty(uri) && !string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                try
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Authorization", token);
                    string body = "grant_type=password" + "&" + $"client_id={clientId}" + "&" + $"username={username}" + "&" + $"password={password}";
                    HttpContent content = new StringContent(body, Encoding.UTF8);
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                    HttpResponseMessage requestStream = await client.PostAsync(new Uri(uri), content);
                    requestStream.EnsureSuccessStatusCode();
                    string responseStream = await requestStream.Content.ReadAsStringAsync();
                    AuthorizationResponse response = JsonConvert.DeserializeObject<AuthorizationResponse>(responseStream);
                    Logger.WriteToOrderLog("ApiUmAuth.Authorization.Response", responseStream, userConnection);

                    try
                    {
                        double timeStep = Convert.ToDouble(SysSettings.GetValue(userConnection, "UsrTimeOffsetStepTokenLifetimeApiUmStarIs", 3300));
                        DateTime tokenLifetime = DateTime.Now.AddSeconds(timeStep);
                        SysSettings.SetDefValue(userConnection, setToken, response.AccessToken);
                        SysSettings.SetDefValue(userConnection, setTokenLifetime, tokenLifetime);
                    }
                    catch (Exception ex) { Logger.WriteToOrderLog("ApiUmAuth.Authorization.SysSettings.tokenLifetime.Exception", ex.Message, userConnection); }
                }
                catch (HttpRequestException ex) { Logger.WriteToOrderLog("ApiUmAuth.Authorization.HttpRequestException", ex.Message, userConnection); }
                catch (Exception ex) { Logger.WriteToOrderLog("ApiUmAuth.Authorization.Exception", ex.Message, userConnection); }
            }
            else { Logger.WriteToOrderLog("ApiUmAuth.Authorization.SysSettings", $"IsNullOrEmpty in SysSettings", userConnection); }
        }

        /// <summary> Получение токена доступа </summary>
        /// <param name="uri"> Значение системной настройки [UriApiUmAuth] </param>
        /// <param name="token"> Значение системной настройки [AuthorizationApiUmAuth] </param>
        /// <param name="timeOffsetStep"> Значение системной настройки [TimeOffsetStepTokenLifetimeApiUm] </param>
        /// <param name="clientId"> Значение системной настройки [ClientIdApiUmAuth] </param>
        /// <param name="username"> Значение системной настройки [UsernameApiUmAuth] </param>
        /// <param name="password"> Значение системной настройки [PasswordApiUmAuth] </param>
        /// <param name="setToken"> Код системной настройки [TokenApiUm] </param>
        /// <param name="setTokenLifetime"> Код системной настройки [TokenLifetimeApiUm] </param>
        /// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
        public async Task Authorization(string uri, string token, double timeOffsetStep, string clientId, string username, string password, string setToken, string setTokenLifetime, UserConnection userConnection)
        {
            if (!string.IsNullOrEmpty(uri) && !string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                try
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Authorization", token);
                    string body = "grant_type=password" + "&" + $"client_id={clientId}" + "&" + $"username={username}" + "&" + $"password={password}";
                    HttpContent content = new StringContent(body, Encoding.UTF8);
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                    HttpResponseMessage requestStream = await client.PostAsync(new Uri(uri), content);
                    requestStream.EnsureSuccessStatusCode();
                    string responseStream = await requestStream.Content.ReadAsStringAsync();
                    AuthorizationResponse response = JsonConvert.DeserializeObject<AuthorizationResponse>(responseStream);
                    Logger.WriteToOrderLog("ApiUmAuth.Authorization.Response", responseStream, userConnection);
                    try
                    {
                        DateTime tokenLifetime = DateTime.Now.AddSeconds(timeOffsetStep);
                        SysSettings.SetDefValue(userConnection, setToken, response.AccessToken);
                        SysSettings.SetDefValue(userConnection, setTokenLifetime, tokenLifetime);
                    }
                    catch (Exception ex) { Logger.WriteToOrderLog("ApiUmAuth.Authorization.SysSettings.tokenLifetime.Exception", ex.Message, userConnection); }
                }
                catch (HttpRequestException ex) { Logger.WriteToOrderLog("ApiUmAuth.Authorization.HttpRequestException", ex.Message, userConnection); }
                catch (Exception ex) { Logger.WriteToOrderLog("ApiUmAuth.Authorization.Exception", ex.Message, userConnection); }
            }
            else { Logger.WriteToOrderLog("ApiUmAuth.Authorization.SysSettings", $"IsNullOrEmpty in SysSettings" , userConnection); }
        }
    }
}