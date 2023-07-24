using EatonDeliveryCheckpoint.Database;
using EatonDeliveryCheckpoint.Database.Dapper;
using EatonDeliveryCheckpoint.Dtos;
using EatonDeliveryCheckpoint.Enums;
using EatonDeliveryCheckpoint.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Services
{
    public class DeliveryService
    {
        public DeliveryService(ConnectionRepositoryManager connection)
        {
            _connection = connection;
        }

        private readonly ConnectionRepositoryManager _connection;

        public IResultDto GetFiles()
        {
            FileResultDto dto;

            // Query all files
            List<DeliveryFileDto> dto = _connection.QueryUploadedDeliveryFiles();

            return new FileResultDto
            {
                Result = true,
                Error = "",
                Files = null,
            };
        }

        public IResultDto Post(dynamic value)
        {
            DeliveryUploadPostDto dto;
            bool result;

            // Check value
            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    MissingMemberHandling = MissingMemberHandling.Error
                };
                dto = JsonConvert.DeserializeObject<DeliveryUploadPostDto>(value.ToString(), settings);
            }
            catch (Exception exp)
            {
                return GetPostResultDto(ResultEnum.False, ErrorEnum.InvalidProperties);
            }

            // Duplicated file
            DeliveryFileContext excelFileContextInDB = _connection.QueryDeliveryFileContextWithFileName(dto.FileName);
            if (excelFileContextInDB != null)
            {
                return GetPostResultDto(ResultEnum.False, ErrorEnum.DuplicatedFileName);
            }

            // Insert into [eaton_delivery_file]
            DeliveryFileContext insertDeliveryFileContext = GetDeliveryFileContext(dto);
            result = _connection.InsertDeliveryFileContext(insertDeliveryFileContext);
            if (result == false)
            {
                return GetPostResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Query inserted file
            DeliveryFileContext insertedDeliveryFileContext = _connection.QueryDeliveryFileContextWithFileName(dto.FileName);
            if (insertedDeliveryFileContext == null)
            {
                return GetPostResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Insert into [eaton_delivery_data]
            List<DeliveryFileDataContext> insertDeliveryDataContexts = GetDeliveryDataContexts(insertedDeliveryFileContext.id, dto.FileData);
            result =  _connection.InsertDeliveryDataContexts(insertDeliveryDataContexts);
            if (result == false)
            {
                return GetPostResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Query inserted data
            List<DeliveryFileDataContext> insertedDeliveryDataContexts = _connection.QueryDeliveryDataContextsWithFileName(dto.FileName);
            if (insertedDeliveryDataContexts == null)
            {
                return GetPostResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Insert into [eaton_delivery_work]
            List<DeliveryWorkContext> insertDeliveryWorkContexts = GetDeliveryWorkContexts(insertedDeliveryFileContext.id, dto.FileData);
            result = _connection.InsertDeliveryWorkContexts(insertDeliveryWorkContexts);
            if (result == false)
            {
                return GetPostResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Query inserted work
            List<DeliveryWorkContext> insertedDeliveryWorkContexts = _connection.QueryDeliveryWorkContextsWithFileName(dto.FileName);
            if (insertedDeliveryWorkContexts == null)
            {
                return GetPostResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            return GetPostResultDto(ResultEnum.True, ErrorEnum.None);
        }

        private ResultDto GetPostResultDto(ResultEnum result, ErrorEnum error)
        {
            return new ResultDto
            {
                Result = result.ToBoolean(),
                Error = error.ToChineseDescription()
            };
        }

        private DeliveryFileContext GetDeliveryFileContext(DeliveryUploadPostDto dto)
        {
            return new DeliveryFileContext
            {
                name = dto.FileName,
                upload_timestamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                json = JsonConvert.SerializeObject(dto),
            };
        }

        private DeliveryWorkContext GetDeliveryWorkContext(int fileId)
        {
            return new DeliveryWorkContext
            {
                f_delivery_file_id = fileId,
                f_delivery_file_data_ids = "",
                start_timestamp = null,
                end_timestamp = null,
                duration = TimeSpan.Zero.ToString(),
                valid_pallet_quantity = 0,
                invalid_pallet_quantity = 0,
                state = 0,
            };
        }

        private List<DeliveryFileDataContext> GetDeliveryDataContexts(int fileId, List<FileDto> fileDtos)
        {
            List<DeliveryFileDataContext> contexts = new List<DeliveryFileDataContext>();
            foreach(var fileDto in fileDtos)
            contexts.Add(new DeliveryFileDataContext
            {
                f_delivery_file_id = fileId,
                delivery = fileDto.Delivery,
                item = fileDto.Item ?? "",
                material = fileDto.Material,
                quantity = fileDto.Quantity,
                no = fileDto.No
            });
            return contexts;
        }

        private List<DeliveryWorkContext> GetDeliveryWorkContexts(int fileId, List<FileDto> fileDtos)
        {
            List<DeliveryWorkContext> contexts = new List<DeliveryWorkContext>();
            List<string> cargoNos = new List<string>();
            var file_data_ids = "";
            for (var i = 0; i < fileDtos.Count; i++)
            {
                var no = fileDtos[i].No;
                if (!cargoNos.Contains(no))
                {
                    cargoNos.Add(no);
                    file_data_ids += (i == 0 ? $"{no}" : $",{no}");
                    contexts.Add(new DeliveryWorkContext
                    {
                        f_delivery_file_id = fileId,
                        f_delivery_file_data_ids = file_data_ids,
                        no = no,
                        start_timestamp = null,
                        end_timestamp = null,
                        duration = TimeSpan.Zero.ToString(),
                        valid_pallet_quantity = 0,
                        invalid_pallet_quantity = 0,
                        state = 0,
                    });
                }
            }
            return contexts;
        }
    }
}
