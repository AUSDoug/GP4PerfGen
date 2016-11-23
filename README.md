# PerfFileGenerator

Program name: 		Performance File Generator
Author: 		Douglas Spangenberg, A.K.A AUS_Doug
Version: 		1.2
Date: 			23rd November 2016
Build Time (Version): 	~ 0.25 Hours
Build Time (Total): 	~ 3.75 Hours
Licenses:               GNU General Public License v3 & GrandPrixGames.org permission policy.

What it does:
---------------------------
 - Taking two Comma Separated Value (CSV) files and, optionally, a target file name, to generate 
 a GPxPatch Performance file for use with Grand Prix 4.

Requirements:
-----------------------------
 - .NET Framework 4.5.2

Basic Usage:
-----------------------------
1)     Modify the Setting.ini, so that:
1.a)   The 'Team CSV' and 'Driver CSV' fields point to the correct files.
1.a.i) Provide preffered name for Performance file
1.b)   Fill out the 'Have Driver X' fields, depending on what your 'Driver CSV' contains.
1.c)   Edit the fields in the 'Team' section, so that they point to the correct fields in your 'Team CSV'
1.d)   Edit the fields in the 'Driver' section, so that the point to the correct fields in your 'Driver CSV'

2)     Have 'Settings.ini', 'GP4 Perf Gen.exe' and 'CsvReader.dll' in the same folder.
3)     Double-click 'GP4 Perf Gen.exe'

Advanced Usage:
-----------------------------
Follow Steps 1 & 2 from above, though you can choose to provide your Settings.ini by using the full path.
'GP4 Perf Gen.exe' will accept, as a command-line argument, a custom-named Settings.ini.
See 'AdvancedRun.bat' for example.
You could, for example, provide a different Settings.ini (containing different Team & Driver CSV names and a unique Performance Pile name) for multiple races at once:
"GP4 Perf Gen.exe" Settings-Melbourne.ini
"GP4 Perf Gen.exe" Settings-Bahrain.ini
"GP4 Perf Gen.exe" Settings-Sepang.ini
............
"GP4 Perf Gen.exe" Settings-AbuDhabi.ini

ChangeLog:
-----------------------------
23rd November 2016 - 1.2 Release.
- More code documentation.
- License information.

18th November 2016 - 1.1 Release.
- Added flexibiliy for column names, using Settings.ini
- For basic use, runs without using Command Prompt or a .bat file.
- Can accept a custom 'Settings.ini' name as a Command Line argument, for advanced usage.
- Added error checking, namely for Index errors and missing files.

15th November 2016 - 1.0 Release.

Notes:
-----------------------------
While much more flexible than 1.0, there are still some rules that must be observed.
- Teams and Drivers must be in separate CSV files.
- Both CSVs need a first row with Column headings.
- Teams need: Team Name, Engine supplier, Race BHP, Qualifying BHP and Reliability.
- The Team CSV must be NO LONGER than 12 Rows; 1 Row of Column Headings, and up to 11 teams.
- Drivers need: Name, Base Race Grip, Race Grip Variance, Base Qualifying Grip, Qualy Grip Variance
- Driver numbers, nationalities and 3-Letter codes are optional.
  - GPxPatch requires that, if you have a 3-Letter code, you must have a Nationality.
    If you provide a 3-Letter code, without a nationality GP4 Perf Gen will provide a default 'UN' Nationality.
- If you lose the Settings.ini file, a new one will be generated at next use with default values.

Latter plans include:
* Wheel and Tread/Rim Texture information.