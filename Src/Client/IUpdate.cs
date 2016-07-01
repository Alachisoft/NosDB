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


namespace Alachisoft.NosDB.Client
{
    public interface IUpdate
    {
        ///<summary>
        ///static. Does not need an instance creation. Should return an 
        /// object of the UpdateBuilder class which then concatenates the results 
        /// within itself.
        /// </summary>
        #region Set Operations
        IUpdate Set<U>(string documentField, U value);
        #endregion

        #region Increment Operations
        IUpdate Increment<U>(string documentField, U value);
        #endregion

        #region Decrement Operations
        IUpdate Decrement<U>(string documentField, U value);
        #endregion

        #region Multiply Operations
        IUpdate Multiply<U>(string documentField, U value);
        #endregion

        #region Array Operations
        #region Insert operations
        /// <summary>
        /// Appends data to the end of an array
        /// </summary>
        /// <param name="documentField"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// 
        IUpdate InsertIntoArray<U>(string documentField, U value);
        IUpdate InsertIntoArray<U>(string documentField, U[] value);
        #endregion

        #region Remove Operations
        /// <summary>
        /// Removes data from an array
        /// </summary>
        /// <param name="documentField"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// 
        IUpdate RemoveFromArray<U>(string documentField, U value);
        IUpdate RemoveFromArray<U>(string documentField, U[] value);
        #endregion
        #endregion

        IUpdate RenameField(string documentField, string value);
        IUpdate RemoveField(string documentField);
    }

    public interface IUpdate<T>
    {
        ///<summary>
        ///static. Does not need an instance creation. Should return an 
        /// object of the UpdateBuilder class which then concatenates the results 
        /// within itself.
        /// </summary>
        #region Set Operations
        IUpdate<T> Set<U>(Func<T, U> documentField, U value);
        #endregion

        #region Increment Operations
        IUpdate<T> Increment<U>(Func<T, U> documentField, U value);
        #endregion

        #region Decrement Operations
        IUpdate<T> Decrement<U>(Func<T, U> documentField, U value);
        #endregion

        #region Multiply Operations
        IUpdate<T> Multiply<U>(Func<T, U> documentField, U value);
        #endregion

        #region Array Operations
        #region Insert operations
        /// <summary>
        /// Appends data to the end of an array
        /// </summary>
        /// <param name="documentField"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// 
        IUpdate<T> InsertIntoArray<U>(Func<T, U> documentField, U value);
        IUpdate<T> InsertIntoArray<U>(Func<T, U> documentField, U[] value);
        #endregion

        #region Remove Operations
        /// <summary>
        /// Removes data from an array
        /// </summary>
        /// <param name="documentField"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// 
        IUpdate<T> RemoveFromArray<U>(Func<T, U> documentField, U value);
        IUpdate<T> RemoveFromArray<U>(Func<T, U> documentField, U[] value);
        #endregion
        #endregion

        IUpdate<T> RenameField<U>(Func<T, U> documentField, U value);
        IUpdate<T> RemoveField<U>(Func<T, U> documentField);
    }
}
