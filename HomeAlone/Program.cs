using HomeAlone;
using HomeAlone.Lights;
using HomeAlone.Scheduler;
using Microsoft.Extensions.Options;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection("Application"));

builder.Services.AddTransient<ActionSenderFactory>();
builder.Services.AddTransient<LightActionJob>();
builder.Services.AddSingleton<IConfigureOptions<QuartzOptions>, ScheduleConfigurator>(); 
builder.Services.AddQuartz();
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

var host = builder.Build();
await host.RunAsync();