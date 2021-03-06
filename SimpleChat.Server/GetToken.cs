using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Communication.Administration;
using SimpleChat.Core;
using Azure.Communication;
using Azure.Communication.Chat;
using Microsoft.Extensions.Configuration;
using Azure.Communication.Identity;

namespace SimpleChat.Server
{
    public class GetToken
    {
        // 危険！こんなことは本番でやったらだめ！！絶対！！だけど、とりあえずのお試しなので static 変数にスレッドクライアントを保持します
        private static ChatThreadClient ChatThreadClient { get; set; }
        // ここにエンドポイントの値を構成情報から設定する
        private readonly string _endpoint;
        private readonly CommunicationIdentityClient _communicationIdentityClient;

        public GetToken(CommunicationIdentityClient communicationIdentityClient, IConfiguration configuration)
        {
            _communicationIdentityClient = communicationIdentityClient ?? throw new ArgumentNullException(nameof(communicationIdentityClient));
            _endpoint = configuration.GetValue<string>("AzureCommunicationServicesEndpoint");
        }

        [FunctionName(nameof(GetToken))]
        public  async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
            ILogger log)
        {
            var id = req.Query["id"].ToString();

            var user = string.IsNullOrWhiteSpace(id) ?
                (await _communicationIdentityClient.CreateUserAsync()).Value :
                new CommunicationUser(id);

            try
            {
                var tokenResponse = await _communicationIdentityClient.IssueTokenAsync(user, new[] { CommunicationTokenScope.Chat });

                // スレッド ID がなかったら作る。同時アクセスがあると死ぬロジックなので本番でマネしないでね！
                var chat = new ChatClient(
                    new Uri(_endpoint),
                    new CommunicationUserCredential(tokenResponse.Value.Token));
                if (ChatThreadClient == null)
                {
                    // Thread が無い場合
                    ChatThreadClient = await chat.CreateChatThreadAsync(
                        "talk",
                        new[] { new ChatThreadMember(user) });
                }
                else
                {
                    // Thread がある場合は参加しておく
                    await ChatThreadClient.AddMembersAsync(new[] { new ChatThreadMember(user) });
                }

                return new OkObjectResult(new GetTokenResponse
                {
                    UserId = tokenResponse.Value.User.Id,
                    Token = tokenResponse.Value.Token,
                    ExpiresOn = tokenResponse.Value.ExpiresOn,
                    ThreadId = ChatThreadClient.Id, // ThreadId も返す
                });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "IssureTokenAsync");
                return new BadRequestResult();
            }
        }
    }
}
