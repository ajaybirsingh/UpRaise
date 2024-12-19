using System.Collections.Generic;

namespace UpRaise.DTOs
{
    public class ResultDTO
    {
        public ResultDTO()
        {
        }

        public ResultDTO(ResultDTOStatuses status, string message)
        {
            Status = status;
            Message = message;
        }

        public ResultDTO(ResultDTOStatuses status, object data)
        {
            Status = status;
            Data = data;
        }

        public ResultDTOStatuses Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

    }

    public enum ResultDTOStatuses
    {
        Success = 1,
        Error = 2
    }

}