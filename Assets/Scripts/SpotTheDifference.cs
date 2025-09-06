using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpotTheDifference : MonoBehaviour
{
    public List<Button> Buttons = new List<Button>();  // Tombol-tombol perbedaan
    public Button wrongButton;                         // Tombol klik salah
    public TextMeshProUGUI textScore;                  // Teks skor
    public TextMeshProUGUI textTimer;                  // Teks timer
    public GameObject redCirclePrefab;                 // Prefab lingkaran merah
    public GameObject crossPrefab;                     // Prefab tanda silang
    public GameObject guidePanel;                      // Panel petunjuk
    public Button startButton;                         // Tombol mulai permainan
    public GameObject gameOverPanel;                   // Panel akhir permainan
    public TextMeshProUGUI gameOverText;               // Teks akhir permainan

    private int currentScore = 0;
    private float timeRemaining = 30f;                 // Waktu permainan
    private bool gameStarted = false;
    private int totalDifferences;                      // Total tombol perbedaan

    void Start()
    {
        // Atur listener untuk tombol salah
        wrongButton.onClick.AddListener(() => WrongClick(wrongButton.transform.position));

        // Atur listener untuk setiap tombol perbedaan
        foreach (var button in Buttons)
        {
            button.onClick.AddListener(() => CorrectClick(button));
        }

        // Listener untuk tombol mulai
        startButton.onClick.AddListener(StartGame);

        // Inisialisasi skor
        textScore.text = "Skor: 0";
        textTimer.text = "Waktu: 30s";

        // Hitung total perbedaan
        totalDifferences = Buttons.Count;

        // Sembunyikan elemen awal
        gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (gameStarted && timeRemaining > 0)
        {
            // Hitung mundur waktu
            timeRemaining -= Time.deltaTime;
            textTimer.text = "Waktu: " + Mathf.CeilToInt(timeRemaining) + "s";

            if (timeRemaining <= 0)
            {
                EndGame(false);
            }
        }
    }

    public void StartGame()
    {
        // Mulai permainan
        guidePanel.SetActive(false);
        gameStarted = true;
        timeRemaining = 30f;
    }

    public void CorrectClick(Button clickedButton)
    {
        // Tambahkan skor
        currentScore++;
        textScore.text = "Skor: " + currentScore;

        // Tampilkan lingkaran merah di lokasi tombol
        Vector3 position = clickedButton.transform.position;
        Instantiate(redCirclePrefab, position, Quaternion.identity);

        // Nonaktifkan tombol agar tidak bisa diklik lagi
        clickedButton.interactable = false;

        // Cek jika semua perbedaan telah ditemukan
        if (currentScore == totalDifferences)
        {
            EndGame(true);
        }
    }

    public void WrongClick(Vector3 clickPosition)
    {
        // Tampilkan tanda silang di lokasi klik
        Instantiate(crossPrefab, clickPosition, Quaternion.identity);

        // Kurangi skor
        currentScore = Mathf.Max(currentScore - 1, 0); // Skor tidak bisa di bawah 0
        textScore.text = "Skor: " + currentScore;
    }

    public void EndGame(bool win)
    {
        // Hentikan permainan
        gameStarted = false;
        textTimer.text = "Waktu: 0s";

        // Tampilkan panel akhir
        gameOverPanel.SetActive(true);
        gameOverText.text = win ? "Selamat! Anda Menang!" : "Waktu Habis! Coba Lagi.";
    }
}
