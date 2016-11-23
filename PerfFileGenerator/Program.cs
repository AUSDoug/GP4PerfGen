﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess; //CSV Tools
using IO = System.IO;
using System.Diagnostics;


//Grand Prix 4 GPxPatch Performance file generator. Version 1.1
//Takes CSVs as input, and generates a GPxPatch 4.44 compatible Performance file for Grand Prix 4
//Copyright(C) 2016 Douglas Spangenberg

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, version 3 of the License.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.

//In relation specifically to use for the Grand Prix series of games by Microprose,
//this program (both source code and compiled .exe) are subject to the GrandPrixGames.org permission policy.
//You should have received a copy of this policy with the program. If not, see <https://www.grandprixgames.org/read.php?4,1010656>


namespace PerfFileGenerator
{
    class Program
    {
        static string appPath = IO.Directory.GetCurrentDirectory();

        //File names
        static String teamCSV, driverCSV, perfFileName;
        //Has a custom .INI?
        static Boolean customINI;
        //Custom INI name
        static String custININame;
        //Column names for Team, Engine, Power, Reliability
        static String teamNameCol, engineCol, racePowerCol, qualPowerCol, reliabilityCol;
        //Column names for Driver, Number, Grip, Nationality, Tag
        static String driverCol, driverNumCol, raceGripCol, raceGripVarCol, qualGripCol, qualGripVarCol, driverNatCol, driverTagCol;
        //Do we have; Driver Numbers? Driver Nationalities? 3-Letter Driver Codes?
        static Boolean driverNumbers, driverNat, driverTag;
        //File names
        static int teamNameColInt, engineColInt, racePowerColInt, qualPowerColInt, reliabilityColInt, driverColInt, driverNumColInt, raceGripColInt, raceGripVarColInt, qualGripColInt, qualGripVarColInt, driverNatColInt, driverTagColInt;

        //Accepts up to three arguments: CSV containing Team info; CSV containing Driver info; Output file name (Optional)
        static void Main(string[] args)
        {
            //If the user provides a Settings.ini
            if (args.Length > 0)
            {
                customINI = true;
                IO.File.Copy(args[0], appPath + "/PerfGenCustSettings.ini");
                custININame = "/PerfGenCustSettings.ini";
            }
            //Setup logging of the console
            consoleLogger();

            //INI Initialisation
            Trace.WriteLine("---INI DATA BEGIN:---\n");
            if (customINI)
            {
                iniLoader(custININame);
            }
            else
            {
                iniLoader("/Settings.ini");
            }

            //Start the writer
            IO.StreamWriter writer;

            try
            {
                //Data from our CSVs
                MutableDataTable team = DataTable.New.ReadCsv(teamCSV);
                MutableDataTable driver = DataTable.New.ReadCsv(driverCSV);

                #region Column Indexes
                teamNameColInt = team.GetColumnIndex(teamNameCol);
                engineColInt = team.GetColumnIndex(engineCol);
                racePowerColInt = team.GetColumnIndex(racePowerCol);
                qualPowerColInt = team.GetColumnIndex(qualPowerCol);
                reliabilityColInt = team.GetColumnIndex(reliabilityCol);
                driverColInt = driver.GetColumnIndex(driverCol);
                if (driverNumbers)
                {
                    driverNumColInt = driver.GetColumnIndex(driverNumCol);
                }
                raceGripColInt = driver.GetColumnIndex(raceGripCol);
                raceGripVarColInt = driver.GetColumnIndex(raceGripVarCol);
                qualGripColInt = driver.GetColumnIndex(qualGripCol);
                qualGripVarColInt = driver.GetColumnIndex(qualGripVarCol);
                if (driverNat)
                {
                    driverNatColInt = driver.GetColumnIndex(driverNatCol);
                }
                if (driverTag)
                {
                    driverTagColInt = driver.GetColumnIndex(driverTagCol);
                }
                #endregion

                //Indexes to cycle after each write.
                int teamIndex = 0, driver1Index = 0, driver2Index = 1;
                //If no driver numbers are provided, fill them in, 1-22
                int driver1Number = 1, driver2Number = 2;
                //Faux Country - If country codes are not provided, but driver tags *are*, we need a Faux country to use
                string fauxCountry = "UN";

                //Looking for custom Performance File name
                if (perfFileName.Length > 0)
                {
                    writer = new IO.StreamWriter(perfFileName + ".txt");
                }
                else
                {
                    writer = new IO.StreamWriter("Performance.txt");
                }

                //Begin writing
                writer.WriteLine("** Grand Prix 4 Performance File - Generated by GP4PerfGen v1.2 **");
                writer.WriteLine("[File]");
                writer.WriteLine("Version=213");

                //For every team
                foreach (Row row in team.Rows)
                {
                    #region Team
                    if (teamIndex < 10)
                    {
                        writer.WriteLine("[Team #0" + teamIndex + "]");
                    }
                    else
                    {
                        writer.WriteLine("[Team #" + teamIndex + "]");
                    }
                    //Team name and Engine
                    writer.WriteLine("Name=" + team.Columns[teamNameColInt].Values[teamIndex] + "," + team.Columns[engineColInt].Values[teamIndex] + "");
                    //Race BHP, Qual BHP, Reliability
                    writer.WriteLine("Performance=" + team.Columns[racePowerColInt].Values[teamIndex] + "," + team.Columns[qualPowerColInt].Values[teamIndex] + "," + team.Columns[reliabilityColInt].Values[teamIndex] + "");
                    #endregion

                    #region Driver 1
                    writer.Write("First Driver=");
                    if (driverNumbers)
                    {
                        writer.Write(driver.Columns[driverNumColInt].Values[driver1Index]);
                    }
                    else
                    {
                        writer.Write(driver1Number);
                    }
                    writer.Write("," + driver.Columns[driverColInt].Values[driver1Index] + "," + driver.Columns[raceGripColInt].Values[driver1Index] + "," + driver.Columns[raceGripVarColInt].Values[driver1Index] + "," + driver.Columns[qualGripColInt].Values[driver1Index] + "," + driver.Columns[qualGripVarColInt].Values[driver1Index]);
                    //If a driver tag is specified, a Nationality must be as well.
                    //If we don't have a nationality, this will force use our Faux Country and then write the tag.
                    if (driverTag)
                    {
                        if (driverNat)
                        {
                            writer.Write("," + driver.Columns[driverNatColInt].Values[driver1Index]);
                        }
                        else
                        {
                            writer.Write("," + fauxCountry);
                        }
                        writer.Write("," + driver.Columns[driverTagColInt].Values[driver1Index] + "\r\n");
                    }
                    //Otherwise, we can just write the Nationality
                    else if (driverNat)
                    {
                        writer.Write("," + driver.Columns[driverNatColInt].Values[driver1Index] + "\r\n");
                    }
                    else
                    {
                        writer.Write("\r\n");
                    }
                    #endregion


                    #region Driver 2
                    writer.Write("Second Driver=");
                    if (driverNumbers)
                    {
                        writer.Write(driver.Columns[driverNumColInt].Values[driver2Index]);
                    }
                    else
                    {
                        writer.Write(driver2Number);
                    }
                    writer.Write("," + driver.Columns[driverColInt].Values[driver2Index] + "," + driver.Columns[raceGripColInt].Values[driver2Index] + "," + driver.Columns[raceGripVarColInt].Values[driver2Index] + "," + driver.Columns[qualGripColInt].Values[driver2Index] + "," + driver.Columns[qualGripVarColInt].Values[driver2Index]);
                    if (driverTag)
                    {
                        if (driverNat)
                        {
                            writer.Write("," + driver.Columns[driverNatColInt].Values[driver2Index]);
                        }
                        else
                        {
                            writer.Write("," + fauxCountry);
                        }
                        writer.Write("," + driver.Columns[driverTagColInt].Values[driver2Index] + "\r\n");
                    }
                    else if (driverNat)
                    {
                        writer.Write("," + driver.Columns[driverNatColInt].Values[driver2Index] + "\r\n");
                    }
                    else
                    {
                        writer.Write("\r\n");
                    }
                    #endregion

                    //Increment indexes
                    teamIndex++;
                    driver1Index = driver1Index + 2;
                    driver2Index = driver2Index + 2;
                    driver1Number = driver1Number + 2;
                    driver2Number = driver2Number + 2;
                }

                //Close the writer
                writer.Close();

                //Clean-up if necessary
                if (customINI)
                    IO.File.Delete(appPath + "/PerfGenCustSettings.ini");
            }
            catch (IndexOutOfRangeException)
            {
                Trace.WriteLine("An index was out of range!");
                Trace.WriteLine("Have you got enough Drivers to fill your Teams?");
            }
            catch (IO.FileNotFoundException ex)
            {
                Trace.WriteLine("A file was missing.");
                Trace.WriteLine(ex.Message);
            }
            Trace.WriteLine("-----Ending Session at " + DateTime.Now + "-----\n");

        }

        //Setup for writing Console Output to a text file.
        private static void consoleLogger()
        {
            Trace.Listeners.Clear();

            TextWriterTraceListener twtl = new TextWriterTraceListener(appPath + "/PerfGenerator.log", AppDomain.CurrentDomain.FriendlyName);
            twtl.Name = "TextLogger";
            twtl.TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime;

            ConsoleTraceListener ctl = new ConsoleTraceListener(false);
            ctl.TraceOutputOptions = TraceOptions.DateTime;

            Trace.Listeners.Add(twtl);
            Trace.Listeners.Add(ctl);
            Trace.AutoFlush = true;

            Trace.WriteLine("-----Starting Session at " + DateTime.Now + "-----\n");

        }

        //Method to check for - and read from - the settings.ini file
        private static void iniLoader(string fileName)
        {

            if (IO.File.Exists(appPath + fileName + "\n"))
            {
                Trace.WriteLine("INI File Found\n");

                #region Get file names
                teamCSV = INIFile.ReadValue("Files", "Team CSV", appPath + fileName);
                Trace.WriteLine("'Teams CSV' name read as " + teamCSV + "\n");
                driverCSV = INIFile.ReadValue("Files", "Driver CSV", appPath + fileName);
                Trace.WriteLine("'Drivers CSV' name read as " + driverCSV + "\n");
                perfFileName = INIFile.ReadValue("Files", "Perf File Name", appPath + fileName);
                Trace.WriteLine("'Perf File' Name name read as " + perfFileName + "\n");
                #endregion


                #region Boolenas
                Boolean.TryParse((INIFile.ReadValue("Settings", "Have Driver Numbers", appPath + fileName)), out driverNumbers);
                Trace.WriteLine("'Have Driver Numbers' read as " + driverNumbers + "\n");
                Boolean.TryParse((INIFile.ReadValue("Settings", "Have Driver Nationality", appPath + fileName)), out driverNat);
                Trace.WriteLine("'Have Driver Numbers' read as " + driverNat + "\n");
                Boolean.TryParse((INIFile.ReadValue("Settings", "Have Driver Tags", appPath + fileName)), out driverTag);
                Trace.WriteLine("'Have Driver Numbers' read as " + driverTag + "\n");
                #endregion

                #region Team Info
                teamNameCol = INIFile.ReadValue("Team", "Team Name", appPath + fileName);
                Trace.WriteLine("'Team Name' Column read as " + teamNameCol + "\n");
                engineCol = INIFile.ReadValue("Team", "Engine", appPath + fileName);
                Trace.WriteLine("'Engine' Column read as " + engineCol + "\n");
                racePowerCol = INIFile.ReadValue("Team", "Race Power", appPath + fileName);
                Trace.WriteLine("'Race Power' Column read as " + racePowerCol + "\n");
                qualPowerCol = INIFile.ReadValue("Team", "Qualifying Power", appPath + fileName);
                Trace.WriteLine("'Qual Power' Column read as " + qualPowerCol + "\n");
                reliabilityCol = INIFile.ReadValue("Team", "Reliability", appPath + fileName);
                Trace.WriteLine("'Reliability' Column read as " + reliabilityCol + "\n");
                #endregion

                #region Driver Info
                driverCol = INIFile.ReadValue("Driver", "Driver Name", appPath + fileName);
                Trace.WriteLine("'Driver Name' Column read as " + driverCol + "\n");
                driverNumCol = INIFile.ReadValue("Driver", "Driver Number", appPath + fileName);
                Trace.WriteLine("'Driver Number' Column read as " + driverNumCol + "\n");
                raceGripCol = INIFile.ReadValue("Driver", "Race Grip", appPath + fileName);
                Trace.WriteLine("'Race Grip' Column read as " + raceGripCol + "\n");
                raceGripVarCol = INIFile.ReadValue("Driver", "Race Grip Var", appPath + fileName);
                Trace.WriteLine("'Race Grip Var' Column read as " + raceGripVarCol + "\n");
                qualGripCol = INIFile.ReadValue("Driver", "Qualifying Grip", appPath + fileName);
                Trace.WriteLine("'Qualifying Grip' Column read as " + qualGripCol + "\n");
                qualGripVarCol = INIFile.ReadValue("Driver", "Qualifying Grip Var", appPath + fileName);
                Trace.WriteLine("'Qualifying Grip Var' Column read as " + qualGripVarCol + "\n");
                driverNatCol = INIFile.ReadValue("Driver", "Driver Nationality", appPath + fileName);
                Trace.WriteLine("'Driver Nationality' Column Column read as " + driverNatCol + "\n");
                driverTagCol = INIFile.ReadValue("Driver", "Driver Tag", appPath + fileName);
                Trace.WriteLine("'Driver Tag' Column Column read as " + driverTagCol + "\n");

                #endregion
            }
            #region No INI
            else
            {
                Trace.WriteLine("No INI File Found\n");
                Trace.WriteLine("Generating Default Values and Reloading");
                INIFile.WriteValue("Files", "Team CSV", "Teams.csv", appPath + "/Settings.ini");
                INIFile.WriteValue("Files", "Driver CSV", "Drivers.csv", appPath + "/Settings.ini");
                INIFile.WriteValue("Files", "Perf File Name", "Performance", appPath + "/Settings.ini");
                INIFile.WriteValue("Settings", "Have Driver Numbers", "False", appPath + "/Settings.ini");
                INIFile.WriteValue("Settings", "Have Driver Nationality", "False", appPath + "/Settings.ini");
                INIFile.WriteValue("Settings", "Have Driver Tags", "False", appPath + "/Settings.ini");
                INIFile.WriteValue("Team", "Team Name", "Team Name", appPath + "/Settings.ini");
                INIFile.WriteValue("Team", "Engine", "Engine", appPath + "/Settings.ini");
                INIFile.WriteValue("Team", "Race Power", "Race Power", appPath + "/Settings.ini");
                INIFile.WriteValue("Team", "Qualifying Power", "Qualifying Power", appPath + "/Settings.ini");
                INIFile.WriteValue("Team", "Reliability", "Reliability", appPath + "/Settings.ini");
                INIFile.WriteValue("Driver", "Driver Name", "Driver", appPath + "/Settings.ini");
                INIFile.WriteValue("Driver", "Driver Number", "Driver Number", appPath + "/Settings.ini");
                INIFile.WriteValue("Driver", "Race Grip", "Base Race Grip", appPath + "/Settings.ini");
                INIFile.WriteValue("Driver", "Race Grip Var", "Race Grip Variance", appPath + "/Settings.ini");
                INIFile.WriteValue("Driver", "Qualifying Grip", "Base Qualifying Grip", appPath + "/Settings.ini");
                INIFile.WriteValue("Driver", "Qualifying Grip Var", "Qualifying Grip Variance", appPath + "/Settings.ini");
                INIFile.WriteValue("Driver", "Driver Nationality", "Nationality", appPath + "/Settings.ini");
                INIFile.WriteValue("Driver", "Driver Tag", "Tag", appPath + "/Settings.ini");
                Trace.WriteLine("Options set to default values......\n");
                Trace.WriteLine("Reloading INI");
                iniLoader("/Settings.ini");
                #endregion
            }

            Trace.WriteLine("---INI DATA END---\n");

        }

    }
}
