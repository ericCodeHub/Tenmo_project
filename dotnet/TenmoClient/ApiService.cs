using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using RestSharp;
using RestSharp.Authenticators;
using TenmoClient.Data;

namespace TenmoClient
{
    public class ApiService
    {

        public string UNAUTHORIZED_MSG { get { return "Authorization is required for this endpoint. Please log in."; } }
        public string FORBIDDEN_MSG { get { return "You do not have permission to perform the requested action"; } }
        public string OTHER_4XX_MSG { get { return "Error occurred - received non-success response: "; } }

        private RestClient client = new RestClient();
        private const string API_URL = "https://localhost:44315/";

        public ApiService()
        {
            
        }

        public void SetAuth(string token)
        {
            client.Authenticator = new JwtAuthenticator(token);
        }

        public User GetUser(string userName)
        {
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            RestRequest request = new RestRequest(API_URL + "user/" + userName);
            IRestResponse<User> response = client.Get<User>(request);
            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                ProcessErrorResponse(response);
            }
            else
            {
                return response.Data;
            }
            return null;
        }

        public List<User> GetUsers()
        {
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            RestRequest request = new RestRequest(API_URL + "user");
            IRestResponse<List<User>> response = client.Get<List<User>>(request);
            if(response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                ProcessErrorResponse(response);
            }
            else
            {
                return response.Data;
            }
            return null;
        }
        public decimal GetBalance()
        {
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            RestRequest request = new RestRequest(API_URL + "user/balance");
            IRestResponse<decimal> response = client.Get<decimal>(request);

            if(response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                ProcessErrorResponse(response);
            }
            else
            {
                return response.Data;
            }
            return 0;
        }
        public List<Transfer> ShowTransfers()
        {
            //List<Transfer> list = new List<Transfer>();
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            RestRequest request = new RestRequest(API_URL + "user/transfers");
            IRestResponse<List<Transfer>> response = client.Get < List<Transfer>>(request);

            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                ProcessErrorResponse(response);
            }
            else
            {
                return response.Data;
            }
            return null;


        }

        public Transfer ShowTransfer(int transferId)
        {
            //List<Transfer> list = new List<Transfer>();
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            RestRequest request = new RestRequest(API_URL + "user/transfers/" + transferId);
            IRestResponse<Transfer> response = client.Get<Transfer>(request);

            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                ProcessErrorResponse(response);
            }
            else
            {
                return response.Data;
            }
            return null;


        }
        public Transfer SendTeBucks(decimal transactionAmount, int recipient)
        {
            client.Authenticator=new JwtAuthenticator(UserService.GetToken());
            RestRequest request = new RestRequest($"{API_URL}user/{transactionAmount}/{recipient}");
            IRestResponse<Transfer> response = client.Put<Transfer>(request);
            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                ProcessErrorResponse(response);
            }
            else
            {
                return response.Data;
            }
            return null;
        }

        public Transfer RequestTeBucks(decimal transactionAmount, int sender)
        {
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            RestRequest request = new RestRequest($"{API_URL}user/request/{transactionAmount}/{sender}");
            IRestResponse<Transfer> response = client.Put<Transfer>(request);
            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                ProcessErrorResponse(response);
            }
            else
            {
                return response.Data;
            }
            return null;
        }


        public string ProcessErrorResponse(IRestResponse response)
        {
            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                return "Error occurred - unable to reach server.";
            }
            else if (!response.IsSuccessful)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return UNAUTHORIZED_MSG;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return FORBIDDEN_MSG;
                }
            }
            return OTHER_4XX_MSG;
        }
    }
}
