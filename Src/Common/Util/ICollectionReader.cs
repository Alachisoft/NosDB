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
using System.Collections.Generic;
using Alachisoft.NosDB.Common.Server.Engine;
using System;

namespace Alachisoft.NosDB.Common
{
    public interface ICollectionReader : IDisposable
    {
        /// <summary>
        /// Gets the value of the specified attribute name.
        /// </summary>
        /// <param name="attribute">The attribute name.</param>
        /// <returns> The value against the specified attribute. </returns>
        object this[string attribute] { get; }

        /// <summary>
        /// Advances the data reader to the next result, when reading the results of bulk transactions.
        /// </summary>
        /// <returns> Returns true if there are more result sets; otherwise false. </returns>
        bool ReadNext();

        /// <summary>
        /// Gets a value that indicates whether the Alachisoft.NosDB.Common.ICollectionReader
        //  contains one or more rows
        /// </summary>
        /// <returns> 
        /// true if the Alachisoft.NosDB.Common.ICollectionReader contains one or more rows;
        /// otherwise false.
        /// </returns>
        bool HasRows { get; }

        /// <summary>
        /// Gets the object as a type.
        /// </summary>
        /// <typeparam name="T"> The type of value to be returned. </typeparam>
        /// <returns> The returned type object. </returns>
        /// <exception cref="System.InvalidCastException"> T doesn't match the type returned by NosDB or cannot be cast.</exception>
        T GetObject<T>();

        /// <summary>
        /// Gets the Alachisoft.NosDB.Common.Server.Engine.IJSONDocument type object.
        /// </summary>
        /// <returns> Alachisoft.NosDB.Common.Server.Engine.IJSONDocument type object.</returns>
        /// <exception cref="System.InvalidCastException"> The specified cast is not valid.</exception>
        IJSONDocument GetDocument();

        /// <summary>
        /// Gets the Alachisoft.NosDB.Common.Server.Engine.IJSONDocument type object against the specified attribute.
        /// </summary>
        /// <param name="attribute">The attribute name.</param>
        /// <returns>Alachisoft.NosDB.Common.Server.Engine.IJSONDocument type object.</returns>
        /// <exception cref="System.InvalidCastException"> The specified cast is not valid.</exception>
        IJSONDocument GetDocument(string attribute);

        /// <summary>
        /// Gets the value of the specified attribute as a 16-bit signed integer.
        /// </summary>
        /// <param name="attribute">The attribute name.</param>
        /// <returns>The value against the specified attribute.</returns>
        /// <exception cref="System.InvalidCastException"> The specified cast is not valid.</exception>
        short GetInt16(string attribute);

        /// <summary>
        /// Gets the value of the specified attribute as a 32-bit signed integer.
        /// </summary>
        /// <param name="attribute">The attribute name.</param>
        /// <returns>The value against the specified attribute.</returns>
        /// <exception cref="System.InvalidCastException"> The specified cast is not valid.</exception>
        int GetInt32(string attribute);

        /// <summary>
        /// Gets the value of the specified attribute as a 64-bit signed integer.
        /// </summary>
        /// <param name="attribute">The attribute name.</param>
        /// <returns>The value against the specified attribute.</returns>
        /// <exception cref="System.InvalidCastException"> The specified cast is not valid.</exception>
        long GetInt64(string attribute);

        /// <summary>
        /// Gets the value of the specified attribute as a double-precision floating point number.
        /// </summary>
        /// <param name="attribute">The attribute name.</param>
        /// <returns>The value against the specified attribute.</returns>
        /// <exception cref="System.InvalidCastException"> The specified cast is not valid.</exception>
        double GetDouble(string attribute);

        /// <summary>
        /// Gets the value of the specified attribute in a System.Decimal object.
        /// </summary>
        /// <param name="attribute">The attribute name.</param>
        /// <returns> The value against the specified attribute.</returns>
        /// <exception cref="System.InvalidCastException"> The specified cast is not valid.</exception>
        decimal GetDecimal(string attribute);

        /// <summary>
        /// Gets the value of the specified attribute as a string.
        /// </summary>
        /// <param name="attribute">The attribute name.</param>
        /// <returns>The value against the specified attribute.</returns>
        /// <exception cref="System.InvalidCastException"> The specified cast is not valid.</exception>
        string GetString(string attribute);

        /// <summary>
        /// Gets the value of the specified attribute as a Boolean.
        /// </summary>
        /// <param name="attribute">The attribute name.</param>
        /// <returns> The value against the specified attribute.</returns>
        /// <exception cref="System.InvalidCastException"> The specified cast is not valid.</exception>
        bool GetBoolean(string attribute);

        /// <summary>
        /// Gets an array against the specified attribute(as a type).
        /// </summary>
        /// <typeparam name="T"> The type of the value to be returned.</typeparam>
        /// <param name="attribute">The attribute name.</param>
        /// <returns>An array of values against the specified attribute.</returns>
        /// <exception cref="System.InvalidCastException"> The specified cast is not valid.</exception>
        T[] GetArray<T>(string attribute);

        /// <summary>
        /// Gets the data type of the specified attribute.
        /// </summary>
        /// <param name="attribute">The attribute name.</param>
        /// <returns> An object of Alachisoft.NosDB.Common.Server.Engine.ExtendedJSONDataTypes</returns>
        /// <exception cref="System.InvalidCastException"> The specified cast is not valid.</exception>
        ExtendedJSONDataTypes GetAttributeDataType(string attribute);

        /// <summary>
        /// Gets the value of the specified attribute as a type.
        /// </summary>
        /// <typeparam name="T">The type of the value to be returned.</typeparam>
        /// <param name="attribute">The attribute name.</param>
        /// <returns>The returned type object.</returns>
        /// <exception cref="System.InvalidCastException"> T doesn't match the type returned by NosDB or cannot be cast.</exception>
        T Get<T>(string attribute);

        /// <summary>
        /// Gets a value that indicates whether Alachisoft.NosDB.Common.ICollectionReader contains the specified attribute.
        /// </summary>
        /// <param name="attribute">The attribute name.</param>
        /// <returns>true if the Alachisoft.NosDB.Common.ICollectionReader contains the specified attribute; 
        /// otherwise false.</returns>
        bool ContainsAttribute(string attribute);

        /// <summary>
        /// Gets all the attributes present in the Alachisoft.NosDB.Common.ICollectionReader.
        /// </summary>
        /// <returns> The attributes in the Alachisoft.NosDB.Common.ICollectionReader.</returns>
        ICollection<string> GetAttributes();
    }
}
