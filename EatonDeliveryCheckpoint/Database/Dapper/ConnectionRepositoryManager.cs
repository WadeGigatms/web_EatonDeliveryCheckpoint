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

        public int QueryCargoNoCount()
            => MsSqlConnectionRepository.QueryCargoNoCount();

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

        internal List<CargoDataInfoContext> QueryCargoDataInfoContexts(List<int> ids)
            => MsSqlConnectionRepository.QueryCargoDataInfoContexts(ids);

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

        #endregion

        #region Update

        public bool UpdateDeliveryCargoWhenStart(DeliveryCargoDto dto)
            => MsSqlConnectionRepository.UpdateDeliveryCargoWhenStart(dto);

        #endregion
    }
}
