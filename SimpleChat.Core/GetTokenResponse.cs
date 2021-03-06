﻿using System;

namespace SimpleChat.Core
{
    public class GetTokenResponse
    {
        public string Token { get; set; }
        public DateTimeOffset ExpiresOn { get; set; }
        public string UserId { get; set; }
        public string ThreadId { get; set; }
    }
}
