namespace PrivateMemoirEnum
{
    public static class PrivateMemoirEnum
    {
        public enum CurrentMemoirField
        {
            NONE = 0,
            MEMOIR_TEXT = 1,
            MEMOIR_TITLE = 2,
            MEMOIR_DATE_CHANGE = 3
        }

        public enum TcpCommands
        {
            ServerFailed = 1,
            ServerOK = 0,
            ServerLoginFailed = 3,
            ServerLoginOK = 2,
            ServerRegistrationFailed = 5,
            ServerRegistrationOK = 6,
            ServerBye = 7,
            ServerGetDataResponse = 10,
            ClientHello = 100,
            ClientRegistrationQuery = 150,
            ClientBye = 200,
            ClientLoginQuery = 20,
            ClientGetDataQuery = 30,
            ClientMarkFieldQuery = 40,
            ClientMarkMemoirQuery = 50,
            ClientAddDataQuery = 60,
            ClientUpdateDataQuery = 70,
            ClientDeleteDataQuery = 80
        };
    }
}