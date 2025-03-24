using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using System.Collections;

namespace Foundations
{
    public class Loading_Mono : MonoBehaviour
    {
        public Slider loadingSlider;

        string m_next_scene_key;

        AsyncOperationHandle<SceneInstance> m_sceneLoadHandle;

        //==================================================================================================

        private void Awake()
        {
            Share_DS.instance.try_get_value("next_scene", out m_next_scene_key);

            StartCoroutine(DownloadAndLoadScene());
        }


        private IEnumerator DownloadAndLoadScene()
        {
            m_sceneLoadHandle = Addressables.LoadSceneAsync(m_next_scene_key, UnityEngine.SceneManagement.LoadSceneMode.Single, activateOnLoad: true);
            m_sceneLoadHandle.Completed += M_sceneLoadHandle_Completed;

            while (!m_sceneLoadHandle.IsDone)
            {
                UpdateProgress(m_sceneLoadHandle.PercentComplete);
                yield return null;
            }

            #region 子函数 load_success
            void M_sceneLoadHandle_Completed(AsyncOperationHandle<SceneInstance> obj)
            {
                Debug.Log($"【{m_next_scene_key}】场景加载成功！");
            }
            #endregion
        }

        

        void UpdateProgress(float progress)
        {
            loadingSlider.value = progress;
        }
    }
}
