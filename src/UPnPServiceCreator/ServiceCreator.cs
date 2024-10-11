using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenPhonos.Utility
{
    /// <summary>
    /// This tool reads the services from a UPnP device and generates C# code to access all the methods
    /// Pass an http address of the device as the first argument eg http://192.168.1.120:1400
    /// Writes all the files to Temp directory
    /// Not a lot of error handling. Run it under the debugger...
    /// </summary>
    class ServiceCreator
    {
        static string NSDevice = "urn:schemas-upnp-org:device-1-0";
        static string NSService = "urn:schemas-upnp-org:service-1-0";
        static string OutputNamespace = "SonosServices";

        static async Task Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: ServiceCreator <IP address of device>");
                return;
            }

            string deviceroot = args[0];
            string destinationDir = Path.GetTempPath();

            var client = new HttpClient();
            var xml = await client.GetStringAsync(deviceroot + "/xml/device_description.xml");
            var rootdevice = XElement.Parse(xml);
            var version = rootdevice.Descendants(XName.Get("softwareVersion", NSDevice)).First().Value;
            var display = rootdevice.Descendants(XName.Get("displayVersion", NSDevice)).First().Value;

            var services = from s in rootdevice.Descendants()
                           where s.Name == XName.Get("SCPDURL", NSDevice)
                           select s;

            foreach (var svc in services)
            {
                xml = await client.GetStringAsync(deviceroot + svc.Value);
                string basename = Path.GetFileName(svc.Value);
                DumpService(basename, xml, destinationDir);
            }
        }

        static string FileTemplate = @"
using System.Threading.Tasks;

// Auto-generated file, do not edit manually

namespace #NS#
{
    public class #CLASSNAME# : OpenPhonos.UPnP.Service
    {
        public #CLASSNAME#(OpenPhonos.UPnP.ServiceInfo info)
            : base(info)
        {
        }

#MEMBERS#
    }
}

";

        static string ActionTemplate = @"

        private static OpenPhonos.UPnP.Service.ActionInfo #NAME#_Info = new OpenPhonos.UPnP.Service.ActionInfo()
        {
            name = ""#NAME#"",
            argnames = new string[] { #ARGLIST# },
            outargs = #OUTARGS#,
        };

        public class #NAME#_Result : OpenPhonos.UPnP.Service.ActionResult
        {
#RESULTLIST#

            public override void Fill(string[] rawdata)
            {
#FILLLIST#
            }
        }
        public async Task<#NAME#_Result> #NAME#(#INARGS#)
        {
            return await base.Action_Async(#NAME#_Info, new object[] { #INARGS2# }, new #NAME#_Result()) as #NAME#_Result;
        }
    
";

        class VariableInfo
        {
            public string Name;
            public string DataType;
        };

        private static void DumpService(string basename, string xml, string destDir)
        {
            string classname = Path.GetFileNameWithoutExtension(basename);
            StringBuilder members = new StringBuilder();

            // Trawl through each method
            XElement doc = XElement.Parse(xml);
            var actions = from item in doc.Descendants(XName.Get("action", NSService))
                          select item;

            var vars = from item in doc.Descendants(XName.Get("stateVariable", NSService))
                       select new VariableInfo()
                       {
                           Name = item.Element(XName.Get("name", NSService)).Value,
                           DataType = item.Element(XName.Get("dataType", NSService)).Value,
                       };

            foreach (var action in actions)
            {
                var arglist = from arg in action.Descendants(XName.Get("argument", NSService))
                              select new
                              {
                                  Name = arg.Element(XName.Get("name", NSService)).Value,
                                  Direction = arg.Element(XName.Get("direction", NSService)).Value,
                                  Type = TypeConversion(vars, arg.Element(XName.Get("relatedStateVariable", NSService)).Value)
                              };

                StringBuilder quotedargs = new StringBuilder();
                StringBuilder resultlist = new StringBuilder();
                StringBuilder filllist = new StringBuilder();
                StringBuilder inargsnotypes = new StringBuilder();
                StringBuilder inargs = new StringBuilder();

                int fillcount = 0;

                foreach (var arg in arglist)
                {
                    if (arg.Direction == "out")
                    {
                        resultlist.AppendLine("\t\t\tpublic " + arg.Type + " " + arg.Name + ";");

                        filllist.AppendLine("\t\t\t\t" + arg.Name + " = " + RawConverter(arg.Type, "rawdata[" + fillcount.ToString() + "]") + ";");
                        fillcount++;
                    }
                    else if (arg.Direction == "in")
                    {
                        quotedargs.Append('\"');
                        quotedargs.Append(arg.Name);
                        quotedargs.Append("\", ");

                        inargs.Append(arg.Type + " " + arg.Name + ", ");
                        inargsnotypes.Append(arg.Name + ", ");
                    }
                }

                Dictionary<string, string> actionlist = new Dictionary<string, string>()
                {
                    {"NAME", action.Element(XName.Get("name",NSService)).Value },
                    {"ARGLIST", ToStringWithout(quotedargs, ", ") },
                    {"OUTARGS", arglist.Count( (a)=>a.Direction=="out").ToString() },
                    {"RESULTLIST", resultlist.ToString() },
                    {"FILLLIST", filllist.ToString() },
                    {"INARGS", ToStringWithout(inargs, ", ") },
                    {"INARGS2",ToStringWithout(inargsnotypes, ", ") }
                };
                string actiontext = TemplateReplace(ActionTemplate, actionlist);
                members.Append(actiontext);
            }

            // We have all the bits, emit the file now
            Dictionary<string, string> MainArgs = new Dictionary<string, string>()
            {
                { "NS", OutputNamespace},
                { "CLASSNAME", classname },
                { "MEMBERS", members.ToString() }
            };
            string file = TemplateReplace(FileTemplate, MainArgs);

            File.WriteAllText(Path.Combine(destDir, classname + ".cs"), file);
        }

        private static string ToStringWithout(StringBuilder sb, string trailing)
        {
            int end = sb.ToString().LastIndexOf(trailing);
            if (end != -1)
                sb.Remove(end, trailing.Length);
            return sb.ToString();
        }

        // Return the C# typename
        private static string TypeConversion(IEnumerable<VariableInfo> vars, string varname)
        {
            var v = vars.Where((x) => x.Name == varname).First().DataType;

            switch (v)
            {
                case "string":
                    return "string";
                case "ui2":
                    return "ushort";
                case "ui4":
                    return "uint";
                case "boolean":
                    return "bool";
                case "i2":
                    return "short";
                case "i4":
                    return "int";
                default:
                    Debug.Assert(false, "Unknown type " + v);
                    return "unknown";
            }
        }

        // Convert a string into the correct format
        private static string RawConverter(string type, string what)
        {
            switch (type)
            {
                case "string":
                    return what;
                case "int":
                case "short":
                case "uint":
                case "ushort":
                    return type + ".Parse(" + what + ")";
                case "bool":
                    return "ParseBool(" + what + ")";
                default:
                    Debug.Assert(false, "bad converter");
                    return "TODO";
            }
        }

        private static string TemplateReplace(string template, Dictionary<string, string> replacements)
        {
            string[] split = template.Split('#');
            StringBuilder sb = new StringBuilder();
            bool content = true;
            foreach (var chunk in split)
            {
                if (content)
                {
                    sb.Append(chunk);
                }
                else
                {
                    string what;
                    if (replacements.TryGetValue(chunk, out what))
                        sb.Append(what);
                    else
                        sb.Append("#" + chunk + "#");
                }

                content = !content;
            }

            return sb.ToString();
        }
    }
}
