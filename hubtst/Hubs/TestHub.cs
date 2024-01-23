using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using HubTstInterfaces;

namespace HubTst.Hubs;

public class TestHub : Hub<IHubTstClientFunctions>,IHubTstServerFunctions {
    private readonly IHubContext<TestHub> _ctx;
    //private static Timer CycleTimer = null!;
    private static List<UserInfo> LogonUsers = new List<UserInfo>();
    private static UserColors colors = new UserColors();
    public TestHub(IHubContext<TestHub> ctx) : base() {
        _ctx = ctx;
        /*** サーバーからの定期メッセージ⇒試験的な物なので、コメントアウト
        if (CycleTimer == null) {
            CycleTimer = new Timer(
                async (_) => {
                    // thisがキャプチャされてしまうので、SendAsyncを使用しないとコケる
                    await _ctx.Clients.All
                        .SendAsync(nameof(IHubTstClientFunctions.ServerMessage),
                            $"Info : {DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}");
                },
                null,0,60000
            );
        }
        */
    }
    /// <summary>
    /// Logon要求
    /// </summary>
    /// <param name="User"></param>
    /// <returns></returns>
    public async Task LogOn(string User) {
        // Hub<T> T:Interfaceをベースにすると、Interfaceに定義されたメソッドを実装しなくても、
        // SendAsyncとして実行されるようだ。
        string? color = colors.GetUserColor(Context.ConnectionId);
        UserInfo uinf = new UserInfo() { ConnectionId = Context.ConnectionId, UserName = User, Color = color }; 
        LogonUsers.Add(uinf);
        await Clients.Others.LogOnMessage(uinf);
    }
    /// <summary>
    /// メッセージ送信
    /// </summary>
    /// <param name="User">ユーザー名</param>
    /// <param name="Message">メッセージ</param>
    /// <returns></returns>
    public async Task SendMessage(string User, string Message) {
        string? color = colors[Context.ConnectionId]?.Color;
        await Clients.All
            .ReceiveMessage(User, Message,color!);
    }
    /// <summary>
    /// LogOff要求
    /// </summary>
    /// <returns></returns>
    public async Task LogOff() {
        var User = LogonUsers.Where(v=>v.ConnectionId == Context.ConnectionId).FirstOrDefault();
        if (User != null) {
            colors.RemoveUserColor(Context.ConnectionId);
            await Clients.Others
                .LogOffMessage(User);
            LogonUsers.Remove(User);
        }
    }
    /// <summary>
    /// 切断時
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var User = LogonUsers.Where(v=>v.ConnectionId == Context.ConnectionId).FirstOrDefault();
        if (User != null) {
            colors.RemoveUserColor(Context.ConnectionId);
            await Clients.Others
                .LogOffMessage(User);
            LogonUsers.Remove(User);
        }
    }
    /// <summary>
    /// ユーザー一覧
    /// </summary>
    /// <returns></returns>
    public async Task UserList() {
        List<UserInfo> users = new List<UserInfo>();
        foreach(var itm in LogonUsers) {
            users.Add(itm);
        }
        await Clients.Caller
            .UserListResult(users);
    }
}
/// <summary>
/// ユーザー文字色
/// </summary>
public class UserColor {
    public string Color { get; set; } = null!;
    public bool Used { get; set; }
    public UserColor() {

    }
    public UserColor(string Color) {
        this.Color = Color;
    }
}
/// <summary>
/// ユーザー文字色操作
/// </summary>
public class UserColors {
    private static List<UserColor> Colors = new List<UserColor>() {
        new UserColor("red"),
        new UserColor("green"),
        new UserColor("blue"),
        new UserColor("magenta"),
        new UserColor("cyan"),
        new UserColor("pink"),
        new UserColor("orange"),
        new UserColor("yellow"),
        new UserColor("darkgreen"),
        new UserColor("darkblue"),
        new UserColor("navy")
    };
    private static Dictionary<string,UserColor> UsedColors = new Dictionary<string,UserColor>();

    /// <summary>
    /// 空きの色を取得する
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public string? GetUserColor(string id) {
        foreach(var itm in Colors) {
            if (!itm.Used) {
                itm.Used = true;
                UsedColors.Add(id,itm);
                return itm.Color;
            }
        }
        return null;
    }
    /// <summary>
    /// インデクサ
    /// </summary>
    /// <value></value>
    public UserColor? this[string id] {
        get {
            if (UsedColors.ContainsKey(id)) {
                return UsedColors[id];
            } else {
                return null;
            }
        }
    }
    /// <summary>
    /// 指定の色を解放する
    /// </summary>
    /// <param name="id"></param>
    public void RemoveUserColor(string id) {
        if (this[id] != null) {
            this[id]!.Used = false;
            UsedColors.Remove(id);
        }
    }
}