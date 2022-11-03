using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using Utils;
// using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace DbUtils {
    class Program {
        static void Main(string[] args) {
            string dbName = String.Empty;
            string serverName = ".";
            bool isFullBackup = true;
            //new CreateDB(".", dbName);
            
            //dotnet run D:\DEMO.MDF diff [server]
            if (args.Length > 0) {
                dbName = args[0];    
                if (args.Length == 2) 
                {
                    if (args[1].ToLower() == "diff")
                    {
                        isFullBackup = false;
                    }
                    else
                    {
                        serverName = args[1];
                    }
                }
                else if (args.Length == 3) 
                {
                    serverName = args[2];
                }
                else if (args.Length > 3) 
                {
                    Console.WriteLine("Maximum 3 arguments allowed: -- dbName [Diff] [Server]");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Please specify database");
                return;
            }

            try {
                //Define Server connection
                // string conn = "Server = " + server + "; Integrated security = true";
                // SqlConnection sqlConn = new SqlConnection(conn);
                // ServerConnection conn = new ServerConnection();            //To Avoid TimeOut Exception
                // conn.ServerInstance = serverName;
                Server srv = new Server(serverName);
                srv.ConnectionContext.NonPooledConnection = true;

                string archiveDir = dbName.Substring(0, dbName.LastIndexOf('\\')) + "\\ARCHIVES" + Path.DirectorySeparatorChar;
                string tempDir = dbName.Substring(0, dbName.LastIndexOf('\\')) + "\\TEMP" + Path.DirectorySeparatorChar;

                CultureInfo myCI = new CultureInfo("en-US");
                Calendar myCal = myCI.Calendar;
                int weekOfTheYear = myCal.GetWeekOfYear(DateTime.Now, myCI.DateTimeFormat.CalendarWeekRule, myCI.DateTimeFormat.FirstDayOfWeek);

                string bakFile = tempDir 
                    // + srv.Name + "_"
                    + srv.ServiceName + "_" 
                    + dbName.Substring(dbName.LastIndexOf('\\') + 1) + "_"
                    // + "22-02-15_13-40" // Specify bak file for restore
                    + DateTime.Now.ToString("yy-MM-week_")
                    + weekOfTheYear.ToString()
                    +".bak";

                BackupDB(srv, dbName, bakFile, isFullBackup);
                // Console.WriteLine($"{dbName} database backed up in {sw.ElapsedMilliseconds} ms.");

                // using (ZipArchive zip = ZipFile.Open(bakFile.Remove(bakFile.Length - 4) + ".zip", ZipArchiveMode.Create)) {
                //     zip.CreateEntryFromFile(bakFile, dbName.Substring(dbName.LastIndexOf('\\') + 1) + ".bak");
                //     Console.WriteLine($"{bakFile} zipped in {sw.ElapsedMilliseconds} ms.");
                // }

                // Create7Zip(bakFile, bakFile.Remove(bakFile.Length - 4) + ".7z"); // about 40 time slower!?

                // if (File.Exists(bakFile)) {
                //     RestoreDB(srv, dbName, bakFile);
                //     // When the restore is complete, delete bak file  
                //     File.Delete(bakFile);
                // } else Console.WriteLine(bakFile + " not found. Can not be deleted");

                // TransferDB(srv, srv, dbName);

                srv.ConnectionContext.Disconnect();

                string zipPath =  // Directory.GetParent(archiveDir).ToString();
                //zipPath = Directory.GetParent(zipPath).ToString() + Path.DirectorySeparatorChar
                archiveDir
                    + srv.ServiceName  + "_" + dbName.Substring(dbName.LastIndexOf('\\') + 1) + "_" 
                    + DateTime.Now.ToString("yyyy'-'MM'-'dd_HH'_'mm") + ".zip";
                
                ZipFile.CreateFromDirectory(tempDir, zipPath);
                Console.WriteLine($"Bak file zipped to {zipPath}. ");

                File.Delete(bakFile);
                Console.Write($"{bakFile} deleted.");

            } catch (IOException ioExp) {
                TextFile.Log(ioExp.Message);
            } catch (FailedOperationException fop) {
                TextFile.Log(fop.ToString());
            } catch (Exception e) {
                TextFile.Log(e.Message);
            }
        }

        public static void BackupDB(Server srv, string dbName, string bakFile, bool isFullBackup) {

            Backup backup = new Backup();
            //Specify the type of backup, the description, the name, and the database to be backed up.

            // backup.Action = BackupActionType.Database;

            //  To only backup the database structure and data(the PRIMARY filegroup; everything except the non-essential images)
            // backup.Action = BackupActionType.Files;
            // backup.DatabaseFileGroups.Add("PRIMARY");

            backup.Database = dbName;
            // You can use back up compression technique of SQL Server 2008, specify CompressionOption property to On for compressed backup
            //backup.CompressionOption = BackupCompressionOptions.On; // zipping or 7z it after that is pointless, the zip file is the same as the bak file

            // Declare a BackupDeviceItem by supplying the backup device file name in the constructor, and the type of device is a file. 
            // backup.Devices.AddDevice(bakFile, DeviceType.File);
            BackupDeviceItem deviceItem = new BackupDeviceItem(bakFile, DeviceType.File);
            backup.Devices.Add(deviceItem);

            backup.BackupSetName = dbName + " Backup";

            backup.BackupSetDescription = "Backup of: " + dbName + "on " + DateTime.Now.ToShortDateString();

            if (isFullBackup) {
                backup.BackupSetDescription += "-Full";

                /* You can specify Initialize = false (default) to create a new 
                * backup set which will be appended as last backup set on the media. You
                * can specify Initialize = true to make the backup as first set on the
                * medium and to overwrite any other existing backup sets if the all the
                * backup sets have expired and specified backup set name matches with
                * the name on the medium */
                backup.Initialize = true;

                // Set the Incremental property to False to specify that this is a full database backup. 
                backup.Incremental = false;
            } else {
                backup.BackupSetDescription += "-Differential";
                backup.Incremental = true;
            }

            //srv.ConnectionContext.StatementTimeout = 60 * 60;
            Database db = srv.Databases[dbName]; // (Reference Database As microsoft.sqlserver.management.smo.database, not as System.entity.database )

            backup.Checksum = true;
            backup.ContinueAfterError = true;

            // backup.ExpirationDate = DateTime.Now.AddDays(1);

            //Specify that the log must be truncated after the backup is complete.        
            // backup.LogTruncation = BackupTruncateLogType.Truncate;
            backup.FormatMedia = false;

            /* Wiring up events for progress monitoring */
            //backup.PercentComplete += CompletionStatusInPercent;
            //backup.Complete += Backup_Completed;

            /* SqlBackup method starts to take back up
            * You can also use SqlBackupAsync method to perform the backup 
            * operation asynchronously */
            backup.SqlBackup(srv);

            //Remove the backup device from the Backup object.           
            backup.Devices.Remove(deviceItem);
            TextFile.Log($"{dbName} backed up to {bakFile}");
        }

        private static void RestoreDB(Server srv, string dbName, string bakFile) {
            Restore restore = new Restore();
            restore.Database = dbName;
            /* Specify whether you want to restore database, files or log */
            //restore.Action = RestoreActionType.Files;
            restore.Devices.AddDevice(bakFile, DeviceType.File);

            /* If you don't want to create new MDF and LDF files, you should be able to specify to SMO Restore that 
             * you want to replace the existing database with the restore operation, by setting it to true: */

            restore.ReplaceDatabase = false;

            /* If you have a differential or log restore after the current restore,
             * you would need to specify NoRecovery = true, this will ensure no
             * recovery performed and subsequent restores are allowed. It means 
             * the database will be in a restoring state. */
            // restore.NoRecovery = false;

            // restore.RelocateFiles.Add(new RelocateFile(dbName, 
            //     dbName.Substring(dbName.LastIndexOf('\\') + 1) + DateTime.Now.ToString("yy-MM-dd_HH-mm") + ".mdf"));
            // restore.RelocateFiles.Add(new RelocateFile(dbName + "_log", 
            //     dbName.Substring(dbName.LastIndexOf('\\') + 1) + DateTime.Now.ToString("yy-MM-dd_HH-mm") + ".ldf"));

            /* Wiring up events for progress monitoring */
            // restore.PercentComplete += CompletionStatusInPercent;
            //restore.Complete += Restore_Completed;

            /* SqlRestore method starts to restore the database
             * You can also use SqlRestoreAsync method to perform restore 
             * operation asynchronously */
            restore.SqlRestore(srv);
            TextFile.Log($"{bakFile} restored to {dbName}");
        }

        private static void TransferDB(Server srcSrv, Server destSrv, string dbName) {
            Database db = srcSrv.Databases[dbName];
            Database dbCopy = new Database(destSrv, dbName + DateTime.Now.ToString("yy-MM-dd"));
            dbCopy.Create();
            //Define a Transfer object and set the required options and properties.   
            Transfer xfr;
            xfr = new Transfer(db);
            xfr.CopyAllTables = true;
            xfr.Options.WithDependencies = true;
            xfr.Options.ContinueScriptingOnError = true;
            xfr.DestinationDatabase = dbCopy.Name;
            xfr.DestinationServer = destSrv.Name;
            xfr.DestinationLoginSecure = true;
            xfr.CopySchema = true;
            //Script the transfer. Alternatively perform immediate data transfer with TransferData method.   
            //xfr.ScriptTransfer();
            xfr.TransferData();
            TextFile.Log($"{dbName} transferred from {srcSrv} to {destSrv}");
        }

        public static void Create7Zip(string source, string target) {
            ProcessStartInfo p = new ProcessStartInfo();
            p.FileName = @"C:\Program Files\7-Zip\7z.exe";
            p.Arguments = "a -tgzip \"" + target + "\" \"" + source + "\" -mx=9";
            p.WindowStyle = ProcessWindowStyle.Hidden;
            Process x = Process.Start(p);
            x.WaitForExit();
            TextFile.Log($"{source} 7zipped to {target}");
        }

        // private static void CompletionStatusInPercent(object sender, PercentCompleteEventArgs args) {
        //     Console.Clear();
        //     Console.WriteLine("Percent completed: {0}%.", args.Percent);
        // }
        // private static void Backup_Completed(object sender, ServerMessageEventArgs args) {
        //     Console.WriteLine("Backup completed.");
        //     Console.WriteLine(args.Error.Message);
        // }
        // private static void Restore_Completed(object sender, ServerMessageEventArgs args) {
        //     Console.WriteLine("Restore completed.");
        //     Console.WriteLine(args.Error.Message);
        // }
    }
}
