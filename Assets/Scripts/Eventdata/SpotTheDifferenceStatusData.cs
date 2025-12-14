using System;

[Serializable]
public class SpotTheDifferenceStatusData
{
    // State global untuk seluruh stage mini-game ini
    public bool started;       // true saat stage sedang berjalan (input on, timer jalan)
    public bool win;           // true saat stage dituntaskan dengan menang
    public bool lose;          // true saat kalah (time habis)
    public bool gameFinished;  // true saat stage sudah selesai (menang/kalah), input dimatikan

    // Opsional: penanda stage sekarang (1/2/…)
    public int stageIndex;

    public static SpotTheDifferenceStatusData Start(int stageIndex)
      => new SpotTheDifferenceStatusData { started = true, stageIndex = stageIndex };

    public static SpotTheDifferenceStatusData Lose(int stageIndex)
      => new SpotTheDifferenceStatusData { lose = true, gameFinished = true, stageIndex = stageIndex };

    public static SpotTheDifferenceStatusData Win(int stageIndex)
      => new SpotTheDifferenceStatusData { win = true, gameFinished = true, stageIndex = stageIndex };

    public static SpotTheDifferenceStatusData Reset(int stageIndex)
      => new SpotTheDifferenceStatusData { started = false, gameFinished = false, stageIndex = stageIndex };
}
