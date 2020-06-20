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
        private static int _Lines;
        private static string _TestLog = "FC";

        private static readonly List<CEEvent> _AllEvents = new List<CEEvent>();
        private static readonly List<CECustom> _AllFlags = new List<CECustom>();

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
        public static void LogToFile(string msg)
        {
            if (CESettings.Instance != null && !CESettings.Instance.LogToggle) return;

            try
            {
                LogMessage(msg);
            }
            catch (Exception)
            {
                LogMessage("FAILEDTOPOST: " + msg);
            }
        }


        [DebuggerStepThroughAttribute]
        public static void LogMessage(string msg)
        {
            if (_Lines >= 1000)
            {
                _TestLog = SetTestLog(_TestLog);
                _Lines = 0;
            }

            var fullPath = (BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/LogFile{0}.txt").Replace("{0}", _TestLog);
            var file = new FileInfo(fullPath);
            file.Directory?.Create();

            if (_Lines == 0) File.WriteAllText((BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/LogFile{0}.txt").Replace("{0}", _TestLog), "");

            var contents = DateTime.Now + " -- " + msg + Environment.NewLine;
            File.AppendAllText(fullPath, contents);
            _Lines++;
        }

        public static List<CECustom> GetFlags()
        {
            return _AllFlags;
        }

        public static List<CEEvent> GetAllVerifiedXSEFSEvents(List<string> modules)
        {
            if (modules.Count > 0)
                foreach (var fullPath in modules)
                {
                    LogMessage("Found new module path to be checked " + fullPath);

                    try
                    {
                        var files = Directory.GetFiles(fullPath, "*.xml", SearchOption.AllDirectories);

                        foreach (var text in files)
                        {
                            if (Path.GetFileNameWithoutExtension(text) == "SubModule") continue;

                            if (Path.GetFileNameWithoutExtension(text).StartsWith("CEModuleFlags"))
                            {
                                LogMessage("Custom Flags Found: " + text);

                                if (XMLFileCompliesWithFlagXSD(text))
                                {
                                    _AllFlags.AddRange(DeserializeXMLFileToFlags(text));
                                    LogMessage("Custom Flags  Added: " + text);
                                }

                                LogMessage("Custom Flags  Added: " + _AllFlags.Count());

                                continue;
                            }

                            LogMessage("Found: " + text);

                            if (!XMLFileCompliesWithStandardXSD(text)) continue;

                            _AllEvents.AddRange(DeserializeXMLFileToObject(text));
                            LogMessage("Added: " + text);
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
                LogMessage("Found new module path to be checked " + fullPath);

                var files = Directory.GetFiles(fullPath, "*.xml", SearchOption.AllDirectories);

                foreach (var text in files)
                {
                    if (Path.GetFileNameWithoutExtension(text).StartsWith("CEModuleFlags"))
                    {
                        LogMessage("Custom Flags Found: " + text);

                        if (XMLFileCompliesWithFlagXSD(text))
                        {
                            _AllFlags.AddRange(DeserializeXMLFileToFlags(text));
                            LogMessage("Custom Flags  Added: " + text);
                        }

                        LogMessage("Custom Flags  Added: " + _AllFlags.Count());

                        continue;
                    }

                    LogMessage("Found: " + text);

                    if (!XMLFileCompliesWithStandardXSD(text)) continue;

                    _AllEvents.AddRange(DeserializeXMLFileToObject(text));
                    LogMessage("Added: " + text);
                }

                return _AllEvents;
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
                                                  msg = e.Message + Environment.NewLine;
                                              });
            }
            catch (Exception innerException)
            {
                var textObject = new TextObject("{=CEEVENTS1002}Failed to load {FILE} for more information refer to Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs\\LoadingFailedXML.txt");
                textObject.SetTextVariable("FILE", file);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Red));
                msg = "ERROR XMLFileCompliesWithFlagXSD:  -- filename: " + file + " : " + innerException;
            }

            if (string.IsNullOrEmpty(msg)) return true;

            LogXMLFlagIssueToFile(msg, file);

            return false;
        }

        private static List<CECustom> DeserializeXMLFileToFlags(string XmlFilename)
        {
            var list = new List<CECustom>();

            try
            {
                if (string.IsNullOrEmpty(XmlFilename)) return null;
                var textReader = new StreamReader(XmlFilename);
                var xmlSerializer = new XmlSerializer(typeof(CECustom));
                var xsefsEvents = (CECustom) xmlSerializer.Deserialize(textReader);
                list.Add(xsefsEvents);
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
            if (_Lines == 0) File.WriteAllText(BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/LoadingFailedFlagXML.txt", "");
            var contents = xmlFile + " does not comply to CEFlagsModal format described in CEFlagsModal.xsd " + msg + Environment.NewLine;
            File.AppendAllText(fullPath, contents);
            _Lines++;
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
                                                  msg = e.Message + Environment.NewLine;
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


        [DebuggerStepThroughAttribute]
        private static void LogXMLIssueToFile(string msg, string xmlFile = "")
        {
            var fullPath = BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/LoadingFailedXML.txt";
            var file = new FileInfo(fullPath);
            file.Directory?.Create();
            if (_Lines == 0) File.WriteAllText(BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/LoadingFailedXML.txt", "");
            var contents = xmlFile + " does not comply to CEEventsModal format described in CEEventsModal.xsd " + msg + Environment.NewLine;
            File.AppendAllText(fullPath, contents);
            _Lines++;
        }


        [DebuggerStepThroughAttribute]
        private static string SetTestLog(string testLog)
        {
            switch (testLog)
            {
                case "FC":
                    testLog = "RT";

                    break;

                case "RT":
                    testLog = "LT";

                    break;

                default:
                    testLog = "FC";

                    break;
            }

            return testLog;
        }
    }
}