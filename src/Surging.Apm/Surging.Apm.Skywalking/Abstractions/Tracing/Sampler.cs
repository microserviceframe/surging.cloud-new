/*
 * Licensed to the Surging.Apm.Skywalking.Abstractions under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The Surging.Apm.Skywalking.Abstractions licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */


using Surging.Cloud.CPlatform.Diagnostics;

namespace Surging.Apm.Skywalking.Abstractions.Tracing
{
    public delegate bool Sampler(SamplingContext samplingContext);

    public class SamplingContext
    {
        public string OperationName { get; }

        public StringOrIntValue Peer { get; }

        public StringOrIntValue EntryEndpoint { get; }

        public StringOrIntValue ParentEndpoint { get; }

        public SamplingContext(string operationName, StringOrIntValue peer, StringOrIntValue entryEndpoint,
            StringOrIntValue parentEndpoint)
        {
            OperationName = operationName;
            Peer = peer;
            EntryEndpoint = entryEndpoint;
            ParentEndpoint = parentEndpoint;
        }
    }
}