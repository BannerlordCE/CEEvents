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

        private static readonly List<CECustomModule> AllModules = new();
        private static readonly List<CEEvent> AllEvents = new();
        private static readonly List<CECustom> AllCustom = new();
        private static readonly List<CEScene> AllScenes = new();

        public static List<CECustom> GetCustom() => AllCustom;

        public static List<CEScene> GetScenes() => AllScenes;

        public static List<CECustomModule> GetModules() => AllModules;

        public static List<CEEvent> GetAllVerifiedXSEFSEvents(List<string> modules)
        {
#if DEBUG
            //TestWrite();
#endif
            string errorPath = BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/LoadingFailedFlagXML.txt";
            FileInfo file = new(errorPath);
            if (file.Exists) file.Delete();
            ErrorLines = 0;
            Lines = 0;

            if (modules.Count != 0)
            {
                foreach (string fullPath in modules)
                {
                    ForceLogToFile("Found new module path to be checked " + fullPath);

                    List<CEEvent> TempEvents = new();

                    try
                    {
                        string[] files = Directory.GetFiles(fullPath, "*.xml", SearchOption.AllDirectories);

                        foreach (string text in files)
                        {
                            if (Path.GetDirectoryName(text).Contains("ModuleData")) continue;

                            if (Path.GetFullPath(text).Contains("\\Scenes\\"))
                            {
                                ForceLogToFile("Custom Scene Found: " + text);

                                if (XMLFileCompliesWithSceneXSD(text))
                                {
                                    AllScenes.AddRange(DeserializeXMLFileToScene(text));
                                    ForceLogToFile("Custom Scene Added: " + text);
                                }

                                continue;
                            }

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

                        CECustomModule item = new(Path.GetFileNameWithoutExtension(fullPath), TempEvents);
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

                List<CEEvent> TempEvents = new();

                foreach (string text in files)
                {
                    if (Path.GetFullPath(text).Contains("\\Scenes\\"))
                    {
                        ForceLogToFile("Custom Scene Found: " + text);

                        if (XMLFileCompliesWithSceneXSD(text))
                        {
                            AllScenes.AddRange(DeserializeXMLFileToScene(text));
                            ForceLogToFile("Custom Scene Added: " + text);
                        }

                        continue;
                    }

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

                CECustomModule item = new(Path.GetFileNameWithoutExtension(fullPath), TempEvents);
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

        // Setting XML
        public static CECustomSettings DeserializeXMLFileToSettings(string XmlFilename)
        {
            CECustomSettings _CESettings;

            try
            {
                if (string.IsNullOrEmpty(XmlFilename)) return null;
                StreamReader textReader = new(XmlFilename);
                XmlSerializer xmlSerializer = new(typeof(CECustomSettings));
                CECustomSettings xsefsevents = (CECustomSettings)xmlSerializer.Deserialize(textReader);
                _CESettings = xsefsevents;
            }
            catch (Exception innerException)
            {
                TextObject textObject = new("{=CEEVENTS1001}Failed to load {FILE} for more information refer to Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs\\LoadingFailedXML.txt");
                textObject.SetTextVariable("FILE", XmlFilename);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Red));

                throw new Exception("ERROR DeserializeXMLFileToSettings:  -- filename: " + XmlFilename, innerException);
            }

            return _CESettings;
        }

        #region Scenes

        private static bool XMLFileCompliesWithSceneXSD(string file)
        {
            string fullPath = BasePath.Name + "Modules/zCaptivityEvents/ModuleLoader/CaptivityRequired/Events/CECustomScenes.xsd";
            XmlSchemaSet xmlSchemaSet = new();
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
                TextObject textObject = new("{=CEEVENTS1002}Failed to load {FILE} for more information refer to Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs\\LoadingFailedXML.txt");
                textObject.SetTextVariable("FILE", file);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Red));
                msg = "ERROR XMLFileCompliesWithSceneXSD:  -- filename: " + file + " : " + innerException;
            }

            bool result;

            if (msg == "")
            {
                result = true;
            }
            else
            {
                LogXMLCustomIssueToFile(msg, file, 1);
                result = false;
            }

            return result;
        }

        public static List<CEScene> DeserializeXMLFileToScene(string XmlFilename)
        {
            List<CEScene> list = new();

            try
            {
                if (string.IsNullOrEmpty(XmlFilename)) return null;
                StreamReader textReader = new(XmlFilename);
                XmlSerializer xmlSerializer = new(typeof(CECustomScenes));
                CECustomScenes xsefsevents = (CECustomScenes)xmlSerializer.Deserialize(textReader);
                list.AddRange(xsefsevents.CEScene);
            }
            catch (Exception innerException)
            {
                TextObject textObject = new("{=CEEVENTS1001}Failed to load {FILE} for more information refer to Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs\\LoadingFailedSceneXML.txt");
                textObject.SetTextVariable("FILE", XmlFilename);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Red));

                throw new Exception("ERROR DeserializeXMLFileToScene:  -- filename: " + XmlFilename, innerException);
            }

            return list;
        }

        #endregion Scenes

        #region Custom Settings

        private static bool XMLFileCompliesWithCustomXSD(string file)
        {
            string fullPath = BasePath.Name + "Modules/zCaptivityEvents/ModuleLoader/CaptivityRequired/Events/CECustomModal.xsd";
            XmlSchemaSet xmlSchemaSet = new();
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
                TextObject textObject = new("{=CEEVENTS1002}Failed to load {FILE} for more information refer to Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs\\LoadingFailedXML.txt");
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
            List<CECustom> list = new();

            try
            {
                if (string.IsNullOrEmpty(XmlFilename)) return null;
                StreamReader textReader = new(XmlFilename);
                XmlSerializer xmlSerializer = new(typeof(CECustom));
                CECustom xsefsevents = (CECustom)xmlSerializer.Deserialize(textReader);
                list.Add(xsefsevents);
            }
            catch (Exception innerException)
            {
                TextObject textObject = new("{=CEEVENTS1001}Failed to load {FILE} for more information refer to Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs\\LoadingFailedXML.txt");
                textObject.SetTextVariable("FILE", XmlFilename);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Red));

                throw new Exception("ERROR DeserializeXMLFileToFlags:  -- filename: " + XmlFilename, innerException);
            }

            return list;
        }

        #endregion Custom Settings

        #region Events

        private static bool XMLFileCompliesWithStandardXSD(string file)
        {
            string fullPath = BasePath.Name + "Modules/zCaptivityEvents/ModuleLoader/CaptivityRequired/Events/CEEventsModal.xsd";
            XmlSchemaSet xmlSchemaSet = new();
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
                TextObject textObject = new("{=CEEVENTS1002}Failed to load {FILE} for more information refer to Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs\\LoadingFailedXML.txt");
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
            List<CEEvent> list = new();

            try
            {
                if (string.IsNullOrEmpty(XmlFilename)) return null;
                StreamReader textReader = new(XmlFilename);
                XmlSerializer xmlSerializer = new(typeof(CEEvents));
                CEEvents xsefsevents = (CEEvents)xmlSerializer.Deserialize(textReader);
                list.AddRange(xsefsevents.CEEvent);
            }
            catch (Exception innerException)
            {
                TextObject textObject = new("{=CEEVENTS1001}Failed to load {FILE} for more information refer to Mount & Blade II Bannerlord\\Modules\\zCaptivityEvents\\ModuleLogs\\LoadingFailedXML.txt");
                textObject.SetTextVariable("FILE", XmlFilename);
                InformationManager.DisplayMessage(new InformationMessage(textObject.ToString(), Colors.Red));

                throw new Exception("ERROR DeserializeXMLFileToObject:  -- filename: " + XmlFilename, innerException);
            }

            return list;
        }

        #endregion Events

        #region Logs

        [DebuggerStepThroughAttribute]
        private static void LogXMLCustomIssueToFile(string msg, string xmlFile = "", int type = 0)
        {
            string location;
            string errorMessage;

            switch (type)
            {
                case 1:
                    location = "LoadingFailedSceneXML.txt";
                    errorMessage = " does not comply to CEScene format described in CECustomScenes.xsd ";
                    break;

                default:
                    location = "LoadingFailedFlagXML.txt";
                    errorMessage = " does not comply to CEFlagsModal format described in CECustomModal.xsd ";
                    break;
            }

            string fullPath = BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/" + location;
            FileInfo file = new(fullPath);
            file.Directory?.Create();
            if (Lines == 0) File.WriteAllText(BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/" + location, "");
            string contents = xmlFile + errorMessage + msg + Environment.NewLine;
            File.AppendAllText(fullPath, contents);
            Lines++;
        }

        [DebuggerStepThroughAttribute]
        private static void LogXMLIssueToFile(string msg, string xmlFile = "")
        {
            string fullPath = BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/LoadingFailedXML.txt";
            FileInfo file = new(fullPath);
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
                TestLog = TestLog switch
                {
                    "FC" => "RT",
                    "RT" => "LT",
                    _ => "FC",
                };
                Lines = 0;
            }

            string fullPath = (BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/LogFile{0}.txt").Replace("{0}", TestLog);
            FileInfo file = new(fullPath);
            file.Directory?.Create();
            if (Lines == 0) File.WriteAllText((BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/LogFile{0}.txt").Replace("{0}", TestLog), "");

            string contents = DateTime.Now + " -- " + msg + Environment.NewLine;
            File.AppendAllText(fullPath, contents);
            Lines++;
        }

        #endregion Logs

#if DEBUG

        public static string GetEventXml(CEEvents obj, XmlSerializer serializer = null, bool omitStandardNamespaces = false)
        {
            XmlSerializerNamespaces ns = null;
            if (omitStandardNamespaces)
            {
                ns = new XmlSerializerNamespaces();
                ns.Add("", ""); // Disable the xmlns:xsi and xmlns:xsd lines.
            }
            using System.IO.StringWriter textWriter = new();
            XmlWriterSettings settings = new() { Indent = true }; // For cosmetic purposes.
            using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
                (serializer ?? new XmlSerializer(obj.GetType())).Serialize(xmlWriter, obj, ns);
            return textWriter.ToString();
        }

        public static void TestWrite()
        {
            CEEvents ceEvents = new()
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
            FileInfo file = new(fullPath);
            file.Directory?.Create();
            File.WriteAllText(BasePath.Name + "Modules/zCaptivityEvents/ModuleLogs/TESTXML.xml", xml);
        }

#endif
    }
}