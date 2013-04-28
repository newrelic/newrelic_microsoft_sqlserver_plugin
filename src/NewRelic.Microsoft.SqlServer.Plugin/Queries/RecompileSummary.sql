
DECLARE @Details TABLE (
			DBName varchar(200) NULL,
			BucketID bigint NULL,
			UseCounts bigint NULL,
			SizeInBytes bigint NULL,
			ObjectType varchar(25) NULL,
			Text varchar(MAX) NULL
		)

INSERT INTO @Details
	SELECT
		DB_NAME(st.dbid)	AS DBName,
		cp.BucketID			AS BucketID,
		cp.UseCounts		AS UseCounts,
		cp.size_in_bytes	AS SizeInBytes,
		cp.objtype			AS ObjectType,
		st.Text				AS Text
	FROM sys.dm_exec_cached_plans cp
	CROSS APPLY sys.dm_exec_sql_text(cp.plan_handle) st
	WHERE cp.cacheobjtype = 'Compiled Plan';

WITH SumsByDatabase AS (
		SELECT
			d.DBName,
			d.ObjectType,
			(
				SELECT
					COUNT(*)
				FROM @Details d2
				WHERE d2.UseCounts = 1 AND d2.DBName = d.DBName AND d2.ObjectType = d.ObjectType)
			AS SingleUseObjects,
			(
				SELECT
					COUNT(*)
				FROM @Details d2
				WHERE d2.UseCounts > 1 AND d2.DBName = d.DBName AND d2.ObjectType = d.ObjectType)
			AS MultipleUseObjects
		FROM @Details d
		GROUP BY	d.DBName,
					d.ObjectType)
SELECT
	DBName,
	ObjectType,
	SingleUseObjects,
	MultipleUseObjects,
	CAST(SingleUseObjects AS decimal(18, 2)) / (SingleUseObjects + MultipleUseObjects) * 100	AS SingleUsePercent
FROM SumsByDatabase