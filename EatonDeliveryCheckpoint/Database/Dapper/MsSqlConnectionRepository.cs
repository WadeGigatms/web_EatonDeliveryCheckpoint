﻿using Dapper;
using EatonDeliveryCheckpoint.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Database.Dapper
{
    public class MsSqlConnectionRepository : ConnectionRepositoryBase
    {
        public MsSqlConnectionRepository(string connectionString) : base(DatabaseConnectionName.MsSql, connectionString)
        {
        }

        #region Query

        public List<DeliveryCargoDto> QueryDeliveryCargoDtos()
        {
            try
            {
                var sql = @"SELECT 
                            f.name AS file_name, 
                            f.upload_timestamp, 
                            c.no, 
                            c.material_quantity, 
                            c.product_quantity, 
                            c.date, 
                            c.start_time, 
                            c.end_time, 
                            c.duration, 
                            c.valid_pallet_quantity, 
                            c.invalid_pallet_quantity, 
                            c.pallet_rate, 
                            c.state 
                            FROM [scannel].[dbo].[eaton_delivery_file] AS f 
                            INNER JOIN [scannel].[dbo].[eaton_delivery_cargo] AS c 
                            ON f.id=c.f_delivery_file_id 
                            WHERE c.state=@state 
                            ORDER BY f.upload_timestamp DESC ";
                return _connection.Query<DeliveryCargoDto>(sql, new
                {
                    state = -1,
                }).ToList();
            }
            catch (Exception exp)
            {
                return null;
            }
        }

        public List<DeliveryCargoDataDto> QueryDeliveryCargoDataDtos(string no)
        {
            try
            {
                var sql = @"SELECT 
                            d.delivery, 
                            d.item, 
                            d.material, 
                            d.quantity, 
                            r.product_quantity AS realtime_product_quantity, 
                            r.pallet_quantity AS realtime_pallet_quantity 
                            FROM [scannel].[dbo].[eaton_delivery_cargo] AS c 
                            INNER JOIN [scannel].[dbo].[eaton_delivery_cargo_data] AS d 
                            ON c.no=@no 
                            INNER JOIN [scannel].[dbo].[eaton_delivery_cargo_data_realtime] AS r 
                            ON d.id=r.f_delivery_cargo_data_id ";
                return _connection.Query<DeliveryCargoDataDto>(sql, new
                {
                    no = no,
                }).ToList();
            }
            catch (Exception exp)
            {
                return null;
            }
        }

        public DeliveryFileContext QueryDeliveryFileContextWithFileName(string name)
        {
            try
            {
                var sql = @"SELECT * FROM [scannel].[dbo].[eaton_delivery_file] 
                            WHERE name=@name ";
                return _connection.Query<DeliveryFileContext>(sql, new
                {
                    name = name,
                }).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public List<DeliveryCargoContext> QueryDeliveryCargoContextsWithFileId(int id)
        {
            try
            {
                var sql = @"SELECT * FROM [scannel].[dbo].[eaton_delivery_cargo] 
                            WHERE f_delivery_file_id=@id ";
                return _connection.Query<DeliveryCargoContext>(sql, new
                {
                    id = id,
                }).ToList();
            }
            catch
            {
                return null;
            }
        }

        public List<DeliveryCargoDataContext> QueryDeliveryCargoDataContextsWithCargoId(List<int> ids)
        {
            try
            {
                var sql = @"SELECT * FROM [scannel].[dbo].[eaton_delivery_cargo_data] 
                            WHERE f_delivery_cargo_id IN @ids ";
                return _connection.Query<DeliveryCargoDataContext>(sql, new
                {
                    ids = ids,
                }).ToList();
            }
            catch
            {
                return null;
            }
        }

        public List<DeliveryCargoDataRealtimeContext> QueryDeliveryCargoDataRealtimeContextsWithCargoDataIds(List<int> ids)
        {
            try
            {
                var sql = @"SELECT * FROM [scannel].[dbo].[eaton_delivery_cargo_data_realtime] 
                            WHERE f_delivery_cargo_data_id IN @ids ";
                return _connection.Query<DeliveryCargoDataRealtimeContext>(sql, new
                {
                    ids = ids,
                }).ToList();
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Insert

        public bool InsertDeliveryFileContext(DeliveryFileContext context)
        {
            try
            {
                var sql = @"INSERT INTO [scannel].[dbo].[eaton_delivery_file](name, upload_timestamp, json) 
                            VALUES(@name, @upload_timestamp, @json) ";
                return _connection.Execute(sql, new
                {
                    name = context.name,
                    upload_timestamp = context.upload_timestamp,
                    json = context.json,
                }) > 0;
            }
            catch
            {
                return false;
            }
        }

        public bool InsertDeliveryCargoContexts(List<DeliveryCargoContext> contexts)
        {
            try
            {
                foreach(var context in contexts) {
                    var sql = @"INSERT INTO [scannel].[dbo].[eaton_delivery_cargo](
                                f_delivery_file_id, 
                                no, 
                                material_quantity, 
                                product_quantity, 
                                date, 
                                start_time, 
                                end_time, 
                                duration, 
                                valid_pallet_quantity, 
                                invalid_pallet_quantity, 
                                pallet_rate, 
                                state) 
                                VALUES (
                                @f_delivery_file_id, 
                                @no, 
                                @material_quantity, 
                                @product_quantity, 
                                @date, 
                                @start_time, 
                                @end_time, 
                                @duration, 
                                @valid_pallet_quantity, 
                                @invalid_pallet_quantity, 
                                @pallet_rate, 
                                @state) ";
                    var result = _connection.Execute(sql, new
                    {
                        f_delivery_file_id = context.f_delivery_file_id,
                        no = context.no,
                        material_quantity = context.material_quantity,
                        product_quantity = context.product_quantity,
                        date = context.date,
                        start_time = context.start_time,
                        end_time = context.end_time,
                        duration = context.duration,
                        valid_pallet_quantity = context.valid_pallet_quantity,
                        invalid_pallet_quantity = context.invalid_pallet_quantity,
                        pallet_rate = context.pallet_rate,
                        state = context.state
                    }) > 0;
                    if (!result) { return false; }
                }
                return true;
            }
            catch (Exception exp)
            {
                return false;
            }
        }

        public bool InsertDeliveryCargoDataContexts(List<DeliveryCargoDataContext> contexts)
        {
            try
            {
                foreach(var context in contexts)
                {
                    var sql = @"INSERT INTO [scannel].[dbo].[eaton_delivery_cargo_data]( 
                                f_delivery_cargo_id, 
                                delivery, 
                                item, 
                                material, 
                                quantity) 
                                VALUES( 
                                @f_delivery_cargo_id, 
                                @delivery, 
                                @item, 
                                @material, 
                                @quantity) ";
                    var result = _connection.Execute(sql, new
                    {
                        f_delivery_cargo_id = context.f_delivery_cargo_id,
                        delivery = context.delivery,
                        item = context.item,
                        material = context.material,
                        quantity = context.quantity
                    }) > 0;
                    if (!result) { return false; }
                }
                return true;
            }
            catch (Exception exp)
            {
                return false;
            }
        }

        public bool InsertDeliveryCargoDataRealtimeContexts(List<DeliveryCargoDataRealtimeContext> contexts)
        {
            try
            {
                foreach (var context in contexts)
                {
                    var sql = @"INSERT INTO [scannel].[dbo].[eaton_delivery_cargo_data_realtime]( 
                                f_delivery_cargo_data_id, 
                                product_quantity, 
                                pallet_quantity) 
                                VALUES( 
                                @f_delivery_cargo_data_id, 
                                @product_quantity, 
                                @pallet_quantity) ";
                    var result = _connection.Execute(sql, new
                    {
                        f_delivery_cargo_data_id = context.f_delivery_cargo_data_id,
                        product_quantity = context.product_quantity,
                        pallet_quantity = context.pallet_quantity,
                    }) > 0;
                    if (!result) { return false; }
                }
                return true;
            }
            catch (Exception exp)
            {
                return false;
            }
        }

        #endregion
    }
}