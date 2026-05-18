using SmartResumeAnalyzer.Core.Entities;
using SmartResumeAnalyzer.Core.Interfaces;

namespace SmartResumeAnalyzer.Infrastructure.Services
{
    public class AnalysisLogService : IAnalysisLogService
    {
        private readonly IAnalysisLogRepository _repository;

        public AnalysisLogService(IAnalysisLogRepository repository)
        {
            _repository = repository;
        }

        public async Task<AnalysisLog> CreateLogAsync(
            Guid? userId,
            string? ipAddress,
            string jobTitle,
            string companyName,
            string jobDescription,
            string seniorityLevel,
            string originalFileName,
            string storedFileName,
            string resultJson)
        {
            var log = new AnalysisLog
            {
                UserId = userId,
                IpAddress = ipAddress,
                JobTitle = jobTitle,
                CompanyName = companyName,
                JobDescription = jobDescription,
                SeniorityLevel = seniorityLevel,
                OriginalFileName = originalFileName,
                StoredFileName = storedFileName,
                ResultJson = resultJson
            };

            await _repository.AddAsync(log);
            await _repository.SaveChangesAsync();

            return log;
        }
    }
}