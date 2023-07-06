using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ordering.Application.Exceptions;

namespace Catalog.API.Common
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {

        private readonly IDictionary<Type, Action<ExceptionContext>> _exceptionHandlers;
        private ILogger<ApiExceptionFilterAttribute> logger;

        public string CurrentLoggedInUser;

        public ApiExceptionFilterAttribute()
        {
            // Register known exception types and handlers.
            _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
            {
                { typeof(NotFoundException), HandleNotFoundException },
            };
        }

        public override void OnException(ExceptionContext context)
        {
            logger = (ILogger<ApiExceptionFilterAttribute>)context.HttpContext.RequestServices.GetService(typeof(ILogger<ApiExceptionFilterAttribute>));
            HandleException(context);

            base.OnException(context);
        }

        private void HandleException(ExceptionContext context)
        {
            Type type = context.Exception.GetType();
            if (_exceptionHandlers.ContainsKey(type))
            {
                _exceptionHandlers[type].Invoke(context);
                return;
            }

            if (!context.ModelState.IsValid)
            {
                HandleInvalidModelStateException(context);
                return;
            }

            HandleUnknownException(context);
        }

        private void HandleUnknownException(ExceptionContext context)
        {
            logger.LogError(context.Exception, "Error occurred while processing request");
            var details = new ErrorResponse
            {
                Message = "An error occurred while processing your request.",
            };

            context.Result = new ObjectResult(details)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };

            context.ExceptionHandled = true;
        }

        private void HandleNotFoundException(ExceptionContext context)
        {
            var exception = context.Exception as NotFoundException;

            var errorResponse = new ErrorResponse
            {
                Message = exception.Message
            };

            context.Result = new NotFoundObjectResult(errorResponse);

            context.ExceptionHandled = true;
        }

        private void HandleInvalidModelStateException(ExceptionContext context)
        {
            var details = new ValidationProblemDetails(context.ModelState)
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            };

            context.Result = new BadRequestObjectResult(details);

            context.ExceptionHandled = true;
        }
    }

    internal class ErrorResponse
    {
        public string Message { get; set; }
    }

    /*
         [TypeFilter(typeof(ExceptionFilter))]  on controller class

         public class ExceptionFilter : IExceptionFilter
    {
        /// <summary>
        /// The logger in use.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The application insights telemetry client in use.
        /// </summary>
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionFilter"/> class.
        /// </summary>
        /// <param name="logger">A reference to the logger to use.</param>
        /// <param name="telemetryClient">A reference to the App Insights telemetry client.</param>
        public ExceptionFilter(
            ILogger<ExceptionFilter> logger,
            TelemetryClient telemetryClient)
        {
            this.logger = logger;
            this.telemetryClient = telemetryClient;
        }

        /// <inheritdoc/>
        public void OnException(ExceptionContext context)
        {
            if (context.Result != null)
            {
                return;
            }

            if (context.Exception is BadRequestException badRequestException)
            {
                var errorResponse = new ErrorResponse
                {
                    // TODO: error codes for services.
                    ErrorCode = "API-400",
                    ErrorMessage = badRequestException.Message,
                    TraceId = context.HttpContext.TraceIdentifier.EncodeToBase64(),
                };

                context.Result = new BadRequestObjectResult(errorResponse);
                context.ExceptionHandled = true;
            }
            else if (context.Exception is NotFoundException notFoundException)
            {
                var errorResponse = new ErrorResponse
                {
                    // TODO: error codes for services.
                    ErrorCode = "API-404",
                    ErrorMessage = notFoundException.Message,
                    TraceId = context.HttpContext.TraceIdentifier.EncodeToBase64(),
                };

                context.Result = new NotFoundObjectResult(errorResponse);
                context.ExceptionHandled = true;
            }
            else if (context.Exception != null)
            {
                var errorResponse = new ErrorResponse
                {
                    // TODO: error codes for services.
                    ErrorCode = "API-500",
                    ErrorMessage = context.Exception.Message,
                    TraceId = context.HttpContext.TraceIdentifier.EncodeToBase64(),
                };

                context.Result = new ObjectResult(errorResponse)
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                };

                this.telemetryClient?.TrackException(context.Exception);

                context.ExceptionHandled = true;
            }

            // Leave context.Result null to actually fail if intented, or override it as above to respond with something explicitly.
        }
    }

     
     
     */
}
