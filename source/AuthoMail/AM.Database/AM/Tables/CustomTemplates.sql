CREATE TABLE [AM].[CustomTemplates] (
    [DocTypeId]          INT           NOT NULL,
    [CustomTemplatePath] VARCHAR (846) NOT NULL,
    [CommunityCode]      VARCHAR (50)  NOT NULL,
    CONSTRAINT [PK_CustomTemplates] PRIMARY KEY CLUSTERED ([DocTypeId] ASC, [CustomTemplatePath] ASC, [CommunityCode] ASC)
);


