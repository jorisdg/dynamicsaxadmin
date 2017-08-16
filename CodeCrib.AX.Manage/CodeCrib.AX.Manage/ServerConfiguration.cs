//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.Manage
{
    class EndPoint
    {
        public static Queries.QueryServiceClient GetQueryClient(string serverName, int wsdlPort)
        {
            Queries.QueryServiceClient queryClient = new Queries.QueryServiceClient(
                new System.ServiceModel.NetTcpBinding() { TransferMode = System.ServiceModel.TransferMode.Streamed },
                new System.ServiceModel.EndpointAddress(String.Format("net.tcp://{0}:{1}/DynamicsAx/Services/QueryService", serverName, wsdlPort)));

            queryClient.Endpoint.Contract = System.ServiceModel.Description.ContractDescription.GetContract(typeof(Queries.IQueryService));

            return queryClient;
        }

        public static GDS.GenericDocumentServiceClient GetGDSClient(string serverName, int wsdlPort)
        {
            GDS.GenericDocumentServiceClient gdsClient = new GDS.GenericDocumentServiceClient(
                new System.ServiceModel.NetTcpBinding(),// { TransferMode = System.ServiceModel.TransferMode.Streamed },
                new System.ServiceModel.EndpointAddress(String.Format("net.tcp://{0}:{1}/DynamicsAx/Services/AifGDS", serverName, wsdlPort)));

            gdsClient.Endpoint.Contract = System.ServiceModel.Description.ContractDescription.GetContract(typeof(GDS.GenericDocumentService));

            return gdsClient;
        }
    }

    public class ServerConfiguration
    {
        public Int64 ClusterRefRecId
        {
            get;
            set;
        }
        public byte EnableBatch
        {
            get;
            set;
        }
        public byte LoadBalancingEnabled
        {
            get;
            set;
        }
        public Int32 MaxSessions
        {
            get;
            set;
        }
        public Guid ServerGuid
        {
            get;
            set;
        }
        public string ServerId
        {
            get;
            set;
        }
        public Int32 TimeAlive
        {
            get;
            set;
        }

        public List<BatchConfiguration> BatchConfigurations
        {
            get;
            set;
        }

        protected static Queries.QueryMetadata Query()
        {
            Queries.QueryMetadata query = new Queries.QueryMetadata();
            Queries.QueryDataSourceMetadata datasource = new Queries.QueryDataSourceMetadata();
            datasource.Table = "SysServerConfig";
            datasource.DynamicFieldList = true;
            datasource.Name = "SysServerConfig";
            datasource.Enabled = true;
            datasource.OrderMode = Queries.OrderMode.OrderBy;

            query.DataSources = new List<Queries.QueryDataSourceMetadata>();
            query.DataSources.Add(datasource);
            query.OrderByFields = new List<Queries.QueryOrderByMetadata>();
            query.OrderByFields.Add(new Queries.QueryDataOrderByMetadata() { DataSource = datasource.Name, SortOrder = Queries.SortOrder.Ascending, FieldName = "RecId" });

            return query;
        }

        protected static GDS.QueryMetadata GetGDSQuery()
        {
            GDS.QueryMetadata query = new GDS.QueryMetadata();
            GDS.QueryDataSourceMetadata datasource = new GDS.QueryDataSourceMetadata();
            datasource.Table = "SysServerConfig";
            datasource.DynamicFieldList = true;
            datasource.DynamicFieldListSpecified = true;
            datasource.Name = "SysServerConfig";
            datasource.Enabled = true;
            datasource.EnabledSpecified = true;
           // datasource.OrderMode = GDS.OrderMode.OrderBy;

            query.DataSources = new GDS.QueryDataSourceMetadata[1]; // new List<GDS.QueryDataSourceMetadata>();
            query.DataSources[0] = datasource; //query.DataSources.Add(datasource);

            //query.OrderByFields = new GDS.QueryOrderByMetadata[1]; // new List<GDS.QueryOrderByMetadata>();
            //query.OrderByFields[0] = new GDS.QueryDataOrderByMetadata() { DataSource = datasource.Name, SortOrder = GDS.SortOrder.Ascending, FieldName = "RecId" };

            return query;
        }

        protected static List<ServerConfiguration> RowsToConfigs(DataSet data)
        {
            List<ServerConfiguration> configs = new List<ServerConfiguration>();

            if (data != null && data.Tables != null && data.Tables.Count >= 1)
            {
                foreach (DataRow row in data.Tables[0].Rows)
                {
                    configs.Add(new ServerConfiguration()
                    {
                        ServerGuid = row.Field<Guid>("ServerGuid"),
                        ServerId = row.Field<string>("ServerId"),
                        ClusterRefRecId = row.Field<Int64>("ClusterRefRecId"),
                        EnableBatch = row.Field<byte>("EnableBatch"),
                        LoadBalancingEnabled = row.Field<byte>("LoadBalancingEnabled"),
                        MaxSessions = row.Field<Int32>("MaxSessions"),
                        TimeAlive = row.Field<Int32>("TimeAlive")
                    });
                }
            }

            return configs;
        }

        public static async Task<List<ServerConfiguration>> GetAsync()
        {
            return await GetAsync("localhost", 8201);
        }

        public static async Task<List<ServerConfiguration>> GetAsync(string serverName, int wsdlPort)
        {
            Queries.QueryServiceClient queryClient = EndPoint.GetQueryClient(serverName, wsdlPort);

            Queries.QueryMetadata query = ServerConfiguration.Query();

            Queries.PositionBasedPaging paging = new Queries.PositionBasedPaging();
            paging.StartingPosition = 1;
            paging.NumberOfRecordsToFetch = 100;

            Queries.ExecuteQueryRequest request = new Queries.ExecuteQueryRequest(query, paging);
            Task<Queries.ExecuteQueryResponse> queryTask = queryClient.ExecuteQueryAsync(request);

            await queryTask;

            List<ServerConfiguration> configs = null;

            if (queryTask.Result != null)
            {
                configs = ServerConfiguration.RowsToConfigs(queryTask.Result.ExecuteQueryResult);

                // Get tasks for fetching batch configuration for each server
                var tasks = from server in configs select BatchConfiguration.GetAsync(server.ServerId, serverName, wsdlPort);
                // Await all batch configuration calls to return
                List<BatchConfiguration>[] batches = await Task.WhenAll(tasks);

                // Assign each batch configuration to its respective server
                foreach (var server in configs)
                {
                    var batchConfig = from b in batches where b[0].ServerId == server.ServerId select b;

                    if (batchConfig != null && batchConfig.Count() > 0)
                    {
                        server.BatchConfigurations = batchConfig.First().ToList();
                    }
                }
            }

            return configs;
        }

        public static List<ServerConfiguration> Get()
        {
            return Get("localhost", 8201);
        }

        public static List<ServerConfiguration> Get(int wsdlPort)
        {
            return Get("localhost", wsdlPort);
        }

        public static List<ServerConfiguration> Get(string serverName, int wsdlPort)
        {
            //Queries.QueryServiceClient queryClient = EndPoint.GetQueryClient(serverName, wsdlPort);
            GDS.GenericDocumentServiceClient client = EndPoint.GetGDSClient(serverName, wsdlPort);
            //Queries.QueryMetadata query = ServerConfiguration.Query();
            GDS.QueryMetadata query = ServerConfiguration.GetGDSQuery();
            //GDS.CallContext callContext = new GDS.CallContext();

            GDS.QueryCriteria criteria = new GDS.QueryCriteria();
            //criteria.CriteriaElement = new GDS.CriteriaElement[1];
            //criteria.CriteriaElement[0] = new GDS.CriteriaElement() { DataSourceName = query.DataSources[0].Name, FieldName = "ServerId", Operator = GDS.Operator.Equal, Value1 = "01@JDEGRUYTERW8" };

            //Queries.Paging paging = new Queries.PositionBasedPaging();
            //(paging as Queries.PositionBasedPaging).StartingPosition = 1;
            //(paging as Queries.PositionBasedPaging).NumberOfRecordsToFetch = 100;

            //DataSet data = queryClient.ExecuteQuery(query, ref paging);
            GDS.EntityKey[] keys = client.findKeys(null, query, criteria);
            var test = client.read(null, query, keys);

            //List<ServerConfiguration> configs = null;

            //if (data != null)
            //{
            //    configs = ServerConfiguration.RowsToConfigs(data);

            //    foreach (var server in configs)
            //    {
            //        server.BatchConfigurations = BatchConfiguration.Get(server.ServerId);
            //    }
            //}

            //return configs;
            return null;
        }

        public static void Set(string serverName, int wsdlPort)
        {
            GDS.GenericDocumentServiceClient gdsClient = EndPoint.GetGDSClient(serverName, wsdlPort);
            GDS.CallContext callContext = new GDS.CallContext();

            GDS.QueryMetadata metaData;
            GDS.EntityKey[] keys = new GDS.EntityKey[1];
            keys[0] = new GDS.EntityKey();
            keys[0].KeyData = new GDS.KeyField[1];
            keys[0].KeyData[0] = new GDS.KeyField() { Field = String.Empty, Value = string.Empty };

            //gdsClient.update(callContext, metaData, keys, "");
            //gdsClient.find(callContext, meta
        }
    }

    public class BatchConfiguration
    {
        public string ServerId
        {
            get;
            set;
        }
        public Int32 MaxBatchSessions
        {
            get;
            set;
        }
        public DateTime StartTime
        {
            get;
            set;
        }
        public DateTime EndTime
        {
            get;
            set;
        }

        protected static Queries.QueryMetadata Query(string serverId)
        {
            Queries.QueryMetadata query = new Queries.QueryMetadata();
            Queries.QueryDataSourceMetadata datasource = new Queries.QueryDataSourceMetadata();
            datasource.Table = "BatchServerConfig";
            datasource.DynamicFieldList = true;
            datasource.Name = "BatchServerConfig";
            datasource.Enabled = true;
            datasource.OrderMode = Queries.OrderMode.OrderBy;

            datasource.Ranges = new List<Queries.QueryRangeMetadata>();
            datasource.Ranges.Add(new Queries.QueryDataRangeMetadata() { Enabled = true, FieldName = "ServerId", Value = serverId });

            query.DataSources = new List<Queries.QueryDataSourceMetadata>();
            query.DataSources.Add(datasource);
            query.OrderByFields = new List<Queries.QueryOrderByMetadata>();
            query.OrderByFields.Add(new Queries.QueryDataOrderByMetadata() { DataSource = datasource.Name, SortOrder = Queries.SortOrder.Ascending, FieldName = "RecId" });

            return query;
        }

        protected static List<BatchConfiguration> RowsToConfigs(DataSet data)
        {
            List<BatchConfiguration> configs = new List<BatchConfiguration>();

            if (data != null && data.Tables != null && data.Tables.Count >= 1)
            {
                foreach (DataRow row in data.Tables[0].Rows)
                {
                    var startTime = TimeSpan.FromSeconds(row.Field<Int32>("StartTime"));
                    var endTime = TimeSpan.FromSeconds(row.Field<Int32>("EndTime"));

                    configs.Add(new BatchConfiguration()
                    {
                        ServerId = row.Field<string>("ServerId"),
                        MaxBatchSessions = row.Field<Int32>("MaxBatchSessions"),
                        StartTime = new DateTime(startTime.Ticks, DateTimeKind.Local),
                        EndTime = new DateTime(endTime.Ticks, DateTimeKind.Local)
                    });
                }
            }

            return configs;
        }

        public static List<BatchConfiguration> Get(string serverId)
        {
            return Get(serverId, "localhost", 8201);
        }

        public static List<BatchConfiguration> Get(string serverId, string serverName, int wsdlPort)
        {
            Queries.QueryServiceClient queryClient = EndPoint.GetQueryClient(serverName, wsdlPort);

            Queries.QueryMetadata query = BatchConfiguration.Query(serverId);

            Queries.Paging paging = new Queries.PositionBasedPaging();
            (paging as Queries.PositionBasedPaging).StartingPosition = 1;
            (paging as Queries.PositionBasedPaging).NumberOfRecordsToFetch = 100;

            Queries.ExecuteQueryRequest request = new Queries.ExecuteQueryRequest(query, paging);
            DataSet data = queryClient.ExecuteQuery(query, ref paging);

            List<BatchConfiguration> configs = null;

            configs = BatchConfiguration.RowsToConfigs(data);

            return configs;
        }

        public static async Task<List<BatchConfiguration>> GetAsync(string serverId)
        {
            return await GetAsync(serverId, "localhost", 8201);
        }

        public static async Task<List<BatchConfiguration>> GetAsync(string serverId, string serverName, int wsdlPort)
        {
            Queries.QueryServiceClient queryClient = EndPoint.GetQueryClient(serverName, wsdlPort);

            Queries.QueryMetadata query = BatchConfiguration.Query(serverId);

            Queries.Paging paging = new Queries.PositionBasedPaging();
            (paging as Queries.PositionBasedPaging).StartingPosition = 1;
            (paging as Queries.PositionBasedPaging).NumberOfRecordsToFetch = 100;

            Queries.ExecuteQueryRequest request = new Queries.ExecuteQueryRequest(query, paging);
            Task<Queries.ExecuteQueryResponse> queryTask = queryClient.ExecuteQueryAsync(request);

            await queryTask;

            List<BatchConfiguration> configs = null;

            if (queryTask.Result != null)
            {
                configs = BatchConfiguration.RowsToConfigs(queryTask.Result.ExecuteQueryResult);
            }

            return configs;
        }
    }
}
