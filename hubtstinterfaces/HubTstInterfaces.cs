using System.Collections.Generic;
using System.Threading.Tasks;
namespace HubTstInterfaces{

    /// <summary>
    /// Hub Functions(Called by Client)
    /// </summary>
    public interface IHubTstServerFunctions {
        Task LogOn(string User);
        Task SendMessage(string User, string Message);
        Task UserList();
        Task LogOff();
    }
    /// <summary>
    /// Client Functions(Called by Hub)
    /// Hub実装をHub<T>をベースとし、T型に定義されたメソッドを呼び出すと
    /// 実装が無くても、SendAsyncが呼び出されるようだ。
    /// 確かに、厳格な型付けはできるが、勝手に実装されるのは
    /// チョット嫌だな。
    /// </summary>
    public interface IHubTstClientFunctions {
        Task ReceiveMessage(string User, string Message, string Color);
        Task LogOnMessage(UserInfo user);
        Task LogOffMessage(UserInfo user);
        Task UserListResult(List<UserInfo> Users);
        Task ServerMessage(string Message);
    }
    /// <summary>
    /// ユーザー情報
    /// </summary>
    public class UserInfo {
        public string ConnectionId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? Color { get; set; } = null;
    }
}