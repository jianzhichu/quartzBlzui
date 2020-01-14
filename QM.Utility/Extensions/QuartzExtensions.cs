using QM.Utility.Quartz;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Quartz.Impl.Matchers;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Newtonsoft.Json;
using Quartz.Logging;

namespace QM.Utility.Extensions
{
    public static class QuartzExtensions
    {
        private static List<QuartzOption> _taskList = new List<QuartzOption>();
        public static List<JobExeInfo> jobExeInfos = new List<JobExeInfo>();
        private static readonly NLogger logger = new NLogger(typeof(QuartzExtensions).Name);
        private static IScheduler scheduler;

        private static string[] SimpleStrategy = new string[] { "HH", "mm", "ss" };

        /// <summary>
        /// 初始化作业
        /// </summary>
        /// <param name="applicationBuilder"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseCustomQuartz(this IApplicationBuilder applicationBuilder)
        {
            return UseCustomQuartz(applicationBuilder, null, null);
        }
        /// <summary>
        /// 初始化作业
        /// </summary>
        /// <param name="applicationBuilder"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseCustomQuartz(this IApplicationBuilder applicationBuilder, Action<IListenerManager> listenerManager)
        {
            return UseCustomQuartz(applicationBuilder, listenerManager, null);
        }
        /// <summary>
        /// 初始化作业
        /// </summary>
        /// <param name="applicationBuilder"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseCustomQuartz(this IApplicationBuilder applicationBuilder, List<QuartzOption> quartzOptions)
        {
            return UseCustomQuartz(applicationBuilder, null, quartzOptions);
        }
        /// <summary>
        /// 初始化作业
        /// </summary>
        /// <param name="applicationBuilder"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseCustomQuartz(this IApplicationBuilder applicationBuilder, Action<IListenerManager> listenerManager, List<QuartzOption> quartzOptions)
        {
            IServiceProvider services = applicationBuilder.ApplicationServices;
            ISchedulerFactory _schedulerFactory = services.GetService<ISchedulerFactory>();

            if (_schedulerFactory == null)
                throw new NullReferenceException("ISchedulerFactory is unRegistered");

            //StdSchedulerFactory factory = new StdSchedulerFactory();

            scheduler = _schedulerFactory?.GetScheduler().Result;
            listenerManager?.Invoke(scheduler.ListenerManager);

            LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());

            quartzOptions?.ForEach(p =>
            {
                var result = _schedulerFactory.AddJob(p).Result;
                logger.LogDebug(JsonConvert.SerializeObject(result));
            });
            return applicationBuilder;
        }
        /// <summary>
        /// 获取所有的作业
        /// </summary>
        /// <param name="schedulerFactory"></param>
        /// <returns></returns>
        public static async Task<List<QuartzOption>> GetJobs(this ISchedulerFactory schedulerFactory)
        {
            List<QuartzOption> list = new List<QuartzOption>();
            try
            {
                var groups = await scheduler.GetJobGroupNames();
                foreach (var groupName in groups)
                {
                    foreach (var jobKey in await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName)))
                    {
                        QuartzOption QuartzOptions = _taskList.Where(x => x.GroupName == jobKey.Group && x.TaskName == jobKey.Name)
                            .FirstOrDefault();
                        if (QuartzOptions == null)
                            continue;

                        var triggers = await scheduler.GetTriggersOfJob(jobKey);
                        foreach (ITrigger trigger in triggers)
                        {
                            DateTimeOffset? dateTimeOffset = trigger.GetPreviousFireTimeUtc();
                            QuartzOptions.LastRunTime = Convert.ToDateTime(dateTimeOffset.ToString());

                        }
                        list.Add(QuartzOptions);
                    }
                }
            }
            catch (Exception ex)
            {
                //FileQuartz.WriteStartLog("获取作业异常：" + ex.Message + ex.StackTrace);
            }
            return list;
        }






        /// <summary>
        /// 添加作业
        /// </summary>
        /// <param name="QuartzOptions"></param>
        /// <param name="schedulerFactory"></param>
        ///是否初始化,否=需要重新生成配置文件，是=不重新生成配置文件
        //trigger.JobDataMap, job.JobDataMap任选一个传参
        //trigger.JobDataMap.Add(typeof(HttpRequestJob).Name, Options.TaskData);
        /// <returns></returns>
        public static async Task<QuartzResult> AddJob(this ISchedulerFactory schedulerFactory, QuartzOption QuartzOptions)
        {
            if (QuartzOptions is null)
            {
                throw new ArgumentNullException(nameof(QuartzOptions));
            }
            if (scheduler == null)
            {
                scheduler = schedulerFactory?.GetScheduler().Result;
            }
            try
            {
                if (await scheduler.CheckExists(new JobKey(QuartzOptions.TaskName, QuartzOptions.GroupName)))
                    return QuartzResult.Error($"任务 {QuartzOptions.TaskName},任务组 {QuartzOptions.GroupName} 已存在");
                IJobDetail jobDetail = QuartzOptions.CreateJobDetail();
                if (jobDetail == null)
                {
                    return QuartzResult.Error($"创建jobDetail 失败");
                }
                ITrigger trigger = null;
                if (QuartzOptions.IntervalType == IntervalType.Cron)
                {
                    if (!CronExpression.IsValidExpression(QuartzOptions.Interval))
                        return QuartzResult.Error($"请确认表达式{QuartzOptions.Interval}是否正确!");

                    trigger = QuartzOptions.CreateCronTrigger();
                }
                else
                {
                    trigger = QuartzOptions.CreateSimpleTrigger();
                }

                _taskList.Add(QuartzOptions);

                await scheduler.ScheduleJob(jobDetail, trigger);
                if (QuartzOptions.TaskStatus == TriggerState.Normal)
                {
                    await scheduler.Start();
                }
                else
                {
                    await schedulerFactory.Pause(QuartzOptions);
                }
                logger.LogDebug($"作业:{QuartzOptions.TaskName},目标:{QuartzOptions.TaskTarget},分组:{QuartzOptions.GroupName},状态为:{QuartzOptions.TaskStatus}");
            }
            catch (Exception ex)
            {
                return QuartzResult.Error(ex.Message);
            }
            return QuartzResult.Ok($"作业:{QuartzOptions.TaskName},分组:{QuartzOptions.GroupName} 添加成功");


        }

        /// <summary>
        /// 添加任务调度（指定IJob实现类）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="QuartzOptions"></param>
        /// <returns></returns>
        public static async Task<QuartzResult> AddJob<T>(this ISchedulerFactory schedulerFactory, QuartzOption QuartzOptions) where T : IJob
        {
            if (QuartzOptions is null)
            {
                throw new ArgumentNullException(nameof(QuartzOptions));
            }
            if (scheduler == null)
            {
                scheduler = schedulerFactory?.GetScheduler().Result;
            }
            try
            {
                //检查任务是否已存在
                if (await scheduler.CheckExists(new JobKey(QuartzOptions.TaskName, QuartzOptions.GroupName)))
                    return QuartzResult.Error($"任务 {QuartzOptions.TaskName},任务组 {QuartzOptions.GroupName} 已存在");
                // 定义这个工作，并将其绑定到我们的IJob实现类
                IJobDetail jobDetail = CreateJobDetail<T>(QuartzOptions);//JobBuilder.CreateForAsync<T>().WithIdentity(Option.TaskName, Option.GroupName).Build();
                // 创建触发器
                ITrigger trigger;
                if (QuartzOptions.IntervalType == IntervalType.Cron)
                {
                    if (!CronExpression.IsValidExpression(QuartzOptions.Interval))
                        return QuartzResult.Error($"请确认表达式{QuartzOptions.Interval}是否正确!");
                    trigger = QuartzOptions.CreateCronTrigger();
                }
                else
                {
                    trigger = CreateSimpleTrigger(QuartzOptions);
                }
                // 设置监听器
                //JobListener listener = new JobListener();
                //// IMatcher<JobKey> matcher = KeyMatcher<JobKey>.KeyEquals(job.Key);
                //scheduler.ListenerManager.AddJobListener(listener, GroupMatcher<JobKey>.AnyGroup());

                await scheduler.ScheduleJob(jobDetail, trigger);
                if (QuartzOptions.TaskStatus == TriggerState.Normal)
                {
                    await scheduler.Start();
                }
                else
                {
                    await schedulerFactory.Pause(QuartzOptions);
                }
                logger.LogDebug($"作业:{QuartzOptions.TaskName},目标:{QuartzOptions.TaskTarget},分组:{QuartzOptions.GroupName},状态为:{QuartzOptions.TaskStatus}");
                return QuartzResult.Ok("添加成功");
            }
            catch (Exception ex)
            {
                logger.LogError($"添加任务出错--{ex.StackTrace}");
                return QuartzResult.Error($"添加任务出错--{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 创建一个SimpleTrigger
        /// </summary>
        /// <param name="QuartzOptions"></param>
        /// <returns></returns>
        private static ITrigger CreateSimpleTrigger(this QuartzOption QuartzOptions)
        {
            if (QuartzOptions is null)
            {
                throw new ArgumentNullException(nameof(QuartzOptions));
            }

            var intervalArr = QuartzOptions.Interval.Split(',');
            if (intervalArr.Length < 2 || !SimpleStrategy.Contains(intervalArr[0]) || !int.TryParse(intervalArr[1], out int time))
            {
                throw new IndexOutOfRangeException("CreateSimpleTrigger  轮询策略 错误，必须为 ss,时间,mm,时间,HH,时间 ");
            }
            var timeType = intervalArr[0];
            return QuartzOptions.GetTriggerBuilder()
                .WithSimpleSchedule(scheduleBuilder =>
               {
                   switch (timeType)
                   {
                       case "ss":
                           scheduleBuilder.WithIntervalInSeconds(time);
                           break;
                       case "mm":
                           scheduleBuilder.WithIntervalInMinutes(time);
                           break;
                       case "HH":
                           scheduleBuilder.WithIntervalInHours(time);
                           break;
                       default:
                           throw new NotImplementedException();
                   }
                   if (QuartzOptions.RunTimes > 0)
                       scheduleBuilder.WithRepeatCount(QuartzOptions.RunTimes);
                   else scheduleBuilder.RepeatForever();
               }).Build();

        }
        /// <summary>
        /// 创建类型Cron的触发器
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private static ITrigger CreateCronTrigger(this QuartzOption QuartzOptions)
        {
            if (QuartzOptions is null)
            {
                throw new ArgumentNullException(nameof(QuartzOptions));
            }
            return QuartzOptions.GetTriggerBuilder()
                .WithCronSchedule(QuartzOptions.Interval, x => x.WithMisfireHandlingInstructionDoNothing()).Build();
        }

        /// <summary>
        ///创建一个基础TriggerBuilder
        /// </summary>
        /// <param name="QuartzOptions"></param>
        /// <returns></returns>
        private static TriggerBuilder GetTriggerBuilder(this QuartzOption QuartzOptions)
        {
            // 作业触发器
            var triggerBuilder = TriggerBuilder.Create()
                   .WithIdentity(QuartzOptions.TaskName, QuartzOptions.GroupName)
                   .ForJob(QuartzOptions.TaskName, QuartzOptions.GroupName);//作业名称
            if (QuartzOptions.BeginTime == null)
            {
                triggerBuilder = triggerBuilder.StartNow();
            }
            else triggerBuilder = triggerBuilder.StartAt(((DateTime)QuartzOptions.BeginTime).ToDateTimeOffset());

            if (QuartzOptions.EndTime != null)
            {
                triggerBuilder = triggerBuilder.EndAt(((DateTime)QuartzOptions.EndTime).ToDateTimeOffset());
            }
            return triggerBuilder;
        }

        /// <summary>
        /// 创建CreateJobDetail
        /// </summary>
        /// <param name="QuartzOptions"></param>
        /// <returns></returns>
        private static IJobDetail CreateJobDetail(this QuartzOption QuartzOptions)
        {

            if (QuartzOptions.ExecuteType == ExecuteType.Asb)
            {
                return CreateJobDetailByAssembly(QuartzOptions);
            }
            else
            {
                return CreateJobDetailByApiUrl(QuartzOptions);
            }

        }
        /// <summary>
        /// 创建基于HTTP请求的任务
        /// </summary>
        /// <param name="QuartzOptions"></param>
        /// <returns></returns>
        private static IJobDetail CreateJobDetailByApiUrl(QuartzOption QuartzOptions)
        {
            try
            {
                var requestResult = QuartzOptions.ExecuteType == ExecuteType.Get ? HttpHelper.HttpGet(QuartzOptions.TaskTarget) : HttpHelper.HttpPost(QuartzOptions.TaskTarget);
            }
            catch (Exception e)
            {
                logger.LogError(e.StackTrace);
                return null;
            }

            var job = JobBuilder.Create<HttpJob>()
           .WithIdentity(QuartzOptions.TaskName, QuartzOptions.GroupName)
           //.UsingJobData("","")--传参
           .Build();
            if (QuartzOptions.TaskData != null)
                job.JobDataMap.Add(typeof(HttpJob).Name, QuartzOptions.TaskData);
            return job;
        }
        /// <summary>
        /// 创建基于扩展类的任务
        /// </summary>
        /// <param name="QuartzOptions"></param>
        /// <returns></returns>
        private static IJobDetail CreateJobDetailByAssembly(QuartzOption QuartzOptions)
        {
            IJobDetail job;
            var assemblyClassInfo = QuartzOptions.TaskTarget.Split(',');
            //加载类型---Assembly,namespace.ClassName
            Assembly assembly = Assembly.Load(new AssemblyName(assemblyClassInfo[0]));
            Type type = assembly.GetType(assemblyClassInfo[1]);
            if (type == null)
                throw new NullReferenceException("指定类型不存在");
            if (!typeof(IJob).IsAssignableFrom(type))
                throw new NotImplementedException("指定类型未实现IJob接口");
            job = new JobDetailImpl(QuartzOptions.TaskName, QuartzOptions.GroupName, type);
            if (QuartzOptions.TaskData != null)
                job.JobDataMap.Add(type.Name, QuartzOptions.TaskData);
            return job;
        }

        private static IJobDetail CreateJobDetail<T>(QuartzOption QuartzOptions) where T : IJob
        {
            IJobDetail jobDetail = JobBuilder.CreateForAsync<T>().WithIdentity(QuartzOptions.TaskName, QuartzOptions.GroupName).Build();
            if (QuartzOptions.TaskData != null)
                jobDetail.JobDataMap.Add(typeof(T).Name, QuartzOptions.TaskData);
            return jobDetail;
        }
        /// <summary>
        /// 移除作业
        /// </summary>
        /// <param name="schedulerFactory"></param>
        /// <returns></returns>
        public async static Task<QuartzResult> Remove(this ISchedulerFactory schedulerFactory, QuartzOption QuartzOptions)
        {
            return await schedulerFactory.TriggerAction(JobAction.Delete, QuartzOptions);
        }

        /// <summary>
        /// 更新作业
        /// </summary>
        /// <param name="schedulerFactory"></param>
        /// <param name="QuartzOptions"></param>
        /// <returns></returns>
        public async static Task<QuartzResult> Update(this ISchedulerFactory schedulerFactory, QuartzOption QuartzOptions)
        {
            return await schedulerFactory.TriggerAction(JobAction.Modify, QuartzOptions);
        }

        /// <summary>
        /// 暂停作业
        /// </summary>
        /// <param name="schedulerFactory"></param>
        /// <param name="QuartzOptions"></param>
        /// <returns></returns>
        public async static Task<QuartzResult> Pause(this ISchedulerFactory schedulerFactory, QuartzOption QuartzOptions)
        {
            return await schedulerFactory.TriggerAction(JobAction.Pause, QuartzOptions);
        }

        /// <summary>
        /// 启动作业
        /// </summary>
        /// <param name="schedulerFactory"></param>
        /// <param name="QuartzOptions"></param>
        /// <returns></returns>
        public async static Task<QuartzResult> Start(this ISchedulerFactory schedulerFactory, QuartzOption QuartzOptions)
        {
            return await schedulerFactory.TriggerAction(JobAction.Start, QuartzOptions);
        }

        /// <summary>
        /// 立即执行一次作业
        /// </summary>
        /// <param name="schedulerFactory"></param>
        /// <param name="QuartzOptions"></param>
        /// <returns></returns>
        public async static Task<QuartzResult> Run(this ISchedulerFactory schedulerFactory, QuartzOption QuartzOptions)
        {
            return await schedulerFactory.TriggerAction(JobAction.StartNow, QuartzOptions);
        }

        public async static Task<QuartzResult> ModifyTaskEntity(this ISchedulerFactory schedulerFactory, QuartzOption QuartzOptions, JobAction action)
        {
            QuartzOption options = null;
            switch (action)
            {
                case JobAction.Delete:
                    for (int i = 0; i < _taskList.Count; i++)
                    {
                        options = _taskList[i];
                        if (options.TaskName == QuartzOptions.TaskName && options.GroupName == QuartzOptions.GroupName)
                        {
                            _taskList.RemoveAt(i);
                        }
                    }
                    break;
                case JobAction.Modify:
                    options = _taskList.Where(x => x.TaskName == QuartzOptions.TaskName && x.GroupName == QuartzOptions.GroupName).FirstOrDefault();
                    //移除以前的配置
                    if (options != null)
                    {
                        _taskList.Remove(options);
                    }
                    //生成任务并添加新配置
                    await schedulerFactory.AddJob(QuartzOptions);//.GetAwaiter().GetResult();
                    break;
                case JobAction.Pause:
                case JobAction.Start:
                case JobAction.Stop:
                case JobAction.StartNow:
                    options = _taskList.Where(x => x.TaskName == QuartzOptions.TaskName && x.GroupName == QuartzOptions.GroupName).FirstOrDefault();
                    if (action == JobAction.Pause)
                    {
                        options.TaskStatus = TriggerState.Paused;
                    }
                    else if (action == JobAction.Stop)
                    {
                        options.TaskStatus = (TriggerState)action;
                    }
                    else
                    {
                        options.TaskStatus = TriggerState.Normal;
                    }
                    break;
            }
            //生成配置文件
            //FileQuartz.WriteJobConfig(_taskList);
            //FileQuartz.WriteJobAction(action, QuartzOptions.TaskName, QuartzOptions.GroupName, "操作对象：" + JsonConvert.SerializeObject(QuartzOptions));
            return QuartzResult.Ok("成功");
        }

        /// <summary>
        /// 触发新增、删除、修改、暂停、启用、立即执行事件
        /// </summary>
        /// <param name="schedulerFactory"></param>
        /// <param name="action"></param>
        /// <param name="QuartzOptions"></param>
        /// <returns></returns>
        public static async Task<QuartzResult> TriggerAction(this ISchedulerFactory schedulerFactory, JobAction action, QuartzOption QuartzOptions)
        {
            try
            {
                List<JobKey> jobKeys = scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(QuartzOptions.GroupName)).Result.ToList();
                if (jobKeys == null || jobKeys.Count() == 0)
                {
                    return QuartzResult.Error($"未找到分组[{QuartzOptions.GroupName}]");
                }
                JobKey jobKey = jobKeys?.Where(x => x.Name == QuartzOptions.TaskName && x.Group == QuartzOptions.GroupName)?.FirstOrDefault();

                if (jobKey == null)
                {
                    return QuartzResult.Error($"未找到触发器[{QuartzOptions.TaskName}]");
                }
                var triggers = await scheduler.GetTriggersOfJob(jobKey);

                ITrigger trigger = triggers?.Where(x => x.JobKey.Name == QuartzOptions.TaskName && x.JobKey.Group == QuartzOptions.GroupName).FirstOrDefault();
                if (trigger == null)
                {
                    return QuartzResult.Error($"未找到触发器[{QuartzOptions.TaskName}]");
                }
                object result = null;
                switch (action)
                {
                    case JobAction.Delete:
                    case JobAction.Modify:
                        await scheduler.PauseTrigger(trigger.Key);
                        await scheduler.UnscheduleJob(trigger.Key);// 移除触发器
                        await scheduler.DeleteJob(trigger.JobKey);
                        result = schedulerFactory.ModifyTaskEntity(QuartzOptions, action);
                        break;
                    case JobAction.Pause:
                    case JobAction.Stop:
                    case JobAction.Start:
                        result = schedulerFactory.ModifyTaskEntity(QuartzOptions, action);
                        if (action == JobAction.Pause)
                        {
                            await scheduler.PauseTrigger(trigger.Key);
                        }
                        else if (action == JobAction.Start)
                        {
                            await scheduler.ResumeTrigger(trigger.Key);
                            //   await scheduler.RescheduleJob(trigger.Key, trigger);
                        }
                        else
                        {
                            await scheduler.Shutdown();
                        }
                        break;
                    case JobAction.StartNow:
                        await scheduler.TriggerJob(jobKey);
                        break;
                }
                return QuartzResult.Ok("成功");
            }
            catch (Exception ex)
            {
                return QuartzResult.Error($"失败 {ex.StackTrace}");
            }
            finally
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>通过作业上下文获取作业对应的配置参数
        /// <returns></returns>
        public static QuartzOption GetQuartzOptions(this IJobExecutionContext context)
        {
            AbstractTrigger trigger = (context as JobExecutionContextImpl).Trigger as AbstractTrigger;
            QuartzOption QuartzOptions = _taskList.Where(x => x.TaskName == trigger.Name && x.GroupName == trigger.Group).FirstOrDefault();
            return QuartzOptions ?? _taskList.Where(x => x.TaskName == trigger.JobName && x.GroupName == trigger.JobGroup).FirstOrDefault();
        }

        /// <summary>
        /// 作业是否存在
        /// </summary>
        /// <param name="QuartzOptions"></param>
        /// <param name="init">初始化的不需要判断</param>
        /// <returns></returns>
        [Obsolete("不建议使用此方法，因为框架自带了验证检查是否存在任务的方法--scheduler.CheckExists")]
        private static QuartzResult Exists(this QuartzOption QuartzOptions)
        {
            if (_taskList.Any(x => x.TaskName == QuartzOptions.TaskName && x.GroupName == QuartzOptions.GroupName))
            {
                return QuartzResult.Error($"作业:{QuartzOptions.TaskName},分组：{QuartzOptions.GroupName}已经存在");
            }
            return QuartzResult.Ok("不存在");
        }

        /// <summary>
        /// 验证cron
        /// </summary>
        /// <param name="cronExpression"></param>
        /// <returns></returns>
        [Obsolete("不建议使用此方法，因为框架自带了验证cron表达式的方法--CronExpression.IsValidExpression")]
        private static (bool, string) IsValidExpression(this string cronExpression)
        {
            try
            {
                CronTriggerImpl trigger = new CronTriggerImpl();
                trigger.CronExpressionString = cronExpression;
                DateTimeOffset? date = trigger.ComputeFirstFireTimeUtc(null);
                return (date != null, date == null ? $"请确认表达式{cronExpression}是否正确!" : "");
            }
            catch (Exception e)
            {
                return (false, $"请确认表达式{cronExpression}是否正确!{e.Message}");
            }
        }
    }
}
