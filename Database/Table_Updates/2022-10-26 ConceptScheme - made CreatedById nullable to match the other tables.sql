USE Navy_RRL_V2

ALTER TABLE [dbo].[ConceptScheme]
ALTER COLUMN [CreatedById] int null;

ALTER TABLE [dbo].[ConceptScheme] DROP CONSTRAINT
	FK_ConceptScheme__AccountCreatedBy

ALTER TABLE [dbo].[ConceptScheme] ADD CONSTRAINT
	FK_ConceptScheme_AccountCreatedBy FOREIGN KEY
	(
	CreatedById
	) REFERENCES dbo.Account
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 