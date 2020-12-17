using System;

namespace LogViewer.Site.Models
{
    public class RequestLogViewModel
    {
        public Guid Id { get; set; }
        public string Ip { get; set; }
        public DateTime DateTime { get; set; }
        public string Method { get; set; }
        public string Adress { get; set; }
        public string UserAgent { get; set; }
        public int Status { get; set; }
        public long ContentLength { get; set; }
    }
}
