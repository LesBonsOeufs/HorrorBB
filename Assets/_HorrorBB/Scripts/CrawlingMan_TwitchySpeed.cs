using NaughtyAttributes;
using System.Collections;
using UnityEngine;

namespace Root
{
    [RequireComponent(typeof(CrawlingMan))]
    public class CrawlingMan_TwitchySpeed : MonoBehaviour
    {
        [SerializeField, MinMaxSlider(0f, 10f)] private Vector2 speedMinMax = new (2f, 8f);
        [SerializeField, MinMaxSlider(0f, 2f)] private Vector2 twitchDurationMinMax = new (0.2f, 0.4f);
        [SerializeField, MinMaxSlider(0f, 2f)] private Vector2 pauseDurationMinMax = new (0.1f, 0.5f);

        private Coroutine twitchCoroutine;
        private CrawlingMan crawlingMan;
        private float initSpeed;

        private float RandomSpeed => Random.Range(speedMinMax.x, speedMinMax.y);
        private float RandomTwitchDuration => Random.Range(twitchDurationMinMax.x, twitchDurationMinMax.y);
        private float RandomPauseDuration => Random.Range(pauseDurationMinMax.x, pauseDurationMinMax.y);

        private void Awake()
        {
            crawlingMan = GetComponent<CrawlingMan>();
        }

        public void SetMaxSpeed(float min) => speedMinMax.x = min;
        public void SetMinSpeed(float max) => speedMinMax.y = max;

        private void OnEnable()
        {
            initSpeed = crawlingMan.speed;
            twitchCoroutine = StartCoroutine(TwitchySpeedControl());
        }

        private void OnDisable()
        {
            if(twitchCoroutine != null)
                StopCoroutine(twitchCoroutine);

            crawlingMan.speed = initSpeed;
        }

        private IEnumerator TwitchySpeedControl()
        {
            while (enabled)
            {
                crawlingMan.speed = RandomSpeed;
                yield return new WaitForSeconds(RandomTwitchDuration);
                crawlingMan.speed = 0f;
                yield return new WaitForSeconds(RandomPauseDuration);
            }
        }
    }
}