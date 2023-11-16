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


        public int QueryDeliveryNumberContextCount()
        {
            try
            {
                var sql = @"SELECT 
                            COUNT(id) 
                            FROM [scannel].[dbo].[eaton_delivery_number] 
                            WHERE state BETWEEN @state_ready AND @state_delivery ";
                return _connection.Query<int>(sql, new
                {
                    state_ready = -1,
                    state_delivery = 0,
                }).FirstOrDefault();
            }
            catch (Exception exp)
            {
                return -1;
            }
        }

        public List<DeliveryNumberDto> QueryDeliveryNumberDtos()
        {
            try
            {
                var sql = @"SELECT 
                            f.name AS file_name, 
                            f.upload_timestamp, 
                            n.no, 
                            n.material_quantity, 
                            n.product_quantity, 
                            n.start_time, 
                            n.end_time, 
                            n.pallet_quantity, 
                            n.miss_pallet_quantity, 
                            n.valid_pallet_quantity, 
                            n.invalid_pallet_quantity, 
                            n.state 
                            FROM [scannel].[dbo].[eaton_delivery_file] AS f 
                            INNER JOIN [scannel].[dbo].[eaton_delivery_number] AS n 
                            ON f.id=n.f_delivery_file_id 
                            WHERE n.state BETWEEN @state_ready AND @state_delivery  
                            ORDER BY f.upload_timestamp DESC ";
                return _connection.Query<DeliveryNumberDto>(sql, new
                {
                    state_ready = -1,
                    state_delivery = 0,
                }).ToList();
            }
            catch (Exception exp)
            {
                return null;
            }
        }

        public List<DeliveryNumberDataDto> QueryDeliveryNumberDataDtos(string no)
        {
            try
            {
                var dataSql = @"SELECT 
                                i.delivery, 
                                i.material, 
                                i.product_count, 
                                i.pallet_count, 
                                i.realtime_product_count, 
                                i.realtime_pallet_count, 
                                i.alert 
                                FROM [scannel].[dbo].[eaton_delivery_number] AS n 
                                INNER JOIN [scannel].[dbo].[eaton_cargo_data_info] AS i 
                                ON n.id=i.f_delivery_number_id 
                                WHERE n.no=@no ";
                List<DeliveryNumberDataDto> dataDtos = _connection.Query<DeliveryNumberDataDto>(dataSql, new
                {
                    no = no,
                }).ToList();
                List<string> materials = dataDtos.Select(dto => dto.material).ToList();
                var recordSql = @"SELECT 
                                    w.epc, 
                                    w.reader_id, 
                                    w.timestamp, 
                                    d.wo, 
                                    d.qty, 
                                    d.pn, 
                                    d.line, 
                                    d.pallet_id 
                                    FROM [scannel].[dbo].[eaton_delivery_number] AS n
                                    INNER JOIN [scannel].[dbo].[eaton_cargo_data_info] AS i 
                                    ON n.id=i.f_delivery_number_id AND i.material IN @materials
                                    INNER JOIN [scannel].[dbo].[eaton_cargo_data_record] AS r 
                                    ON i.id=r.f_cargo_data_info_id 
                                    INNER JOIN [scannel].[dbo].[eaton_epc_data] AS d 
                                    ON d.id=r.f_epc_data_id 
                                    INNER JOIN [scannel].[dbo].[eaton_epc_raw] AS w 
                                    ON w.id=r.f_epc_raw_id 
                                    WHERE n.no=@no 
                                    ORDER BY w.timestamp ";
                List<DeliveryNumberDataRecordDto> recordDtos = _connection.Query<DeliveryNumberDataRecordDto>(recordSql, new
                {
                    no = no,
                    materials = materials
                }).ToList();
                foreach(var data in dataDtos)
                {
                    data.records = recordDtos.Where(d => d.pn == data.material).ToList();
                }
                return dataDtos;
            }
            catch (Exception exp)
            {
                return null;
            }
        }

        public List<DeliveryFileContext> QueryDeliveryFileContextWithDeliverys(List<string> deliverys)
        {
            try
            {
                var sql = @"SELECT 
                            f.id, 
                            f.name, 
                            f.upload_timestamp, 
                            f.json 
                            FROM [scannel].[dbo].[eaton_delivery_file] AS f 
                            INNER JOIN [scannel].[dbo].[eaton_delivery_number] AS n 
                            ON f.id=n.f_delivery_file_id 
                            INNER JOIN [scannel].[dbo].[eaton_cargo_data] AS d 
                            ON n.id=d.f_delivery_number_id 
                            WHERE d.delivery IN @deliverys AND f.name= ";
                return _connection.Query<DeliveryFileContext>(sql, new
                {
                    deliverys = deliverys,
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

        public DeliveryNumberContext QueryDeliveryNumberContextWithFileId(int id)
        {
            try
            {
                var sql = @"SELECT * FROM [scannel].[dbo].[eaton_delivery_number] 
                            WHERE f_delivery_file_id=@id ";
                return _connection.Query<DeliveryNumberContext>(sql, new
                {
                    id = id,
                }).FirstOrDefault();
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
                            WHERE f_delivery_number_id IN @ids ";
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
                            WHERE f_delivery_number_id IN @ids ";
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

        public List<DeliveryNumberDto> QueryDeliveryNumberDtosWithState(int state)
        {
            try
            {
                var sql = @"SELECT * FROM [scannel].[dbo].[eaton_delivery_number] 
                            WHERE state=@state ";
                return _connection.Query<DeliveryNumberDto>(sql, new
                {
                    state = state,
                }).ToList();
            }
            catch
            {
                return null;
            }
        }

        public CargoDataInfoContext QueryCargoDataInfoContextWithMaterial(int f_delivery_number_id, string material)
        {
            try
            {
                var sql = @"SELECT * FROM [scannel].[dbo].[eaton_cargo_data_info] 
                            WHERE f_delivery_number_id=@f_delivery_number_id AND material=@material ";
                return _connection.Query<CargoDataInfoContext>(sql, new
                {
                    f_delivery_number_id = f_delivery_number_id,
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
                            WHERE material=@material AND product_count=@product_count";
                return _connection.Query<CargoDataInfoContext>(sql, new
                {
                    material = context.material,
                    product_count = context.product_count,
                    alert = context.alert,
                }).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public DeliveryNumberContext QueryDeliveryNumberContextWithNo(string no)
        {
            try
            {
                var sql = @"SELECT * FROM [scannel].[dbo].[eaton_delivery_number] 
                            WHERE no=@no ";
                return _connection.Query<DeliveryNumberContext>(sql, new
                {
                    no = no,
                }).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public int QueryQtyByCargoDataRecordContextWithInfoIds(int f_delivery_number_id, int f_cargo_data_info_id)
        {
            try
            {
                var sql = @"SELECT d.qty  
                            FROM [scannel].[dbo].[eaton_cargo_data_record] AS r 
                            INNER JOIN [scannel].[dbo].[eaton_epc_data] AS d 
                            ON r.f_epc_data_id=d.id 
                            WHERE r.valid=@valid 
                            AND f_delivery_number_id=@f_delivery_number_id 
                            AND f_cargo_data_info_id=@f_cargo_data_info_id ";
                return _connection.Query<int>(sql, new
                {
                    f_delivery_number_id = f_delivery_number_id,
                    f_cargo_data_info_id = f_cargo_data_info_id,
                    valid = 0,
                }).FirstOrDefault();
            }
            catch
            {
                return -1;
            }
        }

        public DeliveryNumberDto QueryDeliveryNumberDtoWithDelivery(string delivery)
        {
            try
            {
                var sql = @"SELECT 
                            f.name, 
                            f.upload_timestamp, 
                            n.no, 
                            n.material_quantity, 
                            n.product_quantity, 
                            n.start_time, 
                            n.end_time, 
                            n.pallet_quantity, 
                            n.miss_pallet_quantity, 
                            n.valid_pallet_quantity, 
                            n.invalid_pallet_quantity, 
                            n.state 
                            FROM [scannel].[dbo].[eaton_delivery_number] AS n 
                            INNER JOIN [scannel].[dbo].[eaton_delivery_file] AS f 
                            ON f.id=n.f_delivery_file_id 
                            INNER JOIN [scannel].[dbo].[eaton_cargo_data_info] AS i 
                            ON i.f_delivery_number_id=n.id 
                            WHERE i.delivery=@delivery ";
                return _connection.Query<DeliveryNumberDto>(sql, new
                {
                    delivery = delivery,
                }).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public DeliveryNumberDto QueryDeliveryNumberDtoWithNo(string no)
        {
            try
            {
                var sql = @"SELECT 
                            f.name AS file_name, 
                            f.upload_timestamp, 
                            n.no, 
                            n.material_quantity, 
                            n.product_quantity, 
                            n.start_time, 
                            n.end_time, 
                            n.pallet_quantity, 
                            n.miss_pallet_quantity, 
                            n.valid_pallet_quantity, 
                            n.invalid_pallet_quantity, 
                            n.state 
                            FROM [scannel].[dbo].[eaton_delivery_number] AS n 
                            INNER JOIN [scannel].[dbo].[eaton_delivery_file] AS f 
                            ON f.id=n.f_delivery_file_id 
                            INNER JOIN [scannel].[dbo].[eaton_cargo_data_info] AS i 
                            ON i.f_delivery_number_id=n.id 
                            WHERE n.no=@no ";
                return _connection.Query<DeliveryNumberDto>(sql, new
                {
                    no = no,
                }).FirstOrDefault();
            }
            catch (Exception exp)
            {
                return null;
            }
        }

        public List<DeliveryNumberDataDto> QueryValidDeliveryNumberDataDtos(string no)
        {
            try
            {
                var sql = @"SELECT 
                            i.delivery, 
                            i.material, 
                            i.product_count, 
                            i.pallet_count, 
                            i.realtime_product_count, 
                            i.realtime_pallet_count, 
                            i.alert 
                            FROM [scannel].[dbo].[eaton_delivery_number] AS n 
                            INNER JOIN [scannel].[dbo].[eaton_cargo_data_info] AS i 
                            ON n.id=i.f_delivery_number_id 
                            WHERE n.no=@no AND i.product_count>@product_count ";
                return _connection.Query<DeliveryNumberDataDto>(sql, new
                {
                    no = no,
                    product_count = -1,
                }).ToList();
            }
            catch
            {
                return null;
            }
        }

        public List<DeliveryNumberDataDto> QueryInvalidDeliveryNumberDataDtos(string no)
        {
            try
            {
                var sql = @"SELECT 
                            i.delivery, 
                            i.material, 
                            i.product_count, 
                            i.pallet_count, 
                            d.qty AS realtime_product_count, 
                            1 AS realtime_pallet_count, 
                            1 AS alert  
                            FROM [scannel].[dbo].[eaton_delivery_number] AS n 
                            INNER JOIN [scannel].[dbo].[eaton_cargo_data_info] AS i 
                            ON i.f_delivery_number_id=n.id 
                            INNER JOIN [scannel].[dbo].[eaton_cargo_data_record] AS r 
                            ON r.f_cargo_data_info_id=i.id 
                            iNNER JOIN [scannel].[dbo].[eaton_epc_data] AS d 
                            ON d.id=r.f_epc_data_id 
                            WHERE n.no=@no AND r.valid=@valid ";
                return _connection.Query<DeliveryNumberDataDto>(sql, new
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

        public bool InsertDeliveryNumberContext(DeliveryNumberContext context)
        {
            try
            {
                var sql = @"INSERT INTO [scannel].[dbo].[eaton_delivery_number](
                            f_delivery_file_id, 
                            no, 
                            material_quantity, 
                            product_quantity, 
                            start_time, 
                            end_time, 
                            pallet_quantity, 
                            miss_pallet_quantity, 
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
                            @pallet_quantity, 
                            @miss_pallet_quantity,
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
                    pallet_quantity = context.pallet_quantity,
                    miss_pallet_quantity = context.miss_pallet_quantity,
                    valid_pallet_quantity = context.valid_pallet_quantity,
                    invalid_pallet_quantity = context.invalid_pallet_quantity,
                    state = context.state
                }) > 0;
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
                                f_delivery_number_id, 
                                delivery, 
                                item, 
                                material, 
                                quantity, 
                                unit) 
                                VALUES( 
                                @f_delivery_number_id, 
                                @delivery, 
                                @item, 
                                @material, 
                                @quantity, 
                                @unit) ";
                    var result = _connection.Execute(sql, new
                    {
                        f_delivery_number_id = context.f_delivery_number_id,
                        delivery = context.delivery,
                        item = context.item,
                        material = context.material,
                        quantity = context.quantity,
                        unit = context.unit,
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
                                f_delivery_number_id, 
                                delivery, 
                                material, 
                                product_count, 
                                pallet_count, 
                                realtime_product_count, 
                                realtime_pallet_count, 
                                alert)  
                                VALUES( 
                                @f_delivery_number_id, 
                                @delivery, 
                                @material, 
                                @product_count, 
                                @pallet_count, 
                                @realtime_product_count, 
                                @realtime_pallet_count, 
                                @alert) ";
                    var result = _connection.Execute(sql, new
                    {
                        f_delivery_number_id = context.f_delivery_number_id,
                        delivery = context.delivery, 
                        material = context.material,
                        product_count = context.product_count,
                        pallet_count = context.pallet_count,
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
                            f_delivery_number_id, 
                            delivery, 
                            material, 
                            product_count, 
                            pallet_count, 
                            realtime_product_count, 
                            realtime_pallet_count, 
                            alert)  
                            VALUES( 
                            @f_delivery_number_id, 
                            @delivery, 
                            @material, 
                            @product_count, 
                            @pallet_count, 
                            @realtime_product_count, 
                            @realtime_pallet_count, 
                            @alert) ";
                var result = _connection.Execute(sql, new
                {
                    f_delivery_number_id = context.f_delivery_number_id,
                    delivery = context.delivery,
                    material = context.material,
                    product_count = context.product_count,
                    pallet_count = context.pallet_count,
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
                            f_delivery_number_id, 
                            f_cargo_data_info_id, 
                            f_epc_raw_id, 
                            f_epc_data_id, 
                            valid)  
                            VALUES( 
                            @f_delivery_number_id, 
                            @f_cargo_data_info_id, 
                            @f_epc_raw_id, 
                            @f_epc_data_id, 
                            @valid) ";
                var result = _connection.Execute(sql, new
                {
                    f_delivery_number_id = context.f_delivery_number_id,
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

        public bool UpdateDeliveryNumberContextWhenStart(DeliveryNumberDto dto)
        {
            try
            {
                var sql = @"UPDATE [scannel].[dbo].[eaton_delivery_number] 
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

        public bool UpdateDeliveryNumberContextWhenFinish(DeliveryNumberDto dto)
        {
            try
            {
                var sql = @"UPDATE [scannel].[dbo].[eaton_delivery_number] 
                            SET end_time=@end_time, state=@state, miss_pallet_quantity=@miss_pallet_quantity 
                            WHERE no=@no ";
                return _connection.Execute(sql, new
                {
                    end_time = dto.end_time,
                    miss_pallet_quantity = dto.miss_pallet_quantity,
                    state = dto.state,
                    no = dto.no,
                }) > 0;
            }
            catch (Exception exp)
            {
                return false;
            }
        }

        public bool UpdateDeliveryNumberContextWhenDataInserted(DeliveryNumberContext context)
        {
            try
            {
                var sql = @"UPDATE [scannel].[dbo].[eaton_delivery_number] 
                            SET valid_pallet_quantity=@valid_pallet_quantity, invalid_pallet_quantity=@invalid_pallet_quantity, miss_pallet_quantity=@miss_pallet_quantity 
                            WHERE id=@id ";
                return _connection.Execute(sql, new
                {
                    id = context.id,
                    valid_pallet_quantity = context.valid_pallet_quantity,
                    invalid_pallet_quantity = context.invalid_pallet_quantity,
                    miss_pallet_quantity = context.miss_pallet_quantity,
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

        public bool UpdateToDisableDeliveryNumberState(string no)
        {
            try
            {
                var sql = @"UPDATE [scannel].[dbo].[eaton_delivery_number] 
                            SET state=@state_disable 
                            WHERE no=@no ";
                return _connection.Execute(sql, new
                {
                    no = no,
                    state_disable = -2,
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
