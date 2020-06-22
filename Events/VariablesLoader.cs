using System;
using CaptivityEvents.Custom;
using TaleWorlds.Core;

namespace CaptivityEvents.Events
{
    public class VariablesLoader
    {
        public int GetIntFromXML(string numpassed)
        {
            try
            {
                var number = 0;

                if (numpassed == null) return number;

                if (numpassed.StartsWith("R"))
                {
                    var splitPass = numpassed.Split(' ');

                    switch (splitPass.Length)
                    {
                        case 3:
                            var numberOne = int.Parse(splitPass[1]);
                            var numberTwo = int.Parse(splitPass[2]);

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
                CECustomHandler.ForceLogToFile("Failed to parse " + numpassed);

                return 0;
            }
        }

        public float GetFloatFromXML(string numpassed)
        {
            try
            {
                var number = 0f;

                if (numpassed == null) return number;

                if (numpassed.StartsWith("R"))
                {
                    var splitPass = numpassed.Split(' ');

                    switch (splitPass.Length)
                    {
                        case 3:
                            var numberOne = float.Parse(splitPass[1]);
                            var numberTwo = float.Parse(splitPass[2]);

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
                CECustomHandler.LogToFile("Failed to parse " + numpassed);

                return 0f;
            }
        }
    }
}