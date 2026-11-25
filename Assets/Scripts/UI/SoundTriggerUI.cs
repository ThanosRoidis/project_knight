using Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SoundTriggerUI : MonoBehaviour, ISelectHandler, ISubmitHandler
{
    [SerializeField] private AudioClip selectClip;
    [SerializeField] private AudioClip submitClip;

    private bool skipNextSelectionSound = false;
    
    public void OnSelect(BaseEventData eventData)
    {
        if(!skipNextSelectionSound)
            AudioController.Instance.PlaySFX(selectClip, exclusive:false);
        skipNextSelectionSound = false;
    }

    public void OnSubmit(BaseEventData eventData)
    {
        AudioController.Instance.PlaySFX(submitClip, exclusive: false);
    }

    public void SkipNextSelectionSound()
    {
        skipNextSelectionSound = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
