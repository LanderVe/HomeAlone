using HomeAlone.Lights;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System.Net;
using System.Text.Json;

namespace HomeAlone.Scheduler;

/// <summary>
/// Sample job that turns off a light via ActionSender.
/// </summary>
internal class LightActionJob(IOptions<ApplicationSettings> applicationSettings, ActionSenderFactory actionSenderFactory, ILogger<LightActionJob> logger)
    : IJob
{
    public const string JobInfoKey = "jobInfo";

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            // retrieve information from job data map
            var targetIpAddress = IPAddress.Parse(applicationSettings.Value.TargetIp);
            var targetPort = applicationSettings.Value.TargetPort;

            var jobData = context.JobDetail.JobDataMap;
            string? jobInfoJson = jobData.GetString(JobInfoKey) 
                ?? throw new InvalidDataException($"No valid job info for job {context.JobDetail.Description}");

            var jobInfo = JsonSerializer.Deserialize<JobInformation>(jobInfoJson) 
                ?? throw new InvalidDataException($"Failed to deserialize job info for job {context.JobDetail.Description}");

            // apply random jitter before executing the job
            int jitterMilliseconds = Random.Shared.Next(0, (int)jobInfo.Jitter.TotalMilliseconds);
            await Task.Delay(jitterMilliseconds, context.CancellationToken);

            // create action sender and send the action
            var sender = actionSenderFactory.Create(targetIpAddress, targetPort);
            bool success = await sender.TrySendLightAction(jobInfo.Relais, jobInfo.Action, context.CancellationToken);

            if (success)
            {
                logger.LogInformation("Successfully turned {Action} light at {Relais}.", jobInfo.Action, jobInfo.Relais);
            }
            else
            {
                logger.LogWarning("Failed to turn {Action} light at {Relais}.", jobInfo.Action, jobInfo.Relais);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing {JobName}.", nameof(LightActionJob));
        }
    }
}
