using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    public class BaseController : ControllerBase
    {
        protected int? UserSucursalId
        {
            get
            {
                var claim = User.FindFirst("sucursalId")?.Value;
                return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
            }
        }

        protected bool IsAdmin => User.IsInRole("Admin");

        public override OkResult Ok()
        {
            return base.Ok();
        }

        public override OkObjectResult Ok(object value)
        {
            return base.Ok(GetResult(value));
        }

        private static HttpMessageResult GetResult(object value)
        {
            return new HttpMessageResult()
            {
                Success = true, //resultado de la ejecucion
                Data = value, //objeto de datos de respuesta
                Message = string.Empty, //notificacion o mensaje de error en caso de que Success sea False
                Code = string.Empty, //codigo de error en caso de que Success sea False
                StatusCode = 200
            };
        }
    }

    public class HttpMessageResult
    {
        public bool Success { set; get; }
        public object Data { set; get; }
        public string Message { set; get; }
        public string Code { set; get; }
        public int StatusCode { set; get; }
    }
}
