/*
    Copyright (C) 2014  Muraad Nofal
    Contact: muraad.nofal@gmail.com
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * */

using System;
using System.Threading.Tasks;
#if UNIVERSAL
using Windows.Web;
using Windows.Web.Http;
#else
using System.Net;
using System.Net.Http;
#endif

namespace Nulands.Restless
{
    /// <summary>
    /// A class representing a REST response message.
    /// It contains the raw HttpResponseMessage returned from the request.
    /// Further it contains the deserialized data if no exception occured, T != IVoid 
    /// and the response status code matches.
    /// </summary>
    /// <typeparam name="T">The type of the data that will be deserialized.</typeparam>
    public class RestResponse<T> : IDisposable
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public RestResponse()
        {
            Exception = null;
            HttpResponse = null;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="request">Reference to a BaseRestRequest.</param>
        public RestResponse(RestRequest request)
        {
            Request = request;
            Exception = null;
            HttpResponse = null;
        }

        /// <summary>
        /// That BaseRestRequest this rest response comes from.
        /// </summary>
        public RestRequest Request { get; private set; }

        /// <summary>
        /// The Exception that could be thrown during the request fetching.
        /// </summary>
        public Exception Exception { get; internal set; }

        /// <summary>
        /// The "raw" HttpResponseMessage.
        /// </summary>
        public HttpResponseMessage HttpResponse { get; internal set; }

        /// <summary>
        /// The deserialized data if T is not INothing.
        /// </summary>
        public T Data { get; internal set; }

        /// <summary>
        /// Check if the returned status code matches the wanted status code.
        /// </summary>
        public bool IsSuccessStatusCode { get { return HttpResponse == null ? false : HttpResponse.IsSuccessStatusCode; } }

        /// <summary>
        /// Check if the request that was producing this response has encountered an exception.
        /// </summary>
        public bool IsException
        {
            get { return Exception != null; }
        }

        /// <summary>
        /// Check if T is IVoid
        /// </summary>
        public bool IsNothing
        {
            get { return typeof(T) is IVoid; }
        }

        /// <summary>
        /// Check if a deserialized object is available.
        /// </summary>
        public bool HasData 
        {
            get
            {
                return !IsNothing && Data != null; //  !Data.Equals(default(T));
            }
        }

        /// <summary>
        /// If an exception occurred during the request throw it again.
        /// Usage:
        /// var data = response.ThrowIfException().Data;
        /// </summary>
        /// <returns>this.</returns>
        public RestResponse<T> ThrowIfException()
        {
            if (Exception != null)
                throw Exception;
            return this;
        }

        /// <summary>
        /// Dispose the request.
        /// </summary>
        public void Dispose()
        {
            // free managed resources
            if (Request != null)
                Request.Dispose();
            if (HttpResponse != null)
                HttpResponse.Dispose();
            GC.SuppressFinalize(this);
        }

    }
}
