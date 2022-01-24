using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Application
{
    public class SaveStatus : Models.Curation.ChangeSummary
    {
		public string Context { get; set; }
		public int ActionType { get; set; }
		public bool IsCreateAction
        {
			get { return ActionType == 1; }
        }
		public bool IsUpdateAction
        {
			get { return ActionType == 2;}
        }
		public bool IsDeleteAction
        {
			get { return ActionType == 3;}
        }
		///// <summary>
		///// If true, error encountered somewhere during workflow
		///// </summary>
		//public bool HasErrors { get; set; }
		///// <summary>
		///// Reset HasSectionErrors to false at the start of a new section of validation. Then check at the end of the section for any errors in the section
		///// </summary>
		//public bool HasSectionErrors { get; set; }
		////TBD
		////public List<StatusMessage> Messages { get; set; } = new List<StatusMessage>();
		//public void AddError( string message )
		//{
		//	//Messages.Add( new StatusMessage() { Message = message } );
		//	Messages.Error.Add( message );
		//	HasErrors = true;
		//	HasSectionErrors = true;
		//}
		public void AddErrorRange( List<string> messages )
		{
			//foreach ( string msg in messages )
			//	Messages.Add( new StatusMessage() { Message = msg } );
			foreach ( string msg in messages )
				Messages.Error.Add( msg );
			HasErrors = true;
			HasSectionErrors = true;
		}
		public void AddNote( string message )
		{
			Messages.Note.Add( message );
		}
		public void AddWarning( string message )
		{
			Messages.Warning.Add( message );
		}
		//public List<string> GetAllMessages()
		//{
		//	List<string> messages = new List<string>();
		//	string prefix = "";
		//	foreach ( StatusMessage msg in Messages.OrderBy( m => m.IsWarning ) )
		//	{
		//		if ( msg.IsWarning )
		//			if ( !msg.Message.ToLower().StartsWith( "warning" ) )
		//				prefix = "Warning - ";
		//			else
		//			if ( !msg.Message.ToLower().StartsWith( "error" ) )
		//				prefix = "Error - ";
		//		messages.Add( prefix + msg.Message );
		//	}

		//	return messages;
		//}
	}
    public class StatusMessage
    {
        public string Message { get; set; }
        public bool IsWarning { get; set; }
    }
}
