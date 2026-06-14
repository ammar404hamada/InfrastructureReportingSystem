using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraReportingSystem.Domain.AI
{
    public interface IDescriptionAIClient
    {
<<<<<<< HEAD
        Task<string> GenerateDescriptionAsync(Stream imageStream, string fileName);
    }
=======
        Task<string> GenerateDescriptionAsync(Stream imageStream, string fileName, string contentType);
    }

>>>>>>> 63ae425cacb8f085d875228d89a74a932d83f2df
}
