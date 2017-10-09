/*==========================================================================
INFORMATION ABOUT CODE
============================================================================
>LDF File Parsing
Author: Prajinkya Pimpalghare
Date: 7-October-2017
Version: 1.0
Input Variable: Signal Name| Path of .LDF file
OutPUT: Message name , MinValue, Maximum Value, Scaling , Offset , Unit , Range [Min-Max]
============================================================================*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDF_FILEPARSER
{
    class Program
    {
        public static void  SignalValidation(String InputSignalName, StreamReader reader)
        {
            //Objective: Validate The Signal::It is Present or Not in LDF file
            //Input    : SignalName and OpenFile Stream
            //OutPut   : Returns Signal Is Present Or Not
            string line;
            String SignalStatus = "Signal Not Exist";
            reader.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
            while ((line = reader.ReadLine()) != null)
            {
                if (line == "Signals {")
                {
                    while ((line = reader.ReadLine()) != "}")
                    {
                        string[] Signal = line.Split(':');
                        if (Signal[0].Trim() == InputSignalName)
                            SignalStatus = "Signal Exist In The Provided Ldf File";
                    }
                }
            }
            if (SignalStatus == "Signal Not Exist")
            {
                Console.WriteLine("Signal Not Exist in Provided LDF file");
                Environment.Exit(0);
            }
        }
        public String SignalMesasageSearch(String InputSignalName, StreamReader reader)
        {
            //Objective: Search Related message to the input Signal
            //Input    : SignalName and OpenFile Stream
            //OutPut   : Returns Message Related to the input Signal
            string line;
            String EncodedSignal = null;
            reader.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
            while ((line = reader.ReadLine()) != null)
            {
                if (line == "Signal_representation {")
                {
                    while ((line = reader.ReadLine()) != "}")
                    {
                        string[] Signal = line.Split(':');
                        string[] SignalSplit = Signal[1].Split(',');
                        foreach (string Sig in SignalSplit)
                        {
                            if (Sig.Trim(new Char[] { ';', ' ' }) == InputSignalName)
                                EncodedSignal = Signal[0].Trim();
                        }
                    }
                }
            }
            return (EncodedSignal+" {");// In Signal Encoding It Searches for Message and {
        }
        public Dictionary<string,String> SignalEncoding(String SignalMessageName, StreamReader reader)
        {
            //Objective: Encodes the signal and Provides Physical and Logical Value
            //Input    : Encoded Message
            //OutPut   : Physical 
            Dictionary<string, string> dict = new Dictionary<string, string>();
            string line;
            reader.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
            while ((line = reader.ReadLine()) != null)
            {
                if (line == "Signal_encoding_types {")
                {
                    while ((line = reader.ReadLine()) != "}")
                    {
                        if (line.Trim() == SignalMessageName)
                        {
                            while ((line = reader.ReadLine()) != "\t}")
                            {
                                string[] Signal = line.Split(',');
                                if (Signal[0].Trim() == "physical_value")
                                {
                                    dict.Add("MinValue", Signal[1]);
                                    dict.Add("MaxValue", Signal[2]);
                                    dict.Add("Scaling", Signal[3]);
                                    Program.StringToFloat(Signal[1]);
                                    dict.Add("Offset", Signal[4].Trim(new Char[] { ';', ' ' }));
                                    try
                                    {
                                        dict.Add("Unit", Signal[5].Trim(new Char[] { ';', ' ' }));
                                    }
                                    catch
                                    {
                                        dict.Add("Unit", null);
                                    }
                                    float[] UpdateValue = Program.MaxValueConversion(Program.StringToFloat(Signal[1]), Program.StringToFloat(Signal[2]),Program.StringToFloat(Signal[3]), Program.StringToFloat(Signal[4]), dict["Unit"]);
                                    dict.Add("MinRange", UpdateValue[0].ToString());
                                    dict.Add("MaxRange", UpdateValue[1].ToString());
                                }
                            }
                        }
                    }
                }
            }
            return (dict);
        }
        public static float[] MaxValueConversion(float InitValue, float DefaultValue, float Offset, float MinValue, String Unit)
        {
            //Objective: For Finding The Range Of Signal 
            //Input    : Important=Unit of signal
            //OutPut   : Range of the signal
            if (InitValue == 4)//For Special Kinds Of Signal:ST_DIAG_OBD
            {
                MinValue = InitValue;
            }
            if (Unit == null)
            {
                return new float[] { MinValue, DefaultValue };
            }
            else if (Unit == "\"?C\"" || Unit == "\"A\"")
            {
                return new float[] { MinValue, DefaultValue-Offset};
            }
            else if (Unit == "\"1/min\"" || Unit == "\"V\"")
            {
                return new float[] { MinValue, Offset * DefaultValue };
            }
            else
            {
                return new float[] { MinValue, DefaultValue };//For Encoding Not Possible with LDF 
            }
        }
        public static float StringToFloat(String StringValue)
        {
            //Objective: String To Float Value Conversion
            float Temp;
            float.TryParse(StringValue, out Temp);
            return (Temp);
        }
        public static void PathValidation(String Path)
        {
            //It Validates The Path
            if (System.IO.File.Exists(Path))
            {
                Console.WriteLine("Path Exist");
            }
            else
            {
                Console.WriteLine("Please Enter Correct Path Of LDF File");
                Environment.Exit(0);
            }
        }
        static void Main(string[] args)
        {
			//Main Program Starts here::
			//This module will parse the LDF LIn file and provid ethe Physical value of the signal.
            Console.WriteLine("Please Provide The Signal Name");
            string InputSignalName = Console.ReadLine();
            Console.WriteLine("Please Provide The LDF File Path");
            string LDFFilePath = Console.ReadLine();
            Program.PathValidation(LDFFilePath);
            StreamReader reader = File.OpenText(LDFFilePath);
            Program.SignalValidation(InputSignalName, reader);
            Program SearchSignal = new Program();
            String SignalMessageName=SearchSignal.SignalMesasageSearch(InputSignalName, reader);
            Console.WriteLine("InputSignalName = " + InputSignalName);
            Dictionary<string, string> dict=SearchSignal.SignalEncoding(SignalMessageName, reader);
            if (dict.Count != 0)//For Printing Values, Can bbe removed when integrated with other programs
            {
                foreach (KeyValuePair<string, string> item in dict)
                    Console.WriteLine("Key: {0}, Value: {1}", item.Key, item.Value);
            }
            else
                Console.WriteLine("This Signal Having Only Logical Values");
            Console.ReadLine();

        }
    }
}
