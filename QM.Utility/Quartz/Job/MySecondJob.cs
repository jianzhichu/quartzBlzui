using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QM.Utility.Quartz
{
    public class MySecondJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(() => Console.WriteLine($"this is MySecondJob {context.JobDetail.JobDataMap.Get("MySecondJob") ?? JsonConvert.SerializeObject(context.JobDetail.JobDataMap.Get("MySecondJob"))}"));


        }
    }
}
