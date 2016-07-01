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
using System.IO;
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common.DataStructures.Clustered;

namespace Alachisoft.NosDB.Core.Recovery.Persistence
{
    internal class SegmentManager
    {
        private Dictionary<long, Segment> _segmentMap;
        private Segment _activeSegment = null;
        private long _segmentCount = 0;

        internal SegmentManager()
        {
            _segmentMap = new Dictionary<long, Segment>();
        }

        internal Dictionary<long, long> SegmentOffsetMap
        {
            get
            {
                return _segmentMap.ToDictionary(x => x.Key, x => x.Value.HeaderStartingOffset);
            }
        }

        internal void RecreateSegmentMetaInfo(Stream stream, IDictionary<long, long> offsetMap)
        {
            foreach (long key in offsetMap.Keys)
            {
                Segment _seg = new Segment(key);
                _seg.RecreateMetaInfo(stream, offsetMap[key]);

                _segmentMap.Add(key, _seg);
            }

        }

        internal IDictionary<long, DataSlice> RecreateSliceMetaInfo(Stream stream)
        {
            IDictionary<long, DataSlice> sliceMap = new HashVector<long, DataSlice>();

            foreach (Segment seg in _segmentMap.Values)
            {
                List<DataSlice> _sliceList = seg.RecreateSliceMetaInfo(stream);
                foreach (DataSlice _slice in _sliceList)
                {
                    if (!sliceMap.ContainsKey(_slice.SliceHeader.Id))
                    {
                        sliceMap.Add(_slice.SliceHeader.Id, _slice);
                    }
                    else
                    {
                        sliceMap[_slice.SliceHeader.Id].SliceHeader.SegmentIds.Add(seg.SegmentHeader.Id);
                        sliceMap[_slice.SliceHeader.Id].SliceHeader.TotalSize += _slice.SliceHeader.TotalSize;
                    }
                }
            }

            return sliceMap;
        }

        // change name of this 
        internal SliceFacilitator[] GetFacilitatingSegments(long dataSize, long headerSize, long padding)
        {
            List<SliceFacilitator> segmentList = new List<SliceFacilitator>();
            long remSize = dataSize;
            while (remSize > 0)
            {
                SliceFacilitator _fac = null;
                if (_segmentMap.Count == 0)
                {
                    if (_activeSegment == null)
                    {
                        // create new segment
                        CreateNewSegment();
                    }
                }

                if ((remSize + headerSize + padding) <= _activeSegment.EmptySpace)
                {
                    _fac = new SliceFacilitator();
                    _fac.Size = remSize;
                    _fac.Segment = _activeSegment;
                    remSize = 0;
                }
                else
                {
                    long availableSpace = (_activeSegment.EmptySpace - (headerSize + padding));

                    if (availableSpace > 0)
                    {
                        _fac = new SliceFacilitator();
                        remSize -= availableSpace;
                        _fac.Size = availableSpace;
                        _fac.Segment = _activeSegment;

                    }
                    CreateNewSegment();
                }

                if (_fac != null)
                    segmentList.Add(_fac);

            }
            return segmentList.ToArray();
        }

        private void CreateNewSegment()
        {
            Segment _seg = null;
            _seg = new Segment(_segmentCount);
            _activeSegment = _seg;
            _segmentMap.Add(_segmentCount, _seg);
            _segmentCount++;
        }
        

        internal Segment GetSegment(long segID)
        {
            if (_segmentMap.ContainsKey(segID))
                return _segmentMap[segID];
            else
                return null;
        }
      
    }
}
