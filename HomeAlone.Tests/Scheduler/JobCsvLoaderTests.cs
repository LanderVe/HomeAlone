using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HomeAlone.Lights;
using HomeAlone.Scheduler;
using Xunit;

namespace HomeAlone.Tests.Scheduler
{
    public class JobCsvLoaderTests
    {
        [Fact]
        public async Task LoadJobsFromCsv_ValidCsvFile_ReturnsJobsCorrectly()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            try
            {
                var csvContent = "Time;Relais;Action;Jitter;Description\n" +
                                 "0 0 20 * * ?;2.4;Off;10;Bedtijd kinderen\n" +
                                 "0 15 7 * * ?;1.2;On;5";
                await File.WriteAllTextAsync(tempFile, csvContent);

                // Act
                var jobs = new List<JobInformation>();
                var enumerator = JobCsvLoader.LoadJobsFromCsv(tempFile);

                foreach (var job in enumerator)
                {
                    jobs.Add(job);
                }

                // Assert
                Assert.Equal(2, jobs.Count);

                var firstJob = jobs[0];
                Assert.Equal((byte)2, firstJob.Relais.ModuleId);
                Assert.Equal((byte)4, firstJob.Relais.ChannelId);
                Assert.Equal(LightActions.Off, firstJob.Action);
                Assert.Equal(TimeSpan.FromSeconds(10), firstJob.Jitter);
                Assert.Equal("Bedtijd kinderen", firstJob.Description);

                var secondJob = jobs[1];
                Assert.Equal((byte)1, secondJob.Relais.ModuleId);
                Assert.Equal((byte)2, secondJob.Relais.ChannelId);
                Assert.Equal(LightActions.On, secondJob.Action);
                Assert.Equal(TimeSpan.FromSeconds(5), secondJob.Jitter);
                Assert.Equal(string.Empty, secondJob.Description);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
