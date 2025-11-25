using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
    /// <summary>
    /// 常用的API擴展
    /// </summary>
    public static class UnityExtension
    {
        #region -- Camera

        /// <summary>
        /// 通过鼠标滚轮里滑和外滑的方式来调整摄像机的视角达到放大或缩小场景的目的
        /// </summary>
        /// <param name="self">摄像机</param>
        public static void ScaleScene(this Camera self)
        {
            //里滑 -> 放大
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                if (self.fieldOfView <= 100)
                {
                    self.fieldOfView += 2;
                }
                else if (self.orthographicSize <= 20)
                {
                    self.orthographicSize += 0.5f;
                }
            }

            //外滑 -> 缩小
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                if (self.fieldOfView > 25)
                {
                    self.fieldOfView -= 2;
                }
                else if (self.orthographicSize >= 1)
                {
                    self.orthographicSize -= 0.5f;
                }
            }
        }

        #endregion
        
        #region -- GameObject

        /// <summary>
        /// 獲取或添加組件
        /// </summary>
        /// <param name="self"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>T（組件）</returns>
        public static T GetOrAddComponent<T>(this GameObject self) where T : Component
        {
            T cpt = self.GetComponent<T>();
            return cpt ? cpt : self.AddComponent<T>();
        }
        
        /// <summary>
        /// 通过鼠标滚轮里滑和外滑的方式来调整物体的localScale达到放大和缩小物体的目的
        /// </summary>
        /// <param name="self">要缩放的物体</param>
        /// <param name="speed">速率</param>
        /// <param name="minScale">最小缩放比例限制</param>
        /// <param name="maxScale">最大缩放比例限制</param>
        public static void ScaleGameObject(this GameObject self, float speed, float minScale = 0.3f, float maxScale = 5f)
        {
            if (speed <= 0)
            {
                speed = 1;
            }

            float s = Input.GetAxis("Mouse ScrollWheel");
            float scale = 0.0f;

            if (s > 0)
            {
                scale = s * speed;
            }

            if (s < 0)
            {
                scale = 1f / (s * speed);
            }

            if (self.transform.localScale.x > maxScale)
            {
                self.transform.localScale = new Vector3(maxScale, maxScale, maxScale);
            }

            if (self.transform.localScale.x <= maxScale && self.transform.localScale.x >= minScale)
            {
                self.transform.localScale += new Vector3(scale, scale, scale);
            }

            if (self.transform.localScale.x < minScale)
            {
                self.transform.localScale = new Vector3(minScale, minScale, minScale);
            }
        }

        #endregion

        #region -- Text

        /// <summary>
        /// 打字机效果输出文字内容，一个文字接一个
        /// </summary>
        /// <param name="self">文本组件</param>
        /// <param name="content">文字内容</param>
        /// <param name="intervalTime">间隔时间</param>
        /// <returns></returns>
        public static IEnumerator TypeWriter(this UnityEngine.UI.Text self, string content, float intervalTime)
        {
            self.text = "";
            int index = 0;
            while (index < content.Length)
            {
                yield return new WaitForSeconds(intervalTime);
                self.text += content[index];
                index++;
            }
        }
        
        #endregion

        #region -- Transform -> rotate

        /// <summary>
        /// 物体绕自身旋转
        /// </summary>
        /// <param name="self">自身物体</param>
        /// <param name="speed">旋转速度</param>
        public static void AroundSelf(this Transform self, float speed)
        {
            if (self != null)
            {
                self.Rotate(Vector3.up, -speed * Input.GetAxis("Mouse X"), Space.World);
                self.Rotate(Vector3.right, speed * Input.GetAxis("Mouse Y"), Space.World);
            }
        }
        
        /// <summary>
        /// 物体绕目标物体旋转
        /// </summary>
        /// <param name="self">自身物体</param>
        /// <param name="target">目标物体</param>
        /// <param name="speed">旋转速度</param>
        public static void AroundTarget(Transform self, Transform target, float speed)
        {
            if (self != null && target != null)
            {
                var position = target.position;
                self.RotateAround(position, self.up, speed * Input.GetAxis("Mouse X"));
                self.RotateAround(position, self.right, -speed * Input.GetAxis("Mouse Y"));
            }
        }

        #endregion

        #region -- Transform -> material

        /// <summary>
        /// 得到物体上所有的材质球
        /// </summary>
        /// <param name="self"></param>
        public static Dictionary<string, Material[]> GetAllMaterials(this Transform self)
        {
            if (self != null)
            {
                Renderer[] renderers = self.GetComponentsInChildren<Renderer>();
                Dictionary<string, Material[]> materials = new Dictionary<string, Material[]>();
                foreach (Renderer renderer in renderers)
                {
                    string key = renderer.name;
                    Material[] mats = renderer.materials;
                    if (!materials.ContainsKey(key))
                    {
                        materials.Add(key, mats);
                    }
                }

                return materials;
            }

            return null;
        }

        /// <summary>
        /// 把物体上的所有材质球改为同一个
        /// </summary>
        /// <param name="self"></param>
        /// <param name="material"></param>
        public static void ModifyAllMaterials(this Transform self, Material material)
        {
            if (self == null || material == null)
            {
                Debug.Log("The GameObject or Material is NULL");
                return;
            }

            Renderer[] renderers = self.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] materials = new Material[renderers[i].materials.Length];
                for (int j = 0; j < materials.Length; j++)
                {
                    materials[j] = material;
                }

                renderers[i].materials = materials;
            }
        }

        /// <summary>
        /// 把物体上的所有材质球修改为设置的材质球
        /// </summary>
        /// <param name="self"></param>
        /// <param name="materials"></param>
        public static void ModifyAllMaterials(this Transform self, Material[][] materials)
        {
            if (self == null || materials.Length == 0)
            {
                Debug.Log("The GameObject or Material is NULL");
                return;
            }

            Renderer[] renderers = self.GetComponentsInChildren<Renderer>();
            if (materials.Length != renderers.Length)
            {
                Debug.Log("The count of material is NOT true");
                return;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].materials.Length == materials[i].Length)
                {
                    renderers[i].materials = materials[i];
                }
                else
                {
                    Debug.Log($"物体：{renderers[i].gameObject.name}替换材质球失败");
                }
            }
        }

        #endregion

        #region -- Animator

        /// <summary>
        /// 动画是否播放完毕
        /// </summary>
        /// <param name="self"></param>
        /// <param name="motionName"></param>
        /// <param name="time">动画归一化的时间，默认是1，但是有些动画不能达到1，可以用0.95左右的值去判断</param>
        /// <returns></returns>
        public static bool AnimatorIsOvered(this Animator self, string motionName, float time = 1.0f)
        {
            AnimatorStateInfo animatorStateInfo = self.GetCurrentAnimatorStateInfo(0);
            if (animatorStateInfo.IsName(motionName))
            {
                if (animatorStateInfo.normalizedTime >= time)
                {
                    self.speed = 0;
                    return true;
                }
            }
            else
            {
                Debug.LogError($"动画：{self} 不包含：{motionName}");
            }

            return false;
        }

        #endregion
    }
}