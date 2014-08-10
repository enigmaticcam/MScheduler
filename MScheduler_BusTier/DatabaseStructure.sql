USE [MScheduler]
GO

/****** Object:  Table [dbo].[Meeting]    Script Date: 8/10/2014 4:37:30 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Meeting](
	[MeetingId] [int] IDENTITY(1,1) NOT NULL,
	[MeetingDescription] [nvarchar](max) NOT NULL,
	[MeetingDate] [datetime] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [dbo].[Slot]    Script Date: 8/10/2014 4:37:30 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Slot](
	[SlotId] [int] IDENTITY(1,1) NOT NULL,
	[MeetingId] [int] NOT NULL,
	[SlotFillerId] [int] NOT NULL,
	[Title] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[SortNumber] [int] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [dbo].[SlotFiller]    Script Date: 8/10/2014 4:37:30 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SlotFiller](
	[SlotFillerId] [int] IDENTITY(1,1) NOT NULL,
	[SlotTypeId] [int] NOT NULL
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[SlotType]    Script Date: 8/10/2014 4:37:30 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SlotType](
	[SlotTypeId] [int] IDENTITY(1,1) NOT NULL,
	[Description] [nvarchar](max) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [dbo].[Template]    Script Date: 8/10/2014 4:37:30 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Template](
	[TemplateId] [int] IDENTITY(1,1) NOT NULL,
	[Description] [nvarchar](max) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [dbo].[TemplateSlot]    Script Date: 8/10/2014 4:37:30 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TemplateSlot](
	[TemplateSlotId] [int] IDENTITY(1,1) NOT NULL,
	[TemplateId] [int] NOT NULL,
	[SlotTypeId] [int] NOT NULL,
	[Title] [nvarchar](max) NOT NULL,
	[SortNumber] [int] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [dbo].[User]    Script Date: 8/10/2014 4:37:30 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[User](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[SlotFillerId] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL
) ON [PRIMARY]

GO


CREATE PROCEDURE [dbo].[Meeting_Edit]
@MeetingId int,
@Description nvarchar(max),
@MeetingDate datetime

AS

UPDATE Meeting
	SET MeetingDescription = @Description,
	MeetingDate = @MeetingDate
WHERE MeetingId = @MeetingId
GO

/****** Object:  StoredProcedure [dbo].[Meeting_New]    Script Date: 8/10/2014 4:37:44 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Meeting_New]
@Description nvarchar(max),
@MeetingDate datetime

AS

BEGIN TRANSACTION

INSERT INTO Meeting(MeetingDescription, MeetingDate)
VALUES(@Description, @MeetingDate)

SELECT MAX(MeetingId) FROM Meeting

COMMIT TRANSACTION
GO

/****** Object:  StoredProcedure [dbo].[Slot_Edit]    Script Date: 8/10/2014 4:37:44 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Slot_Edit]
@SlotId int,
@MeetingId int,
@SlotFillerId int,
@Title nvarchar(max),
@Description nvarchar(max),
@SortNumber int

AS

update Slot
	set MeetingId = @MeetingId,
	SlotFillerId = @SlotFillerId,
	Title = @Title,
	[Description] = @Description,
	SortNumber = @SortNumber
where SlotId = @SlotId
GO

/****** Object:  StoredProcedure [dbo].[Slot_New]    Script Date: 8/10/2014 4:37:44 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Slot_New]
@MeetingId int,
@SlotFillerId int,
@Title nvarchar(max),
@Description nvarchar(max),
@SortNumber int

AS

BEGIN TRANSACTION

INSERT INTO Slot(MeetingId, SlotFillerId, Title, Description, SortNumber)
VALUES(@MeetingId, @SlotFillerId, @Title, @Description, @SortNumber)

SELECT MAX(MeetingId)
FROM Meeting

COMMIT TRANSACTION
GO

/****** Object:  StoredProcedure [dbo].[SlotFiller_New]    Script Date: 8/10/2014 4:37:44 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SlotFiller_New]
@SlotTypeId int

as

begin transaction

insert into SlotFiller(SlotTypeId)
values(@SlotTypeId)

select max(SlotFillerId) from SlotFiller

commit transaction
GO

/****** Object:  StoredProcedure [dbo].[Template_Edit]    Script Date: 8/10/2014 4:37:44 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Template_Edit]
@TemplateId int,
@Description nvarchar(max)

as

update Template
	set Description = @Description
where TemplateId = @TemplateId
GO

/****** Object:  StoredProcedure [dbo].[Template_New]    Script Date: 8/10/2014 4:37:44 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Template_New]
@Description nvarchar(max)

as

begin transaction

insert into Template(Description)
values(@Description)

select max(TemplateId) from Template

commit transaction
GO

/****** Object:  StoredProcedure [dbo].[TemplateSlot_Edit]    Script Date: 8/10/2014 4:37:44 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[TemplateSlot_Edit]
@TemplateSlotId int,
@TemplateId int,
@SlotTypeId int,
@Title nvarchar(max),
@SortNumber int

as

update TemplateSlot
	set TemplateId = @TemplateId,
	SlotTypeId = @SlotTypeId,
	Title = @Title,
	SortNumber = @SortNumber
where TemplateSlotId = @TemplateSlotId
GO

/****** Object:  StoredProcedure [dbo].[TemplateSlot_New]    Script Date: 8/10/2014 4:37:44 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[TemplateSlot_New]
@TemplateId int,
@SlotTypeId int,
@Title nvarchar(max),
@SortNumber int

as

begin transaction

insert into TemplateSlot(TemplateId, SlotTypeId, Title, SortNumber)
values(@TemplateId, @SlotTypeId, @Title, @SortNumber)

select max(TemplateSlotId) from TemplateSlot

commit transaction
GO

/****** Object:  StoredProcedure [dbo].[User_Edit]    Script Date: 8/10/2014 4:37:44 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[User_Edit]
@UserId int,
@Name nvarchar(50)

as

update [User]
	set Name = @Name
where UserId = @UserId


GO

/****** Object:  StoredProcedure [dbo].[User_New]    Script Date: 8/10/2014 4:37:44 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[User_New]
@Name nvarchar(50)

as

begin transaction

declare @SlotFillerId int
insert into SlotFiller(SlotTypeId)
values(1)

select @SlotFillerId = max(SlotFillerId) from SlotFiller

insert into [User](SlotFillerId, Name)
values(@SlotFillerId, @Name)

commit transaction


GO


