using EatonDeliveryCheckpoint.Dtos;
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


        public int QueryDeliveryCargoCount()
            => MsSqlConnectionRepository.QueryDeliveryCargoCount();

        public List<DeliveryCargoDto> QueryDeliveryCargoDtos()
            => MsSqlConnectionRepository.QueryDeliveryCargoDtos();

        public List<DeliveryCargoDataDto> QueryDeliveryCargoDataDtos(string no)
            => MsSqlConnectionRepository.QueryDeliveryCargoDataDtos(no);

        public DeliveryFileContext QueryDeliveryFileContextWithFileName(string fileName)
            => MsSqlConnectionRepository.QueryDeliveryFileContextWithFileName(fileName);

        public List<DeliveryCargoContext> QueryDeliveryCargoContextsWithFileId(int id)
            => MsSqlConnectionRepository.QueryDeliveryCargoContextsWithFileId(id);

        public List<CargoDataContext> QueryCargoDataContextsWithCargoId(List<int> ids)
            => MsSqlConnectionRepository.QueryDeliveryCargoDataContextsWithCargoId(ids);

        public List<CargoDataInfoContext> QueryCargoDataInfoContexts(List<int> ids)
            => MsSqlConnectionRepository.QueryCargoDataInfoContexts(ids);

        public List<DeliveryCargoDto> QueryDeliveryCargoDtosWithState(int state)
            => MsSqlConnectionRepository.QueryDeliveryCargoDtosWithState(state);

        public CargoDataInfoContext QueryCargoDataInfoContextWithMaterial(int cargo_id, string material)
            => MsSqlConnectionRepository.QueryCargoDataInfoContextWithMaterial(cargo_id, material);

        public CargoDataInfoContext QueryCargoDataInfoContextWithInvalidContext(CargoDataInfoContext context)
            => MsSqlConnectionRepository.QueryCargoDataInfoContextWithInvalidContext(context);

        public CargoDataInfoContext QueryCargoDataInfoContextWithInvalidNo(string no)
            => MsSqlConnectionRepository.QueryCargoDataInfoContextWithInvalidNo(no);

        public DeliveryCargoContext QueryDeliveryCargoContextWithId(int id)
            => MsSqlConnectionRepository.QueryDeliveryCargoContextWithId(id);

        public DeliveryCargoContext QueryDeliveryCargoContextWithNo(string no)
            => MsSqlConnectionRepository.QueryDeliveryCargoContextWithNo(no);

        public int QueryQtyByCargoDataRecordContextWithInfoIds(int cargo_id, int info_id)
            => MsSqlConnectionRepository.QueryQtyByCargoDataRecordContextWithInfoIds(cargo_id, info_id);

        #endregion

        #region Insert

        public bool InsertDeliveryFileContext(DeliveryFileContext context)
            => MsSqlConnectionRepository.InsertDeliveryFileContext(context);

        public bool InsertDeliveryCargoContexts(List<DeliveryCargoContext> contexts)
            => MsSqlConnectionRepository.InsertDeliveryCargoContexts(contexts);

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

        public bool UpdateDeliveryCargoContextWhenStart(DeliveryCargoDto dto)
            => MsSqlConnectionRepository.UpdateDeliveryCargoContextWhenStart(dto);

        public bool UpdateDeliveryCargoContextWhenFinish(DeliveryCargoDto dto)
            => MsSqlConnectionRepository.UpdateDeliveryCargoContextWhenFinish(dto);

        public bool UpdateDeliveryCargoContextWhenDataInserted(DeliveryCargoContext context)
            => MsSqlConnectionRepository.UpdateDeliveryCargoContextWhenDataInserted(context);

        public bool UpdateDeliveryCargoContextWhenQuit(DeliveryCargoContext context)
            => MsSqlConnectionRepository.UpdateDeliveryCargoContextWhenQuit(context);

        public bool UpdateCargoDataInfoContext(CargoDataInfoContext context)
            => MsSqlConnectionRepository.UpdateCargoDataInfoContext(context);

        #endregion
    }
}
