using Discord;
using Discord.WebSocket;
using DiscordBot.Extensions;
using DiscordBot.Resources;
using GameLogic.ParanoiaCorp;
using GameLogic.ParanoiaCorp.Cycles;
using System.Text;
using WebServer.Shared.ParanoiaCorp.Extensions;

namespace DiscordBot
{
    public class DiscordEndGameController
    {
        public async Task ProcessEndGameAsync(EndGameCycle cycle, SocketTextChannel generalChannel)
        {
            MessageComponent components = new ComponentBuilder()
                .WithButton("Terminate Session", "lobby_terminate", ButtonStyle.Success)
                .Build();
            await generalChannel.SendMessageAsync(components: components);

            EndGameHistory history = cycle.EndGameHistory;

            Embed mainEmbed = new EmbedBuilder()
                .WithTitle(history.Winner.HasValue ? Miscellaneous.GameOverTitle : Miscellaneous.DrawTitle)
                .WithDescription(history.Winner.HasValue
                    ? $"{history.Winner.Value.GetTeamIndicator()} " + string.Format(Miscellaneous.WinnerFactionTitle, history.Winner.Value.GetLocilizedBelonging())
                    : Miscellaneous.DrawWinnerTitle)
                .WithColor(history.Winner.HasValue ? Color.Gold : Color.LightGrey)
                .Build();

            await generalChannel.SendMessageAsync(embed: mainEmbed);

            List<Embed> roundEmbeds = new List<Embed>();

            foreach(EndGameRoundHistory round in history.Rounds)
            {
                EmbedBuilder roundEmbed = new EmbedBuilder()
                    .WithTitle(string.Format(Miscellaneous.TurnNumberTitle, round.Turn))
                    .WithColor(Color.Blue);
                
                string candidatesText = round.CandidatesForFiring != null
                    ? string.Join(", ", round.CandidatesForFiring.Select(id => $"<@{id}>"))
                    : string.Empty;

                string firedText = round.FiredPlayer.HasValue
                    ? $"<@{round.FiredPlayer.Value}>"
                    : string.Empty;

                roundEmbed.AddField(Miscellaneous.DirectorBoardTitle, $"{Miscellaneous.CandidatesTitle} {candidatesText}\n{Miscellaneous.FiredTitle} {firedText}");

                if(round.OvertimeActions != null && round.OvertimeActions.Count > 0)
                {
                    StringBuilder overtimeSb = new StringBuilder();
                    foreach(EndGameOvernightHistory action in round.OvertimeActions)
                    {
                        string targets = action.Targets.Any()
                            ? string.Join(", ", action.Targets.Select(t => $"<@{t}>"))
                            : "None";

                        overtimeSb.AppendLine($"<@{action.Executor}> ({action.ExecutorRoleType.IntoSignature().MapRole().GetLocalizedName()}) ----> {{ {targets} }}");
                    }
                    roundEmbed.AddField(Miscellaneous.OvertimeTitle, overtimeSb.ToString());
                }
                else
                {
                    roundEmbed.AddField(Miscellaneous.OvertimeTitle, Miscellaneous.OvertimeNoActionHappenedInfo);
                }

                roundEmbeds.Add(roundEmbed.Build());
            }

            for(int i = 0; i < roundEmbeds.Count; i += 10)
            {
                Embed[] batch = roundEmbeds.Skip(i).Take(10).ToArray();
                await generalChannel.SendMessageAsync(embeds: batch);
            }

            await generalChannel.SendMessageAsync(Miscellaneous.EndGameSessionDeleteTitle);
        }
    }
}