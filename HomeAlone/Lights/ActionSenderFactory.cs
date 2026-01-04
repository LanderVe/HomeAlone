using Microsoft.Extensions.Logging;
using System.Net;

namespace HomeAlone.Lights;

internal class ActionSenderFactory(ILoggerFactory loggerFactory)
{
    public ActionSender Create(IPAddress ipAddress, ushort port)
    {
        return new ActionSender(ipAddress, port, loggerFactory.CreateLogger<ActionSender>());
    }
}
