using HomeAlone.Lights;

namespace HomeAlone.Scheduler;

internal record JobInformation(
    string CronExpression,
    Relais Relais,
    LightActions Action,
    TimeSpan Jitter,
    string Description
);