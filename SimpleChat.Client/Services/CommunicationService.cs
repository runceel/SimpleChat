using Azure;
using Azure.Communication.Chat;
using Azure.Communication.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SimpleChat.Core;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleChat.Client.Services
{
    public class CommunicationService
    {
        private static string ThreadId => "hello-world-sample";
        private readonly CommunicationServiceSettings _communicationServiceSettings;
        private readonly HttpClient _http;
        private GetTokenResponse _getTokenResponse;
        private ChatClient _chatClient;
        private ChatThreadClient _chatThreadClient;
        public CommunicationService(CommunicationServiceSettings communicationServiceSettings, HttpClient http)
        {
            _communicationServiceSettings = communicationServiceSettings;
            _http = http;
        }

        public bool IsJoined => _chatThreadClient != null;

        // チャットに参加
        public async ValueTask JoinToChatAsync()
        {
            var res = await _http.GetStringAsync("/api/GetToken");
            _getTokenResponse = JsonSerializer.Deserialize<GetTokenResponse>(res,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });

            _chatClient = new ChatClient(
                new Uri(_communicationServiceSettings.Endpoint), 
                new CommunicationUserCredential(_getTokenResponse.Token));
            _chatThreadClient =  _chatClient.GetChatThreadClient(_getTokenResponse.ThreadId);
        }

        // チャットにメッセージを送信
        public async ValueTask SendMessageAsync(string name, string message)
        {
            await _chatThreadClient.SendMessageAsync(message, senderDisplayName: name);
        }

        // チャットのメッセージを取得
        public AsyncPageable<ChatMessage> GetMessagesAsync() => 
            _chatThreadClient.GetMessagesAsync();
    }
}
