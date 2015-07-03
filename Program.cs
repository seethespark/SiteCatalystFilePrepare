/*  
    Copyright (c) 2013 Nick Whiteley
    
    This file is part of RemoveCommasAndQuotes and SiteCatalystFilePrepare.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the MIT License
    along with this program. 
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SiteCatFilePrepare
{
    class Program
    {
        static void Main(string[] args)
        {
            int bufferLength = 8192; // 8k seems to work quickest
            DateTime startTime = DateTime.Now;
            int linesLoaded = 0;
            string tempFileName = "outputTemp_9j9.csv"; // temp name should be unique in the folder.  Only used if source and destination files are the same.

            /// Check for Help request otherwise output standard header
            if (args.Length == 1 && (args[0] == "?" | args[0] == "/?" | args[0] == "-?"))
            {
                Console.WriteLine("");
                Console.WriteLine("# Remove commas and double quotes quotes from a file.");
                Console.WriteLine("# Change format from UTF-8.  Change from UNIX to PC style if necessary.");
                Console.WriteLine("# This allows SQL Bulk Insert to work.");
                Console.WriteLine("# Nick Whiteley - (c) 2012-2013.");
                Console.WriteLine("# http://www.nickwhiteley.com");
                Console.WriteLine("# Argument 1 should be the source file.");
                Console.WriteLine("# Argument 2 should be the destination file.");
                Console.WriteLine("# File names can be local to this program or can be a full Windows file path.");
                Console.WriteLine("# Any file given in argument 2 will be overwritten.");
                Console.WriteLine("# Commas and new lines in quoted text are replaced with HTML escape strings.");
                return;
            }
            else
            {
                Console.WriteLine("");
                Console.WriteLine("#  SiteCat file preperation for SQL Buklk Import.");
                Console.WriteLine("#  Nick Whiteley - (c) 2012-2013.");
                Console.WriteLine("#  For help use ? as only argument.");
                Console.WriteLine("");
            }

            /// Check all is OK with the inputs
            if (args.Length < 2)
            {
                Console.WriteLine("! Error -- Missing arguments.");
                return;
            }
            if (args[0] == null || args[1] == null)
            {
                Console.WriteLine("! Error -- Missing file names.");
                return;
            }
            if (args.Length > 2)
            {
                Console.WriteLine("! Error -- Too many arguments.");
                return;
            }
            if (args[0] == "" || args[1] == "")
            {
                Console.WriteLine("! Error -- Missing file names.");
                return;
            }

            /// Get the file names from the parameeters
            string inputFile = args[0];
            string outputFile = args[1];

            /// Check the parameters are OK
            if (inputFile == "" || outputFile == "")
            {
                Console.WriteLine("! Error -- Missing file names.");
                return;
            }
            if (!File.Exists(inputFile))
            {
                Console.WriteLine("! Error -- Missing input file.");
                return;
            } 
            /// If the input and output filenames are the same set a temp name
            if (inputFile == outputFile)
            {
                if (inputFile.IndexOf("\\") > 0)
                {
                    string filePath;
                    filePath = inputFile.Substring(0, inputFile.LastIndexOf("\\")+1);
                    tempFileName = filePath + tempFileName;
                }

                outputFile = tempFileName;
            }

            try
            {
                bool isUnix = true;
                using (StreamReader sr = new StreamReader(inputFile, true))
                {
                    int i = 0;
                    char[] buffer = new char[bufferLength];
                    /// Check if newlines are \n or \r\n.  If just \n then it's a UNIX file and needs reformating.
                    /// This requires the file to not be more than 16k wide.
                    sr.Read(buffer, 0, buffer.Length);
                    foreach (char c in buffer)
                    {
                        if (c == "\r".ToCharArray()[0])
                            if (buffer[i + 1] == "\n".ToCharArray()[0])
                            {
                                isUnix = false;
                                break;
                            }
                        if (c == "\n".ToCharArray()[0])
                            break;
                        i++;
                    }
                }

                using (StreamReader sr = new StreamReader(inputFile, true))
                {
                    using (FileStream fsW = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                    using (BufferedStream bsW = new BufferedStream(fsW))
                    using (StreamWriter writer = new StreamWriter(bsW, Encoding.ASCII)) // 
                    {
                        bool isQuoted = false;
                        char[] buffer = new char[bufferLength];
                        char emptyChar = "\0".ToCharArray()[0];
                        char commaChar = ",".ToCharArray()[0];
                        char newlineChar = "\n".ToCharArray()[0];
                        char linefeedChar = "\r".ToCharArray()[0];
                        char quoteChar = "\"".ToCharArray()[0];
                        
                        /// Go back to the start of the stream.
                        sr.BaseStream.Position = 0;

                        
                        /// Load data into the buffer and loop through all characters to replace commas in quoted text and quotes.
                        //while ((len = sr.Read(buffer, 0, buffer.Length)) != 0)
                        int streamPosition = 0;
                       
                        while (!sr.EndOfStream)
                        {
                            /// Clean the buffer
                            buffer = new char[bufferLength];
                            sr.Read(buffer, 0, buffer.Length);
                            foreach (char c in buffer)
                            {
                                /// Clean the buffer
                                //buffer = new char[bufferLength];
                                streamPosition++;
                                if (c == quoteChar)
                                {
                                    if (isQuoted == false)
                                        isQuoted = true;
                                    else
                                        isQuoted = false;
                                }
                                if (c == commaChar && isQuoted)
                                    writer.Write("&#44;".ToCharArray());
                                else if (c == newlineChar && isQuoted)
                                    writer.Write("&#10;".ToCharArray());
                                else if (c == linefeedChar && isQuoted)
                                    writer.Write("&#10;".ToCharArray());
                                else if (c == newlineChar && isUnix)
                                {
                                    linesLoaded++;
                                    writer.Write("\r\n");
                                }
                                else if (c == newlineChar)
                                {
                                    linesLoaded++;
                                    writer.Write(c);
                                }
                                else if (c != quoteChar && c != emptyChar)
                                {
                                    writer.Write(c);
                                }
                            }
                        }
                    }

                }
                /// If the input and output filenames are the same then delete and rename
                if (tempFileName == outputFile)
                {
                    File.Delete(inputFile);
                    File.Move(tempFileName, inputFile);
                    File.Delete(tempFileName);
                }
                Console.WriteLine("All done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something broke so I'm now sulking.");
                Console.WriteLine(ex.Message);
                return;
            }
            TimeSpan runTime = DateTime.Now - startTime;
            Console.WriteLine("Converted " + linesLoaded.ToString() + " lines");
            Console.WriteLine("Completed in " + runTime.TotalSeconds.ToString() + " seconds");
            //Console.ReadKey();

        }
    }
}
