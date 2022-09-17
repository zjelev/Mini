USE master;
DROP DATABASE "MinionsDB22-02-15";
DROP DATABASE "D:\DEMO.MDF";
-- DROP DATABASE "D:\DEMO1.MDF";
-- DROP DATABASE "D:\DEMO2.MDF";
-- DROP DATABASE "D:\DEMO3.MDF";
GO
-- RESTORE DATABASE "D:\DEMO.MDF" FROM DISK = 'D:\Documents\repo\DbUtils\bin\Debug\net6.0\Archives\DEMO.MDF.bak'
-- RESTORE DATABASE "D:\DEMO.MDF" FROM DISK = 'D:\Documents\repo\DbUtils\Archives\DEMO.MDF.bak'
RESTORE DATABASE MinionsDB FROM DISK = 'D:\Documents\repo\DbUtils\Archives\MinionsDB.bak'
GO

-- -- shows db files contained in a bakup
RESTORE FILELISTONLY
FROM DISK = 'D:\Documents\repo\DbUtils\Archives\MinionsDB.bak'
WITH FILE = 1

Create database "D:\DEMO.MDF"
On
(
Filename= 'D:\DEMO.MDF',
Filename ='D:\DEMO_log.ldf'
)
For attach;