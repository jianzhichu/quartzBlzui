using System;
using System.Collections.Generic;
using System.Text;

namespace QM.Utility.Quartz
{
    public class QuartzResult
    {
        public bool status { get; set; }
        public string msg { get; set; }


        public static QuartzResult Ok(string msg)
        {
            return new QuartzResult() { status = true, msg = msg };
        }

        public static QuartzResult Error(string msg)
        {
            return new QuartzResult() { status = false, msg = msg };
        }
    }

}
