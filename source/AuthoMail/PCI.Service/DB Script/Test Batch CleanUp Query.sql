--DELETE FROM MRDFResponse

--DELETE FROM MRDFRequest

--DELETE FROM BatchSummary
--DBCC CHECKIDENT('BatchSummary', RESEED, 1234566)

--DELETE MRDF Response file from "/Unknown/PCI/Response" folder
--Get Trigger file from triggers folder to one level up
SELECT * FROM BatchSummary

