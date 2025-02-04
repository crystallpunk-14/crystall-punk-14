using Content.Server.Discord;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Serilog;

namespace Content.Server.Chat.Managers;

/// <summary>
///     OOC Relay module
/// </summary>
internal sealed partial class ChatManager
{
    [Dependency] private readonly DiscordWebhook _discord = default!;
    [Dependency] private readonly IConfigurationManager _cfgManager = default!;

    private WebhookIdentifier? _OOCwebhook;

    private void CP14InitializeOOCRelay()
    {
        var webhookUrl = _cfgManager.GetCVar(CCVars.DiscordRoundUpdateWebhook);

        if (webhookUrl == string.Empty)
            return;
        
        _discord.GetWebhook(webhookUrl,
            data =>
            {
                _OOCwebhook = data.ToIdentifier();
            });
    }

    private async void CP14SendOOCInDiscord(ICommonSession player, string message)
    {
        try
        {
            if (_OOCwebhook == null)
                return;

            var name = player.Name;
            var content =
                message.Replace("@", "\\@")
                    .Replace("<", "\\<")
                    .Replace("/",
                        "\\/"); // @ and < are both problematic for discord due to pinging. / is sanitized solely to kneecap links to murder embeds via blunt force

            var payload = new WebhookPayload {Content = $"**`{name}`**: {content}"};
            await _discord.CreateMessage(_OOCwebhook.Value, payload);
        }
        catch (Exception e)
        {
            Log.Error($"Error while sending discord OOC message:\n{e}");
        }
    }
}
