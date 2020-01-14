using QM.Utility.Extensions;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QM.Utility.Quartz
{
    public class CustomJobListener : IJobListener
    {
        public string Name => "CustomJobListener";

        public async Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            await Task.Run(() =>
            {
                Console.WriteLine($"CustomJobListener JobExecutionVetoed {context.JobDetail.Description}");
            });
        }

        public async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            await Task.Run(() =>
            {
                Console.WriteLine($"CustomJobListener JobToBeExecuted {context.JobDetail.Description}");
            });
        }

        /// <summary>
        /// 调用结束
        /// </summary>
        /// <param name="context"></param>
        /// <param name="jobException"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default(CancellationToken))
        {
            var model = context.GetQuartzOptions();
            var job = QuartzExtensions.jobExeInfos.FirstOrDefault(p => p.JobId == model.Id);
            if (job != null)
                job.ExeCount++;
            else
                QuartzExtensions.jobExeInfos.Add(new JobExeInfo() { JobId = model.Id, JobName = model.TaskName, ExeCount = 1 });
            if (jobException != null)
                Console.WriteLine($"{model.TaskName} 发生异常：{jobException.InnerException}");
            model.LastRunTime = DateTime.Now;
            await Task.Run(() =>
            {
                Console.WriteLine($"CustomJobListener JobWasExecuted {context.JobDetail.Description}");
            });
        }
    }
}
