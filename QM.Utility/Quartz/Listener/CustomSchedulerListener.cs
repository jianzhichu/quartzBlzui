using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QM.Utility.Quartz
{
    public class CustomSchedulerListener : ISchedulerListener
    {
        public async Task JobAdded(IJobDetail jobDetail, CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                Console.WriteLine($"This is {nameof(CustomSchedulerListener)} JobAdded {jobDetail.Description}");
            });
        }

        public Task JobDeleted(JobKey jobKey, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"This is {nameof(CustomSchedulerListener)} JobDeleted {jobKey.Name}");
            });
        }

        public Task JobInterrupted(JobKey jobKey, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"This is {nameof(CustomSchedulerListener)} JobInterrupted {jobKey.Name}");
            });
        }

        public Task JobPaused(JobKey jobKey, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"This is {nameof(CustomSchedulerListener)} JobPaused {jobKey.Name}");
            });
        }

        public Task JobResumed(JobKey jobKey, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"This is {nameof(CustomSchedulerListener)} JobResumed {jobKey.Name}");
            });
        }

        public Task JobScheduled(ITrigger trigger, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"This is {nameof(CustomSchedulerListener)} JobScheduled {trigger.Description}");
            });
        }

        public Task JobsPaused(string jobGroup, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"This is {nameof(CustomSchedulerListener)} JobsPaused {jobGroup}");
            });
        }

        public Task JobsResumed(string jobGroup, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"This is {nameof(CustomSchedulerListener)} JobsResumed {jobGroup}");
            });
        }

        public Task JobUnscheduled(TriggerKey triggerKey, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"This is {nameof(CustomSchedulerListener)} JobUnscheduled {triggerKey.Name}");
            });
        }

        public Task SchedulerError(string msg, SchedulerException cause, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"This is {nameof(CustomSchedulerListener)} SchedulerError {msg}");
            });
        }

        public Task SchedulerInStandbyMode(CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"This is {nameof(CustomSchedulerListener)} SchedulerInStandbyMode");
            });
        }

        public Task SchedulerShutdown(CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"This is {nameof(CustomSchedulerListener)} SchedulerShutdown");
            });
        }

        public Task SchedulerShuttingdown(CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"This is {nameof(CustomSchedulerListener)} SchedulerShuttingdown");
            });
        }

        public async Task SchedulerStarted(CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
           {
               Console.WriteLine($"This is {nameof(CustomSchedulerListener)} SchedulerStarted");
           });
        }

        public async Task SchedulerStarting(CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                Console.WriteLine($"This is {nameof(CustomSchedulerListener)} SchedulerStarting ");
            });
        }

        public Task SchedulingDataCleared(CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"This is {nameof(CustomSchedulerListener)} SchedulingDataCleared");
            });
        }

        public Task TriggerFinalized(ITrigger trigger, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"This is {nameof(CustomSchedulerListener)} TriggerFinalized {trigger.Description}");
            });
        }

        public Task TriggerPaused(TriggerKey triggerKey, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"This is {nameof(CustomSchedulerListener)} TriggerPaused {triggerKey.Name}");
            });
        }

        public Task TriggerResumed(TriggerKey triggerKey, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"This is {nameof(CustomSchedulerListener)} TriggerResumed {triggerKey.Name}");
            });
        }

        public Task TriggersPaused(string triggerGroup, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"This is {nameof(CustomSchedulerListener)} TriggersPaused {triggerGroup}");
            });
        }

        public Task TriggersResumed(string triggerGroup, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"This is {nameof(CustomSchedulerListener)} TriggersResumed {triggerGroup}");
            });
        }
    }
}
