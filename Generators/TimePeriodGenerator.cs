using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataGenerator.Generators
{
    public class TimePeriodGenerator
    {
        private static Random Random { get; } = new Random();

        private long _count;
        public TimePeriodGenerator(int maxCount)
        {
            _count = maxCount;
        }
        // This generates 
        public async Task<IEnumerable<(DateTime start, DateTime stop)>> Generate(DateTime start,
                                                                                 DateTime stop,
                                                                                 TimeSpan minDuration,
                                                                                 TimeSpan maxDuration)
        {
            if (stop.Subtract(start) < minDuration)
            {
                return new List<(DateTime, DateTime)>();
            }

            DateTime t1, t2;
            if (Interlocked.Read(ref _count) <= 0)
            {
                return new List<(DateTime, DateTime)>();
            }
            else
            {
                // assume we will succeed in creating a new span
                Interlocked.Decrement(ref _count);

                t1 = RandomDateTimeBetween(start, stop);

                var actualMaxDuration = stop.Subtract(t1).Ticks > maxDuration.Ticks
                    ? maxDuration
                    : TimeSpan.FromTicks(stop.Subtract(t1).Ticks);

                if (actualMaxDuration.Ticks > minDuration.Ticks)
                {
                    actualMaxDuration = actualMaxDuration.Subtract(minDuration);
                }

                var duration = (long)(Random.NextDouble() * long.MaxValue) % actualMaxDuration.Ticks + minDuration.Ticks;
                t2 = t1.AddTicks(duration);
            }

            var olderPeriodTask = Generate(start, t1, minDuration, maxDuration);
            var newerPeriodTask = Generate(t2, stop, minDuration, maxDuration);
            await Task.WhenAll(olderPeriodTask, newerPeriodTask);

            return olderPeriodTask.Result.Append((t1, t2)).Concat(newerPeriodTask.Result);
        }

        public static DateTime RandomDateTimeBetween(DateTime oldest, DateTime newest)
        {
            if (oldest > newest)
            {
                throw new Exception("Wrong argument order");
            }
            var maxDuration = newest.Subtract(oldest).Ticks;
            var duration = (long) (Random.NextDouble() * long.MaxValue) % maxDuration;

            return oldest.AddTicks(duration);
        }
    }
}