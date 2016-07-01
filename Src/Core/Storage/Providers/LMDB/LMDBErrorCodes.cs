using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Core.Storage.Providers.LMDB
{
    internal class LMDBErrorCodes
    {
        /// <summary>
        /// Successful result
        /// </summary>
        public const int MDB_SUCCESS = 0;

        /// <summary>
        /// key/data pair already exists 
        /// </summary>
        public const int MDB_KEYEXIST = -30799;

        /// <summary>
        /// key/data pair not found (EOF) 
        /// </summary>
        public const int MDB_NOTFOUND = -30798;

        /// <summary>
        /// Requested page not found - this usually indicates corruption 
        /// </summary>
        public const int MDB_PAGE_NOTFOUND = -30797;

        /// <summary>
        /// Located page was wrong type 
        /// </summary>
        public const int MDB_CORRUPTED = -30796;

        /// <summary>
        /// Update of meta page failed or environment had fatal error, probably I/O error
        /// </summary>
        public const int MDB_PANIC = -30795;

        /// <summary>
        /// Environment version mismatch 
        /// </summary>
        public const int MDB_VERSION_MISMATCH = -30794;

        /// <summary>
        /// File is not a valid LMDB file 
        /// </summary>
        public const int MDB_INVALID = -30793;

        /// <summary>
        /// Environment mapsize reached
        /// </summary>
        public const int MDB_MAP_FULL = -30792;

        /// <summary>
        /// Environment maxdbs reached
        /// </summary>
        public const int MDB_DBS_FULL = -30791;

        /// <summary>
        /// Environment maxreaders reached
        /// </summary>
        public const int MDB_READERS_FULL = -30790;

        /// <summary>
        /// Too many TLS keys in use - Windows only 
        /// </summary>
        public const int MDB_TLS_FULL = -30789;

        /// <summary>
        /// Txn has too many dirty pages
        /// </summary>
        public const int MDB_TXN_FULL = -30788;

        /// <summary>
        /// Cursor stack too deep - internal error 
        /// </summary>
        public const int MDB_CURSOR_FULL = -30787;

        /// <summary>
        /// Page has not enough space - internal error 
        /// </summary>
        public const int MDB_PAGE_FULL = -30786;

        /// <summary>
        /// Database contents grew beyond environment mapsize
        /// </summary>
        public const int MDB_MAP_RESIZED = -30785;

        /// <summary>
        ///Operation and DB incompatible, or DB type changed. This can mean:
        ///The operation expects an MDB_DUPSORT / MDB_DUPFIXED database.
        ///Opening a named DB when the unnamed DB has MDB_DUPSORT / MDB_INTEGERKEY.
        ///Accessing a data record as a database, or vice versa.
        ///The database was dropped and recreated with different flags.
        /// </summary>
        public const int MDB_INCOMPATIBLE = -30784;

        /// <summary>
        /// Invalid reuse of reader locktable slot 
        /// </summary>
        public const int MDB_BAD_RSLOT = -30783;

        /// <summary>
        /// Umer- one occurance is on if the transaction gets MDB_MAP_FULL and that transaction is further used 
        /// Transaction must abort, has a child, or is invalid 
        /// </summary>
        public const int MDB_BAD_TXN = -30782;

        /// <summary>
        /// Unsupported size of key/DB name/data, or wrong DUPFIXED size 
        /// </summary>
        public const int MDB_BAD_VALSIZE = -30781;

        /// <summary>
        /// The specified DBI was changed unexpectedly 
        /// </summary>
        public const int MDB_BAD_DBI = -30780;
    }
}
