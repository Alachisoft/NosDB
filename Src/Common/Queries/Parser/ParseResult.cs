namespace Alachisoft.NosDB.Common.Queries.Parser
{
    /// Used internally to represent the result of the Parser.ParseToken method.
    internal enum ParseResult
    {		
        ///
        Accept				= 301,
		
        ///
        Shift				= 302,
		
        ///
        ReduceNormal		= 303,
		
        ///
        ReduceEliminated	= 304,
		
        ///
        SyntaxError			= 305,
		
        ///
        InternalError		= 406
    };
}