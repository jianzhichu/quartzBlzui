using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QM.Utility.Quartz
{
    public class MyFirstJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {

            //throw new NotImplementedException("MyFirstJob error");
            return Task.Run(() =>
            {
                if (context.JobDetail.JobDataMap.Get("MyFirstJob") != null)
                {
                    var paramsData = JsonConvert.SerializeObject(context.JobDetail.JobDataMap.Get("MyFirstJob"));
                    Console.WriteLine($"获取参数： {paramsData}");
                }
            });


        }
    }
}
