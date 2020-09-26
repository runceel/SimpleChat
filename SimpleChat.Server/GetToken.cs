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
        // �댯�I����Ȃ��Ƃ͖{�Ԃł�����炾�߁I�I��΁I�I�����ǁA�Ƃ肠�����̂������Ȃ̂� static �ϐ��ɃX���b�h�N���C�A���g��ێ����܂�
        private static ChatThreadClient ChatThreadClient { get; set; }
        // �����ɃG���h�|�C���g�̒l���\����񂩂�ݒ肷��
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

                // �X���b�h ID ���Ȃ���������B�����A�N�Z�X������Ǝ��ʃ��W�b�N�Ȃ̂Ŗ{�ԂŃ}�l���Ȃ��łˁI
                var chat = new ChatClient(
                    new Uri(_endpoint),
                    new CommunicationUserCredential(tokenResponse.Value.Token));
                if (ChatThreadClient == null)
                {
                    // Thread �������ꍇ
                    ChatThreadClient = await chat.CreateChatThreadAsync(
                        "talk",
                        new[] { new ChatThreadMember(user) });
                }
                else
                {
                    // Thread ������ꍇ�͎Q�����Ă���
                    await ChatThreadClient.AddMembersAsync(new[] { new ChatThreadMember(user) });
                }

                return new OkObjectResult(new GetTokenResponse
                {
                    UserId = tokenResponse.Value.User.Id,
                    Token = tokenResponse.Value.Token,
                    ExpiresOn = tokenResponse.Value.ExpiresOn,
                    ThreadId = ChatThreadClient.Id, // ThreadId ���Ԃ�
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
