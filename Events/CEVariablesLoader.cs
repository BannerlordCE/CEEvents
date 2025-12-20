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

                return [stringpassed];
            }
        }

        public int GetIntFromXML(string numPassed)
        {
            try
            {
                int number = 0;

                if (numPassed == null) return number;

                if (numPassed.StartsWith("R"))
                {
                    string[] splitPass = numPassed.Split(' ');

                    switch (splitPass.Length)
                    {
                        case 3:
                            int numberOne = int.Parse(splitPass[1]);
                            int numberTwo = int.Parse(splitPass[2]);

                            number = numberOne < numberTwo ? MBRandom.RandomInt(numberOne, numberTwo) : MBRandom.RandomInt(numberTwo, numberOne);

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
                    number = int.Parse(numPassed);
                }

                return number;
            }
            catch (Exception)
            {
                CECustomHandler.ForceLogToFile("Failed to parse int " + numPassed);

                return 0;
            }
        }

        public float GetFloatFromXML(string numPassed)
        {
            try
            {
                float number = 0f;

                if (numPassed == null) return number;

                if (numPassed.StartsWith("R"))
                {
                    string[] splitPass = numPassed.Split(' ');

                    switch (splitPass.Length)
                    {
                        case 3:
                            float numberOne = float.Parse(splitPass[1]);
                            float numberTwo = float.Parse(splitPass[2]);

                            number = numberOne < numberTwo ? MBRandom.RandomFloatRanged(numberOne, numberTwo) : MBRandom.RandomFloatRanged(numberTwo, numberOne);

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
                    number = float.Parse(numPassed);
                }

                return number;
            }
            catch (Exception)
            {
                CECustomHandler.LogToFile("Failed to parse float " + numPassed);

                return 0f;
            }
        }
    }
}