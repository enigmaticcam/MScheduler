USE [MScheduler]
GO

/****** Object:  Table [dbo].[Meeting]    Script Date: 9/27/2014 3:40:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Meeting](
	[MeetingId] [int] IDENTITY(1,1) NOT NULL,
	[MeetingDescription] [nvarchar](max) NOT NULL,
	[MeetingDate] [datetime] NOT NULL,
	[IsDeleted] [bit] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [dbo].[Slot]    Script Date: 9/27/2014 3:40:06 PM ******/
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
	[SortNumber] [int] NOT NULL,
	[SlotTypeId] [int] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [dbo].[SlotFiller]    Script Date: 9/27/2014 3:40:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SlotFiller](
	[SlotFillerId] [int] IDENTITY(1,1) NOT NULL,
	[SlotTypeId] [int] NOT NULL,
	[SlotFillerSourceId] [int] NOT NULL
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[SlotType]    Script Date: 9/27/2014 3:40:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SlotType](
	[SlotTypeId] [int] NOT NULL,
	[Description] [nvarchar](max) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [dbo].[Template]    Script Date: 9/27/2014 3:40:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Template](
	[TemplateId] [int] IDENTITY(1,1) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[IsDeleted] [bit] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [dbo].[TemplateSlot]    Script Date: 9/27/2014 3:40:06 PM ******/
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

/****** Object:  Table [dbo].[User]    Script Date: 9/27/2014 3:40:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[User](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL
) ON [PRIMARY]

GO


