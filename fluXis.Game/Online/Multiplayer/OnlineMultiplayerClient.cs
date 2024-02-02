using fluXis.Game.Online.API.Models.Multi;
using fluXis.Game.Online.Fluxel;
using fluXis.Game.Online.Fluxel.Packets.Multiplayer;
using osu.Framework.Allocation;

namespace fluXis.Game.Online.Multiplayer;

public partial class OnlineMultiplayerClient : MultiplayerClient
{
    [Resolved]
    private Fluxel.Fluxel fluxel { get; set; }

    private IMultiplayerClient impl => this;

    [BackgroundDependencyLoader]
    private void load()
    {
        fluxel.RegisterListener<MultiplayerJoinPacket>(EventType.MultiplayerJoin, onUserJoined);
        fluxel.RegisterListener<MultiplayerLeavePacket>(EventType.MultiplayerLeave, onUserLeave);
        fluxel.RegisterListener<MultiplayerRoomUpdate>(EventType.MultiplayerRoomUpdate, onRoomUpdate);
        fluxel.RegisterListener<MultiplayerReadyUpdate>(EventType.MultiplayerReady, onReadyUpdate);
        fluxel.RegisterListener<dynamic>(EventType.MultiplayerStartGame, onStartGame);
    }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);

        fluxel.UnregisterListener<MultiplayerJoinPacket>(EventType.MultiplayerJoin, onUserJoined);
        fluxel.UnregisterListener<MultiplayerLeavePacket>(EventType.MultiplayerLeave, onUserLeave);
        fluxel.UnregisterListener<MultiplayerRoomUpdate>(EventType.MultiplayerRoomUpdate, onRoomUpdate);
        fluxel.UnregisterListener<MultiplayerReadyUpdate>(EventType.MultiplayerReady, onReadyUpdate);
        fluxel.UnregisterListener<dynamic>(EventType.MultiplayerStartGame, onStartGame);
    }

    private void onUserJoined(FluxelResponse<MultiplayerJoinPacket> response) => impl.UserJoined(response.Data.Player);
    private void onUserLeave(FluxelResponse<MultiplayerLeavePacket> response) => impl.UserLeft(response.Data.UserID);
    private void onRoomUpdate(FluxelResponse<MultiplayerRoomUpdate> response) => impl.SettingsChanged(response.Data.Data);
    private void onReadyUpdate(FluxelResponse<MultiplayerReadyUpdate> response) => impl.ReadyStateChanged(response.Data.PlayerID, response.Data.Ready);
    private void onStartGame(FluxelResponse<dynamic> response) => impl.Starting();
}