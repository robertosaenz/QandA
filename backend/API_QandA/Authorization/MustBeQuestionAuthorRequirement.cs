using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_QandA.Authorization
{
    public class MustBeQuestionAuthorRequirement :IAuthorizationRequirement
    {
        public MustBeQuestionAuthorRequirement()
        {
        }
    }
}
