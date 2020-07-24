using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Extensions.Configuration;
using ServiceCatalog.BL;

namespace ServiceCatalog.WepApi.Models
{
    public class PingModels
    {

        [DataContract]
        public class PingStatus
        {
            [DataMember]
            public string Status { get; set; }

            public PingStatus(string status)
            {
                Status = status;
            }
        }

        [DataContract(Name = "PingResponse")]
        public class PingResponse
        {
            [DataMember]
            public PingStatus Ping { get; set; }

            public PingResponse()
            {
                Ping = new PingStatus("OK");
            }
        }

        [DataContract(Name = "FSCConfig")]
        public class PingConfig
        {
            [DataMember]
            public string WebHost { get; set; }
            [DataMember]
            public PingStatus Ping { get; set; }            
            [DataMember]
            DbInfo DB { get; set; }            
            public PingConfig()
            {

                Ping = new PingStatus("OK");
                DB = new DbInfo();
            }           
        }
        public class DbInfo
        {
            [DataMember]
            public string type { get; set; }
            [DataMember]
            public string server { get; set; }
            [DataMember]
            public string host { get; set; }
            public DbInfo()
            {
                var conn = FscApplication.Current.Settings.FscConnectionString;
                type = "Oracle";
                if (!string.IsNullOrEmpty(conn))
                {
                    string[] split = conn.Split("(");
                    for (var i = 0; i < split.Length; i++)
                    {
                        if (split[i].StartsWith("SID"))
                        {
                            server = split[i].ToString();
                            server = server.Replace("(","").Trim();
                            server = server.Replace(")","").Trim();
                            string[] serverSplit = server.Split(";");
                            for(var j =0; j < serverSplit.Length; j++)
                            {
                                if (serverSplit[j].StartsWith("SID"))
                                {
                                   server = serverSplit[j].Contains("SID=") ?
                                            serverSplit[j].Replace("SID=", "").Trim() : serverSplit[j].Replace("SID =", "").Trim();
                                }
                            }
                        }
                        else if (split[i].StartsWith("Host"))
                        {
                            host = split[i].ToString();
                            host = host.Contains("Host=") ? host.Replace("Host=", "") : host.Replace("Host =", "");
                            host = host.Replace(")","").Trim();
                        }
                    }
                }          
            }
        }

    }
}
