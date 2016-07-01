// /*
// * Copyright (c) 2016, Alachisoft. All Rights Reserved.
// *
// * Licensed under the Apache License, Version 2.0 (the "License");
// * you may not use this file except in compliance with the License.
// * You may obtain a copy of the License at
// *
// * http://www.apache.org/licenses/LICENSE-2.0
// *
// * Unless required by applicable law or agreed to in writing, software
// * distributed under the License is distributed on an "AS IS" BASIS,
// * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// * See the License for the specific language governing permissions and
// * limitations under the License.
// */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Storage.Attachments
{
    public class AttachmentAttributes
    {
        public const string ATTACHMENT_COLLECTION = "attachments"; //lowercase is must as collection names are lowercase
        public const string CHUNK_SIZE = "ChunkSize";
        public const string USER_METADATA = "UserMetadata";
        public const string SERVER_ID = "ServerId";
        public const string ATTACHMENT_ID = "AttachmentId";
        public const int MIN_CHUNK_SIZE = 10;
        public const int MAX_CHUNK_SIZE = 12280;

    }
}