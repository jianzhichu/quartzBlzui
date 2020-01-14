using AutoMapper;
using Blazui.Component;
using Blazui.Component.Form;
using Blazui.Component.Table;
using QM.Interface;
using QM.Model;
using QM.BlazorAdmin.Pages.Qtz;
using QM.BlazorAdmin.Utility;
using QM.Utility.Extensions;
using QM.Utility.Quartz;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.OrmLite;

namespace QM.BlazorAdmin.Pages.Qtz
{
    public class QuartzHomeBase : BComponentBase 
    {

        /// <summary>
        /// 不显示的字段
        /// </summary>
        protected string[] IgnoreProp = { "TaskData" , "ExecuteType" , "TaskStatus" , "BeginTime" , "LastErrMsg" , "TaskTarget" , "Id" , "IntervalType" , "EndTime" };

        protected List<QuartzOptionDTO> Datas = new List<QuartzOptionDTO>();
        protected BTableBase table;
        protected int currentPage = 1;
        protected int pageSize = 10;
        protected int Total = 0;
        public TriggerState? TaskStatus;
        public IntervalType? TaskType;
        internal int CurrentPage
        {
            get
            {
                return currentPage;
            }
            set
            {
                currentPage = value;
                requireRender = true;
                ReLoadData().Wait();
            }
        }
        internal bool requireRender = true;
        [Inject]
        MessageService MessageService { get; set; }
        [Inject]
        private IQuartzService quartzService { get; set; }
        [Inject]
        IMapper mapper { get; set; }
        protected BForm searchForm;
        [Inject]
        public ISchedulerFactory schedulerFactory { get; set; }
        [Inject]
        MessageBox MessageBox { get; set; }
        [Inject]
        private IMemoryCache memoryCache { get; set; }
        /// <summary>
        /// 组件渲染完成后
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if ( !firstRender )
            {
                return;
            }
            await table.WithLoadingAsync(async () =>
            {
                await LoadDataSource();
            });

        }


        /// <summary>
        /// 根据条件查询
        /// </summary>
        protected async Task LoadDataSource()
        {
            var expression = BuildExpression();
            await GetJobs(expression);
        }

        /// <summary>
        /// 表达式目录树
        /// </summary>
        /// <returns></returns>
        private Expression<Func<QuartzModel , bool>> BuildExpression()
        {
            Expression<Func<QuartzModel , bool>> expression = p => p.Id > 0;
            Task.Delay(100);//
            var filter = searchForm.GetValue<QuartzSearchFilter>();
            //MessageService.Show($"筛选条件：{JsonConvert.SerializeObject(filter)} ");
            //if (filter.IsAllPropertyNull())
            //    return;
            if ( !string.IsNullOrWhiteSpace(filter.TaskName) )
                expression = expression.And(p => p.TaskName == filter.TaskName);
            if ( !string.IsNullOrWhiteSpace(filter.GroupName) )
                expression = expression.And(p => p.GroupName == filter.GroupName);
            if ( filter.TaskStatus != null )
            {
                var stats = ( int ) filter.TaskStatus;
                expression = expression.And(p => p.TaskStatus == stats);
            }
            if ( filter.TaskType != null )
            {
                var TaskType = ( int ) filter.TaskType;
                expression = expression.And(p => p.IntervalType == TaskType);
            }

            return expression;
        }
        [Inject]
        ILogger<QuartzHomeBase> logger { get; set; }
        /// <summary>
        /// 加载数据
        /// </summary>
        /// <returns></returns>
        public static readonly string JobCacheKey = "Jobs";
        private async Task GetJobs(Expression<Func<QuartzModel , bool>> expression)
        {
            try
            {
                await Task.Delay(100);//用于测试loading
                var cachedata = memoryCache.Get<List<QuartzModel>>(JobCacheKey);
                var pageinfo = new PageInfo() { PageIndex = currentPage , PageSize = pageSize };

                if ( cachedata != null )
                {
                    logger.LogInformation("useing cache");
                    cachedata = cachedata.AsQueryable().Where(expression).ToList();
                    var pagedata = cachedata.Skip(( currentPage - 1 ) * pageSize)?.Take(pageSize)?.ToList();
                    Datas = mapper.Map<List<QuartzOptionDTO>>(pagedata);
                    Total = Convert.ToInt32(cachedata.Count);
                }
                else
                {

                    var datas = await quartzService.QueryPageAsync(pageinfo , expression);
                    Datas = mapper.Map<List<QuartzOptionDTO>>(datas.Rows);
                    Total = Convert.ToInt32(datas.Total);
                    var AllJobs = await quartzService.QueryAsync();
                    memoryCache.Set(JobCacheKey , AllJobs);
                }
                table?.MarkAsRequireRender();
                this.StateHasChanged();
                //this.Refresh();
            }
            catch ( Exception ex )
            {
                Console.WriteLine(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="testData"></param>
        public async void Edit(object testData)
        {
            QuartzOptionDTO quartzOption = ( QuartzOptionDTO ) testData;
            var dictionary = quartzOption.ObjectToDictionary("quartzModel");
            dictionary.Add("Operation" , Operation.Update);
            await DoSth(dictionary , "编辑任务");

        }

        /// <summary>
        /// 新增
        /// </summary>
        protected async void Add()
        {
            try
            {
                var dictionary = new Dictionary<string , object>
            {
                { "Operation", Operation.Add }
            };
                await DoSth(dictionary , "新增任务");
            }
            catch ( Exception ex )
            {
                logger.LogError(ex , "Add 异常");
                throw;
            }
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="testData"></param>
        /// <returns></returns>
        protected async Task Delete(object testData)
        {
            MessageBoxResult Confirm = await MessageBox.ConfirmAsync("确定要删除？");

            MessageService.Show($"您选择了：{Confirm.ToString()}" , MessageType.Success);
            await Task.Delay(300);
            if ( Confirm == MessageBoxResult.Ok )
            {
                QuartzOptionDTO quartzOption = ( QuartzOptionDTO ) testData;
                if ( quartzOption.Id == 1000 || quartzOption.Id == 1001 )
                {
                    MessageService.Show($"不允许删除 " , MessageType.Warning);
                    return;
                }
                var result = await quartzService.DeleteByAsync(p => p.Id == quartzOption.Id);
                if ( result )
                {
                    await schedulerFactory.Remove(mapper.Map<QuartzOption>(quartzOption));
                    await ResetCache();
                }

                MessageService.Show($"删除结果：{JsonConvert.SerializeObject(result)} ");

            }
            await ReLoadData();
        }

        protected async Task DeleteChecked()
        {
            MessageBoxResult Confirm = await MessageBox.ConfirmAsync("确定要删除？");
            if ( Confirm == MessageBoxResult.Ok )
            {

                #region 删除
                QuartzOptionDTO[] quartzOptions = new QuartzOptionDTO[table.SelectedRows.Count()];
                table.SelectedRows.CopyTo(quartzOptions);
                var jobIds = quartzOptions?.Select(p => p.Id);
                if ( jobIds != null && ( jobIds.Contains(1000) || jobIds.Contains(1001) ) )
                {
                    this.MessageService.Show($"默认任务不可删除 " , MessageType.Error);
                    return;
                }

                var JobIds = quartzOptions.Select(p => p.Id);
                var result = await quartzService.DeleteByAsync(p => JobIds.Contains(p.Id));
                if ( result )
                {
                    await ResetCache();
                    foreach ( var quartzOption in quartzOptions )
                    {
                        await schedulerFactory.Remove(mapper.Map<QuartzOption>(quartzOption));
                    }
                }
                MessageService.Show($"删除结果：{JsonConvert.SerializeObject(result)} ");
                #endregion
            }
            await ReLoadData();
        }

        /// <summary>
        /// 开启任务
        /// </summary>
        /// <param name="testData"></param>
        /// <returns></returns>
        protected async Task Start(object testData)
        {
            QuartzOptionDTO quartzOption = ( QuartzOptionDTO ) testData;
            if ( quartzOption.TaskStatus == TriggerState.Normal )
                return;
            var model = quartzService.QueryById(quartzOption.Id);
            if ( model != null )
            {
                model.TaskStatus = ( int ) TriggerState.Normal;
                await StopOrStart(quartzOption , model);

            }

        }

        /// <summary>
        /// 暂停任务
        /// </summary>
        /// <param name="testData"></param>
        /// <returns></returns>
        protected async Task Stop(object testData)
        {
            QuartzOptionDTO quartzOption = ( QuartzOptionDTO ) testData;
            if ( quartzOption.TaskStatus == TriggerState.Paused || quartzOption.TaskStatus == TriggerState.Complete || quartzOption.TaskStatus == TriggerState.Error )
                return;
            var model = quartzService.QueryById(quartzOption.Id);
            if ( model != null )
            {
                model.TaskStatus = ( int ) TriggerState.Paused;
                await StopOrStart(quartzOption , model);
            }
        }

        private async Task StopOrStart(QuartzOptionDTO quartzOption , QuartzModel model)
        {
            model.LastRunTime = DateTime.Now;
            quartzService.Update(model);
            QuartzResult result = null;
            if ( model.TaskStatus == 1 )
            {
                result = await schedulerFactory.Pause(mapper.Map<QuartzOption>(quartzOption));
                MessageService.Show($"停止：{JsonConvert.SerializeObject(result)} ");
            }
            else
            {
                result = await schedulerFactory.Start(mapper.Map<QuartzOption>(quartzOption));
                if ( !result.status )
                {
                    result = await schedulerFactory.AddJob(mapper.Map<QuartzOption>(quartzOption));
                }
                MessageService.Show($"开始：{JsonConvert.SerializeObject(result)} ");
            }

            if ( result != null && result.status )
            {
                await ResetCache();
                await ReLoadData();
            }
        }

        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        [ResponseCache(Duration = 600)]
        private async Task DoSth(Dictionary<string , object> dictionary , string title)
        {
            DialogResult result = await DialogService.ShowDialogAsync<QuartzModify>(title , 460 , dictionary);

            if ( result.Result is bool )
            {
                await ReLoadData();
            }
            //if (!(result.Result is MessageBoxResult))
            //{

            //}
        }

        /// <summary>
        /// 重载数据
        /// </summary>
        protected async Task ReLoadData()
        {
            await table.WithLoadingAsync(async () =>
            {
                await LoadDataSource();

            });

        }
        protected override bool ShouldRender()
        {
            return requireRender;
        }

        public async Task ResetCache()
        {
            var AllJobs = await quartzService.QueryAsync();
            memoryCache.Remove(JobCacheKey);
            memoryCache.Set(JobCacheKey , AllJobs);
        }

        protected void ShowJobExeInfo()
        {
            MessageService.Show($"运行记录：\r\n {JsonConvert.SerializeObject(QuartzExtensions.jobExeInfos)} ");
        }
        public class QuartzSearchFilter
        {
            [Description("任务名称")]
            public string TaskName { get; set; }
            [Description("任务分组")]
            public string GroupName { get; set; }
            [Description("任务状态")]
            public TriggerState? TaskStatus { get; set; }
            [Description("任务类型")]
            public IntervalType? TaskType { get; set; }


        }


    }
}
