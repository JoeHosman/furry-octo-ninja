using System.Collections.Generic;
using Fiddler;

namespace Star.FiddlerRunner.Common
{
    public interface ISessionRepository
    {
        SessionGroupSequence CreateNewSessionGroupSequence();
        SessionGroup CreateNewSessionGroup(SessionGroupSequence sessionGroupSequence);

        PersistentFiddlerSession SaveSession(Session oSession, SessionGroup sessionGroup);
        PersistentFiddlerSession GetSessionWithId(string id);
        IReadOnlyList<SessionGroupSequence> GetSessionSequenceList();
        IReadOnlyList<SessionGroup> GetSessionGroupListBySequenceId(string sequenceId);
        IReadOnlyList<PersistentFiddlerSession> GetSessionListForSequenceId(string sequenceId);
        IReadOnlyList<PersistentFiddlerSession> GetSessionListForGroupId(string groupId);
    }
}