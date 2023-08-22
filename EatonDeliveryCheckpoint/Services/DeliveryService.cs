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
using System.Net.Http;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Services
{
    public class DeliveryService
    {
        public DeliveryService(ConnectionRepositoryManager connection, IMemoryCache memoryCache, IHttpClientFactory httpClientFactory)
        {
            _localMemoryCache = new LocalMemoryCache(memoryCache);
            _connection = connection;
            _httpClientManager = new HttpClientManager(httpClientFactory);
        }

        private readonly HttpClientManager _httpClientManager;
        private readonly LocalMemoryCache _localMemoryCache;
        private readonly ConnectionRepositoryManager _connection;

        #region Public methods

        public DeliveryCargoResultDto GetSearch(string no)
        {
            // Check value
            if (string.IsNullOrEmpty(no))
            {
                return GetDeliveryCargoResultDto(ResultEnum.False, ErrorEnum.InvalidProperties, null);
            }

            DeliveryCargoDto deliveryCargoDto = _connection.QueryDeliveryCargoDtoWithNo(no);
            if (deliveryCargoDto == null)
            {
                return GetDeliveryCargoResultDto(ResultEnum.False, ErrorEnum.InvalidProperties, null);
            }
            deliveryCargoDto.datas = new List<DeliveryCargoDataDto>();
            List<DeliveryCargoDataDto> validDeliveryCargoDataDtos = _connection.QueryValidDeliveryCargoDataDtos(no);
            if (validDeliveryCargoDataDtos != null)
            {
                for(var i = 0; i < validDeliveryCargoDataDtos.Count(); i ++)
                {
                    deliveryCargoDto.datas.Add(validDeliveryCargoDataDtos[i]);
                }
            }
            List<DeliveryCargoDataDto> invalidDeliveryCargoDataDtos = _connection.QueryInvalidDeliveryCargoDataDtos(no);
            if (invalidDeliveryCargoDataDtos != null)
            {
                for (var i = 0; i < invalidDeliveryCargoDataDtos.Count(); i++)
                {
                    deliveryCargoDto.datas.Add(invalidDeliveryCargoDataDtos[i]);
                }
            }

            List<DeliveryCargoDto> deliveryCargoDtos = new List<DeliveryCargoDto>();
            deliveryCargoDtos.Add(deliveryCargoDto);

            return GetDeliveryCargoResultDto(ResultEnum.True, ErrorEnum.None, deliveryCargoDtos);
        }

        public IResultDto GetDnList()
        {
            // Examine for any changes
            List<DeliveryCargoDto> cacheDeliveryCargoDtos = _localMemoryCache.ReadDeliveryCargoDtos();
            int count = _connection.QueryDeliveryCargoCount();
            if (cacheDeliveryCargoDtos != null && cacheDeliveryCargoDtos.Count() == count)
            {
                bool cacheDidChange = _localMemoryCache.ReadCacheDidChange();
                if (cacheDidChange == false)
                {
                    return GetDeliveryCargoResultDto(ResultEnum.True, ErrorEnum.None, cacheDeliveryCargoDtos);
                }
            }

            // Query DeliveryCargoDtos which did not finish
            List<DeliveryCargoDto> deliveryCargoDtos = _connection.QueryDeliveryCargoDtos();
            if (deliveryCargoDtos == null)
            {
                // No available data in database
                return GetDeliveryCargoResultDto(ResultEnum.True, ErrorEnum.None, null);
            }

            // Query DeliveryCargoDataDto matches no
            foreach (var deliveryCargoDto in deliveryCargoDtos)
            {
                List<DeliveryCargoDataDto> deliveryCargoDataDtos = _connection.QueryDeliveryCargoDataDtos(deliveryCargoDto.no);
                if (deliveryCargoDataDtos == null)
                {
                    // No available data in database
                    return GetDeliveryCargoResultDto(ResultEnum.False, ErrorEnum.None, null);
                }
                deliveryCargoDto.datas = deliveryCargoDataDtos;
            }

            // Update cache
            DeliveryCargoDto deliveryingCargoDto = deliveryCargoDtos.Where(dto => dto.state == 0).FirstOrDefault();
            if (deliveryingCargoDto != null)
            {
                _localMemoryCache.SaveDeliveryingCargoDto(deliveryingCargoDto);
            }
            else
            {
                _localMemoryCache.DeleteDeliveryingCargoDto();
            }
            _localMemoryCache.SaveDeliveryCargoDtos(deliveryCargoDtos);
            _localMemoryCache.SaveCacheDidChange(false);

            return GetDeliveryCargoResultDto(ResultEnum.True, ErrorEnum.None, deliveryCargoDtos);
        }

        public IResultDto PostToDismissAlert(dynamic value)
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
                return GetPostResultDto(ResultEnum.False, ErrorEnum.InvalidProperties);
            }

            // Any available deliveryingCargoDto in cache
            DeliveryCargoDto deliveryingCargoDto = _localMemoryCache.ReadDeliveryingCargoDto();
            if (deliveryingCargoDto == null && deliveryingCargoDto.no != dto.no)
            {
                // Get DeliveryingCargoDto from database
                deliveryingCargoDto = _connection.QueryDeliveryCargoDtosWithState(0).FirstOrDefault();
                if (deliveryingCargoDto == null)
                {
                    // Maybe no dn is selected and some pallet had been moved to terminal
                    // So return and no update and insert data into database
                    return GetPostResultDto(ResultEnum.True, ErrorEnum.None);
                }

                // Get DeliveryCargoDataDtos of deliveryingCargoDto from database
                deliveryingCargoDto.datas = _connection.QueryDeliveryCargoDataDtos(deliveryingCargoDto.no);

                // Save to cache
                _localMemoryCache.SaveDeliveryingCargoDto(deliveryingCargoDto);
                List<DeliveryCargoDto> updatedDeliveryCargoDtos = UpdateCacheDeliveryCargoDtos(deliveryingCargoDto);
                _localMemoryCache.SaveDeliveryCargoDtos(updatedDeliveryCargoDtos);
            }

            // Get invalid data from deliveryingCargoDto
            DeliveryCargoDataDto alertDeliveryCargoDataDto = deliveryingCargoDto.datas.Where(data => data.alert == 1).FirstOrDefault();
            if (alertDeliveryCargoDataDto != null)
            {
                if (alertDeliveryCargoDataDto.count > -1 &&
                    alertDeliveryCargoDataDto.count < alertDeliveryCargoDataDto.realtime_product_count)
                {
                    // Over qty
                    UpdateDeliveryCargoDataDtoToRemoveInvalidQty(ref deliveryingCargoDto, alertDeliveryCargoDataDto);

                    // Update deliveryingCargoDto and deliveryCargoDtos in cache
                    _localMemoryCache.SaveDeliveryingCargoDto(deliveryingCargoDto);
                    List<DeliveryCargoDto> updatedDeliveryCargoDtos = UpdateCacheDeliveryCargoDtos(deliveryingCargoDto);
                    _localMemoryCache.SaveDeliveryCargoDtos(updatedDeliveryCargoDtos);

                    // Update database
                    DeliveryCargoContext deliveryCargoContext = _connection.QueryDeliveryCargoContextWithNo(deliveryingCargoDto.no);
                    CargoDataInfoContext cargoDataInfoContext = _connection.QueryCargoDataInfoContextWithMaterial(deliveryCargoContext.id, alertDeliveryCargoDataDto.material);
                    int qty = _connection.QueryQtyByCargoDataRecordContextWithInfoIds(cargoDataInfoContext.f_delivery_cargo_id, cargoDataInfoContext.id);
                    UpdateCargoDataInfoContextForRealtimeToRemoveAlert(ref cargoDataInfoContext, qty);
                    result = _connection.UpdateCargoDataInfoContext(cargoDataInfoContext);
                }
                else if (alertDeliveryCargoDataDto.count == -1)
                {
                    // Miss match material
                    UpdateDeliveryCargoDataDtoToRemoveInvalidMaterial(ref deliveryingCargoDto, alertDeliveryCargoDataDto);

                    // Update deliveryingCargoDto and deliveryCargoDtos in cache
                    _localMemoryCache.SaveDeliveryingCargoDto(deliveryingCargoDto);
                    List<DeliveryCargoDto> updatedDeliveryCargoDtos = UpdateCacheDeliveryCargoDtos(deliveryingCargoDto);
                    _localMemoryCache.SaveDeliveryCargoDtos(updatedDeliveryCargoDtos);

                    // Update database
                    DeliveryCargoContext deliveryCargoContext = _connection.QueryDeliveryCargoContextWithNo(deliveryingCargoDto.no);
                    CargoDataInfoContext cargoDataInfoContext = _connection.QueryCargoDataInfoContextWithMaterial(deliveryCargoContext.id, alertDeliveryCargoDataDto.material);
                    UpdateCargoDataInfoContextForRealtimeToRemoveAlert(ref cargoDataInfoContext);
                    result = _connection.UpdateCargoDataInfoContext(cargoDataInfoContext);
                }
            } 
            else
            {

            }

            // Dismiss alert trigger
            _httpClientManager.PostToNoTriggeTerminalReader();

            _localMemoryCache.SaveCacheDidChange(true);

            return GetPostResultDto(ResultEnum.True, ErrorEnum.None);
        }

        public IResultDto PostFromEpcServer(dynamic value)
        {
            // Called by epc server when epc server is called from teminal reader
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

            // Any available deliveryingCargoDto in cache
            DeliveryCargoDto deliveryingCargoDto = _localMemoryCache.ReadDeliveryingCargoDto();
            if (deliveryingCargoDto == null)
            {
                // Get DeliveryingCargoDto from database
                deliveryingCargoDto = _connection.QueryDeliveryCargoDtosWithState(0).FirstOrDefault();
                if (deliveryingCargoDto == null)
                {
                    // Maybe no dn is selected and some pallet had been moved to terminal
                    // So return and no update and insert data into database
                    return GetPostResultDto(ResultEnum.True, ErrorEnum.None);
                }

                // Get DeliveryCargoDataDtos of deliveryingCargoDto from database
                deliveryingCargoDto.datas = _connection.QueryDeliveryCargoDataDtos(deliveryingCargoDto.no);

                // Save to cache
                _localMemoryCache.SaveDeliveryingCargoDto(deliveryingCargoDto);
                List<DeliveryCargoDto> updatedDeliveryCargoDtos = UpdateCacheDeliveryCargoDtos(deliveryingCargoDto);
                _localMemoryCache.SaveDeliveryCargoDtos(updatedDeliveryCargoDtos);
            }

            // Data received from epc server
            // Examine epc for valid or invalid
            DeliveryCargoDataDto matchedMaterialDeliveryCargoDataDto = deliveryingCargoDto.datas.Where(data => data.material == dto.pn).ToList().FirstOrDefault();
            if (matchedMaterialDeliveryCargoDataDto != null) 
            {
                // Valid material

                if (matchedMaterialDeliveryCargoDataDto.count < matchedMaterialDeliveryCargoDataDto.realtime_product_count + dto.qty)
                {
                    // [ERROR]
                    // Update deliveryingCargoDto and deliveryCargoDtos in cache
                    UpdateDeliveryCargoDtoWithInvalidPallet(ref deliveryingCargoDto);
                    UpdateDeliveryCargoDataDtoForRealtimeWithInvalidData(ref matchedMaterialDeliveryCargoDataDto, dto.qty);
                    _localMemoryCache.SaveDeliveryingCargoDto(deliveryingCargoDto);
                    List<DeliveryCargoDto> updatedDeliveryCargoDtos = UpdateCacheDeliveryCargoDtos(deliveryingCargoDto);
                    _localMemoryCache.SaveDeliveryCargoDtos(updatedDeliveryCargoDtos);

                    // Update database
                    DeliveryCargoContext deliveryCargoContext = _connection.QueryDeliveryCargoContextWithNo(deliveryingCargoDto.no);
                    deliveryCargoContext.invalid_pallet_quantity += 1;
                    result = _connection.UpdateDeliveryCargoContextWhenDataInserted(deliveryCargoContext);

                    CargoDataInfoContext cargoDataInfoContext = _connection.QueryCargoDataInfoContextWithMaterial(deliveryCargoContext.id, dto.pn);
                    UpdateCargoDataInfoContextForRealtimeWithInvalidData(ref cargoDataInfoContext, dto.qty);
                    result = _connection.UpdateCargoDataInfoContext(cargoDataInfoContext);

                    CargoDataRecordContext insertCargoDataRecordContext = GetCargoDataRecordContext(dto, cargoDataInfoContext.f_delivery_cargo_id, cargoDataInfoContext.id, false);
                    result = _connection.InsertCargoDataRecordContext(insertCargoDataRecordContext);

                    // Alert quantity is out of target count
                    result = _httpClientManager.PostToTriggerTerminalReader();
                }
                else
                {
                    // [OK]
                    // Update deliveryingCargoDto and deliveryCargoDtos in cache
                    UpdateDeliveryCargoDtoWithValidQty(ref deliveryingCargoDto);
                    UpdateDeliveryCargoDataDtoForRealtime(ref matchedMaterialDeliveryCargoDataDto, dto.qty);
                    _localMemoryCache.SaveDeliveryingCargoDto(deliveryingCargoDto);
                    List<DeliveryCargoDto> updatedDeliveryCargoDtos = UpdateCacheDeliveryCargoDtos(deliveryingCargoDto);
                    _localMemoryCache.SaveDeliveryCargoDtos(updatedDeliveryCargoDtos);

                    // Update database
                    DeliveryCargoContext deliveryCargoContext = _connection.QueryDeliveryCargoContextWithNo(deliveryingCargoDto.no);
                    deliveryCargoContext.valid_pallet_quantity += 1;
                    result = _connection.UpdateDeliveryCargoContextWhenDataInserted(deliveryCargoContext);

                    CargoDataInfoContext cargoDataInfoContext = _connection.QueryCargoDataInfoContextWithMaterial(deliveryCargoContext.id, dto.pn);
                    UpdateCargoDataInfoContextForRealtime(ref cargoDataInfoContext, dto.qty);
                    result = _connection.UpdateCargoDataInfoContext(cargoDataInfoContext);

                    CargoDataRecordContext insertCargoDataRecordContext = GetCargoDataRecordContext(dto, cargoDataInfoContext.f_delivery_cargo_id, cargoDataInfoContext.id, true);
                    result = _connection.InsertCargoDataRecordContext(insertCargoDataRecordContext);
                }
            } 
            else
            {
                // Invalid material

                // [ERROR]
                // Update deliveryingCargoDto and deliveryCargoDtos in cache
                UpdateDeliveryCargoDtoWithInvalidPallet(ref deliveryingCargoDto);
                InsertDeliveryCargoDataDtoWithInvalidData(ref deliveryingCargoDto, dto);
                _localMemoryCache.SaveDeliveryingCargoDto(deliveryingCargoDto);
                List<DeliveryCargoDto> updatedDeliveryCargoDtos = UpdateCacheDeliveryCargoDtos(deliveryingCargoDto);
                _localMemoryCache.SaveDeliveryCargoDtos(updatedDeliveryCargoDtos);

                // Update database
                DeliveryCargoContext deliveryCargoContext =  _connection.QueryDeliveryCargoContextWithNo(deliveryingCargoDto.no);
                deliveryCargoContext.invalid_pallet_quantity += 1;
                result = _connection.UpdateDeliveryCargoContextWhenDataInserted(deliveryCargoContext);

                CargoDataInfoContext invalidCargoDataInfoContext = GetInvalidCargoDataInfoContext(dto, deliveryCargoContext.id);
                result = _connection.InsertCargoDataInfoContext(invalidCargoDataInfoContext);
                CargoDataInfoContext insertedCargoDataInfoContext = _connection.QueryCargoDataInfoContextWithInvalidContext(invalidCargoDataInfoContext);

                CargoDataRecordContext insertCargoDataRecordContext = GetCargoDataRecordContext(dto, deliveryCargoContext.id, insertedCargoDataInfoContext.id, false);
                result = _connection.InsertCargoDataRecordContext(insertCargoDataRecordContext);

                // Alert pn is out of target material
                result = _httpClientManager.PostToTriggerTerminalReader();
            }

            _localMemoryCache.SaveCacheDidChange(true);

            return GetPostResultDto(ResultEnum.True, ErrorEnum.None);
        }

        public IResultDto PostToStart(dynamic value)
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
                return GetPostResultDto(ResultEnum.False, ErrorEnum.InvalidProperties);
            }

            // Update [eaton_delivery_cargo] in database
            UpdateDeliveryCargoDtoWhenStart(ref dto);
            result = _connection.UpdateDeliveryCargoContextWhenStart(dto);
            if (result == false)
            {
                return GetPostResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Save to cache
            // _localMemoryCache.SaveDeliveryingCargoDto(dto);
            // List<DeliveryCargoDto> updatedDeliveryCargoDtos = UpdateCacheDeliveryCargoDtos(dto);
            // _localMemoryCache.SaveDeliveryCargoDtos(updatedDeliveryCargoDtos);
            _localMemoryCache.SaveCacheDidChange(true);

            return GetPostResultDto(ResultEnum.True, ErrorEnum.None);
        }

        public IResultDto PostToFinish(dynamic value)
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
                return GetPostResultDto(ResultEnum.False, ErrorEnum.InvalidProperties);
            }

            // Update [eaton_delivery_cargo] in database
            UpdateDeliveryCargoDtoWhenFinish(ref dto);
            result = _connection.UpdateDeliveryCargoContextWhenFinish(dto);
            if (result == false)
            {
                return GetPostResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Save to cache
            // _localMemoryCache.DeleteDeliveryingCargoDto();
            _localMemoryCache.SaveCacheDidChange(true);

            return GetPostResultDto(ResultEnum.True, ErrorEnum.None);
        }

        public ResultDto PostToQuit(dynamic value)
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
                return GetPostResultDto(ResultEnum.False, ErrorEnum.InvalidProperties);
            }

            // Update database
            UpdateDeliveryCargoDtoWhenFinish(ref dto);
            result = _connection.UpdateDeliveryCargoContextWhenFinish(dto);

            // Update cache
            _localMemoryCache.SaveCacheDidChange(true);

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

            _localMemoryCache.SaveCacheDidChange(true);

            return GetPostResultDto(ResultEnum.True, ErrorEnum.None);
        }

        #endregion

        #region Private methods

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
                    start_time = "",
                    end_time = "",
                    valid_pallet_quantity = 0,
                    invalid_pallet_quantity = 0,
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
                    alert = 0,
                });
            }
            return contexts;
        }

        private CargoDataInfoContext GetInvalidCargoDataInfoContext(DeliveryTerminalPostDto dto, int id)
        {
            return new CargoDataInfoContext
            {
                f_delivery_cargo_id = id,
                material = dto.pn,
                count = -1,
                realtime_product_count = dto.qty,
                realtime_pallet_count = 1,
                alert = 1,
            };
        }

        private DeliveryCargoResultDto GetDeliveryCargoResultDto(ResultEnum result, ErrorEnum error, List<DeliveryCargoDto> dto)
        {
            return new DeliveryCargoResultDto
            {
                Result = result.ToBoolean(),
                Error = error.ToChineseDescription(),
                deliveryCargoDtos = dto,
            };
        }

        private void UpdateDeliveryCargoDtoWhenStart(ref DeliveryCargoDto dto)
        {
            dto.start_time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            dto.state = 0;
        }

        private void UpdateDeliveryCargoDtoWhenFinish(ref DeliveryCargoDto dto)
        {
            dto.end_time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            dto.state = 1;
        }

        private CargoDataRecordContext GetCargoDataRecordContext(DeliveryTerminalPostDto dto,int f_delivery_cargo_id, int f_cargo_data_info_id, bool valid)
        {
            return new CargoDataRecordContext()
            {
                f_delivery_cargo_id = f_delivery_cargo_id,
                f_cargo_data_info_id = f_cargo_data_info_id,
                f_epc_raw_id = dto.epc_raw_id,
                f_epc_data_id = dto.epc_data_id,
                valid = valid ? 1 : 0,
            };
        }

        private DeliveryCargoDataDto GetDeliveryCargoDataDto(DeliveryTerminalPostDto dto, int count, int alert)
        {
            return new DeliveryCargoDataDto
            {
                material = dto.pn,
                count = count, // -1: miss match material
                realtime_product_count = dto.qty,
                realtime_pallet_count = 1,
                alert = alert, // 0: close, 1: invalid for both miss match material and over qty
            };
        }

        private void UpdateDeliveryCargoDtoWithValidQty(ref DeliveryCargoDto dto)
        {
            dto.valid_pallet_quantity += 1;
        }

        private void UpdateDeliveryCargoDtoWithInvalidPallet(ref DeliveryCargoDto dto)
        {
            dto.invalid_pallet_quantity += 1;
        }

        private void UpdateDeliveryCargoDataDtoForRealtime(ref DeliveryCargoDataDto dto, int qty)
        {
            dto.realtime_product_count += qty;
            dto.realtime_pallet_count += 1;
        }

        private void UpdateDeliveryCargoDataDtoForRealtimeWithInvalidData(ref DeliveryCargoDataDto dto, int qty)
        {
            dto.realtime_product_count += qty;
            dto.realtime_pallet_count += 1;
            dto.alert = 1;
        }

        private void InsertDeliveryCargoDataDtoWithInvalidData(ref DeliveryCargoDto dto, DeliveryTerminalPostDto postDto)
        {
            dto.datas.Add(new DeliveryCargoDataDto
            {
                material = postDto.pn,
                count = -1,
                realtime_product_count = postDto.qty,
                realtime_pallet_count = 1,
                alert = 1
            });
        }

        private void UpdateCargoDataInfoContextForRealtime(ref CargoDataInfoContext context, int qty)
        {
            context.realtime_product_count += qty;
            context.realtime_pallet_count += 1;
            context.alert = 0;
        }

        private void UpdateCargoDataInfoContextForRealtimeWithInvalidData(ref CargoDataInfoContext context, int qty)
        {
            context.realtime_product_count += qty;
            context.realtime_pallet_count += 1;
            context.alert = 1;
        }

        private void UpdateDeliveryCargoDataDtoToRemoveInvalidQty(ref DeliveryCargoDto deliveryingCargoDto, DeliveryCargoDataDto invalidDto)
        {
            var data = deliveryingCargoDto.datas.Where(data => data.material == invalidDto.material).FirstOrDefault();
            data.realtime_product_count -= invalidDto.realtime_product_count;
            data.realtime_pallet_count -= invalidDto.realtime_pallet_count;
            data.alert = 0;
        }

        private void UpdateDeliveryCargoDataDtoToRemoveInvalidMaterial(ref DeliveryCargoDto deliveryingCargoDto, DeliveryCargoDataDto invalidDto)
        {
            deliveryingCargoDto.datas.Remove(invalidDto);
        }

        private List<DeliveryCargoDto> UpdateCacheDeliveryCargoDtos(DeliveryCargoDto dto)
        {
            List<DeliveryCargoDto> deliveryCargoDtos = _localMemoryCache.ReadDeliveryCargoDtos();
            var replaceDto = deliveryCargoDtos.FirstOrDefault(d => d.no == dto.no);
            replaceDto = dto;
            return deliveryCargoDtos;
        }

        private void UpdateCargoDataInfoContextForRealtimeToRemoveAlert(ref CargoDataInfoContext context)
        {
            context.alert = 0;
        }

        private void UpdateCargoDataInfoContextForRealtimeToRemoveAlert(ref CargoDataInfoContext context, int qty)
        {
            context.realtime_product_count -= qty;
            context.realtime_pallet_count -= 1;
            context.alert = 0;
        }

        #endregion
    }
}
