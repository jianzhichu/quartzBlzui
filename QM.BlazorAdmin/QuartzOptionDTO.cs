using Blazui.Component.Table;
using QM.Utility.Extensions;
using Quartz;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace QM.BlazorAdmin
{
    public class QuartzOptionDTO
    {

        public int Id { get; set; }
        /// <summary>
        /// 任务名称
        /// </summary>
        [TableColumn(Text ="任务名称")]

        public string TaskName { get; set; }
        /// <summary>
        /// 任务分组名称
        /// </summary>
        [TableColumn(Text = "任务分组")]
        public string GroupName { get; set; } = "default";
        /// <summary>
        /// 轮询策略 
        /// simple --- ss,100（每100秒执行一次,）--mm,1（每1分钟执行一次,）---HH,1（每一小时执行一次）
        /// cron * * * * * ? *--每秒执行一次
        /// </summary>
         [TableColumn(Text ="轮询策略")]
        public string Interval { get; set; }
        /// <summary>
        /// 轮询类型Simple,Cron
        /// </summary>
         [TableColumn(Text ="轮询类型")]
        public IntervalType? IntervalType { get; set; }

        /// <summary>
        /// 支持http调用，或者通过程序集反射，/api/values ;xxx.Job.MyTestJob
        /// </summary>
         [TableColumn(Text ="目标")]
        public string TaskTarget { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
         [TableColumn(Text ="描述")]
        public string Describe { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
         [TableColumn(Text ="传参")]
        public object TaskData { get; set; }
        /// <summary>
        /// 执行类型Post,Get,Asb--xxxx.dll,xxxx.Test.Job.MyTestJob
        /// </summary>
         [TableColumn(Text ="目标类型")]
        public ExecuteType? ExecuteType { get; set; }
        /// <summary>
        /// 任务状态
        /// </summary>
         [TableColumn(Text ="任务状态")]
        public TriggerState? TaskStatus { get; set; }
        /// <summary>
        /// 运行次数
        /// </summary>
         [TableColumn(Text ="运行次数 ")]
        public int RunTimes { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
         [TableColumn(Text ="开始时间 ")]
        public DateTime? BeginTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
         [TableColumn(Text ="结束时间 ")]
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 上次执行的异常信息
        /// </summary>
         [TableColumn(Text ="异常信息 ")]
        public string LastErrMsg { get; set; }

         [TableColumn(Text ="执行类型 ")]
        public string DisplayExecuteType
        {
            get
            {
                return ExecuteType.Description();
            }
        }
         [TableColumn(Text ="轮询类型 ")]
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
         [TableColumn(Text ="最后运行时间")]
        public DateTime? LastRunTime { get; set; }
        /// <summary>
        /// 显示状态
        /// </summary>
         [TableColumn(Text ="状态 ")]
        public string DisplayState
        {
            get
            {
                var state = string.Empty;
                switch ( TaskStatus )
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
