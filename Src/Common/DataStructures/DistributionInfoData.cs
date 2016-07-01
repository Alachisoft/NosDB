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
using Alachisoft.NosDB.Common.Net;

namespace Alachisoft.NosDB.Common.DataStructures
{
    public class DistributionInfoData
    {
        DistributionMode _distMode;
        ClusterActivity _clustActivity;
        ManualDistType _manualDistType;
        int _percentToMove;
        string _source;
        PartShardInfo _affectedShard;
        string[] _destinations;
        bool _needTransfer = true;

        public DistributionInfoData(DistributionMode distMode, ClusterActivity clustActivity, ManualDistType manDistType, int percentMove, string source, string[] dests)
        {
            _distMode = distMode;
            _clustActivity = clustActivity;
            _manualDistType = manDistType;
            _percentToMove = percentMove;
            _source = source;
            _destinations = dests;
        }

        public DistributionInfoData(DistributionMode distMode, ClusterActivity clustActivity, PartShardInfo affectedShard, bool needTransfer):this(distMode,clustActivity,affectedShard)
        {
            _needTransfer = needTransfer;           
        }

        public DistributionInfoData(DistributionMode distMode, ClusterActivity clustActivity, PartShardInfo affectedShard)
        {
            _distMode = distMode;
            _clustActivity = clustActivity;
            _affectedShard = affectedShard;
        }

        public DistributionMode DistribMode
        {
            get { return _distMode; }
            set { _distMode = value; }
        }

        public ClusterActivity ClustActivity
        {
            get { return _clustActivity; }
            set { _clustActivity = value; }
        }

        public ManualDistType ManualDistType
        {
            get { return _manualDistType; }
            set { _manualDistType = value; }
        }

        //public string Group
        //{
        //    get { return _affectedShard.SubGroup; }
        //    set { _affectedShard.SubGroup = value; }
        //}

        public int PercentToMove
        {
            get { return _percentToMove; }
            set { _percentToMove = value; }
        }

        public string Source
        {
            get { return _source; }
            set { _source = value; }
        }

        public string[] Destinations
        {
            get { return _destinations; }
            set { _destinations = value; }
        }

        public PartShardInfo AffectedShard
        {
            get { return _affectedShard; }
            set { _affectedShard = value; }
        }

        public bool NeedTransfer { get { return _needTransfer; } }


        public override string ToString()
        {
            return "DistributionInfoData( " + AffectedShard.ToString() + ")";
        }
    }
}