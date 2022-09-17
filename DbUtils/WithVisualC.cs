using System;
using Microsoft.SqlServer.Management.Smo;

class A {
    public static void MsDocs() {
        // Connect to the local, default instance of SQL Server.   
        Server srv = new Server();
        // Reference the AdventureWorks2012 database.   
        Database db = srv.Databases["AdventureWorks2012"];

        // Store the current recovery model in a variable.   
        int recoverymod;
        recoverymod = (int)db.DatabaseOptions.RecoveryModel;

        // Define a Backup object variable.   
        Backup bk = new Backup();

        // Specify the type of backup, the description, the name, and the database to be backed up.   
        bk.Action = BackupActionType.Database;
        bk.BackupSetDescription = "Full backup of Adventureworks2012";
        bk.BackupSetName = "AdventureWorks2012 Backup";
        bk.Database = "AdventureWorks2012";

        // Declare a BackupDeviceItem by supplying the backup device file name in the constructor, and the type of device is a file.   
        BackupDeviceItem bdi = default(BackupDeviceItem);
        bdi = new BackupDeviceItem("Test_Full_Backup1", DeviceType.File);

        // Add the device to the Backup object.   
        bk.Devices.Add(bdi);
        // Set the Incremental property to False to specify that this is a full database backup.   
        bk.Incremental = false;

        // Set the expiration date.   
        System.DateTime backupdate = new System.DateTime();
        backupdate = new System.DateTime(2006, 10, 5);
        bk.ExpirationDate = backupdate;

        // Specify that the log must be truncated after the backup is complete.   
        bk.LogTruncation = BackupTruncateLogType.Truncate;

        // Run SqlBackup to perform the full database backup on the instance of SQL Server.   
        bk.SqlBackup(srv);

        // Inform the user that the backup has been completed.   
        System.Console.WriteLine("Full Backup complete.");

        // Remove the backup device from the Backup object.   
        bk.Devices.Remove(bdi);

        // Make a change to the database, in this case, add a table called test_table.   
        Table t = default(Table);
        t = new Table(db, "test_table");
        Column c = default(Column);
        c = new Column(t, "col", DataType.Int);
        t.Columns.Add(c);
        t.Create();

        // Create another file device for the differential backup and add the Backup object.   
        BackupDeviceItem bdid = default(BackupDeviceItem);
        bdid = new BackupDeviceItem("Test_Differential_Backup1", DeviceType.File);

        // Add the device to the Backup object.   
        bk.Devices.Add(bdid);

        // Set the Incremental property to True for a differential backup.   
        bk.Incremental = true;

        // Run SqlBackup to perform the incremental database backup on the instance of SQL Server.   
        bk.SqlBackup(srv);

        // Inform the user that the differential backup is complete.   
        System.Console.WriteLine("Differential Backup complete.");

        // Remove the device from the Backup object.   
        bk.Devices.Remove(bdid);

        // Delete the AdventureWorks2012 database before restoring it  
        // db.Drop();  

        // Define a Restore object variable.  
        Restore rs = new Restore();

        // Set the NoRecovery property to true, so the transactions are not recovered.   
        rs.NoRecovery = true;

        // Add the device that contains the full database backup to the Restore object.   
        rs.Devices.Add(bdi);

        // Specify the database name.   
        rs.Database = "AdventureWorks2012";

        // Restore the full database backup with no recovery.   
        rs.SqlRestore(srv);

        // Inform the user that the Full Database Restore is complete.   
        Console.WriteLine("Full Database Restore complete.");

        // reacquire a reference to the database  
        db = srv.Databases["AdventureWorks2012"];

        // Remove the device from the Restore object.  
        rs.Devices.Remove(bdi);

        // Set the NoRecovery property to False.   
        rs.NoRecovery = false;

        // Add the device that contains the differential backup to the Restore object.   
        rs.Devices.Add(bdid);

        // Restore the differential database backup with recovery.   
        rs.SqlRestore(srv);

        // Inform the user that the differential database restore is complete.   
        System.Console.WriteLine("Differential Database Restore complete.");

        // Remove the device.   
        rs.Devices.Remove(bdid);

        // Set the database recovery mode back to its original value.  
        db.RecoveryModel = (RecoveryModel)recoverymod;

        // Drop the table that was added.   
        db.Tables["test_table"].Drop();
        db.Alter();

        // Remove the backup files from the hard disk.  
        // This location is dependent on the installation of SQL Server  
        System.IO.File.Delete("C:\\Program Files\\Microsoft SQL Server\\MSSQL12.MSSQLSERVER\\MSSQL\\Backup\\Test_Full_Backup1");
        System.IO.File.Delete("C:\\Program Files\\Microsoft SQL Server\\MSSQL12.MSSQLSERVER\\MSSQL\\Backup\\Test_Differential_Backup1");
    }
}