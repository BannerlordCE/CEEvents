using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace CaptivityEvents.Custom
{
    public class CECustomHandler
    {
        public static int Lines;
        public static string TestLog = "FC";

        private static readonly List<CEEvent> AllEvents = new List<CEEvent>();
        private static readonly List<CECustom> AllFlags = new List<CECustom>();

        public static List<CECustom> GetFlags()
        {
            return AllFlags;
        }

        public static List<CEEvent> GetAllVerifiedXSEFSEvents(List<string> modules)
        {
            if (modules.Count != 0)
                foreach (var fullPath in modules)
                {
                    ForceLogToFile("Found new module path to be checked " + fullPath);

                    try
                    {
                        var files = Directory.GetFiles(fullPath, "*.xml", SearchOption.AllDirectories);

                        foreach (var text in files)
                        {
                            if (Path.GetFileNameWithoutExtension(text) == "SubModule") continue;

                            if (Path.GetFileNameWithoutExtension(text).StartsWith("CEModuleFlags"))
                            {
                                ForceLogToFile("Custom Flags Found: " + text);

                                if (XMLFileCompliesWithFlagXSD(text))
                                {
                                    AllFlags.AddRange(DeserializeXMLFileToFlags(text));
                                    ForceLogToFile("Custom Flags  Added: " + text);
                                }

                                ForceLogToFile("Custom Flags  Added: " + AllFlags.Count());

                                continue;
                            }

                            ForceLogToFile("Found: " + text);

                            if (!XMLFileCompliesWithStandardXSD(text)) continue;
                            
                            AllEvents.AddRange(DeserializeXMLFileToObject(text));
                            ForceLogToFile("Added: " + text);
                        }
                    }
                    catch (Exception e)
                    {
                        LogXMLIssueToFile(" ! " + e + " ! ");
                        InformationManager.DisplayMessage(new InformationMessage("{=CEEVENTS1003}Failed to load captivity events more information refer to Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs\\LoadingFailedXML.txt", Colors.Red));
                    }
                }

            try
            {
                var fullPath = BasePath.Name + "Modules\\zCaptivityEvents\\ModuleLoader";
                ForceLogToFile("Found new module path to be checked " + fullPath);
                var files = Directory.GetFiles(fullPath, "*.xml", SearchOption.AllDirectories);

                foreach (var text in files)
                {
                    if (Path.GetFileNameWithoutExtension(text).StartsWith("CEModuleFlags"))
                    {
                        ForceLogToFile("Custom Flags Found: " + text);

                        if (XMLFileCompliesWithFlagXSD(text))
                        {
                            AllFlags.AddRange(DeserializeXMLFileToFlags(text));
                            ForceLogToFile("Custom Flags  Added: " + text);
                        }

                        ForceLogToFile("Custom Flags  Added: " + AllFlags.Count());

                        continue;
                    }

                    ForceLogToFile("Found: " + text);

                    if (!XMLFileCompliesWithStandardXSD(text)) continue;
                    
                    AllEvents.AddRange(DeserializeXMLFileToObject(text));
                    ForceLogToFile("Added: " + text);
                }

                return AllEvents;
            }
            catch (Exception e)
            {
                LogXMLIssueToFile(" ! " + e + " ! ");
                InformationManager.DisplayMessage(new InformationMessage("{=CEEVENTS1003}Failed to load captivity events more information refer to Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs\\LoadingFailedXML.txt", Colors.Red));

                return new List<CEEvent>();
            }
        }

        // Flags
        private static bool XMLFileCompliesWithFlagXSD(string file)
        {
            var fullPath = BasePath.Name + "Modules/zCaptivityEvents/ModuleLoader/CaptivityRequired/Events/CEFlagsModal.xsd";
            var xmlSchemaSet = new XmlSchemaSet();
            var msg = "";

            try
            {
                xmlSchemaSet.Add(null, fullPath);
                var source = XDocument.Load(file);

                source.Validate(xmlSchemaSet, delegate(object o, ValidationEventArgs e)
                                              {
                                                  msg = msg + e.Message + Environment.NewLine;
                                              });
            }
            catch (Exception innerException)
            {
                var textObject = new TextObject("{=CEEVENTS1002}Failed to load {FILE} for more information refer to Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs\\LoadingFailedXML.txt");
                textObject.SetTextVariable("FILE", file);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Red));
                msg = "ERROR XMLFileCompliesWithFlagXSD:  -- filename: " + file + " : " + innerException;
            }

            bool result;

            if (msg == "")
            {
                result = true;
            }
            else
            {
                LogXMLFlagIssueToFile(msg, file);
                result = false;
            }

            return result;
        }

        public static List<CECustom> DeserializeXMLFileToFlags(string XmlFilename)
        {
            var list = new List<CECustom>();

            try
            {
                if (string.IsNullOrEmpty(XmlFilename)) return null;
                var textReader = new StreamReader(XmlFilename);
                var xmlSerializer = new XmlSerializer(typeof(CECustom));
                var xsefsevents = (CECustom) xmlSerializer.Deserialize(textReader);
                list.Add(xsefsevents);
            }
            catch (Exception innerException)
            {
                var textObject = new TextObject("{=CEEVENTS1001}Failed to load {FILE} for more information refer to Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs\\LoadingFailedXML.txt");
                textObject.SetTextVariable("FILE", XmlFilename);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Red));

                throw new Exception("ERROR DeserializeXMLFileToFlags:  -- filename: " + XmlFilename, innerException);
            }

            return list;
        }

        [DebuggerStepThroughAttribute]
        private static void LogXMLFlagIssueToFile(string msg, string xmlFile = "")
        {
            var fullPath = BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/LoadingFailedFlagXML.txt";
            var file = new FileInfo(fullPath);
            file.Directory?.Create();
            if (Lines == 0) File.WriteAllText(BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/LoadingFailedFlagXML.txt", "");
            var contents = xmlFile + " does not comply to CEFlagsModal format described in CEFlagsModal.xsd " + msg + Environment.NewLine;
            File.AppendAllText(fullPath, contents);
            Lines++;
        }

        // Standard Events
        private static bool XMLFileCompliesWithStandardXSD(string file)
        {
            var fullPath = BasePath.Name + "Modules/zCaptivityEvents/ModuleLoader/CaptivityRequired/Events/CEEventsModal.xsd";
            var xmlSchemaSet = new XmlSchemaSet();
            var msg = "";

            try
            {
                xmlSchemaSet.Add(null, fullPath);
                var source = XDocument.Load(file);

                source.Validate(xmlSchemaSet, delegate(object o, ValidationEventArgs e)
                                              {
                                                  msg = msg + e.Message + Environment.NewLine;
                                              });
            }
            catch (Exception innerException)
            {
                var textObject = new TextObject("{=CEEVENTS1002}Failed to load {FILE} for more information refer to Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs\\LoadingFailedXML.txt");
                textObject.SetTextVariable("FILE", file);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Red));
                msg = "ERROR XMLFileCompliesWithStandardXSD:  -- filename: " + file + " : " + innerException;
            }

            bool result;

            if (msg == "")
            {
                result = true;
            }
            else
            {
                LogXMLIssueToFile(msg, file);
                result = false;
            }

            return result;
        }

        public static List<CEEvent> DeserializeXMLFileToObject(string XmlFilename)
        {
            var list = new List<CEEvent>();

            try
            {
                if (string.IsNullOrEmpty(XmlFilename)) return null;
                var textReader = new StreamReader(XmlFilename);
                var xmlSerializer = new XmlSerializer(typeof(CEEvents));
                var xsefsevents = (CEEvents) xmlSerializer.Deserialize(textReader);
                list.AddRange(xsefsevents.CEEvent);
            }
            catch (Exception innerException)
            {
                var textObject = new TextObject("{=CEEVENTS1001}Failed to load {FILE} for more information refer to Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs\\LoadingFailedXML.txt");
                textObject.SetTextVariable("FILE", XmlFilename);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Red));

                throw new Exception("ERROR DeserializeXMLFileToObject:  -- filename: " + XmlFilename, innerException);
            }

            return list;
        }

        [DebuggerStepThroughAttribute]
        private static void LogXMLIssueToFile(string msg, string xmlFile = "")
        {
            var fullPath = BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/LoadingFailedXML.txt";
            var file = new FileInfo(fullPath);
            file.Directory?.Create();
            if (Lines == 0) File.WriteAllText(BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/LoadingFailedXML.txt", "");
            var contents = xmlFile + " does not comply to CEEventsModal format described in CEEventsModal.xsd " + msg + Environment.NewLine;
            File.AppendAllText(fullPath, contents);
            Lines++;
        }


        [DebuggerStepThroughAttribute]
        public static void LogToFile(string msg)
        {
            try
            {
                if (CESettings.Instance == null) return;
                if (!CESettings.Instance.LogToggle) return;

                ForceLogToFile(msg);
            }
            catch (Exception)
            {
                ForceLogToFile("FAILEDTOPOST: " + msg);
            }
        }

        [DebuggerStepThroughAttribute]
        public static void ForceLogToFile(string msg)
        {
            if (Lines >= 1000)
            {
                switch (TestLog)
                {
                    case "FC":
                        TestLog = "RT";

                        break;

                    case "RT":
                        TestLog = "LT";

                        break;

                    default:
                        TestLog = "FC";

                        break;
                }

                Lines = 0;
            }

            var fullPath = (BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/LogFile{0}.txt").Replace("{0}", TestLog);
            var file = new FileInfo(fullPath);
            file.Directory?.Create();
            if (Lines == 0) File.WriteAllText((BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/LogFile{0}.txt").Replace("{0}", TestLog), "");

            var contents = DateTime.Now + " -- " + msg + Environment.NewLine;
            File.AppendAllText(fullPath, contents);
            Lines++;
        }
    }
}