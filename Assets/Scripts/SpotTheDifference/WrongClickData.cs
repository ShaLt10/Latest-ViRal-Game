using System;

[Serializable]
public class WrongClickData
{
    // nilai boost penalti (semakin besar, waktu makin cepat habis)
    public float boostAmount;
    public int stageIndex;

    public WrongClickData(float amount, int stageIndex)
    {
        boostAmount = amount;
        this.stageIndex = stageIndex;
    }
}
