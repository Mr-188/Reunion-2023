using System;
using System.Collections.Generic;
using System.Text;

namespace ReunionShareModel
{
    public abstract class ApiResponse
    {
    }

    public abstract class ApiRequest
    {
        public abstract string Path { get; }
    }

    public abstract class ApiRequest<TResponse> : ApiRequest
        where TResponse : ApiResponse
    {
    }

    public class EmptyResponse : ApiResponse
    {
    }
}