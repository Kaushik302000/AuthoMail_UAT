using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AM.Services
{
    public class JsonResponseFactory
    {
        public static object ErrorResponse(string code, string error)
        {
            return ErrorResponse(code, error, null);
        }
        public static object ErrorResponse(List<string> errors)
        {
            return ErrorResponse(null, null, errors);
        }

        public static object ErrorResponse(string code, string error, object referenceObject)
        {
            return new { Success = false, ErrorCode = code, ErrorMessage = error, Object = referenceObject };
        }

        public static object ErrorResponse(string error)
        {
            return new { Success = false, ErrorMessage = error };
        }

        public static object SuccessResponse()
        {
            return new { Success = true };
        }

        public static object SuccessResponse(object referenceObject)
        {
            return new { Success = true, Object = referenceObject };
        }
    }
}
