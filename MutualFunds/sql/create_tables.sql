USE [MutualFunds]
GO

/****** Object:  Table [dbo].[MutualFundsInfo]    Script Date: 2015-08-29 10:31:46 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[MutualFundsInfo](
	[Date] [date] NOT NULL,
	[UnitPrice] [decimal](18, 2) NOT NULL,
	[DecimalChange] [decimal](18, 2) NOT NULL,
	[PercentChange] [decimal](18, 2) NOT NULL,
	[Balance] [decimal](18, 2) NOT NULL
) ON [PRIMARY]

GO


