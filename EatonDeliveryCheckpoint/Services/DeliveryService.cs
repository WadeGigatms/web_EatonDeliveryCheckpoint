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

        public IResultDto GetCargo()
        {
            // Query did not terminate cargo no
            List<DeliveryCargoDto> cargoDtos = _connection.QueryDeliveryCargoDtos();
            if (cargoDtos == null)
            {
                return GetCargoResultDto(ResultEnum.True, ErrorEnum.None, null);
            }

            // Query cargo data matches cargo no
            foreach(var cargoDto in cargoDtos)
            {
                List<DeliveryCargoDataDto> cargoDataDtos = _connection.QueryDeliveryCargoDataDtos(cargoDto.no);
                if (cargoDataDtos == null)
                {
                    return GetCargoResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase, null);
                }
                cargoDto.datas = cargoDataDtos;
            }

            return GetCargoResultDto(ResultEnum.True, ErrorEnum.None, cargoDtos);
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

            // Check for duplicated file
            DeliveryFileContext existDeliveryFileContext = _connection.QueryDeliveryFileContextWithFileName(dto.FileName);
            if (existDeliveryFileContext != null)
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

            // Insert into [eaton_delivery_cargo]
            List<DeliveryCargoContext> insertDeliveryCargoContexts = GetDeliveryCargoContexts(insertedDeliveryFileContext.id, dto.FileData);
            result = _connection.InsertDeliveryCargoContexts(insertDeliveryCargoContexts);
            if (result == false)
            {
                return GetPostResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Query inserted cargo
            List<DeliveryCargoContext> insertedDeliveryCargoContexts = _connection.QueryDeliveryCargoContextsWithFileId(insertedDeliveryFileContext.id);
            if (insertedDeliveryCargoContexts == null)
            {
                return GetPostResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Insert into [eaton_delivery_cargo_data]
            List<DeliveryCargoDataContext> insertDeliveryCargoDataContexts = GetDeliveryCargoDataContexts(insertedDeliveryCargoContexts, dto.FileData);
            result = _connection.InsertDeliveryCargoDataContexts(insertDeliveryCargoDataContexts);
            if (result == false)
            {
                return GetPostResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Query inserted cargo data
            List<int> cargoIds = insertDeliveryCargoDataContexts.Select(c => c.f_delivery_cargo_id).ToList();
            List<DeliveryCargoDataContext> insertedDeliveryCargoDataContexts = _connection.QueryDeliveryCargoDataContextsWithCargoId(cargoIds);
            if (insertedDeliveryCargoDataContexts == null)
            {
                return GetPostResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Insert into [eaton_delivery_cargo_data_realtime]
            List<int> cargoDataIds = insertedDeliveryCargoDataContexts.Select(c => c.id).ToList();
            List<DeliveryCargoDataRealtimeContext> insertDeliveryCargoRealtimeContexts = GetDeliveryCargoRealtimeContexts(cargoDataIds);
            result = _connection.InsertDeliveryCargoDataRealtimeContexts(insertDeliveryCargoRealtimeContexts);
            if (result == false)
            {
                return GetPostResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Query insert cargo data realtime
            List<DeliveryCargoDataRealtimeContext> insertedDeliveryCargoDataRealtimeContexts = _connection.QueryDeliveryCargoDataRealtimeContextsWithCargoDataIds(cargoDataIds);
            if (insertedDeliveryCargoDataRealtimeContexts == null)
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

        private List<DeliveryCargoContext> GetDeliveryCargoContexts(int id, List<FileDto> fileData)
        {
            List<DeliveryCargoContext> contexts = new List<DeliveryCargoContext>();
            var files = fileData.GroupBy(file => file.No).Select(group => group.ToList()).ToList();
            foreach(var file in files)
            {
                int productCount = file.Sum(f => int.Parse(f.Quantity));
                int materialCount = file.GroupBy(f => f.Material).Count();
                contexts.Add(new DeliveryCargoContext
                {
                    f_delivery_file_id = id,
                    no = file.First().No,
                    material_quantity = materialCount,
                    product_quantity = productCount,
                    date = null,
                    start_time = null,
                    end_time = null,
                    duration = null,
                    valid_pallet_quantity = 0,
                    invalid_pallet_quantity = 0,
                    pallet_rate = 0,
                    state = -1,
                });
            }
            return contexts;
        }

        private List<DeliveryCargoDataContext> GetDeliveryCargoDataContexts(List<DeliveryCargoContext> deliveryCargoContexts, List<FileDto> fileData)
        {
            List<DeliveryCargoDataContext> contexts = new List<DeliveryCargoDataContext>();
            foreach(var data in fileData)
            {
                var cargoId = deliveryCargoContexts.Find(c => c.no == data.No).id;
                contexts.Add(new DeliveryCargoDataContext
                {
                    f_delivery_cargo_id = cargoId,
                    delivery = data.Delivery,
                    item = data.Item,
                    material = data.Material,
                    quantity = int.Parse(data.Quantity),
                });
            }
            return contexts;
        }

        private List<DeliveryCargoDataRealtimeContext> GetDeliveryCargoRealtimeContexts(List<int> dataIds)
        {
            List<DeliveryCargoDataRealtimeContext> contexts = new List<DeliveryCargoDataRealtimeContext>();
            foreach(var dataId in dataIds)
            {
                contexts.Add(new DeliveryCargoDataRealtimeContext
                {
                    f_delivery_cargo_data_id = dataId,
                    product_quantity = 0,
                    pallet_quantity = 0,
                });
            }
            return contexts;
        }

        private CargoResultDto GetCargoResultDto(ResultEnum result, ErrorEnum error, List<DeliveryCargoDto> dto)
        {
            return new CargoResultDto
            {
                Result = result.ToBoolean(),
                Error = error.ToChineseDescription(),
                CargoNos = dto,
            };
        }

        /*
        private List<DeliveryWorkContext> GetDeliveryWorkContexts(int fileId, List<FileDto> fileDtos)
        {
            List<DeliveryWorkContext> contexts = new List<DeliveryWorkContext>();
            List<string> cargoNos = new List<string>();
            for (var i = 0; i < fileDtos.Count; i++)
            {
                var no = fileDtos[i].No;
                if (!cargoNos.Contains(no))
                {
                    cargoNos.Add(no);
                    contexts.Add(new DeliveryWorkContext
                    {
                        f_delivery_file_id = fileId,
                        no = no,
                        start_timestamp = null,
                        end_timestamp = null,
                        duration = TimeSpan.Zero.ToString(),
                        valid_pallet_quantity = 0,
                        invalid_pallet_quantity = 0,
                        pallet_rate = 0,
                        did_terminate = false,
                    });
                }
            }
            return contexts;
        }

        private List<DeliveryFileDataContext> GetDeliveryFileDataContexts(int fileId, List<FileDto> fileDtos, List<DeliveryWorkContext> workContexts)
        {
            List<DeliveryFileDataContext> contexts = new List<DeliveryFileDataContext>();
            foreach(var fileDto in fileDtos)
            {
                var workContext = workContexts.Find(workContext => workContext.no == fileDto.No);
                if (workContext!= null)
                {
                    contexts.Add(new DeliveryFileDataContext
                    {
                        f_delivery_file_id = fileId,
                        f_delivery_work_id = workContext.id,
                        delivery = fileDto.Delivery,
                        item = fileDto.Item ?? "",
                        material = fileDto.Material,
                        quantity = fileDto.Quantity,
                        no = fileDto.No
                    });
                }
            }
            return contexts;
        }
        */
    }
}
