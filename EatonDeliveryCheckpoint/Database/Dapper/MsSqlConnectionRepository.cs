using Dapper;
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


        public int QueryDeliveryCargoCount()
        {
            try
            {
                var sql = @"SELECT 
                            COUNT(id) 
                            FROM [scannel].[dbo].[eaton_delivery_cargo] 
                            WHERE state<@state ";
                return _connection.Query<int>(sql, new
                {
                    state = 1,
                }).FirstOrDefault();
            }
            catch (Exception exp)
            {
                return -1;
            }
        }

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
                            c.start_time, 
                            c.end_time, 
                            c.valid_pallet_quantity, 
                            c.invalid_pallet_quantity, 
                            c.state 
                            FROM [scannel].[dbo].[eaton_delivery_file] AS f 
                            INNER JOIN [scannel].[dbo].[eaton_delivery_cargo] AS c 
                            ON f.id=c.f_delivery_file_id 
                            WHERE c.state<@state 
                            ORDER BY f.upload_timestamp DESC ";
                return _connection.Query<DeliveryCargoDto>(sql, new
                {
                    state = 1,
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
                            i.material, 
                            i.count, 
                            i.realtime_product_count, 
                            i.realtime_pallet_count, 
                            i.alert 
                            FROM [scannel].[dbo].[eaton_delivery_cargo] AS c 
                            INNER JOIN [scannel].[dbo].[eaton_cargo_data_info] AS i 
                            ON c.id=i.f_delivery_cargo_id 
                            WHERE c.no=@no ";
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

        public List<CargoDataContext> QueryDeliveryCargoDataContextsWithCargoId(List<int> ids)
        {
            try
            {
                var sql = @"SELECT * FROM [scannel].[dbo].[eaton_cargo_data] 
                            WHERE f_delivery_cargo_id IN @ids ";
                return _connection.Query<CargoDataContext>(sql, new
                {
                    ids = ids,
                }).ToList();
            }
            catch
            {
                return null;
            }
        }

        public List<CargoDataInfoContext> QueryCargoDataInfoContexts(List<int> ids)
        {
            try
            {
                var sql = @"SELECT * FROM [scannel].[dbo].[eaton_cargo_data_info] 
                            WHERE f_delivery_cargo_id IN @ids ";
                return _connection.Query<CargoDataInfoContext>(sql, new
                {
                    ids = ids,
                }).ToList();
            }
            catch
            {
                return null;
            }
        }

        public List<DeliveryCargoDto> QueryDeliveryCargoDtosWithState(int state)
        {
            try
            {
                var sql = @"SELECT * FROM [scannel].[dbo].[eaton_delivery_cargo] 
                            WHERE state=@state ";
                return _connection.Query<DeliveryCargoDto>(sql, new
                {
                    state = state,
                }).ToList();
            }
            catch
            {
                return null;
            }
        }

        public CargoDataInfoContext QueryCargoDataInfoContextWithMaterial(int f_delivery_cargo_id, string material)
        {
            try
            {
                var sql = @"SELECT * FROM [scannel].[dbo].[eaton_cargo_data_info] 
                            WHERE f_delivery_cargo_id=@f_delivery_cargo_id AND material=@material ";
                return _connection.Query<CargoDataInfoContext>(sql, new
                {
                    f_delivery_cargo_id = f_delivery_cargo_id,
                    material = material,
                }).FirstOrDefault();
            }
            catch (Exception exp)
            {
                return null;
            }
        }

        public CargoDataInfoContext QueryCargoDataInfoContextWithInvalidContext(CargoDataInfoContext context)
        {
            try
            {
                var sql = @"SELECT * FROM [scannel].[dbo].[eaton_cargo_data_info] 
                            WHERE material=@material AND count=@count";
                return _connection.Query<CargoDataInfoContext>(sql, new
                {
                    material = context.material,
                    count = context.count,
                    alert = context.alert,
                }).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public DeliveryCargoContext QueryDeliveryCargoContextWithNo(string no)
        {
            try
            {
                var sql = @"SELECT * FROM [scannel].[dbo].[eaton_delivery_cargo] 
                            WHERE no=@no ";
                return _connection.Query<DeliveryCargoContext>(sql, new
                {
                    no = no,
                }).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public int QueryQtyByCargoDataRecordContextWithInfoIds(int f_delivery_cargo_id, int f_cargo_data_info_id)
        {
            try
            {
                var sql = @"SELECT d.qty  
                            FROM [scannel].[dbo].[eaton_cargo_data_record] AS r 
                            INNER JOIN [scannel].[dbo].[eaton_epc_data] AS d 
                            ON r.f_epc_data_id=d.id 
                            WHERE r.valid=@valid 
                            AND f_delivery_cargo_id=@f_delivery_cargo_id 
                            AND f_cargo_data_info_id=@f_cargo_data_info_id ";
                return _connection.Query<int>(sql, new
                {
                    f_delivery_cargo_id = f_delivery_cargo_id,
                    f_cargo_data_info_id = f_cargo_data_info_id,
                    valid = 0,
                }).FirstOrDefault();
            }
            catch
            {
                return -1;
            }
        }

        public DeliveryCargoDto QueryDeliveryCargoDtoWithNo(string no)
        {
            try
            {
                var sql = @"SELECT 
                            f.name, 
                            f.upload_timestamp, 
                            c.no, 
                            c.material_quantity, 
                            c.product_quantity, 
                            c.start_time, 
                            c.end_time, 
                            c.valid_pallet_quantity, 
                            c.invalid_pallet_quantity, 
                            c.state 
                            FROM [scannel].[dbo].[eaton_delivery_cargo] AS c
                            INNER JOIN [scannel].[dbo].[eaton_delivery_file] AS f 
                            ON f.id=c.f_delivery_file_id 
                            WHERE c.no=@no ";
                return _connection.Query<DeliveryCargoDto>(sql, new
                {
                    no = no,
                }).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public List<DeliveryCargoDataDto> QueryValidDeliveryCargoDataDtos(string no)
        {
            try
            {
                var sql = @"SELECT 
                            i.material, 
                            i.count, 
                            i.realtime_product_count, 
                            i.realtime_pallet_count, 
                            i.alert 
                            FROM [scannel].[dbo].[eaton_delivery_cargo] AS c 
                            INNER JOIN [scannel].[dbo].[eaton_cargo_data_info] AS i 
                            ON c.id=i.f_delivery_cargo_id 
                            WHERE c.no=@no AND i.count>@valid ";
                return _connection.Query<DeliveryCargoDataDto>(sql, new
                {
                    no = no,
                    valid = 0,
                }).ToList();
            }
            catch
            {
                return null;
            }
        }

        public List<DeliveryCargoDataDto> QueryInvalidDeliveryCargoDataDtos(string no)
        {
            try
            {
                var sql = @"SELECT 
                            i.material, 
                            i.count, 
                            d.qty AS realtime_product_count, 
                            1 AS realtime_pallet_count, 
                            1 AS alert  
                            FROM [scannel].[dbo].[eaton_delivery_cargo] AS c 
                            INNER JOIN [scannel].[dbo].[eaton_cargo_data_info] AS i 
                            ON c.id=i.f_delivery_cargo_id 
                            INNER JOIN [scannel].[dbo].[eaton_cargo_data_record] AS r 
                            ON i.id=r.f_cargo_data_info_id 
                            iNNER JOIN [scannel].[dbo].[eaton_epc_data] AS d 
                            ON r.f_epc_data_id=d.id 
                            WHERE c.no=@no AND r.valid=@valid ";
                return _connection.Query<DeliveryCargoDataDto>(sql, new
                {
                    no = no,
                    valid = 0,
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
                                start_time, 
                                end_time, 
                                valid_pallet_quantity, 
                                invalid_pallet_quantity, 
                                state) 
                                VALUES (
                                @f_delivery_file_id, 
                                @no, 
                                @material_quantity, 
                                @product_quantity, 
                                @start_time, 
                                @end_time, 
                                @valid_pallet_quantity, 
                                @invalid_pallet_quantity,
                                @state) ";
                    var result = _connection.Execute(sql, new
                    {
                        f_delivery_file_id = context.f_delivery_file_id,
                        no = context.no,
                        material_quantity = context.material_quantity,
                        product_quantity = context.product_quantity,
                        start_time = context.start_time,
                        end_time = context.end_time,
                        valid_pallet_quantity = context.valid_pallet_quantity,
                        invalid_pallet_quantity = context.invalid_pallet_quantity,
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

        public bool InsertCargoDataContexts(List<CargoDataContext> contexts)
        {
            try
            {
                foreach(var context in contexts)
                {
                    var sql = @"INSERT INTO [scannel].[dbo].[eaton_cargo_data]( 
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

        public bool InsertCargoDataInfoContexts(List<CargoDataInfoContext> contexts)
        {
            try
            {
                foreach (var context in contexts)
                {
                    var sql = @"INSERT INTO [scannel].[dbo].[eaton_cargo_data_info]( 
                                f_delivery_cargo_id, 
                                material, 
                                count, 
                                realtime_product_count, 
                                realtime_pallet_count, 
                                alert)  
                                VALUES( 
                                @f_delivery_cargo_id, 
                                @material, 
                                @count, 
                                @realtime_product_count, 
                                @realtime_pallet_count, 
                                @alert) ";
                    var result = _connection.Execute(sql, new
                    {
                        f_delivery_cargo_id = context.f_delivery_cargo_id,
                        material = context.material,
                        count = context.count,
                        realtime_product_count = context.realtime_product_count,
                        realtime_pallet_count = context.realtime_pallet_count,
                        alert = context.alert,
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

        public bool InsertCargoDataInfoContext(CargoDataInfoContext context)
        {
            try
            {
                var sql = @"INSERT INTO [scannel].[dbo].[eaton_cargo_data_info]( 
                                f_delivery_cargo_id, 
                                material, 
                                count, 
                                realtime_product_count, 
                                realtime_pallet_count, 
                                alert)  
                                VALUES( 
                                @f_delivery_cargo_id, 
                                @material, 
                                @count, 
                                @realtime_product_count, 
                                @realtime_pallet_count, 
                                @alert) ";
                var result = _connection.Execute(sql, new
                {
                    f_delivery_cargo_id = context.f_delivery_cargo_id,
                    material = context.material,
                    count = context.count,
                    realtime_product_count = context.realtime_product_count,
                    realtime_pallet_count = context.realtime_pallet_count,
                    alert = context.alert,
                }) > 0;
                return result;
            }
            catch (Exception exp)
            {
                return false;
            }
        }

        public bool InsertCargoDataRecordContext(CargoDataRecordContext context)
        {
            try
            {
                var sql = @"INSERT INTO [scannel].[dbo].[eaton_cargo_data_record]( 
                            f_delivery_cargo_id, 
                            f_cargo_data_info_id, 
                            f_epc_raw_id, 
                            f_epc_data_id, 
                            valid)  
                            VALUES( 
                            @f_delivery_cargo_id, 
                            @f_cargo_data_info_id, 
                            @f_epc_raw_id, 
                            @f_epc_data_id, 
                            @valid) ";
                var result = _connection.Execute(sql, new
                {
                    f_delivery_cargo_id = context.f_delivery_cargo_id,
                    f_cargo_data_info_id = context.f_cargo_data_info_id,
                    f_epc_raw_id = context.f_epc_raw_id,
                    f_epc_data_id = context.f_epc_data_id,
                    valid = context.valid,
                }) > 0;
                return result;
            }
            catch (Exception exp)
            {
                return false;
            }
        }

        #endregion

        #region Update

        public bool UpdateDeliveryCargoContextWhenStart(DeliveryCargoDto dto)
        {
            try
            {
                var sql = @"UPDATE [scannel].[dbo].[eaton_delivery_cargo] 
                            SET start_time=@start_time, state=@state 
                            WHERE no=@no ";
                return _connection.Execute(sql, new
                {
                    start_time = dto.start_time,
                    state = dto.state,
                    no = dto.no,
                }) > 0;
            }
            catch (Exception exp)
            {
                return false;
            }
        }

        public bool UpdateDeliveryCargoContextWhenFinish(DeliveryCargoDto dto)
        {
            try
            {
                var sql = @"UPDATE [scannel].[dbo].[eaton_delivery_cargo] 
                            SET end_time=@end_time, state=@state 
                            WHERE no=@no ";
                return _connection.Execute(sql, new
                {
                    end_time = dto.end_time,
                    state = dto.state,
                    no = dto.no,
                }) > 0;
            }
            catch (Exception exp)
            {
                return false;
            }
        }

        public bool UpdateDeliveryCargoContextWhenDataInserted(DeliveryCargoContext context)
        {
            try
            {
                var sql = @"UPDATE [scannel].[dbo].[eaton_delivery_cargo] 
                            SET valid_pallet_quantity=@valid_pallet_quantity, invalid_pallet_quantity=@invalid_pallet_quantity 
                            WHERE id=@id ";
                return _connection.Execute(sql, new
                {
                    id = context.id,
                    valid_pallet_quantity = context.valid_pallet_quantity,
                    invalid_pallet_quantity = context.invalid_pallet_quantity,
                }) > 0;
            }
            catch (Exception exp)
            {
                return false;
            }
        }

        public bool UpdateCargoDataInfoContext(CargoDataInfoContext context)
        {
            try
            {
                var sql = @"UPDATE [scannel].[dbo].[eaton_cargo_data_info] 
                            SET realtime_product_count=@realtime_product_count, realtime_pallet_count=@realtime_pallet_count, alert=@alert 
                            WHERE id=@id ";
                return _connection.Execute(sql, new
                {
                    id = context.id,
                    realtime_product_count = context.realtime_product_count,
                    realtime_pallet_count = context.realtime_pallet_count,
                    alert = context.alert,
                }) > 0;
            }
            catch (Exception exp)
            {
                return false;
            }
        }

        #endregion
    }
}
