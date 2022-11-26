# BigBlueUsers
Automating rake commands for creating users in BigBlueButton with Greenlight and sending e-mails.
From txt files with user names and emails generate commands for creating these users in BigBluButton server and informing them about that.
If these is a file ..\Input.txt the program takes the names and emails from it and creates 2 files in the parent directory:
..\CreateUsers.txt : for coping in the console of BBB server.
..\SendEmails.html : for sending emails in chunks of 6 (due to spam restictions).
If started with arguments - the first is the user password, the second - filter for the e-mail domain.
If there is a file ..\CreatedUsers.txt, it takes the names and emails and sorts them in the file ..\UsersSortedByFirstName.txt.

# CodeSums
Add rows to Excel files in subdirectories, Sum data from multiple files
Търси *.xlxs файлове две директории по-надолу, добавя им заглавия за съответната година и/или месец, след което събира сумите по кодове от всички намерени файлове в съответната директория във файл една директория по-горе.

# DbUtils
Backup, restore and transfer SQL Server databases

# MaterialsPlanning
Collects the materials SAP No., name, etc. from file Материали-*.xls? and maps the data to them from mb52.XLS, me3m.XLS and zcoel-yyyy.XLS files into Материали-*-анализ.csv

# WeightNotes
Търси *.txt файлове в горната директория и: 1) сумира полетата Всичко Нето във файла Справка по дни.xlsx 2) Събира редовете от всички файлове във Всички m.{Month}.csv.
3) Maps planned trucks to actual. 4) Sends emails

# Utils
Library

# ParadoxReader
The "Paradox database native .NET reader" library developed by Petr Briza (Thank you Petr :)) ported to .net7.0
