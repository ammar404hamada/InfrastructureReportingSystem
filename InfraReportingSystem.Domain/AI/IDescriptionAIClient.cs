using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Domain.AI
{
    public interface IDescriptionAIClient
    {
        Task<string> GenerateDescriptionAsync(Stream imageStream, string fileName, string contentType);
    }

}
