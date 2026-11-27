using UnityEngine;
using UnityEngine.UI;

public class PointBar : MonoBehaviour
{
    [SerializeField] protected Slider _slider;

    protected Transform barPointsTS;
    protected int _maxPoints;
    protected int _points;

    public int MaxPoints { get => _maxPoints; }
    public int Points { get => _points; }

    protected virtual void Awake()
    {
        barPointsTS = transform.Find("Bar Points");
    }
    protected virtual void Start()
    {
        _slider.value = 0;
    }

    public virtual void SetMaxPoints(int maxPoints)
    {
        if (maxPoints > 0) {
            _maxPoints = maxPoints;
            this.UpdateProgress((float)_points / _maxPoints);
        }
    }

    public virtual void SetPoints(int points)
    {
        if (_maxPoints > 0) {
            _points = points;
            this.UpdateProgress((float)_points / _maxPoints);
        }
    }

    private void UpdateProgress(float progress)
    {
        _slider.value = progress;
    }
}
