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
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Services
{
    public class DeliveryService
    {
        public DeliveryService(ConnectionRepositoryManager manager, IMemoryCache memoryCache, IHttpClientFactory httpClientFactory)
        {
            _manager = manager;
            _localMemoryCache = new LocalMemoryCache(memoryCache);
            _httpClientManager = new HttpClientManager(httpClientFactory);
        }

        private readonly ConnectionRepositoryManager _manager;
        private readonly LocalMemoryCache _localMemoryCache;
        private readonly HttpClientManager _httpClientManager;

        #region Public methods

        public IResultDto GetDnList()
        {
            using (var connection = _manager.MsSqlConnectionRepository.InitConnection())
            {
                connection.Open();

                using (var transaction = _manager.MsSqlConnectionRepository.BeginTransaction())
                {
                    try
                    {
                        // Examine for any changes
                        List<DeliveryNumberDto> cacheDeliveryNumberDtos = _localMemoryCache.ReadDeliveryNumberDtos();
                        DeliveryNumberDto cacheDeliveryingNumberDto = _localMemoryCache.ReadDeliveryingNumberDto();
                        bool cacheDidChange = _localMemoryCache.ReadCacheDidChange();
                        int databaseDeliveryNumberCount = _manager.QueryDeliveryNumberContextCount();
                        if (cacheDeliveryingNumberDto == null)
                        {
                            if (cacheDeliveryNumberDtos != null && cacheDidChange == false)
                            {
                                if (cacheDeliveryNumberDtos.Count() == databaseDeliveryNumberCount)
                                {
                                    // Commit the transaction if everything is successful
                                    transaction.Commit();

                                    return GetDeliveryNumberResultDto(ResultEnum.True, ErrorEnum.None.ToChineseDescription(), cacheDeliveryNumberDtos);
                                }
                            }
                        }

                        // Query DeliveryCargoDtos which did not finish
                        List<DeliveryNumberDto> deliveryNumberDtos = _manager.QueryDeliveryNumberDtos();
                        if (deliveryNumberDtos == null)
                        {
                            // Commit the transaction if everything is successful
                            transaction.Commit();

                            // No available data in database
                            return GetDeliveryNumberResultDto(ResultEnum.True, ErrorEnum.None.ToChineseDescription(), null);
                        }

                        // Query DeliveryCargoDataDto matches no
                        if (deliveryNumberDtos.Count > 0)
                        {
                            foreach (var deliveryNumberDto in deliveryNumberDtos)
                            {
                                List<DeliveryNumberDataDto> deliveryNumberDataDtos = _manager.QueryDeliveryNumberDataDtos(deliveryNumberDto.no);
                                if (deliveryNumberDataDtos == null)
                                {
                                    // No available data in database
                                    throw new Exception(ErrorEnum.NoData.ToChineseDescription());
                                }
                                deliveryNumberDto.datas = deliveryNumberDataDtos;
                            }
                        }

                        // Update cache
                        // 0: new, 1: select, 2: deliverying, 3: finish/search/review, 4: edit, -1: alert/pause
                        DeliveryNumberDto deliveryingNumberDto = deliveryNumberDtos.Where(dto => dto.state == DeliveryStateEnum.Delivery.ToDescription() || dto.state == DeliveryStateEnum.Alert.ToDescription()).FirstOrDefault();
                        if (deliveryingNumberDto != null)
                        {
                            _localMemoryCache.SaveDeliveryingNumberDto(deliveryingNumberDto);
                        }
                        else
                        {
                            _localMemoryCache.RemoveDeliveryingNumberDto();
                        }
                        _localMemoryCache.SaveDeliveryNumberDtos(deliveryNumberDtos);
                        _localMemoryCache.SaveCacheDidChange(false);

                        // Commit the transaction if everything is successful
                        transaction.Commit();

                        return GetDeliveryNumberResultDto(ResultEnum.True, ErrorEnum.None.ToChineseDescription(), deliveryNumberDtos);
                    }
                    catch (Exception exp)
                    {
                        // Handle exceptions and optionally roll back the transaction
                        transaction.Rollback();
                        return GetResultDto(ResultEnum.False, exp.Message);
                    }
                }
            }
        }

        public DeliveryNumberResultDto GetSearch(string delivery)
        {
            // Check value
            if (string.IsNullOrEmpty(delivery))
            {
                return GetDeliveryNumberResultDto(ResultEnum.False, ErrorEnum.InvalidProperties.ToChineseDescription(), null);
            }

            using (var connection = _manager.MsSqlConnectionRepository.InitConnection())
            {
                connection.Open();

                using (var transaction = _manager.MsSqlConnectionRepository.BeginTransaction())
                {
                    try
                    {
                        DeliveryNumberDto deliveryNumberDto = _manager.QueryDeliveryNumberDtoWithDelivery(delivery);
                        if (deliveryNumberDto == null)
                        {
                            throw new Exception(ErrorEnum.NoData.ToChineseDescription());
                        }
                        deliveryNumberDto.datas = new List<DeliveryNumberDataDto>();
                        List<DeliveryNumberDataDto> validDeliveryNumberDataDtos = _manager.QueryValidDeliveryNumberDataDtos(deliveryNumberDto.no);
                        if (validDeliveryNumberDataDtos != null)
                        {
                            for (var i = 0; i < validDeliveryNumberDataDtos.Count(); i++)
                            {
                                deliveryNumberDto.datas.Add(validDeliveryNumberDataDtos[i]);
                            }
                        }
                        List<DeliveryNumberDataDto> invalidDeliveryNumberDataDtos = _manager.QueryInvalidDeliveryNumberDataDtos(deliveryNumberDto.no);
                        if (invalidDeliveryNumberDataDtos != null)
                        {
                            for (var i = 0; i < invalidDeliveryNumberDataDtos.Count(); i++)
                            {
                                deliveryNumberDto.datas.Add(invalidDeliveryNumberDataDtos[i]);
                            }
                        }

                        List<DeliveryNumberDto> deliveryNumberDtos = new List<DeliveryNumberDto>();
                        deliveryNumberDtos.Add(deliveryNumberDto);

                        // Commit the transaction if everything is successful
                        transaction.Commit();

                        return GetDeliveryNumberResultDto(ResultEnum.True, ErrorEnum.None.ToChineseDescription(), deliveryNumberDtos);
                    }
                    catch (Exception exp)
                    {
                        // Handle exceptions and optionally roll back the transaction
                        transaction.Rollback();
                        return GetDeliveryNumberResultDto(ResultEnum.False, exp.Message, null);
                    }
                }
            }
        }

        public DeliveryNumberResultDto GetReview(string no)
        {
            // Check value
            if (string.IsNullOrEmpty(no))
            {
                return GetDeliveryNumberResultDto(ResultEnum.False, ErrorEnum.InvalidProperties.ToChineseDescription(), null);
            }

            using (var connection = _manager.MsSqlConnectionRepository.InitConnection())
            {
                connection.Open();

                using (var transaction = _manager.MsSqlConnectionRepository.BeginTransaction())
                {
                    try
                    {
                        DeliveryNumberDto deliveryNumberDto = _manager.QueryDeliveryNumberDtoWithNo(no);
                        if (deliveryNumberDto == null)
                        {
                            throw new Exception(ErrorEnum.NoData.ToChineseDescription());
                        }
                        deliveryNumberDto.datas = new List<DeliveryNumberDataDto>();
                        List<DeliveryNumberDataDto> validDeliveryNumberDataDtos = _manager.QueryValidDeliveryNumberDataDtos(no);
                        if (validDeliveryNumberDataDtos != null)
                        {
                            for (var i = 0; i < validDeliveryNumberDataDtos.Count(); i++)
                            {
                                deliveryNumberDto.datas.Add(validDeliveryNumberDataDtos[i]);
                            }
                        }
                        List<DeliveryNumberDataDto> invalidDeliveryNumberDataDtos = _manager.QueryInvalidDeliveryNumberDataDtos(no);
                        if (invalidDeliveryNumberDataDtos != null)
                        {
                            for (var i = 0; i < invalidDeliveryNumberDataDtos.Count(); i++)
                            {
                                deliveryNumberDto.datas.Add(invalidDeliveryNumberDataDtos[i]);
                            }
                        }

                        List<DeliveryNumberDto> deliveryNumberDtos = new List<DeliveryNumberDto>();
                        deliveryNumberDtos.Add(deliveryNumberDto);

                        // Commit the transaction if everything is successful
                        transaction.Commit();

                        return GetDeliveryNumberResultDto(ResultEnum.True, ErrorEnum.None.ToChineseDescription(), deliveryNumberDtos);
                    }
                    catch (Exception exp)
                    {
                        // Handle exceptions and optionally roll back the transaction
                        transaction.Rollback();
                        return GetDeliveryNumberResultDto(ResultEnum.False, exp.Message, null);
                    }
                }
            }
        }

        public IResultDto PostFromEpcServer(dynamic value)
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
            catch (Exception exp)
            {
                return GetResultDto(ResultEnum.False, exp.Message);
            }

            using (var connection = _manager.MsSqlConnectionRepository.InitConnection())
            {
                connection.Open();

                using (var transaction = _manager.MsSqlConnectionRepository.BeginTransaction())
                {
                    try
                    {
                        // Called by epc server when epc server is called from teminal reader
                        // Get deliveryingNumberContext in database
                        List<DeliveryStateEnum> states = new List<DeliveryStateEnum>() { DeliveryStateEnum.Delivery, DeliveryStateEnum.Alert };
                        DeliveryNumberContext deliveryingNumberContext = _manager.QueryDeliveryNumberContextWithStates(states);
                        if (deliveryingNumberContext == null)
                        {
                            // Commit the transaction if everything is successful
                            transaction.Commit();

                            return GetResultDto(ResultEnum.True, ErrorEnum.None.ToChineseDescription());
                        }

                        // Get DeliveryCargoDataInfoContexts
                        List<CargoDataInfoContext> cargoDataInfoContexts = _manager.QueryCargoDataInfoContextsWithDeliveryNumberId(deliveryingNumberContext.id);
                        if (cargoDataInfoContexts == null)
                        {
                            // Commit the transaction if everything is successful
                            transaction.Commit();

                            return GetResultDto(ResultEnum.True, ErrorEnum.None.ToChineseDescription());
                        }

                        // Examine epc for valid or invalid
                        CargoDataInfoContext matchedCargoDataInfoContext = cargoDataInfoContexts.Where(context => dto.pn.Contains(context.material)).ToList().FirstOrDefault();
                        if (matchedCargoDataInfoContext != null)
                        {
                            // Valid material

                            if (matchedCargoDataInfoContext.product_count < matchedCargoDataInfoContext.realtime_product_count + dto.qty)
                            {
                                // Above the target number [ERROR] 

                                // Update database
                                UpdateDeliveryNumberContextWithInvalidPallet(ref deliveryingNumberContext);
                                result = _manager.UpdateDeliveryNumberContextWhenDataInserted(deliveryingNumberContext);

                                UpdateCargoDataInfoContextForRealtimeWithInvalidData(ref matchedCargoDataInfoContext, dto.qty);
                                result = _manager.UpdateCargoDataInfoContext(matchedCargoDataInfoContext);

                                CargoDataRecordContext insertCargoDataRecordContext = GetCargoDataRecordContext(dto, matchedCargoDataInfoContext.f_delivery_number_id, matchedCargoDataInfoContext.id, false);
                                result = _manager.InsertCargoDataRecordContext(insertCargoDataRecordContext);

                                // Alert quantity is out of target count
                                result = _httpClientManager.PostToTriggerTerminalReader();
                            }
                            else
                            {
                                // [OK]

                                // Update database
                                int alertNumber = cargoDataInfoContexts.Where(context => context.alert > 0).ToList().Count();
                                UpdateDeliveryNumberContextWithValidQty(ref deliveryingNumberContext, alertNumber);
                                result = _manager.UpdateDeliveryNumberContextWhenDataInserted(deliveryingNumberContext);

                                UpdateCargoDataInfoContextForRealtime(ref matchedCargoDataInfoContext, dto.qty);
                                result = _manager.UpdateCargoDataInfoContext(matchedCargoDataInfoContext);

                                CargoDataRecordContext insertCargoDataRecordContext = GetCargoDataRecordContext(dto, matchedCargoDataInfoContext.f_delivery_number_id, matchedCargoDataInfoContext.id, true);
                                result = _manager.InsertCargoDataRecordContext(insertCargoDataRecordContext);
                            }
                        }
                        else
                        {
                            // Invalid material [ERROR]

                            // Update database
                            UpdateDeliveryNumberContextWithInvalidPallet(ref deliveryingNumberContext);
                            result = _manager.UpdateDeliveryNumberContextWhenDataInserted(deliveryingNumberContext);

                            CargoDataInfoContext invalidCargoDataInfoContext = GetInvalidCargoDataInfoContext(dto, deliveryingNumberContext.id);
                            result = _manager.InsertCargoDataInfoContext(invalidCargoDataInfoContext);
                            CargoDataInfoContext insertedCargoDataInfoContext = _manager.QueryCargoDataInfoContextWithInvalidContext(invalidCargoDataInfoContext);

                            CargoDataRecordContext insertCargoDataRecordContext = GetCargoDataRecordContext(dto, deliveryingNumberContext.id, insertedCargoDataInfoContext.id, false);
                            result = _manager.InsertCargoDataRecordContext(insertCargoDataRecordContext);

                            // Alert pn is out of target material
                            result = _httpClientManager.PostToTriggerTerminalReader();
                        }

                        // Save to cache
                        _localMemoryCache.RemoveDeliveryingNumberDto();
                        _localMemoryCache.RemoveDeliveryNumberDtos();
                        _localMemoryCache.SaveCacheDidChange(true);

                        // Commit the transaction if everything is successful
                        transaction.Commit();

                        return GetResultDto(ResultEnum.True, ErrorEnum.None.ToChineseDescription());
                    }
                    catch (Exception exp)
                    {
                        // Handle exceptions and optionally roll back the transaction
                        transaction.Rollback();
                        return GetResultDto(ResultEnum.False, exp.Message);
                    }
                }
            }
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
                return GetResultDto(ResultEnum.False, exp.Message);
            }

            using (var connection = _manager.MsSqlConnectionRepository.InitConnection())
            {
                connection.Open();

                using (var transaction = _manager.MsSqlConnectionRepository.BeginTransaction())
                {
                    try
                    {
                        // Update [eaton_delivery_number] in database
                        UpdateDeliveryNumberDtoWhenStart(ref dto);
                        result = _manager.UpdateDeliveryNumberContextWhenStart(dto);
                        if (result == false)
                        {
                            throw new Exception(ErrorEnum.FailedToAccessDatabase.ToChineseDescription());
                        }

                        // Save to cache
                        _localMemoryCache.RemoveDeliveryingNumberDto();
                        _localMemoryCache.RemoveDeliveryNumberDtos();
                        _localMemoryCache.SaveCacheDidChange(true);

                        // Commit the transaction if everything is successful
                        transaction.Commit();

                        return GetResultDto(ResultEnum.True, ErrorEnum.None.ToChineseDescription());
                    }
                    catch (Exception exp)
                    {
                        // Handle exceptions and optionally roll back the transaction
                        transaction.Rollback();
                        return GetResultDto(ResultEnum.False, exp.Message);
                    }
                }
            }
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
                return GetResultDto(ResultEnum.False, exp.Message);
            }

            using (var connection = _manager.MsSqlConnectionRepository.InitConnection())
            {
                connection.Open();

                using (var transaction = _manager.MsSqlConnectionRepository.BeginTransaction())
                {
                    try
                    {
                        // Update [eaton_delivery_number] in database
                        UpdateDeliveryNumberDtoWhenFinish(ref dto);
                        result = _manager.UpdateDeliveryNumberContextWhenFinish(dto);
                        if (result == false)
                        {
                            throw new Exception(ErrorEnum.FailedToAccessDatabase.ToChineseDescription());
                        }

                        // Save to cache
                        _localMemoryCache.RemoveDeliveryingNumberDto();
                        _localMemoryCache.RemoveDeliveryNumberDtos();
                        _localMemoryCache.SaveCacheDidChange(true);

                        // Commit the transaction if everything is successful
                        transaction.Commit();

                        return GetResultDto(ResultEnum.True, ErrorEnum.None.ToChineseDescription());
                    }
                    catch (Exception exp)
                    {
                        // Handle exceptions and optionally roll back the transaction
                        transaction.Rollback();
                        return GetResultDto(ResultEnum.False, exp.Message);
                    }
                }
            }
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
                return GetResultDto(ResultEnum.False, exp.Message);
            }

            using (var connection = _manager.MsSqlConnectionRepository.InitConnection())
            {
                connection.Open();

                using (var transaction = _manager.MsSqlConnectionRepository.BeginTransaction())
                {
                    try
                    {
                        // Get DeliveryingNumberContext from databse
                        DeliveryNumberContext deliveryingNumberContext = _manager.QueryDeliveryNumberContextWithNo(dto.no);
                        if (deliveryingNumberContext == null)
                        {
                            // Dismiss alert trigger
                            _httpClientManager.PostToDismissTriggerTerminalReader();

                            // Save to cache
                            _localMemoryCache.RemoveDeliveryingNumberDto();
                            _localMemoryCache.RemoveDeliveryNumberDtos();
                            _localMemoryCache.SaveCacheDidChange(true);

                            throw new Exception(ErrorEnum.FailedToAccessDatabase.ToChineseDescription());
                        }

                        List<CargoDataInfoContext> cargoDataInfoContexts = _manager.QueryCargoDataInfoContextsWithDeliveryNumberId(deliveryingNumberContext.id);
                        if (cargoDataInfoContexts == null)
                        {
                            // Dismiss alert trigger
                            _httpClientManager.PostToDismissTriggerTerminalReader();

                            // Save to cache
                            _localMemoryCache.RemoveDeliveryingNumberDto();
                            _localMemoryCache.RemoveDeliveryNumberDtos();
                            _localMemoryCache.SaveCacheDidChange(true);

                            throw new Exception(ErrorEnum.FailedToAccessDatabase.ToChineseDescription());
                        }

                        // Get Alert CargoDataInfo
                        List<CargoDataInfoContext> alertCargoDataInfoContexts = cargoDataInfoContexts.Where(context => context.alert > 0).ToList();
                        if (alertCargoDataInfoContexts != null)
                        {
                            var alertCargoDataInfoContext = alertCargoDataInfoContexts.FirstOrDefault();
                            if (alertCargoDataInfoContext.product_count > -1 &&
                                alertCargoDataInfoContext.product_count < alertCargoDataInfoContext.realtime_product_count)
                            {
                                // Update database
                                //DeliveryNumberContext deliveryNumberContext = _manager.QueryDeliveryNumberContextWithNo(deliveryingNumberDto.no);
                                //CargoDataInfoContext cargoDataInfoContext = _manager.QueryCargoDataInfoContextWithMaterial(deliveryingNumberContext.id, alertCargoDataInfoContext.material);
                                int qty = _manager.QueryQtyByCargoDataRecordContextWithInfoIds(alertCargoDataInfoContext.f_delivery_number_id, alertCargoDataInfoContext.id);
                                UpdateCargoDataInfoContextForRealtimeToRemoveAlert(ref alertCargoDataInfoContext, qty);
                                result = _manager.UpdateCargoDataInfoContext(alertCargoDataInfoContext);
                                UpdateDeliveryNumberContextWhenDismissAlert(ref deliveryingNumberContext, alertCargoDataInfoContexts.Count());
                                result = _manager.UpdateDeliveryNumberContextWhenDismissAlert(deliveryingNumberContext);
                            }
                            else if (alertCargoDataInfoContext.product_count == -1)
                            {
                                // Update database
                                //DeliveryNumberContext deliveryNumberContext = _manager.QueryDeliveryNumberContextWithNo(deliveryingNumberDto.no);
                                //CargoDataInfoContext cargoDataInfoContext = _manager.QueryCargoDataInfoContextWithMaterial(deliveryingNumberContext.id, alertCargoDataInfoContext.material);
                                UpdateCargoDataInfoContextForRealtimeToRemoveAlert(ref alertCargoDataInfoContext);
                                result = _manager.UpdateCargoDataInfoContext(alertCargoDataInfoContext);
                                UpdateDeliveryNumberContextWhenDismissAlert(ref deliveryingNumberContext, alertCargoDataInfoContexts.Count());
                                result = _manager.UpdateDeliveryNumberContextWhenDismissAlert(deliveryingNumberContext);
                            }
                        }

                        // Dismiss alert trigger
                        _httpClientManager.PostToDismissTriggerTerminalReader();

                        // Save to cache
                        _localMemoryCache.RemoveDeliveryingNumberDto();
                        _localMemoryCache.RemoveDeliveryNumberDtos();
                        _localMemoryCache.SaveCacheDidChange(true);

                        // Commit the transaction if everything is successful
                        transaction.Commit();

                        return GetResultDto(ResultEnum.True, ErrorEnum.None.ToChineseDescription());
                    }
                    catch (Exception exp)
                    {
                        // Handle exceptions and optionally roll back the transaction
                        transaction.Rollback();
                        return GetResultDto(ResultEnum.False, exp.Message);
                    }
                }
            }
        }

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
                throw new Exception(ErrorEnum.InvalidProperties.ToChineseDescription());
            }

            using (var connection = _manager.MsSqlConnectionRepository.InitConnection())
            {
                connection.Open();

                using (var transaction = _manager.MsSqlConnectionRepository.BeginTransaction())
                {
                    try
                    {
                        bool result = _manager.UpdateToDisableDeliveryNumberState(dto.no);
                        if (result == false)
                        {
                            throw new Exception(ErrorEnum.FailedToAccessDatabase.ToChineseDescription());
                        }

                        // Update cache
                        _localMemoryCache.RemoveDeliveryingNumberDto();
                        _localMemoryCache.RemoveDeliveryNumberDtos();
                        _localMemoryCache.SaveCacheDidChange(true);

                        // Commit the transaction if everything is successful
                        transaction.Commit();

                        return GetResultDto(ResultEnum.True, ErrorEnum.None.ToChineseDescription());
                    }
                    catch (Exception exp)
                    {
                        // Handle exceptions and optionally roll back the transaction
                        transaction.Rollback();
                        return GetResultDto(ResultEnum.False, exp.Message);
                    }
                }
            }
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
                return GetResultDto(ResultEnum.False, exp.Message);
            }

            using (var connection = _manager.MsSqlConnectionRepository.InitConnection())
            {
                connection.Open();

                using (var transaction = _manager.MsSqlConnectionRepository.BeginTransaction())
                {
                    try
                    {
                        // Check for duplicated file
                        DeliveryFileContext existDeliveryFileContext = _manager.QueryDeliveryFileContextWithFileName(dto.FileName);
                        if (existDeliveryFileContext != null)
                        {
                            throw new Exception(ErrorEnum.DuplicatedFileName.ToChineseDescription());
                        }

                        // Check for duplicated delivery
                        var deliverys = dto.FileData.Select(d => d.Delivery).ToList();
                        List<DeliveryFileContext> duplicatedFileContexts = _manager.QueryDeliveryFileContextWithDeliverys(deliverys);
                        if (duplicatedFileContexts != null)
                        {
                            throw new Exception(ErrorEnum.DuplicatedDelivery.ToChineseDescription());
                        }

                        // Insert into [eaton_delivery_file]
                        DeliveryFileContext insertDeliveryFileContext = GetDeliveryFileContext(dto);
                        result = _manager.InsertDeliveryFileContext(insertDeliveryFileContext);
                        if (result == false)
                        {
                            throw new Exception(ErrorEnum.FailedToAccessDatabase.ToChineseDescription());
                        }

                        // Query [eaton_delivery_file]
                        DeliveryFileContext insertedDeliveryFileContext = _manager.QueryDeliveryFileContextWithFileName(dto.FileName);
                        if (insertedDeliveryFileContext == null)
                        {
                            throw new Exception(ErrorEnum.FailedToAccessDatabase.ToChineseDescription());
                        }

                        // Insert into [eaton_delivery_number]
                        DeliveryNumberContext insertDeliveryNumberContext = GetDeliveryNumberContexts(insertedDeliveryFileContext.id, dto);
                        result = _manager.InsertDeliveryNumberContext(insertDeliveryNumberContext);
                        if (result == false)
                        {
                            throw new Exception(ErrorEnum.FailedToAccessDatabase.ToChineseDescription());
                        }

                        // Query [eaton_delivery_number]
                        DeliveryNumberContext insertedDeliveryNumberContext = _manager.QueryDeliveryNumberContextWithFileId(insertedDeliveryFileContext.id);
                        if (insertedDeliveryNumberContext == null)
                        {
                            throw new Exception(ErrorEnum.FailedToAccessDatabase.ToChineseDescription());
                        }

                        // Insert into [eaton_cargo_data]
                        List<CargoDataContext> insertCargoDataContexts = GetCargoDataContexts(insertedDeliveryNumberContext, dto.FileData);
                        result = _manager.InsertCargoDataContexts(insertCargoDataContexts);
                        if (result == false)
                        {
                            throw new Exception(ErrorEnum.FailedToAccessDatabase.ToChineseDescription());
                        }

                        // Query [eaton_cargo_data]
                        List<int> cargoIds = insertCargoDataContexts.Select(c => c.f_delivery_number_id).ToList();
                        List<CargoDataContext> insertedCargoDataContexts = _manager.QueryCargoDataContextsWithCargoId(cargoIds);
                        if (insertedCargoDataContexts == null)
                        {
                            throw new Exception(ErrorEnum.FailedToAccessDatabase.ToChineseDescription());
                        }

                        // Insert into [eaton_cargo_data_info]
                        List<CargoDataInfoContext> insertCargoDataInfoContext = GetCargoDataInfoContexts(insertedCargoDataContexts);
                        result = _manager.InsertCargoDataInfoContexts(insertCargoDataInfoContext);
                        if (result == false)
                        {
                            throw new Exception(ErrorEnum.FailedToAccessDatabase.ToChineseDescription());
                        }

                        // Query [eaton_cargo_data_info]
                        List<CargoDataInfoContext> insertedCargoDataInfoContext = _manager.QueryCargoDataInfoContexts(cargoIds);
                        if (insertedCargoDataInfoContext == null)
                        {
                            throw new Exception(ErrorEnum.FailedToAccessDatabase.ToChineseDescription());
                        }

                        _localMemoryCache.RemoveDeliveryingNumberDto();
                        _localMemoryCache.RemoveDeliveryNumberDtos();
                        _localMemoryCache.SaveCacheDidChange(true);

                        // Commit the transaction if everything is successful
                        transaction.Commit();

                        return GetResultDto(ResultEnum.True, ErrorEnum.None.ToChineseDescription());
                    }
                    catch (Exception exp)
                    {
                        // Handle exceptions and optionally roll back the transaction
                        transaction.Rollback();
                        return GetResultDto(ResultEnum.False, exp.Message);
                    }
                }
            }
        }

        #endregion

        #region Private methods

        private ResultDto GetResultDto(ResultEnum result, string error)
        {
            return new ResultDto
            {
                Result = result.ToBoolean(),
                Error = error,
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
            int palletCount = dto.FileData.Sum(f => int.Parse(f.Unit));
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
                state = DeliveryStateEnum.New.ToDescription(),
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
                pallet_count = context.Sum(c => c.unit),
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

        private DeliveryNumberResultDto GetDeliveryNumberResultDto(ResultEnum result, string error, List<DeliveryNumberDto> dto)
        {
            return new DeliveryNumberResultDto
            {
                Result = result.ToBoolean(),
                Error = error,
                DeliveryNumberDtos = dto,
            };
        }

        private void UpdateDeliveryNumberDtoWhenStart(ref DeliveryNumberDto dto)
        {
            dto.start_time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            dto.state = DeliveryStateEnum.Delivery.ToDescription(); // 0: new, 1: select, 2: deliverying, 3: finish/search/review, 4: edit, -1: alert/pause
        }

        private void UpdateDeliveryNumberDtoWhenFinish(ref DeliveryNumberDto dto)
        {
            dto.end_time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            dto.state = DeliveryStateEnum.Finish.ToDescription(); // 0: new, 1: select, 2: deliverying, 3: finish/search/review, 4: edit, -1: alert/pause
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

        private void UpdateDeliveryNumberContextWhenDismissAlert(ref DeliveryNumberContext context, int alertNumber)
        {
            if (alertNumber == 1)
            {
                context.state = DeliveryStateEnum.Delivery.ToDescription();
            }
        }

        private void UpdateDeliveryNumberContextWithInvalidPallet(ref DeliveryNumberContext context)
        {
            context.invalid_pallet_quantity += 1;
            context.state = DeliveryStateEnum.Alert.ToDescription();
        }

        private void UpdateDeliveryNumberContextWithValidQty(ref DeliveryNumberContext context, int alertNumber)
        {
            context.valid_pallet_quantity += 1;
            context.miss_pallet_quantity -= 1;
            if (alertNumber == 0)
            {
                context.state = DeliveryStateEnum.Delivery.ToDescription();
            }
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
            context.alert += 1;
        }

        private void UpdateCargoDataInfoContextForRealtimeToRemoveAlert(ref CargoDataInfoContext context)
        {
            context.alert -= 1;
        }

        private void UpdateCargoDataInfoContextForRealtimeToRemoveAlert(ref CargoDataInfoContext context, int qty)
        {
            context.realtime_product_count -= qty;
            context.realtime_pallet_count -= 1;
            context.alert -= 1;
        }

        #endregion
    }
}
