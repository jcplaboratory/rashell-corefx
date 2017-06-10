﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Rashell
{
    internal class Executor
    {
        protected Command command = new Command();
        private Formatters format = new Formatters();
        private Rashell shell = new Rashell();

        #region "Executor"

        public void exec_in(string command, List<string> Arguments)
        {
            switch (command)
            {
                case "clear":
                    this.command.Clear();
                    break;

                case "bye":
                    this.command.Exit();
                    break;

                case "quit":
                    this.command.Exit();
                    break;

                case "exit":
                    this.command.Exit();
                    break;

                case "time":
                    this.command.Time();
                    break;

                case "date":
                    this.command.Date();
                    break;

                case "mkdir":
                    this.command.Mkdir(Arguments);
                    break;

                case "ls":
                    this.command.Ls(Arguments);
                    break;

                case "cd":
                    this.command.cd(Arguments);
                    break;

                case "pwd":
                    this.command.pwd();
                    break;

                case "echo":
                    this.command.echo(Arguments);
                    break;

                //case "whoiam":
                //    this.command.whoami();
                //    break;

                default:
                    Console.WriteLine("Command or Operator " + "\"" + command + "\"" + " not found." + "\nCheck syntax.");
                    break;
            }
        }

        public bool Execute(string cmd, List<string> arguments)
        {
            string Arguments = null;

            foreach (string arg in arguments)
            {
                Arguments += " " + arg;
            }

            Process process = new Process();
            ProcessStartInfo property = new ProcessStartInfo(cmd)
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = Arguments,
            };

            process.StartInfo = property;

            // Depending on your application you may either prioritize the IO or the exact opposite
            Thread outputThread = new Thread(outputReader) { Name = "ChildIO Output", IsBackground = true };
            Thread errorThread = new Thread(errorReader) { Name = "ChildIO Error", IsBackground = true };
            Thread inputThread = new Thread(inputReader) { Name = "ChildIO Input",  IsBackground = true };

            // Start the IO threads
            try
            {
                process.Start();
                //start reader threads
                outputThread.Start(process);
                errorThread.Start(process);
                inputThread.Start(process);

            } catch (Exception x)
            {
                if (x.ToString().Contains("requires elevation"))
                {
                    Console.WriteLine("Rashell: Unable to start application \"" + cmd + "\"" + ".");
                    format.ConsoleColorWrite("Requires Elevation", ConsoleColor.Red, false);

                    Console.Write("You want to restart Rashell with Elevated Priviledges? (Y/N):");
                    string reply = Console.ReadLine().ToLower();

                    reply = format.RemoveTab(reply);
                    reply = format.RemoveSpace(reply);

                    if (reply == "y" || reply == "yes")
                    {
                        //shell.restartAsAdmin();
                    } else
                    {
                        goto KillThreads;
                    }
                }
                else if (x.ToString().Contains("specified executable is not a valid"))
                {
                    Console.WriteLine("Rashell: Unable to start \"" + cmd + "\"" + ".");
                    format.ConsoleColorWrite("Invalid File Type", ConsoleColor.Red, false);

                    goto KillThreads;
                }
                else
                {
                    Console.WriteLine("Rashell: Unable to start \"" + cmd + "\"" + ".");
                    format.ConsoleColorWrite("Unknown Error", ConsoleColor.Yellow, false);

                    goto KillThreads;
                }
            }

            // Signal to end the application
            ManualResetEvent stopApp = new ManualResetEvent(false);

            // Enables the exited event and set the stopApp signal on exited
            process.EnableRaisingEvents = true;
            process.Exited += (e, sender) => { stopApp.Set(); };

            // Wait for the child app to stop
            stopApp.WaitOne();

            // Kill all started threads when child ends.
     KillThreads:
          
 
            return true;
        }

        /// <summary>
        /// Continuously copies data from one stream to the other.
        /// </summary>
        /// <param name="instream">The input stream.</param>
        /// <param name="outstream">The output stream.</param>
        private static void passThrough(Stream instream, Stream outstream)
        {

            try
            {
                byte[] buffer = new byte[4096];
                while (true)
                {
                    int len;
                    while ((len = instream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        outstream.Write(buffer, 0, len);
             
                        outstream.Flush();
                    }
                }
            } catch (Exception e)
            {

            }
           
        }

        private static void outputReader(object p)
        {
            var process = (Process)p;
            // Pass the standard output of the child to our standard output
            passThrough(process.StandardOutput.BaseStream, Console.OpenStandardOutput());
        }

        private static void errorReader(object p)
        {
            var process = (Process)p;
            // Pass the standard error of the child to our standard error
            passThrough(process.StandardError.BaseStream, Console.OpenStandardError());
        }

        private static void inputReader(object p)
        {
            var process = (Process)p;
            // Pass our standard input into the standard input of the child
            passThrough(Console.OpenStandardInput(), process.StandardInput.BaseStream);
        }
    }

    #endregion "Executors"
}
