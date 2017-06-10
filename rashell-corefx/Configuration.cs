﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Rashell
{
    internal class Configuration : Rashell
    {
        private string config_dir_path = AppContext.BaseDirectory;
        private string config_path = AppContext.BaseDirectory + "\\config.conf";
        private string DEF_WORKING_DIR;
        private List<string> EnvironmentPaths = new List<string>();
        private List<string> Known_Extensions = new List<string>();
        private bool ENABLE_EVT_LOG;
        private bool ENABLE_ERR_LOG;
        private bool PRIORITIZE_BUILTIN_SHELL;
        private bool DISPLAY_WELCOME_MSG;

        private Formatters format = new Formatters();
        private Rashell shell = new Rashell();

        public Configuration()
        {
            this.config_dir_path = AppContext.BaseDirectory;
            this.config_path = this.config_dir_path + "\\config.conf";
            this.DEF_WORKING_DIR = null;
            this.ENABLE_ERR_LOG = false;
            this.ENABLE_EVT_LOG = false;
            this.DISPLAY_WELCOME_MSG = false;

            //Read Configuration file
            this.Read();

            //get OnLoadConfigurations
            //This might override any configuration loaded above in the current Read() Method.
            this.OnLoadConfig();
        }

        #region "Config Operators"
        public bool Read()
        {
            bool FoundEnvi = false;
            bool FoundKnown_Ex = false;
            string Working_Dir = null;

        Start:
            if (File.Exists(config_path))
            {
                var config = new FileStream(this.config_dir_path, FileMode.Open);
                StreamReader ConfigReader = new StreamReader(config);
                while (!ConfigReader.EndOfStream)
                {
                    string line = ConfigReader.ReadLine().ToUpper();
                    line = format.RemoveTab(line);
                    line = format.RemoveSpace(line);

                    if (!string.IsNullOrEmpty(line))
                    {
                        if (line.StartsWith("ENVIRONMENT_PATHS {"))
                        {
                            FoundEnvi = true;
                        }
                        else if (line.StartsWith("KNOWN_EXECUTABLES {"))
                        {
                            FoundKnown_Ex = true;
                        }
                        else if (line.StartsWith("DEF_WORKING_DIR ="))
                        {
                            if (line.EndsWith(";"))
                            {
                                Working_Dir = line.Substring(line.IndexOf("=") + 1, (line.Length - (line.IndexOf("=") + 1)) - 1);

                                if (!string.IsNullOrEmpty(Working_Dir) && !string.IsNullOrWhiteSpace(Working_Dir))
                                {
                                    Working_Dir = format.Break(Working_Dir);
                                    if (Directory.Exists(Working_Dir))
                                    {
                                        this.DEF_WORKING_DIR = Working_Dir;
                                    }
                                }
                            }
                        }
                        else if (line.StartsWith("ENABLE_EVT_LOGGING:"))
                        {
                            if (line.EndsWith(";"))
                            {
                                string log = null;
                                log = line.Substring(line.IndexOf(":") + 2, (line.Length - (line.IndexOf(":") + 1)) - 2);
                                if (log == "ON")
                                {
                                    this.ENABLE_EVT_LOG = true;
                                }
                                else if (log == "OFF")
                                {
                                    this.ENABLE_EVT_LOG = false;
                                }
                            }
                        }
                        else if (line.StartsWith("ENABLE_ERR_LOGGING:"))
                        {
                            if (line.EndsWith(";"))
                            {
                                string log = null;
                                log = line.Substring(line.IndexOf(":") + 2, (line.Length - (line.IndexOf(":") + 1)) - 2);
                                if (log == "ON")
                                {
                                    this.ENABLE_ERR_LOG = true;
                                }
                                else if (log == "OFF")
                                {
                                    this.ENABLE_ERR_LOG = false;
                                }
                            }
                        }
                        else if (line.StartsWith("PRIORITIZE_BUILTIN_SHELL:"))
                        {
                            if (line.EndsWith(";"))
                            {
                                string log = null;
                                log = line.Substring(line.IndexOf(":") + 2, (line.Length - (line.IndexOf(":") + 1)) - 2);
                                if (log == "ON")
                                {
                                    this.PRIORITIZE_BUILTIN_SHELL = true;
                                }
                                else if (log == "OFF")
                                {
                                    this.PRIORITIZE_BUILTIN_SHELL = false;
                                }
                            }
                        }
                        else if (line.StartsWith("WELCOME_MESSAGE:"))
                        {
                            if (line.EndsWith(";"))
                            {
                                string log = null;
                                log = line.Substring(line.IndexOf(":") + 2, (line.Length - (line.IndexOf(":") + 1)) - 2);
                                if (log == "ON")
                                {
                                    this.DISPLAY_WELCOME_MSG = true;
                                }
                                else if (log == "OFF")
                                {
                                    this.DISPLAY_WELCOME_MSG = false;
                                }
                            }
                        }
                        else if (FoundEnvi)
                        {
                            string envipath = null;
                            if (line.EndsWith(";") && line.Length > 1)
                            {
                                envipath = line.Substring(0, line.Length - 1);
                                envipath = format.Break(envipath);

                                if (Directory.Exists(envipath))
                                {
                                    this.EnvironmentPaths.Add(envipath);
                                }
                            }
                            else if (line == "}")
                            {
                                FoundEnvi = false;
                            }
                        }
                        else if (FoundKnown_Ex)
                        {
                            string extension = null;
                            if (line.EndsWith(";") && line.Length > 1)
                            {
                                extension = line.Substring(0, line.Length - 1);
                                if (extension.StartsWith("."))
                                {
                                    this.Known_Extensions.Add(extension);
                                }
                            }
                            else if (line == "}")
                            {
                                FoundKnown_Ex = false;
                            }
                        }
                    }
                }

                IDisposable file = ConfigReader;
                file.Dispose();
            }
            else
            {
                //try
                //{
                //    Generate();
                //} catch (Exception e)
                //{
                //    Rashell shell = new Rashell();
                //    shell.restartAsAdmin();
                //}
                
                //goto Start;
            }
            return true;
        }

        public bool Generate()
        {
            string Default = "----------------------------------------------------------------------- \n"
                             + "This is Rashell Auto-Generated Configuration File. \n"
                             + "Use it to add further relations, references and to tweak settings. \n \n"
                             + "Restart is required after this file is modified. \n \n"
                             + "WARNING... This configuration file is version specific (for v0.2) \n"
                             + "You may need to migrate your changes after you update Rashell. \n\n"
                             + "Character \":\" Denotes a commented line.\n"
                             + "----------------------------------------------------------------------- \n \n"
                             + "---Definition of Environment Paths \n \n"
                             + "ENVIRONMENT_PATHS {\n\t";

            string systempaths = Environment.GetEnvironmentVariable("PATH");

            int num = systempaths.Count(x => x == ';');
            string path;

            for (int i = 0; i < num; i++)
            {
                path = systempaths.Substring(0, systempaths.IndexOf(";"));

                int x = i + 1;
                if (x < num)
                {
                    Default += "\"" + path + "\";\n\t";
                } else
                {
                    Default += "\"" + path + "\";\n";
                }
                
                systempaths = systempaths.Remove(0, systempaths.IndexOf(";") + 1);
            }

            //Default += "\"" + systempaths + "\";\n\t";

            Default += "}\n\n"
                          + "---Definition of Known Executables\n\n"
                          + "KNOWN_EXECUTABLES {\n\t"
                          + ".EXE;\n\t"
                          + ".BAT;\n\t"
                          + ".CMD;\n"
                          + "}\n\n"
                          + "---Definition of Default Working Directory\n\n"
                          + "DEF_WORKING_DIR = %userprofile%;\n\n"
                          + "---Toggle Logging\n\n"
                          + "ENABLE_EVT_LOGGING: ON;\n"
                          + ":ENABLE_EVT_LOGGING: OFF;\n\n" 
                          + "ENABLE_ERR_LOGGING: ON;\n"
                          + ":ENABLE_ERR_LOGGING: OFF;\n\n"
                          +"---Give built-in commands priority\n\n"
                          + "PRIORITIZE_BUILTIN_SHELL: ON;\n"
                          + ":PRIORITIZE_BUILTIN_SHELL: OFF;\n\n"
                          + "---Toggle Welcome Message\n\n"
                          + "WELCOME_MESSAGE: ON;\n"
                          + ":WELCOME_MESSAGE: OFF;";

            if (!File.Exists(this.config_path))
            {
                try
                {
                    var config = new FileStream(config_path, FileMode.Open);
                    StreamWriter write = new StreamWriter(config);
                    write.Write(Default);
                    write.Dispose();
                }
                catch (IOException e)
                {
                }
            }
            else
            {
                try
                {
                    Console.Write(" Configuration file aleady exists.\n If you continue, the file will be overwritten with default values.\n \n Do you want to continue anyway? ");
                    string readUser = Console.ReadLine().ToLower();
                    if (readUser == "yes" || readUser == "y")
                    {
                        File.Delete(config_path);
                        var config = new FileStream(config_path, FileMode.Open);
                        StreamWriter write = new StreamWriter(config);
                        write.Write(Default);
                        write.Dispose();
                    }
                }
                catch (IOException e)
                {
                }
            }

            return true;
        }

        public void OnLoadConfig()
        {
            List<string> options = new List<string>();
            List<string> variables = new List<string>();

            options = shell.getOptions(RashellArguments);
            bool active = false;

            foreach (string op in options)
            {
                if (op.StartsWith("config$"))
                {
                    active = true;

                    variables.Add(op.Substring(7));
                }
            }

            if (active)
            {
               foreach (string set in variables)
                {
                    string value = set.Substring(set.IndexOf("=") + 1).ToUpper();
                    string RealSet = set.Substring(0, set.IndexOf("=")).ToUpper();

                    switch (RealSet)
                    {
                        case "DEF_WORKING_DIR":

                            if (value.Contains(" "))
                            {
                                value = "\"" + value + "\"";
                            }

                            this.DEF_WORKING_DIR = format.Break(value);
                            break;
                        case "DISPLAY_WELCOME_MSG":
                           
                            if (value == "OFF") {

                                this.DISPLAY_WELCOME_MSG = false;
                            } else if ( value == "ON" ) {
                                this.DISPLAY_WELCOME_MSG = true;
                            }
                            break;

                        case "PRIORITIZE_BUILTIN_SHELL":
                           
                            if (value == "OFF")
                            {

                                this.PRIORITIZE_BUILTIN_SHELL = false;
                            }
                            else if (value == "ON")
                            {
                                this.PRIORITIZE_BUILTIN_SHELL = true;
                            }
                            break;

                        case "ENABLE_EVT_LOG":

                            if (value == "OFF")
                            {

                                this.ENABLE_EVT_LOG = false;
                            }
                            else if (value == "ON")
                            {
                                this.ENABLE_EVT_LOG = true;
                            }
                            break;

                        case "ENABLE_ERR_LOG":

                            if (value == "OFF")
                            {

                                this.ENABLE_ERR_LOG = false;
                            }
                            else if (value == "ON")
                            {
                                this.ENABLE_ERR_LOG = true;
                            }
                            break;
                    }
                }
            }
        }
        #endregion

        #region "Config Getters"
        public List<string> GetEnvironmentPaths()
        {
            return this.EnvironmentPaths;
        }

        public List<string> GetKnownExtensions()
        {
            return this.Known_Extensions;
        }

        public string GetWorkingDir()
        {
            return this.DEF_WORKING_DIR;
        }

        public bool DisplayMSG()
        {
            return this.DISPLAY_WELCOME_MSG;
        }

        public bool PrioritizeBuiltInShell()
        {
            return this.PRIORITIZE_BUILTIN_SHELL;
        }
        #endregion
    }
}