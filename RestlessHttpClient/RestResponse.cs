﻿/*
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

        public Exception Exception { get; set; }
        public HttpResponseMessage Response { get; set; }
        public T Data { get; set; }

        public bool IsStatusCodeMissmatch { get; set; }
        public bool IsException
        {
            get { return Exception != null; }
        }
        public bool HasData 
        {
            get
            {
                return Data.Equals(default(T));
            }
        }
    }
}