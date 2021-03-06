﻿using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace JinnSports.WEB.Controllers.Api
{
    public class ValuesController : ApiController
    {

        public HttpResponseMessage Post(Credentials credentials)
        {
            if (credentials.Login == "ivan")
            {
                if (credentials.Password == "123")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Ivan");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
                }

            }
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }
    }

    public class Credentials
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}