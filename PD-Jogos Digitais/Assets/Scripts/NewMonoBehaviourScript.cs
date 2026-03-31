using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public class AudioMenager : MonoBehaviour
    {
        public static AudioMenager Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

    }
}