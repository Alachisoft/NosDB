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
using Alachisoft.NosDB.Common.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Common.Replication
{
    public class OperationId : ICompactSerializable, ICloneable
    {
        public long ElectionId { get; set; }
        public long Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public long ElectionBasedSequenceId { get; set; }

        public OperationId()
        {
            ElectionId = -1;
            Id = 0;
            ElectionBasedSequenceId = -1;
            TimeStamp = new DateTime();
        }

        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            ElectionId = reader.ReadInt64();
            Id = reader.ReadInt64();
            ElectionBasedSequenceId = reader.ReadInt64();
            TimeStamp = new DateTime(reader.ReadInt64());
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.Write(ElectionId);
            writer.Write(Id);
            writer.Write(ElectionBasedSequenceId);
            writer.Write(TimeStamp.Ticks);
        }

        public override bool Equals(object obj)
        {
            OperationId opId = obj as OperationId;
            if (opId != null)
                if (opId.ElectionId == this.ElectionId && opId.ElectionBasedSequenceId == this.ElectionBasedSequenceId)
                {
                    return true;
                }
                else return false;
            else return false;
        }

        #region ICloneable
        public object Clone()
        {
            OperationId opId = new OperationId();
            opId.ElectionId = this.ElectionId;
            opId.ElectionBasedSequenceId = this.ElectionBasedSequenceId;
            opId.TimeStamp = this.TimeStamp;
            opId.Id = this.Id;
            return opId;
        }
        #endregion

        public long Size
        {
            get
            {
                long size = 0;
                size += sizeof(long);
                size += sizeof(long);
                //size calculation for date time
                for (int i = 0; i < 7; i++)
                    size += sizeof(long);
                return size;
            }
        }

        /// <summary>
        /// > Operator is overloaded to compare two operation ids
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Boolean operator !=(OperationId left, OperationId right)
        {
            if (object.ReferenceEquals(left, null) && object.ReferenceEquals(right, null))
            {
                return false;
            }
            if (object.ReferenceEquals(left, null) || object.ReferenceEquals(right, null))
            {
                return true;
            }

            return left.ElectionId != right.ElectionId || left.ElectionBasedSequenceId != right.ElectionBasedSequenceId;
        }

        /// <summary>
        /// > Operator is overloaded to compare two operation ids
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Boolean operator ==(OperationId left, OperationId right)
        {
            if (object.ReferenceEquals(left,null) && object.ReferenceEquals(right,null))
            {
                return true;
            }
            if (object.ReferenceEquals(left, null) || object.ReferenceEquals(right, null))
            {
                return false;
            }

            return left.ElectionId == right.ElectionId && left.ElectionBasedSequenceId == right.ElectionBasedSequenceId;
        }


        /// <summary>
        /// > Operator is overloaded to compare two operation ids
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Boolean operator >(OperationId left,OperationId right)
        {
            if (object.ReferenceEquals(left, null) && object.ReferenceEquals(right, null))
            {
                return false;
            }
            if(object.ReferenceEquals(left, null))
            {
                return false;
            }
            if(object.ReferenceEquals(right, null))
            {
                return true;
            }
            if(left.ElectionId > right.ElectionId)return true;

            if (left.ElectionId == right.ElectionId && left.ElectionBasedSequenceId > right.ElectionBasedSequenceId) return true;
            
            return false;
        }


        /// <summary>
        /// Operator is overloaded to compare two operation ids
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Boolean operator <(OperationId left, OperationId right)
        {
            if (object.ReferenceEquals(left, null) && object.ReferenceEquals(right, null))
            {
                return false;
            }
            if (object.ReferenceEquals(left, null))
            {
                return true;
            }
            if (object.ReferenceEquals(right, null))
            {
                return false;
            }
            if (left.ElectionId < right.ElectionId) return true;

            if (left.ElectionId == right.ElectionId && left.ElectionBasedSequenceId < right.ElectionBasedSequenceId) return true;

            return false;
        }

        ///// <summary>
        ///// == Operator is overloaded to compare two operation ids
        ///// </summary>
        ///// <param name="left"></param>
        ///// <param name="right"></param>
        ///// <returns></returns>
        //public static Boolean operator ==(OperationId left, OperationId right)
        //{
        //    if ((left != null && right != null) && (left.ElectionId == right.ElectionId && left.ElectionBasedSequenceId == left.ElectionBasedSequenceId))
        //        return true;

        //    return false;
        //}

        ///// <summary>
        ///// == Operator is overloaded to compare two operation ids
        ///// </summary>
        ///// <param name="left"></param>
        ///// <param name="right"></param>
        ///// <returns></returns>
        //public static Boolean operator !=(OperationId left, OperationId right)
        //{
        //    if(left!=null)
        //    return false;
        //}
    }
}
