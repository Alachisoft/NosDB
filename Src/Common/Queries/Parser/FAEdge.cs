using System;

// C# Translation of GoldParser, by Marcus Klimstra <klimstra@home.nl>.
// Based on GOLDParser by Devin Cook <http://www.devincook.com/goldparser>.
namespace Alachisoft.NosDB.Common.Queries.Parser
{
	/// Each state in the Determinstic Finite Automata contains multiple edges which
	/// link to other states in the automata. This class is used to represent an edge.
	internal class FAEdge
	{
		private String	m_characters;
		private int		m_targetIndex;
		
		/* constructor */

		public FAEdge(String p_characters, int p_targetIndex)
		{
			m_characters = p_characters;
			m_targetIndex = p_targetIndex;
		}
		
		/* properties */

		public String Characters
		{
			get { return m_characters; }
			set { m_characters = value; }
		}

		public int TargetIndex
		{
			get { return m_targetIndex; }
			set { m_targetIndex = value; }
		}
		
		/* public methods */

		public void AddCharacters(String p_characters)
		{
			m_characters = m_characters + p_characters;
		}

		public override String ToString()
		{
			return "DFA edge [chars=[" + m_characters + "],action=" + m_targetIndex + "]";
		}
	}
}
