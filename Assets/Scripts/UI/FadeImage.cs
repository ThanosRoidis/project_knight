using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shared
{
    public class FadeImage : Singleton<FadeImage>
    {
        public enum FadeMethod
        {
            Fill,
            Transparent,
            Flash //Black -> White -> Transparent
        }
        public FadeMethod fadeMethod = FadeMethod.Fill;

        [SerializeField] private float fadeClearDuration = 1.5f;
        [SerializeField] private float fadeFillDuration = 1.5f;
        [SerializeField] private int transparencySteps = 10;

        [SerializeField] private AudioClip fadeClearClip;
        [SerializeField] private AudioClip fadeFillClip;
        [SerializeField] private bool playFadeClearSound = true;
        [SerializeField] private bool playFadeFillSound = true;

        private UnityEngine.UI.Image fadeImage;
        private Color initialColor;


        public float FadeFillDuration { set => fadeFillDuration = value; get => fadeFillDuration; }
        public float FadeClearDuration { set => fadeClearDuration = value; get => fadeClearDuration; }


        public Action ExecuteOnClear;
        public Action ExecuteOnFill;

        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();
            fadeImage = GetComponent<UnityEngine.UI.Image>();
            initialColor = fadeImage.color;
        }

        /// <summary>
        /// From black to transparent
        /// </summary>
        public void FadeClear()
        {
            StartCoroutine(Fade(fadeClearDuration, fill: false));
            if (playFadeClearSound)
                AudioController.Instance.PlaySFX(fadeClearClip);
        }

        /// <summary>
        /// Turn to black
        /// </summary>
        /// <param name="duration"></param>
        public void FadeFill()
        {
            StartCoroutine(Fade(fadeFillDuration, fill: true));
            if (playFadeFillSound)
                AudioController.Instance.PlaySFX(fadeFillClip);
        }

        public IEnumerator Fade(float fadeDuration, bool fill = true)
        {
            //Initialize Fade Image filling
            if (fadeMethod == FadeMethod.Fill) {
                initialColor.a = 1f; // When using fill, transparency should be 1
                fadeImage.color = initialColor;
                if (fill) {
                    fadeImage.fillAmount = 0;
                } else {
                    fadeImage.fillAmount = 1f;
                }
            } else if (fadeMethod == FadeMethod.Transparent) {
                fadeImage.fillAmount = 1f; // When using transparency, fill amount should be 1
                if (fill) {
                    initialColor.a = 0f;
                } else {
                    initialColor.a = 1f;
                }
                fadeImage.color = initialColor;
            } else if (fadeMethod == FadeMethod.Flash) {
                // Only clear with FadeMethod.Flash
                fadeImage.fillAmount = 1f; // When using transparency, fill amount should be 1
                initialColor.a = 1f;
                fadeImage.color = initialColor;

            }

            Color targetFlashColor = Color.white;
            float flashStayPerc = 0.2f;
            float flashLowerBound = 0.5f - flashStayPerc / 2;
            float flashUpperBound = 0.5f + flashStayPerc / 2;
            float transparencyStep = 1f / transparencySteps;

            // Progress the image
            float startTime = Time.timeScale == 0 ? Time.unscaledTime : Time.time;
            while (true) {
                float time = Time.timeScale == 0 ? Time.unscaledTime : Time.time;
                float perc = (time - startTime) / fadeDuration;


                switch (fadeMethod) {
                    case FadeMethod.Fill:
                        fadeImage.fillAmount = fill ? perc : 1 - perc;
                        break;

                    case FadeMethod.Transparent:
                        float transparency = fill ? perc : 1 - perc;
                        transparency = MathUtils.RoundStep(transparency, transparencyStep);
                        initialColor.a = transparency;
                        fadeImage.color = initialColor;

                        break;
                    case FadeMethod.Flash:
                        // Flash method works only with fading clear...

                        if (perc <= flashLowerBound) {
                            float transparencyPercentage = MathUtils.RoundStep(perc / flashLowerBound, transparencyStep);
                            fadeImage.color = Color.Lerp(initialColor, targetFlashColor, transparencyPercentage);

                        } else if (perc >= flashUpperBound) {
                            float transparencyPercentage = (perc - flashUpperBound) / flashLowerBound;
                            transparency = fill ? transparencyPercentage : 1 - transparencyPercentage;
                            transparency = MathUtils.RoundStep(transparency, transparencyStep);
                            targetFlashColor.a = transparency;
                            fadeImage.color = targetFlashColor;
                        }



                        break;

                }


                if (perc >= 1) {
                    break;
                }
                yield return null;
            }

            if (fill) {
                if (ExecuteOnFill != null) {
                    ExecuteOnFill();
                }
            } else {
                if (ExecuteOnClear != null) {
                    ExecuteOnClear();
                }
            }

        }


    }
}