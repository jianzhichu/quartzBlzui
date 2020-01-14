using QM.Utility.Extensions;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QM.Utility.Quartz
{
    //[PersistJobDataAfterExecution]//执行后保留数据,更新JobDataMap
    [DisallowConcurrentExecution]//拒绝同一时间重复执行，同一任务串行
    public class HttpJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            #region 获取参数
            //JobDataMap dataMap = context.JobDetail.JobDataMap;
            //var data = dataMap.Get(this.ToString());
            //if (data == null)
            //{
            //   dataMap = context.Trigger.JobDataMap;
            //    data = dataMap.Get(this.ToString());
            //}
            //if (data != null)
            //{
            //    Console.WriteLine(data);
            //}
            #endregion


            var options = context.GetQuartzOptions();//获取任务配置
            if (options.ExecuteType == ExecuteType.Post)
            {
                var result = await HttpHelper.HttpPostAsync(options.TaskTarget, options.TaskData == null ? null : JsonConvert.SerializeObject(options.TaskData));
                Console.WriteLine($"post-请求：{options.TaskTarget} 结果：{result}");
            }
            else
            {
                var result = await HttpHelper.HttpGetAsync(options.TaskTarget);
                Console.WriteLine($"get-请求：{options.TaskTarget} 结果：{result}");
            }
        }
    }
}
