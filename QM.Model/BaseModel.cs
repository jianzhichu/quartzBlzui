using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace QM.Model
{
    public class BaseModel
    {

        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }
    }
}
