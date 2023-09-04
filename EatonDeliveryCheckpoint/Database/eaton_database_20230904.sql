USE [scannel]
GO
/****** Object:  Table [dbo].[eaton_cargo_data]    Script Date: 2023/9/4 下午 01:13:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[eaton_cargo_data](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[f_delivery_number_id] [int] NOT NULL,
	[delivery] [varchar](16) NOT NULL,
	[item] [varchar](6) NULL,
	[material] [varchar](16) NOT NULL,
	[quantity] [int] NOT NULL,
 CONSTRAINT [PK_eaton_delivery_cargo_data] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[eaton_cargo_data_info]    Script Date: 2023/9/4 下午 01:13:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[eaton_cargo_data_info](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[f_delivery_number_id] [int] NOT NULL,
	[delivery] [varchar](16) NOT NULL,
	[material] [varchar](16) NOT NULL,
	[count] [int] NOT NULL,
	[realtime_product_count] [int] NOT NULL,
	[realtime_pallet_count] [int] NOT NULL,
	[alert] [int] NOT NULL,
 CONSTRAINT [PK_f_cargo_data_count] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[eaton_cargo_data_record]    Script Date: 2023/9/4 下午 01:13:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[eaton_cargo_data_record](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[f_delivery_number_id] [int] NOT NULL,
	[f_cargo_data_info_id] [int] NOT NULL,
	[f_epc_raw_id] [int] NOT NULL,
	[f_epc_data_id] [int] NOT NULL,
	[valid] [int] NOT NULL,
 CONSTRAINT [PK_eaton_delivery_cargo_realtime_record] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[eaton_delivery_file]    Script Date: 2023/9/4 下午 01:13:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[eaton_delivery_file](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [varchar](50) NOT NULL,
	[upload_timestamp] [varchar](30) NOT NULL,
	[json] [varchar](max) NOT NULL,
 CONSTRAINT [PK_eaton_delivery_excel_file] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[eaton_delivery_number]    Script Date: 2023/9/4 下午 01:13:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[eaton_delivery_number](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[f_delivery_file_id] [int] NOT NULL,
	[no] [varchar](20) NOT NULL,
	[material_quantity] [int] NOT NULL,
	[product_quantity] [int] NOT NULL,
	[start_time] [varchar](30) NOT NULL,
	[end_time] [varchar](30) NOT NULL,
	[valid_pallet_quantity] [int] NOT NULL,
	[invalid_pallet_quantity] [int] NOT NULL,
	[state] [int] NOT NULL,
 CONSTRAINT [PK_eaton_delivery_cargo] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[eaton_epc_data]    Script Date: 2023/9/4 下午 01:13:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[eaton_epc_data](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[f_epc_raw_ids] [varchar](max) NOT NULL,
	[wo] [varchar](12) NOT NULL,
	[qty] [varchar](4) NOT NULL,
	[pn] [varchar](20) NOT NULL,
	[line] [varchar](4) NOT NULL,
	[pallet_id] [varchar](12) NOT NULL,
 CONSTRAINT [PK_eaton_epc_data] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[eaton_epc_raw]    Script Date: 2023/9/4 下午 01:13:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[eaton_epc_raw](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[epc] [varchar](150) NOT NULL,
	[reader_id] [varchar](20) NOT NULL,
	[timestamp] [varchar](30) NOT NULL,
 CONSTRAINT [PK_eaton_epc] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[eaton_cargo_data_record] ADD  CONSTRAINT [DF_eaton_delivery_cargo_realtime_record_f_epc_raw_id]  DEFAULT ((0)) FOR [f_epc_raw_id]
GO
ALTER TABLE [dbo].[eaton_cargo_data_record] ADD  CONSTRAINT [DF_eaton_delivery_cargo_realtime_record_f_epc_data_id]  DEFAULT ((0)) FOR [f_epc_data_id]
GO
ALTER TABLE [dbo].[eaton_cargo_data_record] ADD  CONSTRAINT [DF_eaton_delivery_cargo_realtime_record_state]  DEFAULT ((0)) FOR [valid]
GO
