using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Application
{
    public class SaveStatus
    {
		/// <summary>
		/// If true, error encountered somewhere during workflow
		/// </summary>
		public bool HasErrors { get; set; }
		/// <summary>
		/// Reset HasSectionErrors to false at the start of a new section of validation. Then check at the end of the section for any errors in the section
		/// </summary>
		public bool HasSectionErrors { get; set; }
		//TBD
		public List<StatusMessage> Messages { get; set; }
		public void AddError( string message )
		{
			Messages.Add( new StatusMessage() { Message = message } );
			HasErrors = true;
			HasSectionErrors = true;
		}
		public void AddErrorRange( List<string> messages )
		{
			foreach ( string msg in messages )
				Messages.Add( new StatusMessage() { Message = msg } );
			HasErrors = true;
			HasSectionErrors = true;
		}
		public List<string> GetAllMessages()
		{
			List<string> messages = new List<string>();
			string prefix = "";
			foreach ( StatusMessage msg in Messages.OrderBy( m => m.IsWarning ) )
			{
				if ( msg.IsWarning )
					if ( !msg.Message.ToLower().StartsWith( "warning" ) )
						prefix = "Warning - ";
					else
					if ( !msg.Message.ToLower().StartsWith( "error" ) )
						prefix = "Error - ";
				messages.Add( prefix + msg.Message );
			}

			return messages;
		}
	}
    public class StatusMessage
    {
        public string Message { get; set; }
        public bool IsWarning { get; set; }
    }
}
