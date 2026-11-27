using TMPro;
using UnityEngine;

public class EnemyUI : Shared.Singleton<EnemyUI>
{
    [SerializeField] private PointBar healthBar;
    [SerializeField] private TMP_Text enemyName;
    
    private CanvasGroup canvasGroup;

    private Actor currentEnemy;

    protected override void Awake()
    {
        base.Awake();

        if (canvasGroup == null) {
            canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null) {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        Hide();
    }

    void Update()
    {
        if (currentEnemy == null) {
            Hide();
        }
    }

    public void Show(Actor enemy)
    {
        if (enemy == null) {
            Hide();
            return;
        }

        if (currentEnemy == enemy) {
            RefreshUI();
            SetVisible(true);
            return;
        }

        UnbindCurrentEnemy();
        currentEnemy = enemy;
        currentEnemy.HealthChanged += OnEnemyHealthChanged;

        RefreshUI();
        SetVisible(true);
    }

    public void Hide(Actor enemy)
    {
        if (currentEnemy != enemy) return;
        Hide();
    }

    public void Hide()
    {
        UnbindCurrentEnemy();

        if (enemyName != null) {
            enemyName.text = string.Empty;
        }

        if (healthBar != null) {
            healthBar.SetMaxPoints(1);
            healthBar.SetPoints(0);
        }

        SetVisible(false);
    }

    private void OnDisable()
    {
        UnbindCurrentEnemy();
    }

    private void OnEnemyHealthChanged(Actor enemy)
    {
        if (currentEnemy != enemy) return;
        RefreshHealthBar();
    }

    private void RefreshUI()
    {
        if (currentEnemy == null) return;

        if (enemyName != null) {
            enemyName.text = currentEnemy.ActorName;
        }

        RefreshHealthBar();
    }

    private void RefreshHealthBar()
    {
        if (healthBar == null || currentEnemy == null) return;

        healthBar.SetMaxPoints(currentEnemy.MaxHealth);
        healthBar.SetPoints(currentEnemy.CurrentHealth);
    }

    private void UnbindCurrentEnemy()
    {
        if (currentEnemy != null) {
            currentEnemy.HealthChanged -= OnEnemyHealthChanged;
            currentEnemy = null;
        }
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.blocksRaycasts = visible;
        canvasGroup.interactable = visible;
    }
}
