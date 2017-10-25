/*
 * * Created by Traesh for AshamaneProject (https://github.com/AshamaneProject)
 */
using System.Net;
using System.Text;

namespace WowHeadParser
{
    [System.ComponentModel.DesignerCategory("Code")]
    class WowheadWebclient : WebClient
    {
        public WowheadWebclient() : base()
        {
            this.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            this.Encoding = Encoding.UTF8;
        }
    }
}
