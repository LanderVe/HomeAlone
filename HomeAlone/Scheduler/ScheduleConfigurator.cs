using Microsoft.Extensions.Options;
using Quartz;
using System.Text.Json;

namespace HomeAlone.Scheduler;

internal class ScheduleConfigurator(IOptions<ApplicationSettings> appSettings)
    : IConfigureOptions<QuartzOptions>
{
    public void Configure(QuartzOptions options)
    {
        string csvPath = appSettings.Value.JobsCsvPath;
        var jobInfos = JobCsvLoader.LoadJobsFromCsv(csvPath);

        foreach (var jobInfo in jobInfos)
        {
            JobKey jobKey = new(Guid.NewGuid().ToString("N"), "LightActionJobs");

            options.AddJob<LightActionJob>(jobDetailOptions =>
            {
                jobDetailOptions.WithIdentity(jobKey);
                jobDetailOptions.WithDescription(jobInfo.Description);
                var jobInfoJson = JsonSerializer.Serialize(jobInfo);
                jobDetailOptions.UsingJobData(LightActionJob.JobInfoKey, jobInfoJson);
            });

            options.AddTrigger(triggerOptions =>
            {
                triggerOptions.ForJob(jobKey);
                triggerOptions.WithCronSchedule(jobInfo.CronExpression);
                triggerOptions.WithDescription($"Trigger for job '{jobInfo.Description}'");
            });
        }
    }
}
