declare @mailbatchId int=1316

select PipelineStatus,* from am.mailbatches  where id=@mailbatchId 

select ShippingStatus,* from am.mailpieces where mailbatchid=@mailbatchId 

select * from am.mailpiececustominfo where mailpieceid in (select id from am.mailpieces where mailbatchid=@mailbatchId )

select * from am.PrintBatches where mailbatchid=@mailbatchId

select * from pns.batchsummary where BatchIdentifier in (select uniqueid from am.PrintBatches where mailbatchid=@mailbatchId)

select * from pns.mrdfrequest where jobid IN (select printbatch from pns.batchsummary where BatchIdentifier in (select uniqueid from am.PrintBatches where mailbatchid=@mailbatchId))
--BEGIN TRAN
--delete from pns.mrdfrequest where jobid IN (select printbatch from pns.batchsummary where BatchIdentifier in (select uniqueid from am.PrintBatches where mailbatchid=@mailbatchId))
--delete from pns.batchsummary where BatchIdentifier in (select uniqueid from am.PrintBatches where mailbatchid=@mailbatchId)
--delete from Common.MailPiecePageInfo where mailpieceid in (select id from am.mailpieces where mailbatchid=@mailbatchId )
--delete from am.mailpiececustominfo where mailpieceid in (select id from am.mailpieces where mailbatchid=@mailbatchId )
--delete from am.mailpieces where mailbatchid=@mailbatchId 
--delete from am.PrintBatches where mailbatchid=@mailbatchId
--delete from am.mailbatches  where id=@mailbatchId 

----Rollback TRAN

--Commit tran