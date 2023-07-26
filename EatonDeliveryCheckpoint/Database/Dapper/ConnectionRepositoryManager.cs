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

        public List<DeliveryCargoDto> QueryDeliveryCargoDtos()
            => MsSqlConnectionRepository.QueryDeliveryCargoDtos();

        public List<DeliveryCargoDataDto> QueryDeliveryCargoDataDtos(string no)
            => MsSqlConnectionRepository.QueryDeliveryCargoDataDtos(no);

        public DeliveryFileContext QueryDeliveryFileContextWithFileName(string fileName)
            => MsSqlConnectionRepository.QueryDeliveryFileContextWithFileName(fileName);

        public List<DeliveryCargoContext> QueryDeliveryCargoContextsWithFileId(int id)
            => MsSqlConnectionRepository.QueryDeliveryCargoContextsWithFileId(id);

        public List<DeliveryCargoDataContext> QueryDeliveryCargoDataContextsWithCargoId(List<int> ids)
            => MsSqlConnectionRepository.QueryDeliveryCargoDataContextsWithCargoId(ids);

        public List<DeliveryCargoDataRealtimeContext> QueryDeliveryCargoDataRealtimeContextsWithCargoDataIds(List<int> ids)
            => MsSqlConnectionRepository.QueryDeliveryCargoDataRealtimeContextsWithCargoDataIds(ids);

        #endregion

        #region Insert

        public bool InsertDeliveryFileContext(DeliveryFileContext context)
            => MsSqlConnectionRepository.InsertDeliveryFileContext(context);

        public bool InsertDeliveryCargoContexts(List<DeliveryCargoContext> contexts)
            => MsSqlConnectionRepository.InsertDeliveryCargoContexts(contexts);

        public bool InsertDeliveryCargoDataContexts(List<DeliveryCargoDataContext> contexts)
            => MsSqlConnectionRepository.InsertDeliveryCargoDataContexts(contexts);

        public bool InsertDeliveryCargoDataRealtimeContexts(List<DeliveryCargoDataRealtimeContext> contexts)
            => MsSqlConnectionRepository.InsertDeliveryCargoDataRealtimeContexts(contexts);

        #endregion
    }
}
