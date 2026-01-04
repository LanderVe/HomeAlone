using HomeAlone.Lights;

namespace HomeAlone.Scheduler;

internal static class JobCsvLoader
{
    private static readonly Relais dummyRelais = new() { ModuleId = 0, ChannelId = 0 };

    /// <summary>
    /// Reads job definitions from a CSV file and returns an enumerator that yields each job as a <see cref="JobInformation"/>
    /// object.
    /// </summary>
    /// <remarks>The CSV file is expected to have a header row and at least three columns per job entry. Lines
    /// with insufficient columns are ignored. The method does not validate the content of each field beyond basic
    /// parsing; invalid data may result in exceptions during enumeration.</remarks>
    /// <param name="csvFilePath">The full path to the CSV file containing job definitions. The file must exist and be accessible for reading.</param>
    /// <returns>An enumerator that yields <see cref="JobInformation"/> objects parsed from each row of the CSV file, excluding the header
    /// row. Rows that do not contain the required fields are skipped.</returns>
    public static IEnumerable<JobInformation> LoadJobsFromCsv(string csvFilePath, char separator = ';')
    {
        var lines =  File.ReadAllLines(csvFilePath);

        foreach (var line in lines.Skip(1))
        {
            ReadOnlySpan<char> trimmedLine = line.AsSpan().Trim();
            int index = 0;

            string cronExpression = string.Empty;
            Relais relais = dummyRelais;
            LightActions action = LightActions.Off;
            TimeSpan jitter = TimeSpan.Zero;
            string description = string.Empty;

            foreach (var range in trimmedLine.Split(separator))
            {
                var value = trimmedLine[range].Trim();

                if (index == 0)
                {
                    cronExpression = value.ToString();
                }
                else if (index == 1)
                {
                    relais = Relais.Parse(value);
                }
                else if (index == 2)
                {
                    action = Enum.Parse<LightActions>(value, ignoreCase: true);
                }
                else if (index == 3)
                {
                    jitter = TimeSpan.FromSeconds(int.Parse(value));
                }
                else if (index == 4)
                {
                    description = value.ToString();
                }

                index++;
            }

            if (index < 3)
                continue;

            // since index is at least 3 here, we have valid cronExpression and relais
            JobInformation info = new(cronExpression, relais, action, jitter, description);

            yield return info;

        }

    }
}
