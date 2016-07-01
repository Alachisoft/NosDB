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
using Alachisoft.NosDB.Serialization.IO;

namespace Alachisoft.NosDB.Serialization.Surrogates
{
    /// <summary>
    /// Surrogate for <see cref="System.Boolean[]"/> type.
    /// </summary>
    sealed class NullableArraySerializationSurrogate<T> : SerializationSurrogate
        where T : struct
    {
        public NullableArraySerializationSurrogate() : base(typeof(T?[])) { }

        public override object Read(CompactBinaryReader reader)
        {
            // read type handle
            short handle = reader.ReadInt16();

            // Find an appropriate surrogate by handle
            ISerializationSurrogate typeSurr = TypeSurrogateSelector.GetSurrogateForTypeHandle(handle,null);
            if (typeSurr == null)
            {
                typeSurr = TypeSurrogateSelector.GetSurrogateForSubTypeHandle(handle, reader.ReadInt16(), reader.Context.CacheContext);
            }

            int length = reader.ReadInt32();

            T?[] array = new T?[length];
            while (true)
            {
                int index = reader.ReadInt32();
                if (index < 0) break;

                array[index] = (T)typeSurr.Read(reader);
            }
            return array;
        }

        public override void Write(CompactBinaryWriter writer, object graph)
        {
            // Find an appropriate surrogate for the object
            ISerializationSurrogate typeSurr = TypeSurrogateSelector.GetSurrogateForType(typeof(T), null);

            T?[] array = (T?[])graph;

            // write type handle
            writer.Write(typeSurr.TypeHandle);
            writer.Write(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].HasValue)
                {
                    writer.Write(i);
                    typeSurr.Write(writer, array[i].Value);
                }
            }
            writer.Write(-1);
        }

        public override void Skip(CompactBinaryReader reader)
        {
            // read type handle
            short handle = reader.ReadInt16();

            // Find an appropriate surrogate by handle
            ISerializationSurrogate typeSurr = TypeSurrogateSelector.GetSurrogateForTypeHandle(handle, null);
            if (typeSurr == null)
            {
                typeSurr = TypeSurrogateSelector.GetSurrogateForSubTypeHandle(handle, reader.ReadInt16(), reader.Context.CacheContext);
            }

            int length = reader.ReadInt32();
            T?[] array = new T?[length];
            while (true)
            {
                int index = reader.ReadInt32();
                if (index < 0) break;

                typeSurr.Skip(reader);
            }
        }

        //public override object Read(CompactBinaryReader reader)
        //{
        //    // read type handle
        //    short handle = reader.ReadInt16();
        //    int length = reader.ReadInt32();

        //    // Find an appropriate surrogate by handle
        //    ISerializationSurrogate typeSurr = TypeSurrogateSelector.GetSurrogateForTypeHandle(handle);

        //    T?[] array = new T?[length];
        //    for (int i = 0; i < length;)
        //    {
        //        bool dataRun = reader.ReadBoolean();
        //        int runLength = reader.ReadInt32();
        //        if (dataRun)
        //        {
        //            for (int k = 0; k < runLength; k++)
        //                array[i + k] = (T?)typeSurr.Read(reader);
        //        }
        //        i += runLength;
        //    }
        //    return array;
        //}

        //public override void Write(CompactBinaryWriter writer, object graph)
        //{
        //    // Find an appropriate surrogate for the object
        //    ISerializationSurrogate typeSurr = TypeSurrogateSelector.GetSurrogateForType(typeof(T));

        //    T?[] array = (T?[])graph;

        //    // write type handle
        //    writer.Write(typeSurr.TypeHandle);
        //    writer.Write(array.Length);

        //    bool dataRun = array[0].HasValue;
        //    for (int i = 0; i < array.Length;)
        //    {
        //        int runLength = 0;
        //        for (int j = i; j < array.Length; j++, runLength++)
        //            if (array[j].HasValue != dataRun) break;

        //        writer.Write(dataRun);
        //        writer.Write(runLength);

        //        if (dataRun)
        //        {
        //            for (int k = 0; k < runLength; k++)
        //                typeSurr.Write(writer, array[i + k].Value);
        //        }

        //        i += runLength;
        //        dataRun = !dataRun;
        //    }
        //}
    }
}