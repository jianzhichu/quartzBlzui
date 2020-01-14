using System;
using System.Collections.Generic;
using System.Text;

namespace QM.Utility.Quartz
{
    public enum JobAction
    {
        Add = 1,
        Delete = 2,
        Modify = 3,
        Pause = 4,
        Stop,
        Start,
        StartNow
    }
}
