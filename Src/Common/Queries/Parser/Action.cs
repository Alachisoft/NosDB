namespace Alachisoft.NosDB.Common.Queries.Parser
{
    /// constants associated with what action should be performed 
    internal enum Action
    {		
        ///
        Shift				= 1,
		
        ///
        Reduce				= 2,
		
        ///
        Goto				= 3,
		
        ///
        Accept				= 4,
		
        ///
        Error				= 5
    };
}