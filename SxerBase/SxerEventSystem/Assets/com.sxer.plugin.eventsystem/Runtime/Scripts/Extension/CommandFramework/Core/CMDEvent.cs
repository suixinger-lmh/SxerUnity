
namespace Sxer.Plugin.EventSystem.Cmd
{
    public class CMDEvent : CoreEvent<CMDEvent>
    {
        public CMDEvent(string id,CommandHandlerArg arg) {
            
            cmdID = id;
            commandHandlerArg = arg;
        }

        public string cmdID;
        public CommandHandlerArg commandHandlerArg;
    }
}