using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ComputerClub.Messages;

public class SessionChangedMessage(int computerId) : ValueChangedMessage<int>(computerId);