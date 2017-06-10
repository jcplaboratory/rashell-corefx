﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rashell
{
    class Formatters
    {
        protected List<string> arguments = new List<string>();
        #region "Formatters"
        public string Break(string stdin)
        {
         
            int spaces = CountSpaces(stdin);
            int tabs = CountTabs(stdin);

            stdin = RemoveTab(stdin);
            stdin = RemoveSpace(stdin); //reload reformatted commands into main stream
            
            //create array to store args
            string[] args = new string[spaces + tabs + 1];
            args.Initialize();


            //separate cmd and args
            int i = 0;
            bool foundDCom = false; //double inverted commas presence checker
            bool foundEnvi = false;
            string enviVar = null;

            foreach (char c in stdin)
            {
                if (c.ToString() == " ") //looks out for space
                {
                    if (!foundDCom) //move to the next in array if double comma wasn't previously found
                    {
                        i++; //increment array index
                    }
                    else //if comma was previous found continue adding characters to the current index
                    {
                        args[i] += " ";
                    }
                }
                else if (c.ToString() == "%" && foundEnvi == false) //looks for '%'
                {
                    int count = 0;
                    foreach (char ch in stdin) //counts the number of '%' in stdin
                    {
                        if (ch.ToString().Equals("%"))
                        {
                            count++;
                        }
                    }

                    double rm = count % 2; //number of '%' must be even

                    if (rm.Equals(0)) //if even number of '%' was found add chars to enviVar
                    {
                        foundEnvi = true;
                        enviVar = null;
                        enviVar += c;
                    }
                    else
                    {
                        args[i] += c; //if not even, continue adding to current index
                    }
                }
                else
                {
                    if (c.ToString().Equals("\"") && foundDCom == false) //if double inverted comma found set foundDcom = true
                    {
                        foundDCom = true;
                    }
                    else if (c.ToString().Equals("\"") && foundDCom)
                    {
                        foundDCom = false; //if comma was previously found, means closing comma was found. Set foundDcom = false
                    }
                    else if (!c.ToString().Equals("%") && foundEnvi)
                    {
                        enviVar += c; //increment enviVar while closing '%' is not found
                    }
                    else if (c.ToString().Equals("%") && foundEnvi)
                    {
                        foundEnvi = false; //closing '%' found. set foundEnvi = false and expand Variable in enviVar
                        enviVar += c;
                        args[i] = Environment.ExpandEnvironmentVariables(enviVar);
                    }
                    else { args[i] += c; }

                }
            }


            this.arguments.Clear();
            i = 0;
            foreach (string item in args)
            {
                if (!i.Equals(0))
                {
                    this.arguments.Add(item);
                }
                i++;
            }
        
            return args[0];
        }

        public List<string> getArguments()
        {
            return this.arguments;
        }

        public string RemoveSpace(string stdin)
        {
            //count and remove redundant spaces.
            bool foundSpace = false;
            string newStdin = null;
      
            if (!string.IsNullOrEmpty(stdin) && !string.IsNullOrWhiteSpace(stdin))
            {
                int i = 1;
                foreach (char chr in stdin)
                {
                    if (!foundSpace && chr.ToString() != " ")
                    {
                        newStdin += chr;
                    }
                    else if (foundSpace && chr.ToString() != " ")
                    {
                        newStdin += chr;
                        foundSpace = false;
                    }
                    else if (!foundSpace && chr.ToString() == " ")
                    {
                        if (!string.IsNullOrEmpty(newStdin))
                        {
                            if (i != stdin.Length) //prevents unneeded space after the line
                            {
                                newStdin += " ";
                            } 
                        }
                        foundSpace = true;
                    }
                    i++;
                }
            }
           
            return newStdin;
        }

        public string RemoveTab(string stdin)
        {
            //count and remove redundant spaces.
            bool foundTab = false;
            string newStdin = null;

            if (!string.IsNullOrEmpty(stdin) && !string.IsNullOrWhiteSpace(stdin))
            {
                foreach (char chr in stdin)
                {
                    if (!foundTab && chr.ToString() != "\t")
                    {
                        newStdin += chr;
                    }
                    else if (foundTab && chr.ToString() != "\t")
                    {
                        newStdin += chr;
                        foundTab = false;
                    }
                    else if (!foundTab && chr.ToString() == "\t")
                    {
                        if (!string.IsNullOrEmpty(newStdin))
                        {
                            newStdin += " ";
                        }
                        foundTab = true;
                    }
                }
            }
               
            return newStdin;
        }

        public int CountSpaces(string stdin)
        {
            int spaces = 0;
            foreach (char c in stdin)
            {
                if (c.ToString() == " ")
                {
                    spaces++;
                }
            }
            return spaces;
        }

        public int CountTabs(string stdin)
        {
            int tabs = 0;
            foreach (char c in stdin)
            {
                if (c.ToString() == "\t")
                {
                    tabs++;
                }
            }
            return tabs;
        }

        public void ConsoleColorWrite(string value, ConsoleColor color, bool InLine)
        {
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            Console.ForegroundColor = color;

            if (InLine)
            {
                Console.Write(value/*PadRight(Console.WindowWidth - 1)*/); // <-- see note
                                                                            //
                                                                            // Reset the color.
                                                                            //
            } else
            {
                Console.WriteLine(value.PadRight(Console.WindowWidth - 1));
            }

            //Reset Color
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public string GetInvalidChars(string stdin)
        {
            char[] InvalidChars = Path.GetInvalidPathChars();
            bool UseLong = false;
            string invalidChr = null;
            string Long = null;

            foreach (char chr in InvalidChars)
            {
                if (chr.ToString() != "\"")
                {
                    if (stdin.Contains(chr.ToString()))
                    {
                        if (invalidChr == null)
                        {
                            invalidChr = chr.ToString();
                        }
                        else
                        {

                            if (chr.ToString() == "\t")
                            {
                                Long = "[TAB]";
                                UseLong = true;
                            }

                            if (UseLong)
                            {
                                invalidChr += ", " + Long;
                                UseLong = false;
                            }
                            else
                            {
                                invalidChr += ", " + chr.ToString();
                            }

                        }
                    }
                }
            }

            invalidChr = RemoveSpace(invalidChr);
           
            return invalidChr;
        }
        #endregion
    }
}
