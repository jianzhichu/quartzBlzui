using BlazAdmin;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace QM.BlazorAdmin.Shared
{
    public class MainLayoutBase : LayoutComponentBase
    {
        protected LoginInfoModel DefaultUser { get; set; } 
        protected List<MenuModel> Menus { get; set; } = new List<MenuModel>();

        protected override void OnInitialized()
        {
            Menus.Add(new MenuModel()
            {
                Label = "QuartzManager",
                Icon = "el-icon-s-promotion",
                Route = "/quartz"
          
            });
      

        }
    }
}
