using AutoMapper;
using Blazui.Component;
using Blazui.Component.Form;
using QM.Interface;
using QM.Model;
using QM.BlazorAdmin.Utility;
using QM.Utility.Extensions;
using QM.Utility.Quartz;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QM.BlazorAdmin.Pages.Qtz
{
    public class QuartzModifyBase : BComponentBase
    {
        internal LabelAlign formAlign;

        [Inject]
        IQuartzService Quartzservice { get; set; }

        [Inject]
        IMapper mapper { get; set; }

        protected TriggerState? TaskStatus;
        protected IntervalType? TaskType;
        protected ExecuteType? ExecuteType;


        [Inject]
        MessageService MessageService { get; set; }

        [Parameter]
        public QuartzOptionDTO quartzModel { get; set; }

        [Parameter]
        public Operation Operation { get; set; }
        public static BForm demoForm;
        [Inject]
        public ISchedulerFactory schedulerFactory { get; set; }

        [Inject]
        private IMemoryCache memoryCache { get; set; }
        protected async Task Submit()
        {
            formAlign = LabelAlign.Right;
            if (!demoForm.IsValid())
            {
                return;
            }
            bool result = false;
            var taskCount = await Quartzservice.CountAsync();
            if (taskCount > 15)
                return;
            var quartzOption = demoForm.GetValue<QuartzOptionDTO>();
            var quartzModel = mapper.Map<QuartzModel>(quartzOption);
            quartzModel.LastRunTime = DateTime.Now;
            quartzModel.Describe ??= quartzOption.TaskName;

            if (Operation == Operation.Update)
            {
                if (quartzModel.Id == 1000 || quartzModel.Id == 1001)
                {
                    this.MessageService.Show($"默认任务不可修改 ", MessageType.Error);
                    return;
                }
                var oldjob = mapper.Map<QuartzOptionDTO>(Quartzservice.QueryById(quartzModel.Id));
                QuartzResult operationResult = null;
                if (oldjob != null)
                {
                    if (oldjob.TaskStatus != quartzOption.TaskStatus)
                    {
                        switch (quartzOption.TaskStatus)
                        {
                            case TriggerState.Normal:
                                operationResult = await schedulerFactory.Start(mapper.Map<QuartzOption>(oldjob));
                                break;
                            case TriggerState.Paused:
                                operationResult = await schedulerFactory.Pause(mapper.Map < QuartzOption > (oldjob));
                                break;
                            case TriggerState.Complete:
                            case TriggerState.Error:
                            case TriggerState.Blocked:
                            case TriggerState.None:
                                operationResult = await schedulerFactory.Remove(mapper.Map<QuartzOption>(oldjob));
                                break;

                            default:
                                throw new NotImplementedException(" unkown TriggerState");
                        }
                    }

                    result = await Quartzservice.UpdateAsync(quartzModel);
                    if (operationResult != null && operationResult.status)
                    {
                        this.MessageService.Show($"操作结果：{JsonConvert.SerializeObject(operationResult)} ");
                    }

                }
            }
            else
            {
                var AddJobResult = await schedulerFactory.AddJob(mapper.Map<QuartzOption>(quartzOption));
                if (!AddJobResult.status)
                {
                    this.MessageService.Show($"添加调度任务失败：{JsonConvert.SerializeObject(AddJobResult)} ");
                    return;
                }
                result = await Quartzservice.InsertAsync(quartzModel);
                if (!result)
                    await schedulerFactory.Remove(mapper.Map<QuartzOption>(quartzOption));

            }
            if (result)
            {
                await ResetCache();
            }
            this.MessageService.Show($"操作结果：{result} ");
            await DialogService.CloseDialogAsync(this, result);
        }



        protected void Reset()
        {
            demoForm.Reset();
        }


        public async Task ResetCache()
        {
            var AllJobs = await Quartzservice.QueryAsync();
            memoryCache.Remove(QuartzHomeBase.JobCacheKey);
            memoryCache.Set(QuartzHomeBase.JobCacheKey, AllJobs);
        }
    }
}
