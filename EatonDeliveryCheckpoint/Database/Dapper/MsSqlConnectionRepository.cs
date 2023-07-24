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

        #region Query uploaded files

        public List<DeliveryFileDto> QueryUploadedDeliveryFiles()
        {
            try
            {
                // Work
                var workSql = @"SELECT 
                                w.f_delivery_file_id, 
                                w.f_delivery_file_data_ids, 
                                w.no, 
                                w.start_timestamp, 
                                w.end_timestamp, 
                                w.duration, 
                                w.valid_pallet_quantity, 
                                w.invalid_pallet_quantity, 
                                w.state 
                                FROM [scannel].[dbo].[eaton_delivery_file] AS f 
                                INNER JOIN [scannel].[dbo].[eaton_delivery_work] AS w 
                                ON w.f_delivery_file_id=f.id 
                                WHERE w.state=@state 
                                ORDER BY w.no ";
                var workContexts = _connection.Query<DeliveryWorkContext>(workSql, new
                {
                    state = 0,
                }).ToList();

                // File
                var fileSql = @"SELECT 
                                f.name, 
                                f.upload_timestamp, 
                                f.no 
                                FROM [scannel].[dbo].[eaton_delivery_file] AS f 
                                INNER JOIN [scannel].[dbo].[eaton_delivery_work] AS w 
                                ON w.f_delivery_file_id=f.id 
                                WHERE w.state=@state 
                                ORDER BY w.no ";
                var fileContexts = _connection.Query<DeliveryWorkContext>(fileSql, new
                {
                    state = 0,
                }).ToList();

                // Return dto
                List<DeliveryFileDto> dtos = new List<DeliveryFileDto>();
                foreach (var fileContext in fileContexts)
                {
                    var workContext = workContexts.Select(context => context.f_delivery_file_id == fileContext.id);
                    dtos.Add(new DeliveryFileDto
                    {
                        File = fileContext,
                        FileData = workContext,
                    });
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Query

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

        public List<DeliveryFileDataContext> QueryDeliveryDataContextsWithFileName(string name)
        {
            try
            {
                var sql = @"SELECT 
                            d.f_delivery_file_id, 
                            d.delivery, 
                            d.item, 
                            d.material, 
                            d.quantity, 
                            d.no 
                            FROM [scannel].[dbo].[eaton_delivery_data] AS d 
                            INNER JOIN [scannel].[dbo].[eaton_delivery_file] AS f 
                            ON d.f_delivery_file_id=f.id 
                            WHERE f.name=@name ";
                return _connection.Query<DeliveryFileDataContext>(sql, new
                {
                    name = name,
                }).ToList();
            }
            catch
            {
                return null;
            }
        }

        public List<DeliveryWorkContext> QueryDeliveryWorkContextsWithFileName(string name)
        {
            try
            {
                var sql = @"SELECT 
                            w.f_delivery_file_id, 
                            w.f_delivery_file_data_ids, 
                            w.no, 
                            w.start_timestamp, 
                            w.end_timestamp, 
                            w.duration, 
                            w.valid_pallet_quantity, 
                            w.invalid_pallet_quantity, 
                            w.state 
                            FROM [scannel].[dbo].[eaton_delivery_work] AS w 
                            INNER JOIN [scannel].[dbo].[eaton_delivery_file] AS f 
                            ON w.f_delivery_file_id=f.id 
                            WHERE f.name=@name ";
                return _connection.Query<DeliveryWorkContext>(sql, new
                {
                    name = name,
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

        public bool InsertDeliveryDataContexts(List<DeliveryFileDataContext> contexts)
        {
            try
            {
                foreach(var context in contexts)
                {
                    var sql = @"INSERT INTO [scannel].[dbo].[eaton_delivery_data](f_delivery_file_id, delivery, item, material, quantity, no) 
                                VALUES(@f_delivery_file_id, @delivery, @item, @material, @quantity, @no) ";
                    var result = _connection.Execute(sql, new
                    {
                        f_delivery_file_id = context.f_delivery_file_id,
                        delivery = context.delivery,
                        item = context.item,
                        material = context.material,
                        quantity = context.quantity,
                        no = context.no,
                    }) > 0;
                    if (!result) { return false; }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool InsertDeliveryWorkContexts(List<DeliveryWorkContext> contexts)
        {
            try
            {
                foreach (var context in contexts)
                {
                    var sql = @"INSERT INTO [scannel].[dbo].[eaton_delivery_work](f_delivery_file_id, f_delivery_file_data_ids, no, start_timestamp, end_timestamp, duration, valid_pallet_quantity, invalid_pallet_quantity, state) 
                                VALUES(@f_delivery_file_id, @f_delivery_file_data_ids, @no, @start_timestamp, @end_timestamp, @duration, @valid_pallet_quantity, @invalid_pallet_quantity, @state) ";
                    var result = _connection.Execute(sql, new
                    {
                        f_delivery_file_id = context.f_delivery_file_id,
                        f_delivery_file_data_ids = context.f_delivery_file_data_ids,
                        no = context.no,
                        start_timestamp = context.start_timestamp,
                        end_timestamp = context.end_timestamp,
                        duration = context.duration,
                        valid_pallet_quantity = context.valid_pallet_quantity,
                        invalid_pallet_quantity = context.invalid_pallet_quantity,
                        state = context.state,
                    }) > 0;
                    if (!result) { return false; }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
