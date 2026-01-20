using UnityEngine;
using System; // ���� System ��ʹ�� Action �¼�

/// <summary>
/// �÷ֹ�����������ģʽ��
/// �ں��˷������㣨�ؿ��ڣ�����ʯ���ҹ������־û���
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    [Tooltip("����Destructible��ǩ������÷֣�Ĭ��2�֣�")]
    public int rockBreakScore = 2;

    [Tooltip("�ռ�Diamond��ǩ����ʯ�÷֣�ÿ��10�֣�")]
    public int diamondScore = 10;

    [Tooltip("����ͨ�ط�����Ĭ��100�֣�")]
    public int baseScore = 100;

    [Header("Debug")]
    [Tooltip("�Ƿ����õ�����־")]
    public bool enableDebugLog = false;

    // === �������� ===

    // 1. �ؿ����� (ͨ��������)
    private int currentScore = 0;

    // 2. �������� (�̵������) - �־û�����
    // ʹ�� PlayerPrefs ������ݳ�ʼ����ȷ�����ֲ���ʧ
    public int currentPoints { get; private set; } = 0;

    // === �¼�ϵͳ ===
    // �����������仯ʱ֪ͨ UI (��������ǰ��������)
    public event Action<int> OnPointsChanged;

    void Awake()
    {
        // ����ģʽ��ȷ��ֻ��һ��ʵ��
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // �糡������
            InitializeData(); // ��ʼ��
        }
        else
        {
            // ����Ѵ���ʵ�������ٵ�ǰ����
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ��ʼ������
    /// </summary>
    private void InitializeData()
    {
        // 1. ��ʼ���ؿ��������������ۣ�
        currentScore = baseScore;

        // 2. ��ʼ������ (�Ӵ浵��ȡ�����û����Ϊ0 - ��ʼ�ʽ�Ϊ0)
        currentPoints = PlayerPrefs.GetInt("PlayerPoints", 0);

        if (enableDebugLog)
        {
            Debug.Log($"ScoreManager: ��ʼ����ɡ���������: {baseScore}, ��ǰ����: {currentPoints}");
        }
    }

    // ==========================================
    //Part 1: ��Ϸ����Ϊ (ʰȡ/����) - ������Ӿ�����
    // ==========================================

    /// <summary>
    /// ���ӻ�����ʯ�÷�
    /// </summary>
    public void AddRockBreakScore(Vector3 position)
    {
        // 1. 增加关卡分数（用于关卡评价）
        currentScore += rockBreakScore;

        // 2. 增加积分（用于商店购买）- 击碎障碍物每个+2积分
        AddPoints(rockBreakScore);

        if (enableDebugLog) Debug.Log($"ScoreManager: �����ϰ� +{rockBreakScore}���ܷ�: {currentScore}");

        // ��ʾ�÷ֶ��������ɫ��
        FloatingScoreText.Create(position, $"+{rockBreakScore}", new Color(1f, 0.7f, 0.2f, 1f), false);
    }

    /// <summary>
    /// ��Ϸ���ռ���ʯ���ӹؿ��������������ۣ��ͻ��֣������̵꣩
    /// </summary>
    public void AddDiamondScore(Vector3 position)
    {
        // 1. �ӹؿ����� (���ڹ�������)
        currentScore += diamondScore;

        // 2. �ӻ��� (�����̵�) - �ռ���ʯ���10����
        AddPoints(diamondScore);

        if (enableDebugLog) Debug.Log($"ScoreManager: �ռ���ʯ! �ؿ�����+{diamondScore}, ����+{diamondScore}, ��ǰ����: {currentPoints}");

        // 3. ��ʾ�÷ֶ�������ɫ + ��Ч��
        FloatingScoreText.Create(position, $"+{diamondScore}", new Color(0.2f, 0.8f, 1f, 1f), true);
    }

    /// <summary>
    /// �����Զ������
    /// </summary>
    public void AddScore(int points, Vector3 position, Color? color = null)
    {
        currentScore += points;

        Color textColor = color ?? Color.white;
        FloatingScoreText.Create(position, $"+{points}", textColor);
    }

    // ==========================================
    // Part 2: ����ϵͳ (�̵꽻��/UI����)
    // ==========================================

    /// <summary>
    /// ������������ (���漰�ؿ���������Ч)
    /// �̵�ۿ�ʱ���ã�AddPoints(-100);
    /// ͨ�ؽ���ʱ���ã�AddPoints(100);
    /// </summary>
    /// <param name="amount">���� (�����ӣ�������)</param>
    public void AddPoints(int amount)
    {
        currentPoints += amount;
        
        // ȷ�����ֲ�Ϊ����
        if (currentPoints < 0)
        {
            currentPoints = 0;
        }

        // ���浽Ӳ�̣���ֹ����
        PlayerPrefs.SetInt("PlayerPoints", currentPoints);
        PlayerPrefs.Save();

        // �㲥�¼���֪ͨ UI ����
        OnPointsChanged?.Invoke(currentPoints);

        if (enableDebugLog) Debug.Log($"���ֱ䶯: {amount}, ��ǰ����: {currentPoints}");
    }

    /// <summary>
    /// ͨ�ؽ������ɹ�ͨ��һ�ػ��100����
    /// </summary>
    public void AddVictoryReward()
    {
        AddPoints(100);
        
        if (enableDebugLog)
        {
            Debug.Log($"ScoreManager: ͨ�ؽ��������100���֣���ǰ����: {currentPoints}");
        }
    }

    /// <summary>
    /// �����ͷ��������һ��������0���֣����㱾�ػ�õĻ��֣�
    /// ע�⣺ֻ���㱾�ػ�õĻ��֣���Ӱ��֮ǰ���۵Ļ���
    /// </summary>
    public void ResetLevelPoints()
    {
        // ���㱾�ػ�õĻ���
        int levelEarnedPoints = currentScore - baseScore;
        
        // ������ػ���˻��֣���۳�
        if (levelEarnedPoints > 0)
        {
            AddPoints(-levelEarnedPoints);
            
            if (enableDebugLog)
            {
                Debug.Log($"ScoreManager: �����ͷ����۳����ػ�õĻ���: {levelEarnedPoints}����ǰ����: {currentPoints}");
            }
        }
    }

    // ==========================================
    // Part 3: ����������
    // ==========================================

    /// <summary>
    /// ��ȡ��ǰ�ؿ��ܷ�
    /// </summary>
    public int GetCurrentScore()
    {
        return currentScore;
    }

    /// <summary>
    /// ���ùؿ��÷֣��������¿�ʼ��Ϸ��
    /// ע�⣺������ currentPoints�����֣�����Ϊ������һ��ܵĲƲ�
    /// </summary>
    public void ResetScore()
    {
        currentScore = baseScore;

        if (enableDebugLog) Debug.Log($"ScoreManager: ���ùؿ�������");
    }

    public void SetScore(int score)
    {
        currentScore = score;
    }

    /// <summary>
    /// ���´�PlayerPrefs��ȡ���ݣ�����������ݺ����ã�
    /// </summary>
    public void ReinitializeFromPlayerPrefs()
    {
        // ���¶�ȡ����
        currentPoints = PlayerPrefs.GetInt("PlayerPoints", 0);
        
        // �㲥�¼���֪ͨUI����
        OnPointsChanged?.Invoke(currentPoints);
        
        if (enableDebugLog)
        {
            Debug.Log($"ScoreManager: ���³�ʼ����ɣ���ǰ����: {currentPoints}");
        }
    }
}