namespace Alachisoft.NosDB.Common.Queries.Parser
{
    /// Represents the type of an entry in the CGT file.
    internal enum EntryContent
    {		
        ///
        Empty				= 69,
		
        ///
        Integer				= 73,
		
        ///
        String				= 83,
		
        ///
        Boolean				= 66,
		
        ///
        Byte				= 98,
		
        ///
        Multi				= 77
    };
}