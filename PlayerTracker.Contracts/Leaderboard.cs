namespace PlayerTracker.Contracts;

public class Leaderboard
{
	public Tournament Tournament { get; set; }
	public List<PlayerResult> Players { get; set; }
}