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
using System.Data;
using System.Threading.Tasks;
using System.Data.Common;

namespace Alachisoft.NosDB.ADO.NETProvider
{
    public class NosDataParameter : DbParameter
    {
        private System.Data.DbType _dbType = System.Data.DbType.Object;
        private string _parameterName;
        private object _parameterValue;
        private ParameterDirection _direction = ParameterDirection.Input;
        private int _paramSize = 1024;
        private string _sourceColumn;
        private bool _isNullable = false;
        private DataRowVersion _sourceVersion = DataRowVersion.Current;

        public NosDataParameter()
        { }

        public NosDataParameter(string parameterName, object value)
        {
            this._parameterName = parameterName;
            this._parameterValue = value;
        }
        public NosDataParameter(string parameterName, System.Data.DbType type)
        {
            this._parameterName = parameterName;
            this._dbType = type;
        }
        public NosDataParameter(string parameterName, System.Data.DbType dbType, string sourceColumn)
        {
            this._parameterName = parameterName;
            this._dbType = dbType;
            this._sourceColumn = sourceColumn;
        }
        
        public override System.Data.DbType DbType
        {
            get { return this._dbType; }
            set { this._dbType = value; }
        }

        public override System.Data.ParameterDirection Direction
        {
            get { return this._direction; }
            set { this._direction = value; }
        }

        public override bool IsNullable
        {
            get { return this._isNullable; }
            set { this._isNullable = value; }
        }

        public override string ParameterName
        {
            get { return this._parameterName; }
            set { this._parameterName = value; }
        }

        public override string SourceColumn
        {
            get { return this._sourceColumn; }
            set { this._sourceColumn = value; }
        }

        public override System.Data.DataRowVersion SourceVersion
        {
            get { return this._sourceVersion; }
            set { this._sourceVersion = value; }
        }

        public override object Value
        {
            get { return this._parameterValue; }
            set
            {
                this._parameterValue = value;
                this._dbType = ValidateType(value);
            }
        }

        private System.Data.DbType ValidateType(object value)
        {
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Empty:
                    throw new Exception();
                case TypeCode.Object:
                    return DbType.Object;
                case TypeCode.Boolean:
                    return DbType.Boolean;
                case TypeCode.Byte:
                    return DbType.Byte;
                case TypeCode.Int16:
                    return DbType.Int16;
                case TypeCode.Int32:
                    return DbType.Int32;
                case TypeCode.Int64:
                    return DbType.Int64;
                case TypeCode.Single:
                    return DbType.Single;
                case TypeCode.Double:
                    return DbType.Double;
                case TypeCode.Decimal:
                    return DbType.Decimal;
                case TypeCode.DateTime:
                    return DbType.DateTime;
                case TypeCode.String:
                    return DbType.String;
                default:
                    throw new SystemException("Value is of unknown data type");
            }
        }
        
        public override void ResetDbType()
        {
            this._dbType = System.Data.DbType.Object;
        }

        public override int Size
        {
            get { return _paramSize; }
            set { this._paramSize = value; }
        }

        public override bool SourceColumnNullMapping
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

    }
}
