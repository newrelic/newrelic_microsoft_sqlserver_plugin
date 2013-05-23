-- Total Current Requests Azure
-- Returns total for current DB context

--If number of concurrent requests made to a database exceed 400, all transactions that have been running for 1 minute or more are terminated.
--Error returned
--         40549 : Session is terminated because you have a long-running transaction. Try shortening your transaction.
--                 Limit



select count(request_id) as TotalCurrentRequests
 from sys.dm_exec_requests