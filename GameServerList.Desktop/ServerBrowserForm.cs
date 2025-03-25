using GameServerList.Common.External;
using GameServerList.Common.Model;
using GameServerList.Common.Utils;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using static System.Windows.Forms.ListView;

namespace GameServerList.Desktop
{
    public partial class ServerBrowserForm : Form
    {
        private static List<Game> Games { get; set; }
        private static IMemoryCache Cache { get; set; } = new MemoryCache(new MemoryCacheOptions());

        public ServerBrowserForm()
        {
            InitializeComponent();
            LoadGameList();

            DoubleBuffered = true;
            Text = "Game Server Browser";

            var tableLayout = new TableLayoutPanel();
            tableLayout.Parent = this;
            tableLayout.Dock = DockStyle.Fill;
            tableLayout.CellBorderStyle = TableLayoutPanelCellBorderStyle.Outset;

            var serverList = new ListView();
            serverList.Parent = tableLayout;
            serverList.Dock = DockStyle.Fill;
            serverList.BorderStyle = BorderStyle.FixedSingle;
            serverList.View = View.Details;
            serverList.FullRowSelect = true;
            serverList.HoverSelection = serverList.MultiSelect = false;

            serverList.Columns.Add("Name");
            serverList.Columns.Add("Players");
            serverList.Columns.Add("Map");

            serverList.DoubleClick += (s1, e1) => ConnectToServer(serverList.SelectedItems);

            var games = new ComboBox();
            games.Parent = tableLayout;
            games.Dock = DockStyle.Fill;
            games.DropDownStyle = ComboBoxStyle.DropDownList;
            games.SelectedIndexChanged += async (s1, e1) => await RefreshServerList(games.SelectedIndex, serverList);

            foreach (var game in Games)
                games.Items.Add(game.Name);

            games.SelectedIndex = 0;

            tableLayout.SetCellPosition(games, new TableLayoutPanelCellPosition(0, 0));
            tableLayout.SetCellPosition(serverList, new TableLayoutPanelCellPosition(0, 1));

            tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100.0f));

            UpdateListColumns(serverList);
            Resize += (s1, e1) => UpdateListColumns(serverList);
        }

        private static void LoadGameList()
        {
            var gameData = FileUtils.LoadDataFromFile<List<Game>>("games.json");
            if (gameData is null)
                return;
            Games = [.. gameData.OrderBy(g => g.Name)];
        }

        private static void UpdateListColumns(ListView view)
        {
            if (view is null)
                return;

            int size = view.ClientSize.Width;
            foreach (ColumnHeader h in view.Columns)
                h.Width = (size / view.Columns.Count);
        }

        private static async Task RefreshServerList(int index, ListView serverList)
        {
            if (Games.Count == 0 || index < 0 || index >= Games.Count)
                return;

            serverList.Items.Clear();
            var game = Games[index];
            var items = await Query(game);

            foreach (var item in items)
            {
                serverList.Items.Add(new ListViewItem([item.Name, $"{item.CurrentPlayers} / {item.MaxPlayers}", item.Map])
                {
                    Tag = item
                });
            }
        }

        private static async Task<List<GameServerItem>> Query(Game game, int timeoutServers = 500, int timeoutMasterServer = 15000)
        {
            if (game is null || game.MasterServer.HasValue == false)
            {
                MessageBox.Show("Invalid game - null or missing master server info!");
                return [];
            }

            return await Cache.GetOrCreateAsync<List<GameServerItem>>($"game-servers-{game.Name.ToLower()}",
                async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                var servers = await A2SQuery.QueryServerList(game.MasterServer.Value, game, timeoutMasterServer);
                var items = new List<GameServerItem>();

                foreach (var server in servers)
                {
                    var obj = await A2SQuery.QueryServerInfo(server.Address, timeoutServers);
                    if (obj is null || !obj.HasValue) continue;
                    items.Add(obj.Value.MapToGameServerItem(game));
                }

                return [.. items.OrderByDescending(o => o.CurrentPlayers)];
            });
        }

        private static void ConnectToServer(SelectedListViewItemCollection selectedItems)
        {
            if (selectedItems.Count == 0)
                return;

            var server = selectedItems[0].Tag as GameServerItem;
            if (server is null)
                return;

            Process.Start("explorer.exe", $"steam://connect/{server.Address}");
        }
    }
}
