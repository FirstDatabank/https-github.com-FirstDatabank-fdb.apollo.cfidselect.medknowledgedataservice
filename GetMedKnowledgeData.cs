using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;

namespace fdb.apollo.cfidselect.mkdataservice
{
    public class GetMedKnowledgeData
    {
        private IConfiguration _configuration;

        public GetMedKnowledgeData(IConfiguration configuration)
        
        {
            _configuration = configuration;
        }

        [FunctionName("getmedknowledgedata")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function process started.");
            var result = GetData(log);
            if (result.isSuccess)
                return new OkObjectResult(result.Data);
            else
                return new BadRequestObjectResult(result.ErrorMessage);

        }

        private QueryResult GetData(ILogger log)
        {
            var query = @"SELECT * FROM (SELECT ndc, gcn_seqno, df, daddnc AS daddnc_fdb, obsdtec AS obsdtec_fdb,dupdc AS dupdc_fdb FROM wizard.ubr_rndc_dy) WHERE ROWNUM <= 10";
            var connString = _configuration["OracleConnString"];
            var conn = new OracleConnection(connString);
            
            var list = new List<string>();
            string errorMessage = "";
            try
            {
                conn.Open();
                log.LogInformation("Oracle connection open");
                using (var dbCommand = conn.CreateCommand())
                {
                    dbCommand.CommandText = query;
                    using (var reader = dbCommand.ExecuteReader())
                    {
                        log.LogInformation("Command executed");
                        while (reader.Read())
                        {
                            list.Add(reader.GetString(0));
                        }
                    }
                }
                return new QueryResult { Data = list, isSuccess = true };

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                errorMessage = ex.Message + " " + ex?.InnerException?.Message;
                return new QueryResult { isSuccess = false, ErrorMessage = errorMessage };
            }
            finally
            {
                conn.Close();
            }


        }
    }
}

