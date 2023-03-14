using CaptivityEvents.Custom;
using System;
using TaleWorlds.Core;

namespace CaptivityEvents.Events
{
    public class CEVariablesLoader
    {
        public string[] GetStringFromXML(string stringpassed)
        {
            try
            {
                string[] stringArray = stringpassed.Split(',');

                for (int i = 0; i < stringArray.Length; i++)
                {
                    stringArray[i] = stringArray[i].Trim();
                }
                return stringArray;
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Failed to parse int " + stringpassed);

                return new string[1] { stringpassed };
            }
        }

        public int GetIntFromXML(string numpassed)
        {
            try
            {
                int number = 0;

                if (numpassed == null) return number;

                if (numpassed.StartsWith("R"))
                {
                    string[] splitPass = numpassed.Split(' ');

                    switch (splitPass.Length)
                    {
                        case 3:
                            int numberOne = int.Parse(splitPass[1]);
                            int numberTwo = int.Parse(splitPass[2]);

                            number = numberOne < numberTwo
                                ? MBRandom.RandomInt(numberOne, numberTwo)
                                : MBRandom.RandomInt(numberTwo, numberOne);
                            break;

                        case 2:
                            number = MBRandom.RandomInt(int.Parse(splitPass[1]));
                            break;

                        default:
                            number = MBRandom.RandomInt();
                            break;
                    }
                }
                else
                {
                    number = int.Parse(numpassed);
                }

                return number;
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Failed to parse int " + numpassed);

                return 0;
            }
        }

        public float GetFloatFromXML(string numpassed)
        {
            try
            {
                float number = 0f;

                if (numpassed == null) return number;

                if (numpassed.StartsWith("R"))
                {
                    string[] splitPass = numpassed.Split(' ');

                    switch (splitPass.Length)
                    {
                        case 3:
                            float numberOne = float.Parse(splitPass[1]);
                            float numberTwo = float.Parse(splitPass[2]);

                            number = numberOne < numberTwo
                                ? MBRandom.RandomFloatRanged(numberOne, numberTwo)
                                : MBRandom.RandomFloatRanged(numberTwo, numberOne);

                            break;

                        case 2:
                            number = MBRandom.RandomFloatRanged(float.Parse(splitPass[1]));

                            break;

                        default:
                            number = MBRandom.RandomFloat;

                            break;
                    }
                }
                else
                {
                    number = float.Parse(numpassed);
                }

                return number;
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Failed to parse float " + numpassed);

                return 0f;
            }
        }
    }
}