using System.Collections.Generic;
using Fiddler;

namespace Star.FiddlerRunner.Common
{
    public interface ISessionRepository
    {
        SessionGroupSequence CreateNewSessionGroupSequence(string url);
        SessionGroupSequence GetSessionGroupSequence(string sequenceId);
        SessionGroupSequence SaveSessionGroupSequence(SessionGroupSequence sessionGroupSequence);

        SessionGroup CreateNewSessionGroup(SessionGroupSequence sessionGroupSequence, string url);
        SessionGroup GetSessionGroup(string groupId);
        SessionGroup SaveSessionGroup(SessionGroup sessionGroup);


        PersistentFiddlerSession SaveSession(Session oSession, SessionGroup sessionGroup);
        PersistentFiddlerSession GetSessionWithId(string id);
        IReadOnlyList<SessionGroupSequence> GetSessionSequenceList();
        IReadOnlyList<SessionGroup> GetSessionGroupListBySequenceId(string sequenceId);
        IReadOnlyList<PersistentFiddlerSession> GetSessionListForSequenceId(string sequenceId);
        IReadOnlyList<PersistentFiddlerSession> GetSessionListForGroupId(string groupId);
    }
}