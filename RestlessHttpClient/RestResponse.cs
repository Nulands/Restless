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
using System.Net;
using System.Net.Http;

namespace Restless
{
    public sealed class RestResponse<T>
    {
        public RestResponse()
        {
            IsStatusCodeMissmatch = false;
            Exception = null;
            Response = null;
        }

        /// <summary>
        /// The Exception that could be thrown during the request fetching.
        /// </summary>
        public Exception Exception { get; internal set; }

        /// <summary>
        /// The "raw" HttpResponseMessage.
        /// </summary>
        public HttpResponseMessage Response { get; internal set; }

        /// <summary>
        /// The deserialized data if T is not INothing.
        /// </summary>
        public T Data { get; internal set; }

        /// <summary>
        /// Check if the returned status code matches the wanted status code.
        /// </summary>
        public bool IsStatusCodeMissmatch { get; internal set; }

        /// <summary>
        /// Check if the request that was producing this response has encountered an exception.
        /// </summary>
        public bool IsException
        {
            get { return Exception != null; }
        }

        /// <summary>
        /// Check if T is INothing
        /// </summary>
        public bool IsNothing
        {
            get { return typeof(T) is INothing; }
        }

        /// <summary>
        /// Check if deserialized object is available.
        /// </summary>
        public bool HasData 
        {
            get
            {
                return !IsNothing && Data.Equals(default(T));
            }
        }

        /// <summary>
        /// If an exception occurred during the request throw it again.
        /// Usage:
        /// var data = response.ThrowIfException().Data;
        /// 
        /// </summary>
        /// <returns>this.</returns>
        public RestResponse<T> ThrowIfException()
        {
            if (Exception != null)
                throw Exception;
            return this;
        }
    }
}
