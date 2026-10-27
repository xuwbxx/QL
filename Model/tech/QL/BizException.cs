using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.tech.QL
{
    public class BizException : Exception
    {
        public BizException(string message) : base(message) { }
        public BizException(string message, Exception inner) : base(message, inner) { }
    }
}
