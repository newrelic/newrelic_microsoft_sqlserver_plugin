declare @SingleUse int, @MultiUse int, @DetailSummary varchar(1)

set @DetailSummary = 'S'

declare @Temp1 Table(
	DBName Varchar(200) NULL,
	bucketid bigint NULL,
	usecounts bigint NULL,
	size_in_bytes bigint NULL,
	objtype varchar(25) NULL,
	SQLtext varchar(max) NULL
)

insert into @Temp1
    select
        db_name(st.dbid) as DBName,
        cp.bucketid,
        cp.usecounts,
        cp.size_in_bytes,
        cp.objtype,
        st.text
    from sys.dm_exec_cached_plans cp
    cross apply sys.dm_exec_sql_text(cp.plan_handle) st
    where cp.cacheobjtype = 'Compiled Plan'

select @SingleUse = count(*)
from @Temp1
where usecounts = 1

select @MultiUse = count(*)
from @Temp1
where usecounts > 1

if @DetailSummary = 'S'
select
    @SingleUse as SingleUses,
    @MultiUse as MultiUses,
    cast(@SingleUse as decimal(18,2)) / (@SingleUse + @MultiUse) * 100
        as SingleUsePercent
        

if @DetailSummary = 'D'
    select
	*
	from @Temp1