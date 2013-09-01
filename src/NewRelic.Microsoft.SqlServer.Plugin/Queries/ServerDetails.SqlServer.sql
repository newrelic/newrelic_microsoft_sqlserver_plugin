-- SQL Server Version Information
-- Maps the Product Version into a human-readable version


SELECT
	SERVERPROPERTY('productversion')	AS ProductVersion,
	CASE
		WHEN CAST(LEFT(CAST(SERVERPROPERTY('productversion') AS varchar(20)), 4) AS float) >= 12 THEN 'SQL 2014+'
		WHEN CAST(LEFT(CAST(SERVERPROPERTY('productversion') AS varchar(20)), 4) AS float) >= 11 THEN 'SQL 2012'
		WHEN CAST(LEFT(CAST(SERVERPROPERTY('productversion') AS varchar(20)), 4) AS float) >= 10.5 THEN 'SQL 2008 R2'
		WHEN CAST(LEFT(CAST(SERVERPROPERTY('productversion') AS varchar(20)), 4) AS float) >= 10 THEN 'SQL 2008'
		WHEN CAST(LEFT(CAST(SERVERPROPERTY('productversion') AS varchar(20)), 4) AS float) >= 9 THEN 'SQL 2005'
		WHEN CAST(LEFT(CAST(SERVERPROPERTY('productversion') AS varchar(20)), 4) AS float) >= 8 THEN 'SQL 2000'
	END									AS SQLTitle,
	SERVERPROPERTY('productlevel')		AS ProductLevel,
	SERVERPROPERTY('edition')			AS Edition