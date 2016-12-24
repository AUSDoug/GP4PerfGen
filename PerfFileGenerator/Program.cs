using System;
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
        //Do we have; Driver Numbers? Driver Nationalities? 3-Letter Driver Codes?
        static Boolean driverNumbers, driverNat, driverTag;
        //Column names for Team, Engine, Power, Reliability, Gears, Tread Index, Rim Index
        static String teamNameCol, engineCol, racePowerCol, qualPowerCol, reliabilityCol, gearsCol, treadCol, rimCol;
        //Column names for Driver, Number, Grip, Nationality, Tag
        static String driverCol, driverNumCol, raceGripCol, raceGripVarCol, qualGripCol, qualGripVarCol, driverNatCol, driverTagCol;
        //Names of Tread & Rim textures
        static String tex00, tex01, tex02, tex03, tex04, tex05, tex06, tex07, tex08, tex09, tex10, tex11, tex12;
        //Integer versions of Team and Driver info columns
        static int teamNameColInt, engineColInt, racePowerColInt, qualPowerColInt, reliabilityColInt, gearsColInt, treadColInt, rimColInt, driverColInt, driverNumColInt, raceGripColInt, raceGripVarColInt, qualGripColInt, qualGripVarColInt, driverNatColInt, driverTagColInt;

        //Accepts up to one command-line argument; A custom-named Settings.ini
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
                gearsColInt = team.GetColumnIndex(gearsCol);
                treadColInt = team.GetColumnIndex(treadCol);
                rimColInt = team.GetColumnIndex(rimCol);

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
                writer.WriteLine("** Grand Prix 4 Performance File - Generated by GP4PerfGen v1.3 **");
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

                //Reset the Team Index counter to 0
                teamIndex = 0;

                //Write out each Team's Gear counts.
                writer.WriteLine("\r\n[Gears]");
                foreach (Row row in team.Rows)
                {
                    if (teamIndex < 10)
                    {
                        writer.Write("Team #0" + teamIndex + "=");
                    }
                    else
                    {
                        writer.Write("Team #" + teamIndex + "=");
                    }
                    writer.Write(team.Columns[gearsColInt].Values[teamIndex] + "\r\n");
                    teamIndex++;
                }

                //Write out the list of provided texture names
                writer.WriteLine("\r\n[TyreTextureNames]");
                writer.WriteLine("Name #00=" + tex00);
                writer.WriteLine("Name #01=" + tex01);
                writer.WriteLine("Name #02=" + tex02);
                writer.WriteLine("Name #03=" + tex03);
                writer.WriteLine("Name #04=" + tex04);
                writer.WriteLine("Name #05=" + tex05);
                writer.WriteLine("Name #06=" + tex06);
                writer.WriteLine("Name #07=" + tex07);
                writer.WriteLine("Name #08=" + tex08);
                writer.WriteLine("Name #08=" + tex09);
                writer.WriteLine("Name #09=" + tex10);
                writer.WriteLine("Name #10=" + tex11);
                writer.WriteLine("Name #11=" + tex12);

                //Reset the Team Index counter to 0
                teamIndex = 0;

                //Write out each Team's Tread textures.
                writer.WriteLine("\r\n[TreadTextures]");
                foreach (Row row in team.Rows)
                {
                    if (teamIndex < 10)
                    {
                        writer.Write("Team #0" + teamIndex + "=");
                    }
                    else
                    {
                        writer.Write("Team #" + teamIndex + "=");
                    }
                    writer.Write(team.Columns[treadColInt].Values[teamIndex]+ "\r\n");
                    teamIndex++;
                }

                //Reset the Team Index counter to 0
                teamIndex = 0;

                //Write out each Team's Rim textures.
                writer.WriteLine("\r\n[WheelTextures]");
                foreach (Row row in team.Rows)
                {
                    if (teamIndex < 10)
                    {
                        writer.Write("Team #0" + teamIndex + "=");
                    }
                    else
                    {
                        writer.Write("Team #" + teamIndex + "=");
                    }
                    writer.Write(team.Columns[rimColInt].Values[teamIndex] + "\r\n");
                    teamIndex++;
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
                gearsCol = INIFile.ReadValue("Team", "Gears", appPath + fileName);
                Trace.WriteLine("'Gears' Column read as " + gearsCol + "\n");
                treadCol = INIFile.ReadValue("Team", "Tread", appPath + fileName);
                Trace.WriteLine("'Tread' Column read as " + treadCol + "\n");
                rimCol = INIFile.ReadValue("Team", "Rim", appPath + fileName);
                Trace.WriteLine("'Rim' Column read as " + rimCol + "\n");
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

                #region Texture Info
                tex00 = INIFile.ReadValue("TextureNames", "Name #00", appPath + fileName);
                Trace.WriteLine("'Tex #00' read as " + tex00 + "\n");
                tex01 = INIFile.ReadValue("TextureNames", "Name #01", appPath + fileName);
                Trace.WriteLine("'Tex #01' read as " + tex01 + "\n");
                tex02 = INIFile.ReadValue("TextureNames", "Name #02", appPath + fileName);
                Trace.WriteLine("'Tex #02' read as " + tex02 + "\n");
                tex03 = INIFile.ReadValue("TextureNames", "Name #03", appPath + fileName);
                Trace.WriteLine("'Tex #03' read as " + tex03 + "\n");
                tex04 = INIFile.ReadValue("TextureNames", "Name #04", appPath + fileName);
                Trace.WriteLine("'Tex #04' read as " + tex04 + "\n");
                tex05 = INIFile.ReadValue("TextureNames", "Name #05", appPath + fileName);
                Trace.WriteLine("'Tex #05' read as " + tex05 + "\n");
                tex06 = INIFile.ReadValue("TextureNames", "Name #06", appPath + fileName);
                Trace.WriteLine("'Tex #06' read as " + tex06 + "\n");
                tex07 = INIFile.ReadValue("TextureNames", "Name #07", appPath + fileName);
                Trace.WriteLine("'Tex #07' read as " + tex07 + "\n");
                tex08 = INIFile.ReadValue("TextureNames", "Name #08", appPath + fileName);
                Trace.WriteLine("'Tex #08' read as " + tex08 + "\n");
                tex09 = INIFile.ReadValue("TextureNames", "Name #09", appPath + fileName);
                Trace.WriteLine("'Tex #09' read as " + tex09 + "\n");
                tex10 = INIFile.ReadValue("TextureNames", "Name #10", appPath + fileName);
                Trace.WriteLine("'Tex #10' read as " + tex10 + "\n");
                tex11 = INIFile.ReadValue("TextureNames", "Name #11", appPath + fileName);
                Trace.WriteLine("'Tex #11' read as " + tex11 + "\n");
                tex12 = INIFile.ReadValue("TextureNames", "Name #12", appPath + fileName);
                Trace.WriteLine("'Tex #12' read as " + tex12 + "\n");
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
                INIFile.WriteValue("Team", "Gears", "Gears", appPath + "/Settings.ini");
                INIFile.WriteValue("Team", "Tread", "Tread", appPath + "/Settings.ini");
                INIFile.WriteValue("Team", "Rim", "Rim", appPath + "/Settings.ini");
                INIFile.WriteValue("Driver", "Driver Name", "Driver", appPath + "/Settings.ini");
                INIFile.WriteValue("Driver", "Driver Number", "Driver Number", appPath + "/Settings.ini");
                INIFile.WriteValue("Driver", "Race Grip", "Base Race Grip", appPath + "/Settings.ini");
                INIFile.WriteValue("Driver", "Race Grip Var", "Race Grip Variance", appPath + "/Settings.ini");
                INIFile.WriteValue("Driver", "Qualifying Grip", "Base Qualifying Grip", appPath + "/Settings.ini");
                INIFile.WriteValue("Driver", "Qualifying Grip Var", "Qualifying Grip Variance", appPath + "/Settings.ini");
                INIFile.WriteValue("Driver", "Driver Nationality", "Nationality", appPath + "/Settings.ini");
                INIFile.WriteValue("Driver", "Driver Tag", "Tag", appPath + "/Settings.ini");
                INIFile.WriteValue("TextureNames", "Name #00", "ultrasoft", appPath + "/Settings.ini");
                INIFile.WriteValue("TextureNames", "Name #01", "superosft", appPath + "/Settings.ini");
                INIFile.WriteValue("TextureNames", "Name #02", "soft", appPath + "/Settings.ini");
                INIFile.WriteValue("TextureNames", "Name #03", "medium", appPath + "/Settings.ini");
                INIFile.WriteValue("TextureNames", "Name #04", "hard", appPath + "/Settings.ini");
                INIFile.WriteValue("TextureNames", "Name #05", "advanti", appPath + "/Settings.ini");
                INIFile.WriteValue("TextureNames", "Name #06", "apptech", appPath + "/Settings.ini");
                INIFile.WriteValue("TextureNames", "Name #07", "bbs", appPath + "/Settings.ini");
                INIFile.WriteValue("TextureNames", "Name #08", "enkei", appPath + "/Settings.ini");
                INIFile.WriteValue("TextureNames", "Name #09", "motegi", appPath + "/Settings.ini");
                INIFile.WriteValue("TextureNames", "Name #10", "oz1", appPath + "/Settings.ini");
                INIFile.WriteValue("TextureNames", "Name #11", "oz2", appPath + "/Settings.ini");
                INIFile.WriteValue("TextureNames", "Name #12", "rays", appPath + "/Settings.ini");
                Trace.WriteLine("Options set to default values......\n");
                Trace.WriteLine("Reloading INI");
                iniLoader("/Settings.ini");
                #endregion
            }

            Trace.WriteLine("---INI DATA END---\n");

        }

    }
}
