public class SpotTheDifferenceStatusData
{
    public bool gameStart;
    public bool gameWin;
    public bool gameFinished;
    public int InstanceId;

    public SpotTheDifferenceStatusData(bool gameStart, bool gameWin, bool gamefinished , int instanceId =0)
    {
        this.gameStart = gameStart;
        this.gameWin = gameWin;
        this.gameFinished = gamefinished;
        InstanceId = instanceId;
    }
}
