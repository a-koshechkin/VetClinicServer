using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VetClinicServer
{
    internal struct Response
    {
        public int StatusCode;
        public string StatusString;

        public Response(int statusCode, string statusString = "")
        {
            StatusCode = statusCode;
            StatusString = statusString;
        }

        public static Response OK() => new Response(200, "OK");

        public static Response BadRequest(string text = null)
        {
            string s = "Bad Request";
            if (!StringExtensions.IsNullOrWhiteSpace(text))
                s += $". {text}";
            return new Response(400, s);
        }

        public static Response NotFound() => new Response(404, "Not Found");

        public static Response InternalError(string text = null)
        {
            string s = "Internal Server Error";
            if (!StringExtensions.IsNullOrWhiteSpace(text))
                s += $". {text}";
            return new Response(500, s);
        }

        public static Response NotImplemented() => new Response(501, "Not Implemented");

    }
}
