using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalasZoomNotificationFormadores.objects
{
    public class ReferenceMB
    {
        public string Status { get; set; }
        public List<string> Message { get; set; }
        public string Id { get; set; }
        public Method Method { get; set; }
        public Customer Customer { get; set; }
    }
    public class Method
    {
        public string Type { get; set; }
        public string Status { get; set; }
        public string Entity { get; set; }
        public string Reference { get; set; }
        public DateTime ExpirationDate { get; set; }
    }

    public class Customer
    {
        public string Id { get; set; }
    }
}
