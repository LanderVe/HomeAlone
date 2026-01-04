using HomeAlone.Lights;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HomeAlone.Tests.Lights
{
    public class ActionSenderTests
    {
        private static readonly IPAddress TestTarget = IPAddress.Parse("192.168.0.147");
        private const ushort TestPort = 10001;

        [Fact(Skip = "Only for local testing")]
        public async Task TrySendLightAction_Normal()
        {
            var sender = new ActionSender(TestTarget, TestPort, NullLogger<ActionSender>.Instance)
            {
                TimeoutPerAttempt = TimeSpan.FromDays(1)
            };

            for (int i = 0; i < 10; i++)
            {
                bool result = await sender.TrySendLightAction(new Relais(3, 4), LightActions.On, CancellationToken.None);
                await Task.Delay(1000);
                bool result2 = await sender.TrySendLightAction(new Relais(3, 4), LightActions.Off, CancellationToken.None);
                await Task.Delay(1000);
            }

            Assert.True(true);
        }

    }
}
