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
            _connection = connection;
            _localMemoryCache = new LocalMemoryCache(memoryCache);
            _httpClientManager = new HttpClientManager(httpClientFactory);
        }

        private readonly ConnectionRepositoryManager _connection;
        private readonly LocalMemoryCache _localMemoryCache;
        private readonly HttpClientManager _httpClientManager;

        #region Public methods

        public IResultDto PostToDisable(dynamic value)
        {
            DeliveryNumberDto dto;

            // Check value
            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    MissingMemberHandling = MissingMemberHandling.Error
                };
                dto = JsonConvert.DeserializeObject<DeliveryNumberDto>(value.ToString(), settings);
            }
            catch
            {
                return GetResultDto(ResultEnum.False, ErrorEnum.InvalidProperties);
            }

            bool result = _connection.UpdateToDisableDeliveryNumberState(dto.no);
            if (result == false)
            {
                return GetResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Update cache
            _localMemoryCache.SaveCacheDidChange(true);

            return GetResultDto(ResultEnum.True, ErrorEnum.None);
        }

        public DeliveryNumberResultDto GetSearch(string delivery)
        {
            // Check value
            if (string.IsNullOrEmpty(delivery))
            {
                return GetDeliveryNumberResultDto(ResultEnum.False, ErrorEnum.InvalidProperties, null);
            }

            DeliveryNumberDto deliveryNumberDto = _connection.QueryDeliveryNumberDtoWithDelivery(delivery);
            if (deliveryNumberDto == null)
            {
                return GetDeliveryNumberResultDto(ResultEnum.False, ErrorEnum.InvalidProperties, null);
            }
            deliveryNumberDto.datas = new List<DeliveryNumberDataDto>();
            List<DeliveryNumberDataDto> validDeliveryNumberDataDtos = _connection.QueryValidDeliveryNumberDataDtos(deliveryNumberDto.no);
            if (validDeliveryNumberDataDtos != null)
            {
                for (var i = 0; i < validDeliveryNumberDataDtos.Count(); i++)
                {
                    deliveryNumberDto.datas.Add(validDeliveryNumberDataDtos[i]);
                }
            }
            List<DeliveryNumberDataDto> invalidDeliveryNumberDataDtos = _connection.QueryInvalidDeliveryNumberDataDtos(deliveryNumberDto.no);
            if (invalidDeliveryNumberDataDtos != null)
            {
                for (var i = 0; i < invalidDeliveryNumberDataDtos.Count(); i++)
                {
                    deliveryNumberDto.datas.Add(invalidDeliveryNumberDataDtos[i]);
                }
            }

            List<DeliveryNumberDto> deliveryNumberDtos = new List<DeliveryNumberDto>();
            deliveryNumberDtos.Add(deliveryNumberDto);

            return GetDeliveryNumberResultDto(ResultEnum.True, ErrorEnum.None, deliveryNumberDtos);
        }

        public DeliveryNumberResultDto GetReview(string no)
        {
            // Check value
            if (string.IsNullOrEmpty(no))
            {
                return GetDeliveryNumberResultDto(ResultEnum.False, ErrorEnum.InvalidProperties, null);
            }

            DeliveryNumberDto deliveryNumberDto = _connection.QueryDeliveryNumberDtoWithNo(no);
            if (deliveryNumberDto == null)
            {
                return GetDeliveryNumberResultDto(ResultEnum.False, ErrorEnum.InvalidProperties, null);
            }
            deliveryNumberDto.datas = new List<DeliveryNumberDataDto>();
            List<DeliveryNumberDataDto> validDeliveryNumberDataDtos = _connection.QueryValidDeliveryNumberDataDtos(no);
            if (validDeliveryNumberDataDtos != null)
            {
                for(var i = 0; i < validDeliveryNumberDataDtos.Count(); i ++)
                {
                    deliveryNumberDto.datas.Add(validDeliveryNumberDataDtos[i]);
                }
            }
            List<DeliveryNumberDataDto> invalidDeliveryNumberDataDtos = _connection.QueryInvalidDeliveryNumberDataDtos(no);
            if (invalidDeliveryNumberDataDtos != null)
            {
                for (var i = 0; i < invalidDeliveryNumberDataDtos.Count(); i++)
                {
                    deliveryNumberDto.datas.Add(invalidDeliveryNumberDataDtos[i]);
                }
            }

            List<DeliveryNumberDto> deliveryNumberDtos = new List<DeliveryNumberDto>();
            deliveryNumberDtos.Add(deliveryNumberDto);

            return GetDeliveryNumberResultDto(ResultEnum.True, ErrorEnum.None, deliveryNumberDtos);
        }

        public IResultDto GetDnList()
        {
            // Examine for any changes
            List<DeliveryNumberDto> cacheDeliveryNumberDtos = _localMemoryCache.ReadDeliveryNumberDtos();
            int count = _connection.QueryDeliveryNumberContextCount();
            if (cacheDeliveryNumberDtos != null && cacheDeliveryNumberDtos.Count() == count)
            {
                bool cacheDidChange = _localMemoryCache.ReadCacheDidChange();
                if (cacheDidChange == false)
                {
                    return GetDeliveryNumberResultDto(ResultEnum.True, ErrorEnum.None, cacheDeliveryNumberDtos);
                }
            }

            // Query DeliveryCargoDtos which did not finish
            List<DeliveryNumberDto> deliveryNumberDtos = _connection.QueryDeliveryNumberDtos();
            if (deliveryNumberDtos == null)
            {
                // No available data in database
                return GetDeliveryNumberResultDto(ResultEnum.True, ErrorEnum.None, null);
            }

            // Query DeliveryCargoDataDto matches no
            if (deliveryNumberDtos.Count > 0)
            {
                foreach (var deliveryNumberDto in deliveryNumberDtos)
                {
                    List<DeliveryNumberDataDto> deliveryNumberDataDtos = _connection.QueryDeliveryNumberDataDtos(deliveryNumberDto.no);
                    if (deliveryNumberDataDtos == null)
                    {
                        // No available data in database
                        return GetDeliveryNumberResultDto(ResultEnum.False, ErrorEnum.None, null);
                    }
                    deliveryNumberDto.datas = deliveryNumberDataDtos;
                }
            }

            // Update cache
            DeliveryNumberDto deliveryingNumberDto = deliveryNumberDtos.Where(dto => dto.state == 0).FirstOrDefault();
            if (deliveryingNumberDto != null)
            {
                _localMemoryCache.SaveDeliveryingNumberDto(deliveryingNumberDto);
            }
            else
            {
                _localMemoryCache.DeleteDeliveryingNumberDto();
            }
            _localMemoryCache.SaveDeliveryNumberDtos(deliveryNumberDtos);
            _localMemoryCache.SaveCacheDidChange(false);

            return GetDeliveryNumberResultDto(ResultEnum.True, ErrorEnum.None, deliveryNumberDtos);
        }

        public IResultDto PostToDismissAlert(dynamic value)
        {
            DeliveryNumberDto dto;
            bool result;

            // Check value
            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    MissingMemberHandling = MissingMemberHandling.Error
                };
                dto = JsonConvert.DeserializeObject<DeliveryNumberDto>(value.ToString(), settings);
            }
            catch (Exception exp)
            {
                return GetResultDto(ResultEnum.False, ErrorEnum.InvalidProperties);
            }

            // Any available deliveryingCargoDto in cache
            DeliveryNumberDto deliveryingNumberDto = _localMemoryCache.ReadDeliveryingNumberDto();
            if (deliveryingNumberDto == null && deliveryingNumberDto.no != dto.no)
            {
                // Get DeliveryingCargoDto from database
                deliveryingNumberDto = _connection.QueryDeliveryNumberDtosWithState(0).FirstOrDefault();
                if (deliveryingNumberDto == null)
                {
                    // Maybe no dn is selected and some pallet had been moved to terminal
                    // So return and no update and insert data into database
                    return GetResultDto(ResultEnum.True, ErrorEnum.None);
                }

                // Get DeliveryCargoDataDtos of deliveryingCargoDto from database
                deliveryingNumberDto.datas = _connection.QueryDeliveryNumberDataDtos(deliveryingNumberDto.no);

                // Save to cache
                _localMemoryCache.SaveDeliveryingNumberDto(deliveryingNumberDto);
                List<DeliveryNumberDto> updatedDeliveryNumberDtos = UpdateCacheDeliveryNumberDtos(deliveryingNumberDto);
                _localMemoryCache.SaveDeliveryNumberDtos(updatedDeliveryNumberDtos);
            }

            // Get invalid data from deliveryingCargoDto
            DeliveryNumberDataDto alertDeliveryNumberDataDto = deliveryingNumberDto.datas.Where(data => data.alert == 1).FirstOrDefault();
            if (alertDeliveryNumberDataDto != null)
            {
                if (alertDeliveryNumberDataDto.product_count > -1 &&
                    alertDeliveryNumberDataDto.product_count < alertDeliveryNumberDataDto.realtime_product_count)
                {
                    // Over qty
                    UpdateDeliveryNumberDataDtoToRemoveInvalidQty(ref deliveryingNumberDto, alertDeliveryNumberDataDto);

                    // Update deliveryingCargoDto and deliveryCargoDtos in cache
                    _localMemoryCache.SaveDeliveryingNumberDto(deliveryingNumberDto);
                    List<DeliveryNumberDto> updatedDeliveryNumberDtos = UpdateCacheDeliveryNumberDtos(deliveryingNumberDto);
                    _localMemoryCache.SaveDeliveryNumberDtos(updatedDeliveryNumberDtos);

                    // Update database
                    DeliveryNumberContext deliveryNumberContext = _connection.QueryDeliveryNumberContextWithNo(deliveryingNumberDto.no);
                    CargoDataInfoContext cargoDataInfoContext = _connection.QueryCargoDataInfoContextWithMaterial(deliveryNumberContext.id, alertDeliveryNumberDataDto.material);
                    int qty = _connection.QueryQtyByCargoDataRecordContextWithInfoIds(cargoDataInfoContext.f_delivery_number_id, cargoDataInfoContext.id);
                    UpdateCargoDataInfoContextForRealtimeToRemoveAlert(ref cargoDataInfoContext, qty);
                    result = _connection.UpdateCargoDataInfoContext(cargoDataInfoContext);
                }
                else if (alertDeliveryNumberDataDto.product_count == -1)
                {
                    // Miss match material
                    UpdateDeliveryCargoDataDtoToRemoveInvalidMaterial(ref deliveryingNumberDto, alertDeliveryNumberDataDto);

                    // Update deliveryingCargoDto and deliveryCargoDtos in cache
                    _localMemoryCache.SaveDeliveryingNumberDto(deliveryingNumberDto);
                    List<DeliveryNumberDto> updatedDeliveryNumberDtos = UpdateCacheDeliveryNumberDtos(deliveryingNumberDto);
                    _localMemoryCache.SaveDeliveryNumberDtos(updatedDeliveryNumberDtos);

                    // Update database
                    DeliveryNumberContext deliveryNumberContext = _connection.QueryDeliveryNumberContextWithNo(deliveryingNumberDto.no);
                    CargoDataInfoContext cargoDataInfoContext = _connection.QueryCargoDataInfoContextWithMaterial(deliveryNumberContext.id, alertDeliveryNumberDataDto.material);
                    UpdateCargoDataInfoContextForRealtimeToRemoveAlert(ref cargoDataInfoContext);
                    result = _connection.UpdateCargoDataInfoContext(cargoDataInfoContext);
                }
            }

            // Dismiss alert trigger
            _httpClientManager.PostToNoTriggeTerminalReader();

            _localMemoryCache.SaveCacheDidChange(true);

            return GetResultDto(ResultEnum.True, ErrorEnum.None);
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
                return GetResultDto(ResultEnum.False, ErrorEnum.InvalidProperties);
            }

            // Any available deliveryingCargoDto in cache
            DeliveryNumberDto deliveryingNumberDto = _localMemoryCache.ReadDeliveryingNumberDto();
            if (deliveryingNumberDto == null)
            {
                // Get DeliveryingCargoDto from database
                deliveryingNumberDto = _connection.QueryDeliveryNumberDtosWithState(0).FirstOrDefault();
                if (deliveryingNumberDto == null)
                {
                    // Maybe no dn is selected and some pallet had been moved to terminal
                    // So return and no update and insert data into database
                    return GetResultDto(ResultEnum.True, ErrorEnum.None);
                }

                // Get DeliveryCargoDataDtos of deliveryingCargoDto from database
                deliveryingNumberDto.datas = _connection.QueryDeliveryNumberDataDtos(deliveryingNumberDto.no);

                // Save to cache
                _localMemoryCache.SaveDeliveryingNumberDto(deliveryingNumberDto);
                List<DeliveryNumberDto> updatedDeliveryCargoDtos = UpdateCacheDeliveryNumberDtos(deliveryingNumberDto);
                _localMemoryCache.SaveDeliveryNumberDtos(updatedDeliveryCargoDtos);
            }

            // Data received from epc server
            // Examine epc for valid or invalid
            DeliveryNumberDataDto matchedMaterialDeliveryNumberDataDto = deliveryingNumberDto.datas.Where(data => data.material == dto.pn).ToList().FirstOrDefault();
            if (matchedMaterialDeliveryNumberDataDto != null) 
            {
                // Valid material

                if (matchedMaterialDeliveryNumberDataDto.product_count < matchedMaterialDeliveryNumberDataDto.realtime_product_count + dto.qty)
                {
                    // [ERROR]
                    // Update deliveryingCargoDto and deliveryCargoDtos in cache
                    UpdateDeliveryNumberDtoWithInvalidPallet(ref deliveryingNumberDto);
                    UpdateDeliveryNumberDataDtoForRealtimeWithInvalidData(ref matchedMaterialDeliveryNumberDataDto, dto.qty);
                    _localMemoryCache.SaveDeliveryingNumberDto(deliveryingNumberDto);
                    List<DeliveryNumberDto> updatedDeliveryNumberDtos = UpdateCacheDeliveryNumberDtos(deliveryingNumberDto);
                    _localMemoryCache.SaveDeliveryNumberDtos(updatedDeliveryNumberDtos);

                    // Update database
                    DeliveryNumberContext deliveryNumberContext = _connection.QueryDeliveryNumberContextWithNo(deliveryingNumberDto.no);
                    deliveryNumberContext.invalid_pallet_quantity += 1;
                    result = _connection.UpdateDeliveryNumberContextWhenDataInserted(deliveryNumberContext);

                    CargoDataInfoContext cargoDataInfoContext = _connection.QueryCargoDataInfoContextWithMaterial(deliveryNumberContext.id, dto.pn);
                    UpdateCargoDataInfoContextForRealtimeWithInvalidData(ref cargoDataInfoContext, dto.qty);
                    result = _connection.UpdateCargoDataInfoContext(cargoDataInfoContext);

                    CargoDataRecordContext insertCargoDataRecordContext = GetCargoDataRecordContext(dto, cargoDataInfoContext.f_delivery_number_id, cargoDataInfoContext.id, false);
                    result = _connection.InsertCargoDataRecordContext(insertCargoDataRecordContext);

                    // Alert quantity is out of target count
                    result = _httpClientManager.PostToTriggerTerminalReader();
                }
                else
                {
                    // [OK]
                    // Update deliveryingCargoDto and deliveryCargoDtos in cache
                    UpdateDeliveryNumberDtoWithValidQty(ref deliveryingNumberDto);
                    UpdateDeliveryNumberDataDtoForRealtime(ref matchedMaterialDeliveryNumberDataDto, dto.qty);
                    _localMemoryCache.SaveDeliveryingNumberDto(deliveryingNumberDto);
                    List<DeliveryNumberDto> updatedDeliveryNumberDtos = UpdateCacheDeliveryNumberDtos(deliveryingNumberDto);
                    _localMemoryCache.SaveDeliveryNumberDtos(updatedDeliveryNumberDtos);

                    // Update database
                    DeliveryNumberContext deliveryNumberContext = _connection.QueryDeliveryNumberContextWithNo(deliveryingNumberDto.no);
                    UpdateDeliveryNumberContextWithValidQty(ref deliveryNumberContext);
                    result = _connection.UpdateDeliveryNumberContextWhenDataInserted(deliveryNumberContext);

                    CargoDataInfoContext cargoDataInfoContext = _connection.QueryCargoDataInfoContextWithMaterial(deliveryNumberContext.id, dto.pn);
                    UpdateCargoDataInfoContextForRealtime(ref cargoDataInfoContext, dto.qty);
                    result = _connection.UpdateCargoDataInfoContext(cargoDataInfoContext);

                    CargoDataRecordContext insertCargoDataRecordContext = GetCargoDataRecordContext(dto, cargoDataInfoContext.f_delivery_number_id, cargoDataInfoContext.id, true);
                    result = _connection.InsertCargoDataRecordContext(insertCargoDataRecordContext);
                }
            } 
            else
            {
                // Invalid material

                // [ERROR]
                // Update deliveryingCargoDto and deliveryCargoDtos in cache
                UpdateDeliveryNumberDtoWithInvalidPallet(ref deliveryingNumberDto);
                InsertDeliveryNumberDataDtoWithInvalidData(ref deliveryingNumberDto, dto);
                _localMemoryCache.SaveDeliveryingNumberDto(deliveryingNumberDto);
                List<DeliveryNumberDto> updatedDeliveryNumberDtos = UpdateCacheDeliveryNumberDtos(deliveryingNumberDto);
                _localMemoryCache.SaveDeliveryNumberDtos(updatedDeliveryNumberDtos);

                // Update database
                DeliveryNumberContext deliveryNumberContext =  _connection.QueryDeliveryNumberContextWithNo(deliveryingNumberDto.no);
                deliveryNumberContext.invalid_pallet_quantity += 1;
                result = _connection.UpdateDeliveryNumberContextWhenDataInserted(deliveryNumberContext);

                CargoDataInfoContext invalidCargoDataInfoContext = GetInvalidCargoDataInfoContext(dto, deliveryNumberContext.id);
                result = _connection.InsertCargoDataInfoContext(invalidCargoDataInfoContext);
                CargoDataInfoContext insertedCargoDataInfoContext = _connection.QueryCargoDataInfoContextWithInvalidContext(invalidCargoDataInfoContext);

                CargoDataRecordContext insertCargoDataRecordContext = GetCargoDataRecordContext(dto, deliveryNumberContext.id, insertedCargoDataInfoContext.id, false);
                result = _connection.InsertCargoDataRecordContext(insertCargoDataRecordContext);

                // Alert pn is out of target material
                result = _httpClientManager.PostToTriggerTerminalReader();
            }

            _localMemoryCache.SaveCacheDidChange(true);

            return GetResultDto(ResultEnum.True, ErrorEnum.None);
        }

        public IResultDto PostToStart(dynamic value)
        {
            DeliveryNumberDto dto;
            bool result;

            // Check value
            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    MissingMemberHandling = MissingMemberHandling.Error
                };
                dto = JsonConvert.DeserializeObject<DeliveryNumberDto>(value.ToString(), settings);
            }
            catch (Exception exp)
            {
                return GetResultDto(ResultEnum.False, ErrorEnum.InvalidProperties);
            }

            // Update [eaton_delivery_number] in database
            UpdateDeliveryNumberDtoWhenStart(ref dto);
            result = _connection.UpdateDeliveryNumberContextWhenStart(dto);
            if (result == false)
            {
                return GetResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Save to cache
            _localMemoryCache.SaveCacheDidChange(true);

            return GetResultDto(ResultEnum.True, ErrorEnum.None);
        }

        public IResultDto PostToFinish(dynamic value)
        {
            DeliveryNumberDto dto;
            bool result;

            // Check value
            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    MissingMemberHandling = MissingMemberHandling.Error
                };
                dto = JsonConvert.DeserializeObject<DeliveryNumberDto>(value.ToString(), settings);
            }
            catch (Exception exp)
            {
                return GetResultDto(ResultEnum.False, ErrorEnum.InvalidProperties);
            }

            // Update [eaton_delivery_number] in database
            UpdateDeliveryNumberDtoWhenFinish(ref dto);
            result = _connection.UpdateDeliveryNumberContextWhenFinish(dto);
            if (result == false)
            {
                return GetResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Save to cache
            _localMemoryCache.SaveCacheDidChange(true);

            return GetResultDto(ResultEnum.True, ErrorEnum.None);
        }

        public ResultDto PostToQuit(dynamic value)
        {
            DeliveryNumberDto dto;
            bool result;

            // Check value
            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    MissingMemberHandling = MissingMemberHandling.Error
                };
                dto = JsonConvert.DeserializeObject<DeliveryNumberDto>(value.ToString(), settings);
            }
            catch (Exception exp)
            {
                return GetResultDto(ResultEnum.False, ErrorEnum.InvalidProperties);
            }

            // Update database
            UpdateDeliveryNumberDtoWhenFinish(ref dto);
            result = _connection.UpdateDeliveryNumberContextWhenFinish(dto);

            // Update cache
            _localMemoryCache.SaveCacheDidChange(true);

            return GetResultDto(ResultEnum.True, ErrorEnum.None);
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
                return GetResultDto(ResultEnum.False, ErrorEnum.InvalidProperties);
            }

            // Check for duplicated file
            DeliveryFileContext existDeliveryFileContext = _connection.QueryDeliveryFileContextWithFileName(dto.FileName);
            if (existDeliveryFileContext != null)
            {
                return GetResultDto(ResultEnum.False, ErrorEnum.DuplicatedFileName);
            }

            // Check for duplicated delivery
            var deliverys = dto.FileData.Select(d => d.Delivery).ToList();
            List<DeliveryFileContext> duplicatedFileContexts = _connection.QueryDeliveryFileContextWithDeliverys(deliverys);
            if (duplicatedFileContexts != null)
            {
                return GetResultDto(ResultEnum.False, ErrorEnum.DuplicatedDelivery);
            }

            // Insert into [eaton_delivery_file]
            DeliveryFileContext insertDeliveryFileContext = GetDeliveryFileContext(dto);
            result = _connection.InsertDeliveryFileContext(insertDeliveryFileContext);
            if (result == false)
            {
                return GetResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Query [eaton_delivery_file]
            DeliveryFileContext insertedDeliveryFileContext = _connection.QueryDeliveryFileContextWithFileName(dto.FileName);
            if (insertedDeliveryFileContext == null)
            {
                return GetResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Insert into [eaton_delivery_number]
            DeliveryNumberContext insertDeliveryNumberContext = GetDeliveryNumberContexts(insertedDeliveryFileContext.id, dto);
            result = _connection.InsertDeliveryNumberContext(insertDeliveryNumberContext);
            if (result == false)
            {
                return GetResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Query [eaton_delivery_number]
            DeliveryNumberContext insertedDeliveryNumberContext = _connection.QueryDeliveryNumberContextWithFileId(insertedDeliveryFileContext.id);
            if (insertedDeliveryNumberContext == null)
            {
                return GetResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Insert into [eaton_cargo_data]
            List<CargoDataContext> insertCargoDataContexts = GetCargoDataContexts(insertedDeliveryNumberContext, dto.FileData);
            result = _connection.InsertCargoDataContexts(insertCargoDataContexts);
            if (result == false)
            {
                return GetResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Query [eaton_cargo_data]
            List<int> cargoIds = insertCargoDataContexts.Select(c => c.f_delivery_number_id).ToList();
            List<CargoDataContext> insertedCargoDataContexts = _connection.QueryCargoDataContextsWithCargoId(cargoIds);
            if (insertedCargoDataContexts == null)
            {
                return GetResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Insert into [eaton_cargo_data_info]
            List<CargoDataInfoContext> insertCargoDataInfoContext = GetCargoDataInfoContexts(insertedCargoDataContexts);
            result = _connection.InsertCargoDataInfoContexts(insertCargoDataInfoContext);
            if (result == false)
            {
                return GetResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            // Query [eaton_cargo_data_info]
            List<CargoDataInfoContext> insertedCargoDataInfoContext = _connection.QueryCargoDataInfoContexts(cargoIds);
            if (insertedCargoDataInfoContext == null) 
            {
                return GetResultDto(ResultEnum.False, ErrorEnum.FailedToAccessDatabase);
            }

            _localMemoryCache.SaveCacheDidChange(true);

            return GetResultDto(ResultEnum.True, ErrorEnum.None);
        }

        #endregion

        #region Private methods

        private ResultDto GetResultDto(ResultEnum result, ErrorEnum error)
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

        private DeliveryNumberContext GetDeliveryNumberContexts(int id, DeliveryUploadPostDto dto)
        {
            List<DeliveryNumberContext> contexts = new List<DeliveryNumberContext>();
            int productCount = dto.FileData.Sum(f => int.Parse(f.Quantity));
            int materialCount = dto.FileData.GroupBy(f => f.Material).Count();
            int palletCount = dto.FileData.Sum(f => (int)Math.Ceiling(decimal.ToDouble(int.Parse(f.Quantity)) / decimal.ToDouble(int.Parse(f.Unit))));
            return new DeliveryNumberContext
            {
                f_delivery_file_id = id,
                no = dto.FileName.Split(".")[0],
                material_quantity = materialCount,
                product_quantity = productCount,
                start_time = "",
                end_time = "",
                pallet_quantity = palletCount,
                miss_pallet_quantity = palletCount,
                valid_pallet_quantity = 0,
                invalid_pallet_quantity = 0,
                state = -1,
            };
        }

        private List<CargoDataContext> GetCargoDataContexts(DeliveryNumberContext deliveryCargoContext, List<FileDto> fileData)
        {
            List<CargoDataContext> contexts = new List<CargoDataContext>();
            foreach(var data in fileData)
            {
                contexts.Add(new CargoDataContext
                {
                    f_delivery_number_id = deliveryCargoContext.id,
                    delivery = data.Delivery,
                    item = data.Item,
                    material = data.Material,
                    quantity = int.Parse(data.Quantity),
                    unit = int.Parse(data.Unit)
                });
            }
            return contexts;
        }

        private List<CargoDataInfoContext> GetCargoDataInfoContexts(List<CargoDataContext> contexts)
        {
            List<CargoDataInfoContext> insertCargoDataInfoContext = new List<CargoDataInfoContext>();
            var groupByMaterialContexts = contexts.GroupBy(context => context.material).Select(context => new
            {
                f_delivery_number_id = context.Select(c => c.f_delivery_number_id).FirstOrDefault(),
                delivery = context.Select(c => c.delivery).FirstOrDefault(),
                material = context.Key,
                product_count = context.Sum(c => c.quantity),
                pallet_count = (int)Math.Ceiling(decimal.ToDouble(context.Sum(c => c.quantity))/decimal.ToDouble(context.Select(c => c.unit).FirstOrDefault())),
            });
            foreach (var groupByMaterialContext in groupByMaterialContexts)
            {
                insertCargoDataInfoContext.Add(new CargoDataInfoContext
                {
                    f_delivery_number_id = groupByMaterialContext.f_delivery_number_id,
                    delivery = groupByMaterialContext.delivery,
                    material = groupByMaterialContext.material,
                    product_count = groupByMaterialContext.product_count,
                    pallet_count = groupByMaterialContext.pallet_count,
                    realtime_product_count = 0,
                    realtime_pallet_count = 0,
                    alert = 0,
                });
            }

            return insertCargoDataInfoContext;
        }

        private CargoDataInfoContext GetInvalidCargoDataInfoContext(DeliveryTerminalPostDto dto, int id)
        {
            return new CargoDataInfoContext
            {
                f_delivery_number_id = id,
                delivery = "-",
                material = dto.pn,
                product_count = -1,
                pallet_count = -1,
                realtime_product_count = dto.qty,
                realtime_pallet_count = 1,
                alert = 1,
            };
        }

        private DeliveryNumberResultDto GetDeliveryNumberResultDto(ResultEnum result, ErrorEnum error, List<DeliveryNumberDto> dto)
        {
            return new DeliveryNumberResultDto
            {
                Result = result.ToBoolean(),
                Error = error.ToChineseDescription(),
                DeliveryNumberDtos = dto,
            };
        }

        private void UpdateDeliveryNumberDtoWhenStart(ref DeliveryNumberDto dto)
        {
            dto.start_time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            dto.state = 0;
        }

        private void UpdateDeliveryNumberDtoWhenFinish(ref DeliveryNumberDto dto)
        {
            dto.end_time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            dto.state = 1;
        }

        private CargoDataRecordContext GetCargoDataRecordContext(DeliveryTerminalPostDto dto,int f_delivery_number_id, int f_cargo_data_info_id, bool valid)
        {
            return new CargoDataRecordContext()
            {
                f_delivery_number_id = f_delivery_number_id,
                f_cargo_data_info_id = f_cargo_data_info_id,
                f_epc_raw_id = dto.epc_raw_id,
                f_epc_data_id = dto.epc_data_id,
                valid = valid ? 1 : 0,
            };
        }

        private void UpdateDeliveryNumberDtoWithValidQty(ref DeliveryNumberDto dto)
        {
            dto.valid_pallet_quantity += 1;
            dto.miss_pallet_quantity -= 1;
        }

        private void UpdateDeliveryNumberContextWithValidQty(ref DeliveryNumberContext context)
        {
            context.valid_pallet_quantity += 1;
            context.miss_pallet_quantity -= 1;
        }

        private void UpdateDeliveryNumberDtoWithInvalidPallet(ref DeliveryNumberDto dto)
        {
            dto.invalid_pallet_quantity += 1;
        }

        private void UpdateDeliveryNumberDataDtoForRealtime(ref DeliveryNumberDataDto dto, int qty)
        {
            dto.realtime_product_count += qty;
            dto.realtime_pallet_count += 1;
        }

        private void UpdateDeliveryNumberDataDtoForRealtimeWithInvalidData(ref DeliveryNumberDataDto dto, int qty)
        {
            dto.realtime_product_count += qty;
            dto.realtime_pallet_count += 1;
            dto.alert = 1;
        }

        private void InsertDeliveryNumberDataDtoWithInvalidData(ref DeliveryNumberDto dto, DeliveryTerminalPostDto postDto)
        {
            dto.datas.Add(new DeliveryNumberDataDto
            {
                delivery = "-",
                material = postDto.pn,
                product_count = -1,
                pallet_count = -1,
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

        private void UpdateDeliveryNumberDataDtoToRemoveInvalidQty(ref DeliveryNumberDto deliveryingNumberDto, DeliveryNumberDataDto invalidDataDto)
        {
            var data = deliveryingNumberDto.datas.Where(data => data.material == invalidDataDto.material).FirstOrDefault();
            data.realtime_product_count -= invalidDataDto.realtime_product_count;
            data.realtime_pallet_count -= invalidDataDto.realtime_pallet_count;
            data.alert = 0;
        }

        private void UpdateDeliveryCargoDataDtoToRemoveInvalidMaterial(ref DeliveryNumberDto deliveryingNumberDto, DeliveryNumberDataDto invalidDataDto)
        {
            deliveryingNumberDto.datas.Remove(invalidDataDto);
        }

        private List<DeliveryNumberDto> UpdateCacheDeliveryNumberDtos(DeliveryNumberDto dto)
        {
            List<DeliveryNumberDto> deliveryNumberDtos = _localMemoryCache.ReadDeliveryNumberDtos();
            var replaceDto = deliveryNumberDtos.FirstOrDefault(d => d.no == dto.no);
            replaceDto = dto;
            return deliveryNumberDtos;
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
