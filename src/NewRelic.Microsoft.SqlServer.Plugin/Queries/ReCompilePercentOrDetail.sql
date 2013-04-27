DECLARE	@SingleUse int,
		@MultiUse int,
		@DetailSummary varchar(1)



SET @DetailSummary = 'S'



DECLARE @Temp1 TABLE (
	DBName varchar(200) NULL,
	bucketid bigint NULL,
	usecounts bigint NULL,
	size_in_bytes bigint NULL,
	objtype varchar(25) NULL,
	SQLtext varchar(MAX) NULL
)

INSERT INTO @Temp1
	SELECT
		DB_NAME(st.dbid) AS DBName,
		cp.bucketid,
		cp.usecounts,
		cp.size_in_bytes,
		cp.objtype,
		st.text
	FROM sys.dm_exec_cached_plans cp
	CROSS APPLY sys.dm_exec_sql_text(cp.plan_handle) st
	WHERE cp.cacheobjtype = 'Compiled Plan'

SELECT
	@SingleUse = COUNT(*)
FROM @Temp1
WHERE usecounts = 1

SELECT
	@MultiUse = COUNT(*)
FROM @Temp1
WHERE usecounts > 1

IF @DetailSummary = 'S'
	SELECT
		@SingleUse AS SingleUses,
		@MultiUse AS MultiUses,
		CAST(@SingleUse AS decimal(18, 2)) / (@SingleUse + @MultiUse) * 100
		AS SingleUsePercent


IF @DetailSummary = 'D'
	SELECT
		*
	FROM @Temp1