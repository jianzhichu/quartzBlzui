using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using AutoMapper.Configuration.Annotations;
using QM.Utility.Extensions;
using Quartz;
namespace QM.Utility.Quartz
{


    public class QuartzOption
    {

        public int Id { get; set; }
        /// <summary>
        /// 任务名称
        /// </summary>
        [Description("任务名称")]
        public string TaskName { get; set; }
        /// <summary>
        /// 任务分组名称
        /// </summary>
        [Description("任务分组")]
        public string GroupName { get; set; } = "default";
        /// <summary>
        /// 轮询策略 
        /// simple --- ss,100（每100秒执行一次,）--mm,1（每1分钟执行一次,）---HH,1（每一小时执行一次）
        /// cron * * * * * ? *--每秒执行一次
        /// </summary>
        [Description("轮询策略")]
        public string Interval { get; set; }
        /// <summary>
        /// 轮询类型Simple,Cron
        /// </summary>
        [Description("轮询类型")]
        public IntervalType? IntervalType { get; set; }

        /// <summary>
        /// 支持http调用，或者通过程序集反射，/api/values ;xxx.Job.MyTestJob
        /// </summary>
        [Description("目标")]
        public string TaskTarget { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Description("描述")]
        public string Describe { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        [Description("传参")]
        public object TaskData { get; set; }
        /// <summary>
        /// 执行类型Post,Get,Asb--xxxx.dll,xxxx.Test.Job.MyTestJob
        /// </summary>
        [Description("目标类型")]
        public ExecuteType? ExecuteType { get; set; }
        /// <summary>
        /// 任务状态
        /// </summary>
        [Description("任务状态")]
        public TriggerState? TaskStatus { get; set; }
        /// <summary>
        /// 运行次数
        /// </summary>
        [Description("运行次数 ")]
        public int RunTimes { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        [Description("开始时间 ")]
        public DateTime? BeginTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        [Description("结束时间 ")]
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 上次执行的异常信息
        /// </summary>
        [Description("异常信息 ")]
        public string LastErrMsg { get; set; }

        [Description("执行类型 ")]
        public string DisplayExecuteType
        {
            get
            {
                return ExecuteType.Description();
            }
        }
        [Description("轮询类型 ")]
        public string DisplayIntervalType
        {
            get
            {
                //return IntervalType == 1 ? "Cron" : "Simple";
                return IntervalType.Description();
            }
        }
        /// <summary>
        /// 最后运行时间
        /// </summary>
        [Description("最后运行时间")]
        public DateTime? LastRunTime { get; set; }
        /// <summary>
        /// 显示状态
        /// </summary>
        [Description("状态 ")]
        public string DisplayState
        {
            get
            {
                var state = string.Empty;
                switch (TaskStatus)
                {
                    case TriggerState.Normal:
                        state = "正常";
                        break;
                    case TriggerState.Paused:
                        state = "暂停";
                        break;
                    case TriggerState.Complete:
                        state = "完成";
                        break;
                    case TriggerState.Error:
                        state = "异常";
                        break;
                    case TriggerState.Blocked:
                        state = "阻塞";
                        break;
                    case TriggerState.None:
                        state = "不存在";
                        break;
                    default:
                        state = "未知";
                        break;
                }
                return state;
            }
        }

    }


    public class JobExeInfo
    {
        public int JobId { get; set; }
        public string JobName { get; set; }
        public int ExeCount { get; set; }
    }
    public enum ExecuteType
    {
        [Description("POST")]
        Post,
        [Description("Get")]
        Get,
        [Description("Assembly")]
        Asb//--Assembly
    }
    public enum IntervalType
    {
        [Description("Simple")]
        Simple,
        [Description("Cron")]
        Cron
    }

}
