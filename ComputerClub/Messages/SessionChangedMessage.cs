using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ComputerClub.Messages;

public class SessionChangedMessage : ValueChangedMessage<int>
{
    public SessionChangedMessage(int computerId) : base(computerId)
    {
    }
}