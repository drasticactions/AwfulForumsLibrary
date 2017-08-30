using System;
using System.Collections.Generic;
using System.Text;

namespace AwfulForumsLibrary.Models.Web
{
    public class Result
    {
        public Result(bool isSuccess = false, string html = "", string json = "", string type = "", string uri = "")
        {
            IsSuccess = isSuccess;
            ResultHtml = html;
            ResultJson = json;
            Type = type;
            AbsoluteUri = uri;
        }

        /// <summary>
        /// If the request we've recieved was gotten successfully.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// The result of the request, in HTML form.
        /// </summary>
        public string ResultHtml { get; set; }

        /// <summary>
        /// The Uri of the request
        /// </summary>
        public string AbsoluteUri { get; set; }

        /// <summary>
        /// The request Uri
        /// </summary>
        public string RequestUri { get; set; }

        /// <summary>
        /// The result of the request, in JSON form.
        /// </summary>
        public string ResultJson { get; set; }

        /// <summary>
        /// The type of the object.
        /// </summary>
        public string Type { get; set; }
    }
}
