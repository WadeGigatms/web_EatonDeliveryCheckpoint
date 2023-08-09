using EatonDeliveryCheckpoint.Database;
using EatonDeliveryCheckpoint.Database.Dapper;
using EatonDeliveryCheckpoint.Dtos;
using EatonDeliveryCheckpoint.Enums;
using EatonDeliveryCheckpoint.HttpClients;
using EatonDeliveryCheckpoint.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Services
{
    public class DeliveryService
    {
        public DeliveryService(ConnectionRepositoryManager connection, IMemoryCache memoryCache)
        {
            _localMemoryCache = new LocalMemoryCache(memoryCache);
            _connection = connection;
        }

        private readonly LocalMemoryCache _localMemoryCache;
        private readonly ConnectionRepositoryManager _connection;

        public IResultDto GetCargo()
        {
            // Read local memory
            var cacheDtos = _localMemoryCache.ReadDeliveryCargoDtos();
            var cacheCount = _localMemoryCache.ReadCargoNoCount();
            var dbCount = _connection.QueryCargoNoCount();
            if (dbCount == cacheCount)
            {
                return GetCargoResultDto(ResultEnum.True, ErrorEnum.None, cacheDtos);
            }

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

            // Save to local memory
            _localMemoryCache.SaveCargoNoCount(cargoDtos.Count());
            _localMemoryCache.SaveDeliveryCargoDtos(cargoDtos);

            return GetCargoResultDto(ResultEnum.True, ErrorEnum.None, cargoDtos);
        }

        public ResultDto PostFromTerminal(dynamic value)
        {
            DeliveryTerminalPostDto dto;
            bool result;

            // Check value
            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    MissingMemberHandling = MissingMemberHandling.Error
                };
                dto = JsonConvert.DeserializeObject<DeliveryTerminalPostDto>(value.ToString(), settings);
            }
            catch
            {
                return GetPostResultDto(ResultEnum.False, ErrorEnum.InvalidProperties);
            }

            // 
            DeliveryCargoDto deliveryingCargoDto = _localMemoryCache.ReadDeliveryingCargoDto();
            if (deliveryingCargoDto == null)
            {
                // get data from db
            }

            // Data received from epc server and go check for valid or not
            var datas = deliveryingCargoDto.datas;
            var materials = deliveryingCargoDto.datas.Select(data => data.material).ToList();
            if (materials.Contains(dto.pn)) 
            {
                var matchedData = datas.Where(data => data.material == dto.pn).First();
                if (matchedData.count < matchedData.realtime_product_count + dto.qty)
                {
                    // Alert quantity is out of target count
                    result = HttpClientManager.PostToTerminalReader(30, new int[] { 1 });
                    // Update cache
                }
                else
                {
                    matchedData.realtime_product_count += dto.qty;
                    matchedData.realtime_pallet_count += 1;
                    // Update cache
                    _localMemoryCache.SaveDeliveryingCargoDto(deliveryingCargoDto);
                    // Update database
                }
            } 
            else
            {
                // Alert pn is out of target material
                result = HttpClientManager.PostToTerminalReader(30, new int[] { 1 });
                // Update cache
                // Update database
            }

            return GetPostResultDto(ResultEnum.True, ErrorEnum.None);
        }

        public ResultDto PostToStart(dynamic value)
        {
            DeliveryCargoDto dto;
            bool result;

            // Check value
            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    MissingMemberHandling = MissingMemberHandling.Error
                };
                dto = JsonConvert.DeserializeObject<DeliveryCargoDto>(value.ToString(), settings);
            }
            catch (Exception exp)
            {
                return GetPostResultDto(ResultEnum.False, ErrorEnum.None);
            }

            // Update [eaton_delivery_cargo]
            UpdateDeliveryCargoDtoWhenStart(ref dto);
            result = _connection.UpdateDeliveryCargoWhenStart(dto);
            if (result == false)
            {
                return GetPostResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Save to local memory
            _localMemoryCache.SaveDeliveryingCargoDto(dto);


            return GetPostResultDto(ResultEnum.True, ErrorEnum.None);
        }

        public IResultDto PostToUploadFile(dynamic value)
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

            // Query [eaton_delivery_file]
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

            // Query [eaton_delivery_cargo]
            List<DeliveryCargoContext> insertedDeliveryCargoContexts = _connection.QueryDeliveryCargoContextsWithFileId(insertedDeliveryFileContext.id);
            if (insertedDeliveryCargoContexts == null)
            {
                return GetPostResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Insert into [eaton_cargo_data]
            List<CargoDataContext> insertCargoDataContexts = GetCargoDataContexts(insertedDeliveryCargoContexts, dto.FileData);
            result = _connection.InsertCargoDataContexts(insertCargoDataContexts);
            if (result == false)
            {
                return GetPostResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Query [eaton_cargo_data]
            List<int> cargoIds = insertCargoDataContexts.Select(c => c.f_delivery_cargo_id).ToList();
            List<CargoDataContext> insertedCargoDataContexts = _connection.QueryCargoDataContextsWithCargoId(cargoIds);
            if (insertedCargoDataContexts == null)
            {
                return GetPostResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Insert into [eaton_cargo_data_info]
            List<CargoDataInfoContext> insertCargoDataInfoContext = GetCargoDataInfoContexts(insertedCargoDataContexts);
            result = _connection.InsertCargoDataInfoContexts(insertCargoDataInfoContext);
            if (result == false)
            {
                return GetPostResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Query [eaton_cargo_data_info]
            List<CargoDataInfoContext> insertedCargoDataInfoContext = _connection.QueryCargoDataInfoContexts(cargoIds);
            if (insertedCargoDataInfoContext == null) 
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

        private List<CargoDataContext> GetCargoDataContexts(List<DeliveryCargoContext> deliveryCargoContexts, List<FileDto> fileData)
        {
            List<CargoDataContext> contexts = new List<CargoDataContext>();
            foreach(var data in fileData)
            {
                var cargoId = deliveryCargoContexts.Find(c => c.no == data.No).id;
                contexts.Add(new CargoDataContext
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

        private List<CargoDataInfoContext> GetCargoDataInfoContexts(List<CargoDataContext> insertedCargoDataContexts)
        {
            List<CargoDataInfoContext> contexts = new List<CargoDataInfoContext>();
            var groupedContexts = insertedCargoDataContexts.GroupBy(context => context.material).Select(context => new
            {
                f_delivery_cargo_id = context.Select(c => c.f_delivery_cargo_id).FirstOrDefault(),
                material = context.Key,
                count = context.Sum(d => d.quantity),
            }).ToList();
            foreach(var groupedContext in groupedContexts)
            {
                contexts.Add(new CargoDataInfoContext
                {
                    f_delivery_cargo_id = groupedContext.f_delivery_cargo_id,
                    material = groupedContext.material,
                    count = groupedContext.count,
                    realtime_product_count = 0,
                    realtime_pallet_count = 0,
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

        private void UpdateDeliveryCargoDtoWhenStart(ref DeliveryCargoDto dto)
        {
            DateTime datetime = DateTime.Now;
            dto.date = datetime.ToString("yyyy/MM/dd");
            dto.start_time = datetime.ToString("HH:mm:ss");
            dto.state = 0;
        }
    }
}
