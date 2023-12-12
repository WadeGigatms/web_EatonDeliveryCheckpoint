using EatonDeliveryCheckpoint.Dtos;
using EatonDeliveryCheckpoint.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Database.Dapper
{
    public class ConnectionRepositoryManager
    {
        public MsSqlConnectionRepository MsSqlConnectionRepository { get; private set; }

        public ConnectionRepositoryManager(MsSqlConnectionRepository msSqlConnectionRepository)
        {
            MsSqlConnectionRepository = msSqlConnectionRepository;
        }

        #region Query


        public int QueryDeliveryNumberContextCount()
            => MsSqlConnectionRepository.QueryDeliveryNumberContextCount();

        public List<DeliveryNumberDto> QueryDeliveryNumberDtos()
            => MsSqlConnectionRepository.QueryDeliveryNumberDtos();

        public List<DeliveryNumberDataDto> QueryDeliveryNumberDataDtos(string no)
            => MsSqlConnectionRepository.QueryDeliveryNumberDataDtos(no);

        public DeliveryFileContext QueryDeliveryFileContextWithFileName(string fileName)
            => MsSqlConnectionRepository.QueryDeliveryFileContextWithFileName(fileName);

        public List<DeliveryFileContext> QueryDeliveryFileContextWithDeliverys(List<string> deliverys)
            => MsSqlConnectionRepository.QueryDeliveryFileContextWithDeliverys(deliverys);

        public DeliveryNumberContext QueryDeliveryNumberContextWithFileId(int id)
            => MsSqlConnectionRepository.QueryDeliveryNumberContextWithFileId(id);

        public List<CargoDataContext> QueryCargoDataContextsWithCargoId(List<int> ids)
            => MsSqlConnectionRepository.QueryDeliveryCargoDataContextsWithCargoId(ids);

        public List<CargoDataInfoContext> QueryCargoDataInfoContexts(List<int> ids)
            => MsSqlConnectionRepository.QueryCargoDataInfoContexts(ids);

        public List<DeliveryNumberDto> QueryDeliveryNumberDtosWithState(DeliveryStateEnum state)
            => MsSqlConnectionRepository.QueryDeliveryNumberDtosWithState(state);

        public CargoDataInfoContext QueryCargoDataInfoContextWithMaterial(int cargo_id, string material)
            => MsSqlConnectionRepository.QueryCargoDataInfoContextWithMaterial(cargo_id, material);

        public CargoDataInfoContext QueryCargoDataInfoContextWithInvalidContext(CargoDataInfoContext context)
            => MsSqlConnectionRepository.QueryCargoDataInfoContextWithInvalidContext(context);

        public DeliveryNumberContext QueryDeliveryNumberContextWithNo(string no)
            => MsSqlConnectionRepository.QueryDeliveryNumberContextWithNo(no);

        public int QueryQtyByCargoDataRecordContextWithInfoIds(int cargo_id, int info_id)
            => MsSqlConnectionRepository.QueryQtyByCargoDataRecordContextWithInfoIds(cargo_id, info_id);

        public DeliveryNumberDto QueryDeliveryNumberDtoWithDelivery(string delivery)
            => MsSqlConnectionRepository.QueryDeliveryNumberDtoWithDelivery(delivery);

        public DeliveryNumberDto QueryDeliveryNumberDtoWithNo(string no)
            => MsSqlConnectionRepository.QueryDeliveryNumberDtoWithNo(no);

        public List<DeliveryNumberDataDto> QueryValidDeliveryNumberDataDtos(string no)
            => MsSqlConnectionRepository.QueryValidDeliveryNumberDataDtos(no);

        public List<DeliveryNumberDataDto> QueryInvalidDeliveryNumberDataDtos(string no)
            => MsSqlConnectionRepository.QueryInvalidDeliveryNumberDataDtos(no);

        #endregion

        #region Insert

        public bool InsertDeliveryFileContext(DeliveryFileContext context)
            => MsSqlConnectionRepository.InsertDeliveryFileContext(context);

        public bool InsertDeliveryNumberContext(DeliveryNumberContext context)
            => MsSqlConnectionRepository.InsertDeliveryNumberContext(context);

        public bool InsertCargoDataContexts(List<CargoDataContext> contexts)
            => MsSqlConnectionRepository.InsertCargoDataContexts(contexts);

        public bool InsertCargoDataInfoContexts(List<CargoDataInfoContext> contexts)
            => MsSqlConnectionRepository.InsertCargoDataInfoContexts(contexts);

        public bool InsertCargoDataInfoContext(CargoDataInfoContext context)
            => MsSqlConnectionRepository.InsertCargoDataInfoContext(context);

        public bool InsertCargoDataRecordContext(CargoDataRecordContext context)
            => MsSqlConnectionRepository.InsertCargoDataRecordContext(context);

        #endregion

        #region Update

        public bool UpdateDeliveryNumberContextWhenStart(DeliveryNumberDto dto)
            => MsSqlConnectionRepository.UpdateDeliveryNumberContextWhenStart(dto);

        public bool UpdateDeliveryNumberContextWhenFinish(DeliveryNumberDto dto)
            => MsSqlConnectionRepository.UpdateDeliveryNumberContextWhenFinish(dto);

        public bool UpdateDeliveryNumberContextWhenDone(DeliveryNumberDto dto)
            => MsSqlConnectionRepository.UpdateDeliveryNumberContextWhenDone(dto);

        public bool UpdateDeliveryNumberContextWhenDataInserted(DeliveryNumberContext context)
            => MsSqlConnectionRepository.UpdateDeliveryNumberContextWhenDataInserted(context);

        public bool UpdateDeliveryNumberContextWhenDismissAlert(DeliveryNumberContext context)
            => MsSqlConnectionRepository.UpdateDeliveryNumberContextWhenDismissAlert(context);

        public bool UpdateCargoDataInfoContext(CargoDataInfoContext context)
            => MsSqlConnectionRepository.UpdateCargoDataInfoContext(context);

        public bool UpdateToDisableDeliveryNumberState(string no)
            => MsSqlConnectionRepository.UpdateToDisableDeliveryNumberState(no);

        #endregion
    }
}
