using System;

// C# Translation of GoldParser, by Marcus Klimstra <klimstra@home.nl>.
// Based on GOLDParser by Devin Cook <http://www.devincook.com/goldparser>.

namespace Alachisoft.NosDB.Common.Queries.Parser
{
	/// This class represents an action in a LALR State. 
	/// There is one and only one action for any given symbol.
	internal class LRAction
	{
		private Symbol		m_symbol;
		private Action		m_action;
		private int			m_value;
		
		/* properties */

		public Symbol Symbol
		{
			get { return m_symbol; }
			set { m_symbol = value; }
		}
		
		public Action Action
		{
			get { return m_action; }
			set { m_action = value; }
		}
		
		public int Value
		{
			get { return m_value; }
			set { m_value = value; }
		}
		
		/* public methods */
		
		public override String ToString()
		{
			return "LALR action [symbol=" + m_symbol + ",action=" + m_action + ",value=" + m_value + "]";
		}		
	}
}
