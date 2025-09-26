using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace RaAI.UI
{
    public static class ServerCommunication
    {
        public static async Task<string> SendRequestAsync(string endpoint, string data)
        {
            using var client = new HttpClient();
            var response = await client.PostAsync(endpoint, new StringContent(data));
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> GetResponseAsync(string endpoint)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(endpoint);
            return await response.Content.ReadAsStringAsync();
        }
    }
}