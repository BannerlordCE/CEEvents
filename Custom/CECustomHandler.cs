using CaptivityEvents.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
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
        public static int ErrorLines;
        public static int Lines;
        public static string TestLog = "FC";

        private static readonly List<CECustomModule> AllModules = new List<CECustomModule>();
        private static readonly List<CEEvent> AllEvents = new List<CEEvent>();
        private static readonly List<CECustom> AllCustom = new List<CECustom>();

        public static List<CECustom> GetCustom() => AllCustom;

        public static List<CECustomModule> GetModules() => AllModules;

        public static List<CEEvent> GetAllVerifiedXSEFSEvents(List<string> modules)
        {
#if DEBUG
            //TestWrite();
#endif
            string errorPath = BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/LoadingFailedFlagXML.txt";
            FileInfo file = new FileInfo(errorPath);
            if (file.Exists) file.Delete();
            ErrorLines = 0;
            Lines = 0;

            if (modules.Count != 0)
            {
                foreach (string fullPath in modules)
                {
                    ForceLogToFile("Found new module path to be checked " + fullPath);

                    List<CEEvent> TempEvents = new List<CEEvent>();

                    try
                    {
                        string[] files = Directory.GetFiles(fullPath, "*.xml", SearchOption.AllDirectories);

                        foreach (string text in files)
                        {
                            if (Path.GetDirectoryName(text).Contains("ModuleData")) continue;

                            if (Path.GetFileNameWithoutExtension(text) == "SubModule") continue;

                            if (Path.GetFileNameWithoutExtension(text).StartsWith("CEModuleCustom"))
                            {
                                ForceLogToFile("Custom Settings Found: " + text);

                                if (XMLFileCompliesWithCustomXSD(text))
                                {
                                    AllCustom.AddRange(DeserializeXMLFileToFlags(text));
                                    ForceLogToFile("Custom Settings Added: " + text);
                                }

                                ForceLogToFile("Total Custom Skills: " + AllCustom.Sum((CECustom ce) =>
                                {
                                    if (ce.CESkills != null) return ce.CESkills.Count;
                                    return 0;
                                }));

                                continue;
                            }

                            ForceLogToFile("Found: " + text);

                            if (!XMLFileCompliesWithStandardXSD(text)) continue;

                            AllEvents.AddRange(DeserializeXMLFileToObject(text));
                            TempEvents.AddRange(DeserializeXMLFileToObject(text));
                            ForceLogToFile("Added: " + text);
                        }

                        CECustomModule item = new CECustomModule(Path.GetFileNameWithoutExtension(fullPath), TempEvents);
                        AllModules.Add(item);
                    }
                    catch (Exception e)
                    {
                        LogXMLIssueToFile(" ! " + e + " ! ");
                        InformationManager.DisplayMessage(new InformationMessage("{=CEEVENTS1003}Failed to load captivity events more information refer to Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs\\LoadingFailedXML.txt", Colors.Red));
                    }
                }
            }

            try
            {
                string fullPath = BasePath.Name + "Modules\\zCaptivityEvents\\ModuleLoader";
                ForceLogToFile("Found new module path to be checked " + fullPath);
                string[] files = Directory.GetFiles(fullPath, "*.xml", SearchOption.AllDirectories);

                List<CEEvent> TempEvents = new List<CEEvent>();

                foreach (string text in files)
                {
                    if (Path.GetFileNameWithoutExtension(text).StartsWith("CESettings")) continue;

                    if (Path.GetFileNameWithoutExtension(text).StartsWith("CEModuleCustom"))
                    {
                        ForceLogToFile("Custom Flags Found: " + text);

                        if (XMLFileCompliesWithCustomXSD(text))
                        {
                            AllCustom.AddRange(DeserializeXMLFileToFlags(text));
                            ForceLogToFile("Custom Flags  Added: " + text);
                        }

                        ForceLogToFile("Custom Flags  Added: " + AllCustom.Count());

                        continue;
                    }

                    ForceLogToFile("Found: " + text);

                    if (!XMLFileCompliesWithStandardXSD(text)) continue;

                    AllEvents.AddRange(DeserializeXMLFileToObject(text));
                    TempEvents.AddRange(DeserializeXMLFileToObject(text));
                    ForceLogToFile("Added: " + text);
                }

                CECustomModule item = new CECustomModule(Path.GetFileNameWithoutExtension(fullPath), TempEvents);
                AllModules.Add(item);

                return AllEvents;
            }
            catch (Exception e)
            {
                LogXMLIssueToFile(" ! " + e + " ! ");
                InformationManager.DisplayMessage(new InformationMessage("{=CEEVENTS1003}Failed to load captivity events more information refer to Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs\\LoadingFailedXML.txt", Colors.Red));

                return new List<CEEvent>();
            }
        }

        public static CECustomSettings LoadCustomSettings()
        {
            string fullPath = BasePath.Name + "Modules/zCaptivityEvents/ModuleLoader/CaptivityRequired/Events/CESettings.xml";
            try
            {
                return DeserializeXMLFileToSettings(fullPath);
            }
            catch (Exception e)
            {
                ForceLogToFile(e.ToString());
                return null;
            }
        }
        // Settings
        public static CECustomSettings DeserializeXMLFileToSettings(string XmlFilename)
        {
            CECustomSettings _CESettings;

            try
            {
                if (string.IsNullOrEmpty(XmlFilename)) return null;
                StreamReader textReader = new StreamReader(XmlFilename);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(CECustomSettings));
                CECustomSettings xsefsevents = (CECustomSettings)xmlSerializer.Deserialize(textReader);
                _CESettings = xsefsevents;
            }
            catch (Exception innerException)
            {
                TextObject textObject = new TextObject("{=CEEVENTS1001}Failed to load {FILE} for more information refer to Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs\\LoadingFailedXML.txt");
                textObject.SetTextVariable("FILE", XmlFilename);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Red));

                throw new Exception("ERROR DeserializeXMLFileToSettings:  -- filename: " + XmlFilename, innerException);
            }

            return _CESettings;
        }

        // Custom
        private static bool XMLFileCompliesWithCustomXSD(string file)
        {
            string fullPath = BasePath.Name + "Modules/zCaptivityEvents/ModuleLoader/CaptivityRequired/Events/CECustomModal.xsd";
            XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
            string msg = "";

            try
            {
                xmlSchemaSet.Add(null, fullPath);
                XDocument source = XDocument.Load(file);

                source.Validate(xmlSchemaSet, delegate (object o, ValidationEventArgs e)
                                              {
                                                  msg = msg + e.Message + Environment.NewLine;
                                              });
            }
            catch (Exception innerException)
            {
                TextObject textObject = new TextObject("{=CEEVENTS1002}Failed to load {FILE} for more information refer to Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs\\LoadingFailedXML.txt");
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
                LogXMLCustomIssueToFile(msg, file);
                result = false;
            }

            return result;
        }

        public static List<CECustom> DeserializeXMLFileToFlags(string XmlFilename)
        {
            List<CECustom> list = new List<CECustom>();

            try
            {
                if (string.IsNullOrEmpty(XmlFilename)) return null;
                StreamReader textReader = new StreamReader(XmlFilename);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(CECustom));
                CECustom xsefsevents = (CECustom)xmlSerializer.Deserialize(textReader);
                list.Add(xsefsevents);
            }
            catch (Exception innerException)
            {
                TextObject textObject = new TextObject("{=CEEVENTS1001}Failed to load {FILE} for more information refer to Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs\\LoadingFailedXML.txt");
                textObject.SetTextVariable("FILE", XmlFilename);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Red));

                throw new Exception("ERROR DeserializeXMLFileToFlags:  -- filename: " + XmlFilename, innerException);
            }

            return list;
        }

        [DebuggerStepThroughAttribute]
        private static void LogXMLCustomIssueToFile(string msg, string xmlFile = "")
        {
            string fullPath = BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/LoadingFailedFlagXML.txt";
            FileInfo file = new FileInfo(fullPath);
            file.Directory?.Create();
            if (Lines == 0) File.WriteAllText(BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/LoadingFailedFlagXML.txt", "");
            string contents = xmlFile + " does not comply to CEFlagsModal format described in CECustomModal.xsd " + msg + Environment.NewLine;
            File.AppendAllText(fullPath, contents);
            Lines++;
        }

        // Standard Events
        private static bool XMLFileCompliesWithStandardXSD(string file)
        {
            string fullPath = BasePath.Name + "Modules/zCaptivityEvents/ModuleLoader/CaptivityRequired/Events/CEEventsModal.xsd";
            XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
            string msg = "";

            try
            {
                xmlSchemaSet.Add(null, fullPath);
                LoadOptions opts = LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo;
                XDocument source = XDocument.Load(file, opts);

                source.Validate(xmlSchemaSet, delegate (object o, ValidationEventArgs e)
                                              {
                                                  msg = msg + e.Message + Environment.NewLine;
                                              });
            }
            catch (Exception innerException)
            {
                TextObject textObject = new TextObject("{=CEEVENTS1002}Failed to load {FILE} for more information refer to Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs\\LoadingFailedXML.txt");
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
            List<CEEvent> list = new List<CEEvent>();

            try
            {
                if (string.IsNullOrEmpty(XmlFilename)) return null;
                StreamReader textReader = new StreamReader(XmlFilename);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(CEEvents));
                CEEvents xsefsevents = (CEEvents)xmlSerializer.Deserialize(textReader);
                list.AddRange(xsefsevents.CEEvent);
            }
            catch (Exception innerException)
            {
                TextObject textObject = new TextObject("{=CEEVENTS1001}Failed to load {FILE} for more information refer to Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs\\LoadingFailedXML.txt");
                textObject.SetTextVariable("FILE", XmlFilename);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Red));

                throw new Exception("ERROR DeserializeXMLFileToObject:  -- filename: " + XmlFilename, innerException);
            }

            return list;
        }

#if DEBUG
        public static string GetEventXml(CEEvents obj, XmlSerializer serializer = null, bool omitStandardNamespaces = false)
        {
            XmlSerializerNamespaces ns = null;
            if (omitStandardNamespaces)
            {
                ns = new XmlSerializerNamespaces();
                ns.Add("", ""); // Disable the xmlns:xsi and xmlns:xsd lines.
            }
            using (System.IO.StringWriter textWriter = new System.IO.StringWriter())
            {
                XmlWriterSettings settings = new XmlWriterSettings() { Indent = true }; // For cosmetic purposes.
                using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
                    (serializer ?? new XmlSerializer(obj.GetType())).Serialize(xmlWriter, obj, ns);
                return textWriter.ToString();
            }
        }

        public static void TestWrite()
        {
            CEEvents ceEvents = new CEEvents
            {
                CEEvent = new CEEvent[]
                {
                    new CEEvent {
                        TerrainTypesRequirements = new TerrainType[][]
                        {
                            new TerrainType[] {
                                TerrainType.Water,
                                TerrainType.Steppe
                            }
                        }
                    }
                }
            };

            string xml = GetEventXml(ceEvents, omitStandardNamespaces: true);
            string fullPath = BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/TESTXML.xml";
            FileInfo file = new FileInfo(fullPath);
            file.Directory?.Create();
            File.WriteAllText(BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/TESTXML.xml", xml);
        }
#endif


        [DebuggerStepThroughAttribute]
        private static void LogXMLIssueToFile(string msg, string xmlFile = "")
        {
            string fullPath = BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/LoadingFailedXML.txt";
            FileInfo file = new FileInfo(fullPath);
            file.Directory?.Create();
            string contents = xmlFile + " does not comply to CEEventsModal format described in CEEventsModal.xsd : " + msg + Environment.NewLine;
            if (ErrorLines == 0) File.WriteAllText(BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/LoadingFailedXML.txt", contents);
            else File.AppendAllText(fullPath, contents);
            ErrorLines++;
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

            string fullPath = (BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/LogFile{0}.txt").Replace("{0}", TestLog);
            FileInfo file = new FileInfo(fullPath);
            file.Directory?.Create();
            if (Lines == 0) File.WriteAllText((BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/LogFile{0}.txt").Replace("{0}", TestLog), "");

            string contents = DateTime.Now + " -- " + msg + Environment.NewLine;
            File.AppendAllText(fullPath, contents);
            Lines++;
        }
    }
}