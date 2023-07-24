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



        public List<DeliveryFileDto> QueryUploadedDeliveryFiles()
            => MsSqlConnectionRepository.QueryUploadedDeliveryFiles();

        #region Query

        public DeliveryFileContext QueryDeliveryFileContextWithFileName(string fileName)
            => MsSqlConnectionRepository.QueryDeliveryFileContextWithFileName(fileName);

        public List<DeliveryFileDataContext> QueryDeliveryDataContextsWithFileName(string fileName)
            => MsSqlConnectionRepository.QueryDeliveryDataContextsWithFileName(fileName);

        public List<DeliveryWorkContext> QueryDeliveryWorkContextsWithFileName(string fileName)
            => MsSqlConnectionRepository.QueryDeliveryWorkContextsWithFileName(fileName);

        #endregion

        #region Insert

        public bool InsertDeliveryFileContext(DeliveryFileContext context)
            => MsSqlConnectionRepository.InsertDeliveryFileContext(context);

        public bool InsertDeliveryDataContexts(List<DeliveryFileDataContext> contexts)
            => MsSqlConnectionRepository.InsertDeliveryDataContexts(contexts);

        public bool InsertDeliveryWorkContexts(List<DeliveryWorkContext> contexts)
            => MsSqlConnectionRepository.InsertDeliveryWorkContexts(contexts);

        #endregion
    }
}
