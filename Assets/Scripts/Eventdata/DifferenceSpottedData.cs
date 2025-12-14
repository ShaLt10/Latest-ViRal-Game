using System;

[Serializable]
public class DifferenceSpottedData
{
    // berapa titik benar yang terdeteksi (default 1)
    public int addDifference;
    public int stageIndex;

    public DifferenceSpottedData(int add = 1, int stageIndex = 1)
    {
        addDifference = add < 1 ? 1 : add;
        this.stageIndex = stageIndex;
    }
}
