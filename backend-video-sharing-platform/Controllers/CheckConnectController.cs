using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Mvc;

namespace backend_video_sharing_platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckConnectController : ControllerBase
    {
        private readonly ILogger<CheckConnectController> _logger;
        private readonly IAmazonDynamoDB _dynamoDbClient;

        public CheckConnectController(ILogger<CheckConnectController> logger, IAmazonDynamoDB dynamoDbClient)
        {
            _logger = logger;
            _dynamoDbClient = dynamoDbClient;
        }

        [HttpGet("pingDynamoDB")]
        public async Task<bool> PingDynamoDB()
        {
            try
            {
                _logger.LogInformation("Pinging DynamoDB...");
                var response = await _dynamoDbClient.ListTablesAsync();
                if (response.TableNames.Count > 0)
                {
                    _logger.LogInformation($"✅ Connected! Found {response.TableNames.Count} tables. First table: {response.TableNames[0]}");
                }
                else
                {
                    _logger.LogInformation("✅ Connected, but no tables found.");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error pinging DynamoDB: {ex.Message}");
                return false;
            }
        }
    }
}
