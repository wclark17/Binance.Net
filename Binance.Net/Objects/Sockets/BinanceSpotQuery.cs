﻿using Binance.Net.Objects.Internal;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binance.Net.Objects.Sockets
{
    internal class BinanceSpotQuery<T> : Query<T> where T : BinanceResponse
    {
        public override HashSet<string> ListenerIdentifiers { get; set; }

        public BinanceSpotQuery(BinanceSocketQuery request, bool authenticated, int weight = 1) : base(request, authenticated, weight)
        {
            ListenerIdentifiers = new HashSet<string> { request.Id.ToString() };
        }

        public override Task<CallResult<T>> HandleMessageAsync(SocketConnection connection, DataEvent<T> message)
        {
            if (message.Data.Status != 200)
            {
                if (message.Data.Status == 419 || message.Data.Status == 428)
                {
                    // Rate limit error 
                    return Task.FromResult(new CallResult<T>(new BinanceRateLimitError(message.Data.Error!.Code, message.Data.Error!.Message, null)
                    {
                        RetryAfter = message.Data.Error.Data!.RetryAfter
                    }, message.OriginalData));
                }

                return Task.FromResult(new CallResult<T>(new ServerError(message.Data.Error!.Code, message.Data.Error!.Message), message.OriginalData));
            }

            return Task.FromResult(new CallResult<T>(message.Data, message.OriginalData, null));
        }
    }
}
